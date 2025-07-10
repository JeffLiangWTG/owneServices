using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update.CodeMappings;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DataTransfer.Native.Business.Update.Rates
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
	public class RateInterceptor : BaseInterceptor
	{
		#region Constants

		const string ClientRateCode = "SAL";
		const string CostingCode = "COS";
		const string QuotationCode = "QTE";
		const string CompanyTariffCode = "GLB";
		const string PublisherPropertyName = "Publisher";

		const string RateTypePropertyName = "RateType";
		const string CompanyTarifflevelPropertyName = "CompanyTariffLevel";
		const string CurrencyPropertyName = "Currency";
		const string ConversionFactorPropertyName = "ConversionFactor";
		const string FactorNumeratorPropertyName = "FactorNumerator";
		const string FactorDenominatorPropertyName = "FactorDenominator";
		const string CodePropertyName = "Code";
		const string PKPropertyName = "PK";
		const string RateEntryCreationSourcePropertyName = "CreationSource";
		const string CarrierServiceLevelEntityName = "CarrierServiceLevel";
		const string CarrierServiceLevelDescriptionPropertyName = "CarrierServiceLevelDescription";
		const string ConditionalExpressionPropertyName = "ConditionalExpression";
		const string ConditionPropertyName = "Condition";
		const string ProviderReferenceID = "ProviderReferenceID";

		#endregion

		public RateInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}

		readonly BusinessObjectFactory factory;

		RateSetting Setting
		{
			get { return (RateSetting)InterceptorSetting; }
		}

		public override void Invoke(IEntitySet entitySet)
		{
			var checkResult = CheckRateEntities(entitySet);

			var root = entitySet.Root;
			root.DepthFirstTraversal((entity, _) => UpdateRateEntities(entity, checkResult));

			ValidateCompany(checkResult.RateType, checkResult.IsLocalRate);
			ValidateConversionFactorOnRateLines(root);
			ValidateCurrencyOnRateLines(root);
			ValidateConditionalExpressionOnRateLines(root);

			try
			{
				Function(entitySet);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var dbErrorMatch = ex.InnerException is SqlException innerSqlException ? new DbErrorMatch(innerSqlException) : null;
				var exceptionType = dbErrorMatch?.ExceptionType;

				switch (exceptionType)
				{
					case DbErrorType.RatingRateLineInvalidChargeCode:
						throw new NativeXMLUserVisibleException("Rates cannot be imported as Charge Code(s) are invalid or cannot be found in the current Company.");

					case DbErrorType.RatingRateEntryOverlap:
						throw new NativeXMLUserVisibleException("Rates cannot be imported as duplicated rates existed.");

					default:
						throw;
				}
			}
		}

		#region Check and Update Rate Entities

		struct RateEntitiesCheckResult
		{
			public string RateType;
			public bool ShouldIgnoreEntitiesPK;
			public bool IsLocalRate;
			public Guid TargetCompanyPK;
			public bool FromEdiMessage;
		}

		RateEntitiesCheckResult CheckRateEntities(IEntitySet entitySet)
		{
			// RatingHeader is the root entity
			var root = entitySet.Root;

			var rateTypeProperty = FindPropertyByName(root, RateTypePropertyName, true);
			var rateType = (string)rateTypeProperty?.Value;

			SkipRateLinesWithHighCompanyTariffLevel(root, rateType);

			var shouldUseProviderReferenceIDs = root.ChildrenCollection
				.Select(rateEntryEntity => rateEntryEntity.GetPropertyOrBlankString(ProviderReferenceID))
				.Any(id => id.Length > 0);

			var checkResult = new RateEntitiesCheckResult
			{
				RateType = rateType,
				ShouldIgnoreEntitiesPK = shouldUseProviderReferenceIDs,
			};

			var recipientRoleCollection = Setting.Source?.DataContext?.RecipientRoleCollection;
			var hasClientRecipientRole = CheckAndUpdateClientRateOrganization(root, recipientRoleCollection, rateTypeProperty);
			checkResult.ShouldIgnoreEntitiesPK = checkResult.ShouldIgnoreEntitiesPK || hasClientRecipientRole;

			CheckRateEntitiesPkAndTargetCompany(root, recipientRoleCollection, ref checkResult);

			return checkResult;
		}

		static Property FindPropertyByName(IEntity entity, string propertyName, bool isMandatoryProperty = false, bool isOrgProperty = false)
		{
			var property = entity.Properties.FirstOrDefault(x => x.Name == propertyName);

			if (property == null && isMandatoryProperty)
			{
				var errorMessage = isOrgProperty
					? Invariant($"Can not find rate organization {propertyName} property")
					: Invariant($"Can not find property {propertyName}");

				throw new NativeXMLUserVisibleException(errorMessage);
			}

			return property;
		}

		/// <summary>
		/// RateLines with CompanyTariffLevel higher than 1 are only supported in CompanyTariff
		/// </summary>
		void SkipRateLinesWithHighCompanyTariffLevel(IEntity entity, string rateType)
		{
			if (rateType == CompanyTariffCode)
			{
				return;
			}

			foreach (var rateEntry in entity.Children)
			{
				foreach (var rateLine in rateEntry.Children.ToList())
				{
					var tariffLevel = FindPropertyByName(rateLine, CompanyTarifflevelPropertyName, isMandatoryProperty: false);
					if (tariffLevel == null)
					{
						continue;
					}

					if (!int.TryParse(tariffLevel.Value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var companyTariffLevel) || companyTariffLevel <= 1)
					{
						continue;
					}

					rateEntry.ChildrenCollection.Remove(rateLine);
					var info = Invariant($"Skipped Rate Line with Tariff Level {companyTariffLevel}");
					sessionServices.Logger.Log(LogType.Information, info);
				}
			}
		}

		bool CheckAndUpdateClientRateOrganization(IEntity root, List<DataContextWrapper.RecipientRoleWrapper> recipientRoleCollection, Property rateTypeProperty)
		{
			var hasClientRecipientRole = recipientRoleCollection?.Any(x => x.Code == MessageRecipientPartyTypeList.Codes.Client) ?? false;
			var rateType = (string)rateTypeProperty.Value;
			if (rateType == ClientRateCode && hasClientRecipientRole)
			{
				rateTypeProperty.Value = CostingCode;
				UpdateRateOrganization(root);
			}

			return hasClientRecipientRole;
		}

		void UpdateRateOrganization(IEntity entity)
		{
			var rateOrgEntity = entity.Parents.FirstOrDefault(x => x.Definition.MainAssociation.ForeignKeys.Any(y => y.Name == RatingHeaderSchema.Constants.TH_OH)) ?? throw new NativeXMLUserVisibleException("Can not find organization entity for rate");

			var ownerCode = Setting.Source?.OwnerCode ?? "";
			var codeProperty = FindPropertyByName(rateOrgEntity, CodePropertyName, true, true);
			var codeMappingRepository = new CodeMappingRepository(Setting.Context.ObjectFactory);
			var mappedCode = codeMappingRepository.MapLocalCode(Constants.OrgPatternMatchOverrideRelationships.Organisation, ownerCode, OrgHeader.DefaultOrg.PK.ToGuid());

			var originalCode = codeProperty.Value;
			codeProperty.Value = string.IsNullOrEmpty(mappedCode) ? ownerCode : mappedCode;
			rateOrgEntity.InternalPK = Guid.Empty;

			var info = Invariant($"Carrier sell rates have been transformed to client buy rates. Carrier: {codeProperty.Value}, Client: {originalCode}");
			sessionServices.Logger.Log(LogType.Information, info);
		}

		void CheckRateEntitiesPkAndTargetCompany(IEntity root, List<DataContextWrapper.RecipientRoleWrapper> recipientRoleCollection, ref RateEntitiesCheckResult checkResult)
		{
			var importingRatingHeaderCompanyPk = GetRatingHeaderCompanyPk(root);

			// NativeXML from EdiMessage should always set a company to import the rates to.
			var targetCompanyPk = Setting.Source?.TargetCompanyPK ?? Guid.Empty;
			var fromEdiMessage = !targetCompanyPk.IsEmpty();

			var shouldIgnoreEntitiesPk = true;
			if (root.Action == EntityAction.INSERT)
			{
				targetCompanyPk = fromEdiMessage ? targetCompanyPk : Env.CurrentCompanyPK;
			}
			else
			{
				shouldIgnoreEntitiesPk = checkResult.ShouldIgnoreEntitiesPK;
				if (!shouldIgnoreEntitiesPk)
				{
					shouldIgnoreEntitiesPk = recipientRoleCollection?.Any(x => x.Code == MessageRecipientPartyTypeList.Codes.OrgProxy) ?? false;
				}

				targetCompanyPk = fromEdiMessage ? targetCompanyPk : importingRatingHeaderCompanyPk;

				// Final check to see if we should ignore the PKs of the entities in the XML because there is no such RatingHeader for the target company.
				// We provide this flexibility to allow the user to export data with ACTIONs other than INSERT in a company but can add new data in another company.
				// It can only make sense when the ACTION is MERGE, but I will leave it as is for now.
				if (!shouldIgnoreEntitiesPk && root.InternalPK != Guid.Empty)
				{
					var query = new ZQuery(RatingHeaderSchema.PK, root.InternalPK);
					query.AddToFilter(
						RatingHeaderSchema.TH_GC,
						targetCompanyPk != Guid.Empty
							? targetCompanyPk
							: DBNull.Value);
					shouldIgnoreEntitiesPk = !factory.ExistsInDatabase(RatingHeaderSchema.Constants.TableName, query);
				}
			}

			checkResult.ShouldIgnoreEntitiesPK = shouldIgnoreEntitiesPk;
			checkResult.IsLocalRate = importingRatingHeaderCompanyPk != Guid.Empty;
			checkResult.TargetCompanyPK = targetCompanyPk;
			checkResult.FromEdiMessage = fromEdiMessage;
		}

		/// <returns>
		///		- Guid.Empty: the entity is a global rate (no associated company).
		///		- otherwise, the entity is a local rate (bound to a company).
		/// </returns>
		Guid GetRatingHeaderCompanyPk(IEntity entity)
		{
			var glbCompanyEntity = entity.Parents.FirstOrDefault(x => x.EntityName == GlbCompanySchema.Constants.TableName);
			if (glbCompanyEntity == null)
			{
				return Guid.Empty;
			}

			var companyPk = glbCompanyEntity.InternalPK;
			if (companyPk != Guid.Empty)
			{
				return companyPk;
			}

			var companyCode = FindPropertyByName(glbCompanyEntity, CodePropertyName)?.Value?.ToString();
			if (string.IsNullOrEmpty(companyCode))
			{
				return Guid.Empty;
			}

			var glbCompany = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);

			return glbCompany?.PK.ToGuid() ?? Guid.Empty;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdateRateEntities(IEntity entity, RateEntitiesCheckResult checkResult)
		{
			switch (entity.EntityName)
			{
				case AccChargeCodeSchema.Constants.TableName:
					RemoveEntityPK(entity, true);
					break;

				case RatingHeaderSchema.Constants.TableName:
				case RateLinesSchema.Constants.TableName:
				case StmNoteSchema.Constants.TableName:
					RemoveEntityPKIfNeeded(entity, checkResult.ShouldIgnoreEntitiesPK);
					break;

				case RateEntrySchema.Constants.TableName:
					UpdateCreationSource(entity);
					RemoveEntityPKIfNeeded(entity, checkResult.ShouldIgnoreEntitiesPK);
					break;

				case RateLineItemsSchema.Constants.TableName:
					if (!checkResult.ShouldIgnoreEntitiesPK)
					{
						break;
					}

					RemoveEntityPKIfNeeded(entity, shouldIgnoreEntitiesPK: true);

					var parentProviderReferenceId = entity.Parent.GetPropertyOrBlankString(ProviderReferenceID);
					if (string.IsNullOrEmpty(parentProviderReferenceId))
					{
						// No PK nor ProviderReferenceID, how can this entity be identified?
						// UniqueCriteria (see RateSetDefinition.xml) are used for matching.
						// If one of them is changed, the entity will be considered as a new one.
						break;
					}

					// The current point runs in the pre-update stage, when the engine traverses the entities in depth-first order to check and modify them.
					// Later, in the update rows stage, we always delete all RateLineItem rows that belong to a parent RateLine having a ProviderReferenceID.
					// See the extra-nursing for RateLine in EntityRepository.FindRows.
					// It gives us the benefit of dealing with all the RateLineItem entities without worrying about the PKs or IDs.

					// When parent RateLine action is DELETE, we can tell the engine to ignore this entity, no matter what its action is.
					// When parent RateLine action is not DELETE but RateLineItem action is DELETE, we also ignore this entity.
					// In all other cases, the entity should be insert back.
					entity.Action = entity.Parent.Action == EntityAction.DELETE || entity.Action == EntityAction.DELETE
						? EntityAction.IGNORE
						: EntityAction.INSERT;

					break;

				case GlbCompanySchema.Constants.TableName:
					if (checkResult.IsLocalRate)
					{
						UpdateCompanyPK(entity, checkResult.FromEdiMessage, checkResult.TargetCompanyPK, isLocal: true);
					}
					break;

				case PublisherPropertyName:
					UpdateCompanyPK(entity, checkResult.FromEdiMessage, checkResult.TargetCompanyPK, checkResult.IsLocalRate);
					break;

				case OrgMiscServSchema.Constants.TableName:
					UpdateOrgMiscServicesPK(entity);
					break;

				case CarrierServiceLevelEntityName:
					ClearCarrierServiceLevelDefaultValues(entity);
					break;
			}
		}

		void RemoveEntityPKIfNeeded(IEntity entity, bool shouldIgnoreEntitiesPK)
		{
			if (entity.Action != EntityAction.DELETE)
			{
				RemoveEntityPK(entity, shouldIgnoreEntitiesPK);
			}
		}

		static void UpdateCreationSource(IEntity entity)
		{
			if (entity.Action == EntityAction.INSERT || entity.Action == EntityAction.MERGE)
			{
				// This "XML" is defined in RateEntryCreator.Sources.FromNativeXML
				// but I didn't want to add a reference to it in DataTransfer
				entity[RateEntryCreationSourcePropertyName] = "XML";
			}
		}

		void RemoveEntityPK(IEntity entity, bool shouldIgnoreEntitiesPK)
		{
			if (shouldIgnoreEntitiesPK)
			{
				if (entity.HasProperty(PKPropertyName))
				{
					entity[PKPropertyName] = Guid.Empty;
				}
				entity.InternalPK = Guid.Empty;
			}
		}

		/// <summary>
		///		When isLocal is false, the rate is a global rate and the companyPK should be empty.
		///		However, it can have a dictated non-empty value from EDIMessage which means a global rate can be converted to a local rate.
		/// </summary>
		void UpdateCompanyPK(IEntity entity, bool fromEdiMessage, Guid companyPK, bool isLocal)
		{
			if (companyPK == Guid.Empty)
			{
				return;
			}

			if (fromEdiMessage && !hasLoggedOverridingCompany)
			{
				hasLoggedOverridingCompany = true;
				var info = isLocal
					? Invariant($"GlbCompany set by TargetCompanyPK ({companyPK}) from ediMessage.")
					: Invariant($"Publisher set by TargetCompanyPK ({companyPK}) from ediMessage.");
				sessionServices.Logger.Log(LogType.Information, info);
			}

			entity[PKPropertyName] = companyPK;
			entity.InternalPK = companyPK;
		}

		bool hasLoggedOverridingCompany;

		void UpdateOrgMiscServicesPK(IEntity entity)
		{
			var orgHeader = entity.Parents.FirstOrDefault(x => x.EntityName == OrgHeaderSchema.Constants.TableName);
			var orgCode = orgHeader?.Properties.FirstOrDefault(x => x.Name == CodePropertyName)?.Value?.ToString();
			if (orgCode == null)
			{
				return;
			}

			var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgMiscServSchema.OM_OH);
			subQuery.AddToFilter(OrgHeaderSchema.OH_Code, orgCode);
			var query = new ZDBOnlyQuery(typeof(OrgMiscServ));
			query.AddSubQuery(subQuery, JoinCondition.And);

			var actualOrgMiscServ = Setting.Context.ObjectFactory.LoadTop1<OrgMiscServ>(query);
			if (actualOrgMiscServ != null)
			{
				entity[PKPropertyName] = actualOrgMiscServ.PK;
			}
		}

		static void ClearCarrierServiceLevelDefaultValues(IEntity entity)
		{
			var code = entity.HasProperty(CodePropertyName)
				? entity[CodePropertyName].ToString().ToUpper(CultureInfo.InvariantCulture)
				: null;

			var description = entity.HasProperty(CarrierServiceLevelDescriptionPropertyName)
				? entity[CarrierServiceLevelDescriptionPropertyName].ToString()
				: null;

			if (code == OrgCarrierServiceLevel.StandardCode && (string.IsNullOrEmpty(description) || description == OrgCarrierServiceLevel.StandardDescription.GetUnresolvedString()))
			{
				entity.Action = EntityAction.IGNORE;
			}
		}

		#endregion

		#region Validations

		static void ValidateConversionFactorOnRateLines(IEntity entity)
		{
			foreach (var rateEntry in entity.Children)
			{
				foreach (var rateLine in rateEntry.Children.ToArray())
				{
					var factor = FindPropertyByName(rateLine, ConversionFactorPropertyName);
					var factorNumerator = FindPropertyByName(rateLine, FactorNumeratorPropertyName)?.Value?.ToString() ?? string.Empty;
					var factorDenominator = FindPropertyByName(rateLine, FactorDenominatorPropertyName)?.Value?.ToString() ?? string.Empty;

					decimal factorParsed = 0;
					if (factor == null || decimal.TryParse(factor?.Value?.ToString(), out factorParsed))
					{
						var conversionFactor = new ConversionFactor(factorParsed, factorNumerator, factorDenominator);
						if (!conversionFactor.IsValid && !conversionFactor.IsEmpty)
						{
							throw new NativeXMLUserVisibleException(Invariant($"Expected: conversion factor, numerator and denominator to be all set or all empty. Actual: {factorParsed} {factorNumerator}/{factorDenominator}"));
						}
					}
					else
					{
						throw new NativeXMLUserVisibleException(Invariant($"The conversion factor is invalid: {factor} {factorNumerator}/{factorDenominator}"));
					}
				}
			}
		}

		static void ValidateCurrencyOnRateLines(IEntity entity)
		{
			foreach (var rateEntry in entity.Children)
			{
				var linesToValidate = rateEntry.Children
					.Where(c => c.Action == EntityAction.INSERT || rateEntry.Action == EntityAction.INSERT)
					.ToList();

				foreach (var rateLine in linesToValidate)
				{
					var currency = rateLine.Parents.FirstOrDefault(x => x.EntityName == CurrencyPropertyName) ?? throw new NativeXMLUserVisibleException(Invariant($"The rate has no currency"));
				}
			}
		}

		static void ValidateConditionalExpressionOnRateLines(IEntity entity)
		{
			foreach (var rateEntry in entity.Children)
			{
				foreach (var rateLine in rateEntry.Children.ToArray())
				{
					var condition = FindPropertyByName(rateLine, ConditionPropertyName)?.Value?.ToString() ?? string.Empty;
					var conditionalExpression = FindPropertyByName(rateLine, ConditionalExpressionPropertyName)?.Value?.ToString() ?? string.Empty;

					if (condition != RateLineConditions.UserDefined && !string.IsNullOrEmpty(conditionalExpression))
					{
						throw new NativeXMLUserVisibleException(Invariant($"RateLines.ConditionalExpression validation failed: ConditionalExpression can only be set when Condition is set to USR"));
					}
					else if (condition == RateLineConditions.UserDefined && string.IsNullOrEmpty(conditionalExpression))
					{
						throw new NativeXMLUserVisibleException(Invariant($"RateLines.ConditionalExpression validation failed: ConditionalExpression should be specified when Condition is set to USR"));
					}
				}
			}
		}

		static void ValidateCompany(string rateType, bool isLocal)
		{
			if (isLocal)
			{
				return;
			}

			switch (rateType)
			{
				case QuotationCode:
					throw new NativeXMLUserVisibleException("To import this Quotation XML, please add the GlbCompany.");

				case CompanyTariffCode:
					ThrowErrorIfNotAllowed("Company Tariff", Env.Security.GlobalTariffRates);
					break;

				case ClientRateCode:
					ThrowErrorIfNotAllowed("Client Rate", Env.Security.GlobalClientRates);
					break;

				case CostingCode:
					ThrowErrorIfNotAllowed("Costing", Env.Security.GlobalCostingRates);
					break;
			}
		}

		#endregion

		static void ThrowErrorIfNotAllowed(string rateTypeDesc, SecurityCheckpoint security)
		{
			if (!security.IsAllowed)
			{
				var securityPath = security.DisplayTextPathToSecurityRight.ToString();
				var message = Invariant($"You do not have security rights to import {rateTypeDesc} XMLs without a Company. Either add a GlbCompany to import as Local {rateTypeDesc} or enable security: {securityPath}");

				throw new NativeXMLUserVisibleException(message);
			}
		}
	}
}
