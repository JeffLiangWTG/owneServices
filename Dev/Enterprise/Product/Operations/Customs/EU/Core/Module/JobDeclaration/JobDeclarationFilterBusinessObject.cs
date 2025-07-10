using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;
using JobDeclaration = Enterprise.Customs.EU.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.EU.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.EU.IJobDeclarationFilterBusinessObject
	{
		protected override void AddShipmentTypeAndShipmentSubTypeFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.EntryType, JobDeclarationSchema.JE_MessageType, Lookups.MessageTypeList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("FD89CDA2-3690-4E6E-B74A-D5DB964A687F", DeclarationFilterConstants.EntryType);
			AddEntrySubTypeFilter(filters, Lookups.EntrySubTypes);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddShipmentTypeFilters(filters);
			AddDeclarationTypeFilters(filters, Lookups.DeclarationTypeList);
			AddCTStatusFilters(filters);
			AddClearanceDateFilters(filters);
			AddEntryStyleFilters(filters);
			AddLocationOfGoodsFilters(filters);
			AddCustomsOfficeFilters(filters);
			AddIndirectExportFilter(filters);
			AddSupportingDocumentFilters(filters);
			AddPreviousDocumentFilters(filters);
			AddGuaranteeFilters(filters);
			AddVehicleVINFilters(filters);
			AddPackingFilters(filters);
			AddSealFilters(filters);
			AddInvoiceLineRelatedFilters(filters);
			AddInlandTransportDetails(filters);
			AddExitPresentationStatusFilter(filters);
			AddRequestedProcedureFilters(filters);
			AddCusEntryHeaderFilters(filters);
			return filters;
		}

		protected virtual ModuleTextFilter AddCusEntryHeaderFilters(ModuleFilterCollection filters)
		{
			var exitedStatus = filters.AddTextFilter(EntryHeaderFilterBusinessObject.EUFilterConstants.ExitedStatus, GetEntryExitedStatusQuery, Lookups.ExportExitStatusList);
			exitedStatus.Category = FilterCategories.StatusAndFlags;
			exitedStatus.MaxLength = CusEntryHeaderSchema.CH_ExitedStatus.MaxLength;
			exitedStatus.MultilingualDescription = ResString.GetMultilingualString("1817765F-069B-4D53-811E-69FB5CA57084", EntryHeaderFilterBusinessObject.EUFilterConstants.ExitedStatus);
			return exitedStatus;
		}

		void AddInlandTransportDetails(ModuleFilterCollection filters)
		{
			var transportIdFilter = new AddInfoModuleTextFilter(DeclarationFilterConstants.InlandTransportDetails.TransportIdInland, GetUCC6EquivalentOfZG_Box18TransportIDQuery)
				.WithMaxLengthOf<ModuleTextFilter>(EUAddInfoSchema.ZG_Box18TransportID);
			transportIdFilter.Category = FilterCategories.TextSearch;
			transportIdFilter.MultilingualDescription = ResString.GetMultilingualString("EDB3A3B4-0E1E-4099-8A5D-37A565839544", DeclarationFilterConstants.InlandTransportDetails.TransportIdInland);
			filters.AddFilter(transportIdFilter);

			var moduleFilter3 = filters.AddTextFilter(DeclarationFilterConstants.InlandTransportDetails.TransportModeInland, JobDeclarationSchema.JE_TransportModeInland, Lookups.TransportTypeList);
			moduleFilter3.Category = FilterCategories.ModesAndTypes;
			moduleFilter3.MultilingualDescription = ResString.GetMultilingualString("2D455134-7389-445F-8EA6-3F3B16400784", DeclarationFilterConstants.InlandTransportDetails.TransportModeInland);
			moduleFilter3.PropertyValidation = TransportTypeValidation;

			var transportNationalityInland = filters.AddNkFilter(DeclarationFilterConstants.InlandTransportDetails.TransportNationalityInland, GetUCC6EquivalentOfZG_Box18TransportNationalityQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(JobDeclarationSchema.JE_RN_NKTransportNationalityInland);
			transportNationalityInland.Category = FilterCategories.Locations;
			transportNationalityInland.MultilingualDescription = ResString.GetMultilingualString("6A0260BE-AAF4-4B6E-A869-49C226A43214", DeclarationFilterConstants.InlandTransportDetails.TransportNationalityInland);
			transportNationalityInland.PropertyValidation = CodeFilterValidation;

			var nKfilter = filters.AddNkFilter(DeclarationFilterConstants.InlandTransportDetails.TransportNationality, JobDeclarationSchema.JE_RN_NKTransportNationality, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			nKfilter.Category = FilterCategories.Locations;
			nKfilter.MultilingualDescription = ResString.GetMultilingualString("AC9E1FC1-E547-4408-AD63-D476923E3463", DeclarationFilterConstants.InlandTransportDetails.TransportNationality);
			nKfilter.PropertyValidation = CodeFilterValidation;
		}

		void CodeFilterValidation(ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			if (!info.Value.IsEmpty)
			{
				ListValidation.WarnIfInvalidCode(info, new RefCountryCollection(Factory));
			}
		}

		void TransportTypeValidation(ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			if (!info.Value.IsEmpty)
			{
				ListValidation.WarnIfInvalidCode(info, Lookups.TransportTypeList);
			}
		}

		ZQuery GetUCC6EquivalentOfZG_Box18TransportIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = AddInfoFilterRepository.GetAddInfoQuery(comparisonOperator, value, JobDeclarationSchema.JE_AddInfo, JobDeclaration.Schema.ZG_Box18TransportID.Substring(3));

			result.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_TransportIDInland, comparisonOperator, value);
			return result;
		}

		ZQuery GetEntryExitedStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				var entryHeaderQuery = GetEntryExitedStatusSubQuery(comparisonOperator, value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.Or);
				query.AddSubQuery(GetHasEntryHeadersSubQuery(true), JoinCondition.And);
			}
			else if (comparisonOperator.IsNegativeSQLOperator())
			{
				var entryHeaderQuery = GetEntryExitedStatusSubQuery(comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			else
			{
				var entryHeaderQuery = GetEntryExitedStatusSubQuery(comparisonOperator, value, false);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				if (comparisonOperator == SQLComparisonOperator.IsBlank)
				{
					query.AddSubQuery(GetHasEntryHeadersSubQuery(false), JoinCondition.Or);
				}
			}
			return query;
		}

		ZDBOnlySubQuery GetHasEntryHeadersSubQuery(bool hasHeaders) => new(typeof(Customs.Business.CusEntryHeader), CusEntryHeaderSchema.CH_JE, !hasHeaders);

		ZDBOnlySubQuery GetEntryExitedStatusSubQuery(SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
		{
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryHeader), CusEntryHeaderSchema.CH_JE, notIn);
			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_ExitedStatus, SQLComparisonOperator.IsBlank, value);
			}
			else
			{
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_ExitedStatus, comparisonOperator, value);
			}
			return entryHeaderQuery;
		}

		ZQuery GetUCC6EquivalentOfZG_Box18TransportNationalityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = AddInfoFilterRepository.GetAddInfoQuery(comparisonOperator, value, JobDeclarationSchema.JE_AddInfo, JobDeclaration.Schema.ZG_Box18TransportNationality.Substring(3));

			result.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_RN_NKTransportNationalityInland, comparisonOperator, value);
			return result;
		}

		void AddShipmentTypeFilters(ModuleFilterCollection filters)
		{
			var filter = GetAddInfoTextFilter(DeclarationFilterConstants.ShipmentType, JobDeclaration.Schema.ZG_ShipmentType.Substring(3), Lookups.ShipmentTypeList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("2FFC7627-3579-4930-B96F-A7BF3BC044EA", DeclarationFilterConstants.ShipmentType);
			filters.AddFilter(filter);
		}

		void AddDeclarationTypeFilters(ModuleFilterCollection filters, CodeDescriptionPairList declarationTypeList)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.DeclarationType,
				(SQLComparisonOperator comparisonOperator, ZString value) =>
				{
					var ceiQ = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
					ceiQ.AddToFilter(CusEntryInstructionSchema.CEI_Style, comparisonOperator, value);
					var mainQ = new ZDBOnlyQuery(typeof(JobDeclaration));
					mainQ.AddSubQuery(ceiQ, JoinCondition.And);
					return mainQ;
				},
				() => declarationTypeList)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryInstructionSchema.CEI_Style);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("07B7533D-A008-4DEC-AAA3-6D333D936A64", DeclarationFilterConstants.DeclarationType);
		}

		void AddCTStatusFilters(ModuleFilterCollection filters)
		{
			var filter = GetAddInfoTextFilter(DeclarationFilterConstants.CTStatus, JobDeclaration.Schema.ZG_CTStatusID.Substring(3), Lookups.CommunityTransitStatusIDList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("3F239BC8-FB89-4C5F-94BD-D41AC6E0F6F0", DeclarationFilterConstants.CTStatus);
			filters.AddFilter(filter);
		}

		void AddClearanceDateFilters(ModuleFilterCollection filters)
		{
			var dateFilter = filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ClearanceDate,
			(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate) =>
			{
				var isNotInQuery = comparisonOperator == DateComparisonOperator.HasNoDateEntered;
				comparisonOperator = comparisonOperator == DateComparisonOperator.HasNoDateEntered ? DateComparisonOperator.HasDateEntered : comparisonOperator;

				var query = new ZDBOnlyQuery(typeof(JobDeclaration));
				var subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, isNotInQuery);
				subQuery.AddToFilter(StmALogSchema.SL_Table, JobDeclarationSchema.Constants.TableName);
				subQuery.AddToFilter(JobDeclaration.GetDateOfClearanceQueryForStmALog());
				AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, fromDate, toDate);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			});
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("0D96D833-4986-48D6-AA6B-45FDB6CA341E", DeclarationFilterConstants.DateFilterTypes.ClearanceDate);
		}

		void AddInvoiceLineRelatedFilters(ModuleFilterCollection filters)
		{
			var invLineSubGroup = new InvoiceLineSpecificFieldSubGroup();
			var cpcFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.CustomsProcedureCode, JobComInvoiceLineSchema.JI_Procedure)
								.WithMaxLengthOf<ModuleNumberFilter>(JobComInvoiceLineSchema.JI_Procedure);
			cpcFilter.MultilingualDescription = ResString.GetMultilingualString("2FD453C3-04DF-4A00-9B8E-58042FB8473E", DeclarationFilterConstants.NumberFilterTypes.CustomsProcedureCode);
			cpcFilter.SubGroup = invLineSubGroup;

			if (!SupportsMultipleVehicles)
			{
				var vehicleModelFilter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.VehicleModel, JobComInvoiceLineSchema.JI_Model)
											.WithMaxLengthOf<ModuleTextFilter>(JobComInvoiceLineSchema.JI_Model);
				vehicleModelFilter.MultilingualDescription = ResString.GetMultilingualString("D9D224C8-39BF-4F72-8304-35DE61784B3A", DeclarationFilterConstants.NumberFilterTypes.VehicleModel);
				vehicleModelFilter.SubGroup = invLineSubGroup;
				vehicleModelFilter.UseMultiSearch = true;

				var vehicleBrandFilter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.VehicleBrand, JobComInvoiceLineSchema.JI_BrandName)
											.WithMaxLengthOf<ModuleTextFilter>(JobComInvoiceLineSchema.JI_BrandName);
				vehicleBrandFilter.MultilingualDescription = ResString.GetMultilingualString("DF95C51C-ACFF-49CB-A280-3FBF8E162896", DeclarationFilterConstants.NumberFilterTypes.VehicleBrand);
				vehicleBrandFilter.SubGroup = invLineSubGroup;
				vehicleBrandFilter.UseMultiSearch = true;
			}

			var preferenceFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.Preference, JobComInvoiceLineSchema.JI_PrimaryPreference)
										.WithMaxLengthOf<ModuleNumberFilter>(JobComInvoiceLineSchema.JI_PrimaryPreference);
			preferenceFilter.MultilingualDescription = ResString.GetMultilingualString("7B9A6DC7-E9CA-4C45-B90E-988A280D9F18", DeclarationFilterConstants.NumberFilterTypes.Preference);
			preferenceFilter.SubGroup = invLineSubGroup;

			var originFilter = filters.AddTextFilter(DeclarationFilterConstants.OriginInvoiceLine, JobComInvoiceLineSchema.JI_CountryOfOrigin, Lookups.CountryOfOrigins)
							.WithMaxLengthOf<ModuleTextFilter>(JobComInvoiceLineSchema.JI_CountryOfOrigin);
			originFilter.Category = FilterCategories.Locations;
			originFilter.MultilingualDescription = ResString.GetMultilingualString("B6426C2D-1D59-4138-99A2-866B6CC3BE8B", DeclarationFilterConstants.OriginInvoiceLine);
			originFilter.SubGroup = invLineSubGroup;
		}

		public bool SupportsMultipleVehicles => SupportsMultipleVehiclesCore;
		protected virtual bool SupportsMultipleVehiclesCore => false;

		void AddEntryStyleFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.EntryStyle,
				(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(JobDeclarationSchema.JE_MessageSubType, comparisonOperator, value),
				() =>
				{
					var entryStyleList = new CodeDescriptionPairList();
					entryStyleList.AddRange(Factory.GetCachedValue<EntryStyleListImport>());
					entryStyleList.AddRange(Factory.GetCachedValue<EntryStyleListExport>());
					entryStyleList.Sort();
					return entryStyleList;
				})
				.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_MessageSubType);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("3C4C6767-0CD1-4F5B-8482-147DF7C249F6", DeclarationFilterConstants.EntryStyle);
		}

		public bool SupportsExitControl => SupportsExitControlCore;
		protected virtual bool SupportsExitControlCore => false;
		protected void AddExitPresentationStatusFilter(ModuleFilterCollection filters)
		{
			if (SupportsExitControl)
			{
				var filter = filters.AddTextFilter(DeclarationFilterConstants.ExitPresentationStatus, GetExitPresentationStatusQuery, () => Lookups.ExitPresentationStatuses);
				filter.Category = FilterCategories.StatusAndFlags;
				filter.MultilingualDescription = ResString.GetMultilingualString("2E7858F7-BB04-4CD6-8AC1-A4E130528B38", DeclarationFilterConstants.ExitPresentationStatus);
			}
		}

		public bool SupportRequestedProcedure => SupportRequestedProcedureCore;
		protected virtual bool SupportRequestedProcedureCore => false;

		void AddRequestedProcedureFilters(ModuleFilterCollection filters)
		{
			if (SupportRequestedProcedure)
			{
				var requestedProcedureFilter = filters.AddTextFilter(DeclarationFilterConstants.RequestedProcedure, GetRequestedProcedureQuery, Lookups.RequestedProcedureList);
				requestedProcedureFilter.MaxLength = 2;
				requestedProcedureFilter.Category = FilterCategories.NumbersAndReferences;
				requestedProcedureFilter.MultilingualDescription = DeclarationFilterConstants.RequestedProcedureMultilingualDescription;
				RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(requestedProcedureFilter);

				var previousProcedureFilter = filters.AddTextFilter(DeclarationFilterConstants.PreviousProcedure, GetPreviousProcedureQuery, Lookups.PreviousProcedureCodeList);
				previousProcedureFilter.MaxLength = 2;
				previousProcedureFilter.Category = FilterCategories.NumbersAndReferences;
				previousProcedureFilter.MultilingualDescription = DeclarationFilterConstants.PreviousProcedureMultilingualDescription;
				RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(previousProcedureFilter);

				var additionalProcedureFilter = filters.AddTextFilter(DeclarationFilterConstants.AdditionalProcedure, GetAdditionalProcedureQuery, Lookups.AdditionalProcedureCodeList);
				additionalProcedureFilter.MaxLength = 3;
				additionalProcedureFilter.Category = FilterCategories.NumbersAndReferences;
				additionalProcedureFilter.MultilingualDescription = DeclarationFilterConstants.AdditionalProcedureMultilingualDescription;
				RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(additionalProcedureFilter);
			}
		}

		ZQuery GetExitPresentationStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			ZString sqlString;
			var sqlParams = new ZSqlParameterCollection();
			if (!value.EqualsIgnoringCase(AESEntryStatusList.Codes.MultipleStatus)) // TODO: Use JE_ClusterKey when CXH_ClusterKey has been updated to use parent clusterkey
			{
				sqlString = string.Format(@"JE_PK IN (SELECT CXH_ParentID FROM dbo.CusExitHeader INNER JOIN dbo.CusExitReport ON CXH_PK = CER_CXH_Header AND CER_Type = '" + ExitReportTypeList.Codes.Presentation + "' AND CXH_ParentTableCode = 'JE' WHERE {0} AND CXH_ApplicationCode = 'XIT')", SQLAndParametersForOneCondition(comparisonOperator, value, CusExitReportSchema.CER_Status, sqlParams));
			}
			else
			{
				sqlString = string.Format(@"JE_PK IN (SELECT CXH_ParentID FROM dbo.CusExitHeader WHERE {0} AND CXH_ApplicationCode = 'XIT' AND CXH_PK IN (SELECT CER_CXH_Header FROM dbo.CusExitReport WHERE CER_Type = '" + ExitReportTypeList.Codes.Presentation + "' GROUP BY CER_CXH_Header HAVING COUNT(DISTINCT(CER_Status)) > 1))", SQLAndParametersForOneCondition(SQLComparisonOperator.Equal, "JE", CusExitHeaderSchema.CXH_ParentTableCode, sqlParams));
			}
			result.AddFilterAndZSQLParameterCollection(sqlString, sqlParams);

			return result;
		}

		protected virtual ModuleTextFilter AddLocationOfGoodsFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.LocationOfGoods, GetLocationOfGoodsQuery);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("80B9C2F2-8680-471F-A302-C89254141DFB", DeclarationFilterConstants.LocationOfGoods);
			return filter;
		}

		ZQuery GetLocationOfGoodsQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_LocationOfGoods, comparisonOperator, value);
			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
			var goodsLocationSubQuery = new ZDBOnlySubQuery(typeof(CusGoodsLocation), CusGoodsLocationSchema.CGL_ParentID);
			var adressSubQuery = new ZDBOnlySubQuery(typeof(CusGoodsLocationAddress),JobDocAddressSchema.E2_ParentID);
			adressSubQuery.AddToFilter(JobDocAddressSchema.E2_GovRegNum, comparisonOperator, value);
			goodsLocationSubQuery.AddSubQuery(adressSubQuery, JoinCondition.Or);
			entryInstructionSubQuery.AddSubQuery(goodsLocationSubQuery, JoinCondition.Or);

			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.Or);
			return result;
		}

		void AddCustomsOfficeFilters(ModuleFilterCollection filters)
		{
			var filter = new CustomsOfficeFilter(DeclarationFilterConstants.NumberFilterTypes.CustomsOffice, GetCustomsOfficeQuery, Lookups.CustomsOfficePurposeList)
				.WithMaxLengthOf<CustomsOfficeFilter>(JobDeclarationSchema.JE_CustomsOffice);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("ACA2E517-811C-4737-8F0E-9EAF27502AE9", DeclarationFilterConstants.NumberFilterTypes.CustomsOffice);

			filters.AddCustomFilter(filter);
		}

		ZQuery GetCustomsOfficeQuery(SQLComparisonOperator comparisonOperator, ZString purposeValue, ZString officeValue)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			var customsOfficeSubQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
			customsOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Data, comparisonOperator, officeValue);
			customsOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.OfficeCode);
			if (!purposeValue.IsEmpty)
			{
				customsOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Code, purposeValue);
			}
			else
			{
				result.AddToFilter(JobDeclarationSchema.JE_CustomsOffice, comparisonOperator, officeValue);
			}
			result.AddSubQuery(customsOfficeSubQuery, JoinCondition.Or);
			return result;
		}

		void AddEntrySubTypeFilter(ModuleFilterCollection filters, CodeDescriptionPairList declarationTypeList)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.EntrySubstyle, CusEntryInstructionSchema.CEI_SubStyle, declarationTypeList);
			filter.SubGroup = new EntryInstructionSubGroup();
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("7F8DAE75-B1FC-4D22-A063-C2EEC2576E16", DeclarationFilterConstants.EntrySubstyle);
		}

		void AddIndirectExportFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter(DeclarationFilterConstants.IndirectExport, new string[] { Res.GetString("9ec4184a-75f4-4045-a08a-b410f26c7713", "Show only indirect exports") }, new GetFlagsQuery[] { GetIndirectExportQuery });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("FC003F11-96B4-491C-94DC-93B8E07F7F99", DeclarationFilterConstants.IndirectExport);
		}

		protected virtual ZQuery GetIndirectExportQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (value)
			{
				query.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.Business.JobMessageTypeList.Codes.Export);
				var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.CurrentCulture, "{0} <> '' AND {0} Not Like '{1}%'", JobDeclarationSchema.Constants.JE_CustomsOffice, countryCode), new ZSqlParameterCollection());
			}
			return query;
		}

		#region CusSupportingInfo Filters

		#region Supporting Document Filters

		void AddSupportingDocumentFilters(ModuleFilterCollection filters)
		{
			var typeFilter = filters.AddNkFilter(DeclarationFilterConstants.SupportingDocumentType, GetSupDocTypeQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.SupportingDocumentsType);
			typeFilter.Category = FilterCategories.SupportingDocument;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("906E3F15-2119-4D99-8602-20A508C49743", DeclarationFilterConstants.SupportingDocumentType);

			var refFilter = filters.AddTextFilter(DeclarationFilterConstants.SupportingDocumentRef, GetSupDocRefQuery);
			refFilter.Category = FilterCategories.SupportingDocument;
			refFilter.MultilingualDescription = ResString.GetMultilingualString("905AB4B8-AC18-4127-8BAB-56FA8F999175", DeclarationFilterConstants.SupportingDocumentRef);

			var issueFilter = filters.AddDateFilter(DeclarationFilterConstants.SupportingDocumentIssueDate, GetSupDocIssueDateQuery);
			issueFilter.Category = FilterCategories.SupportingDocument;
			issueFilter.MultilingualDescription = ResString.GetMultilingualString("7C4BF4EB-8F3F-4320-B90E-01EA4ED80CF7", DeclarationFilterConstants.SupportingDocumentIssueDate);

			var expiryFilter = filters.AddDateFilter(DeclarationFilterConstants.SupportingDocumentExpiryDate, GetSupDocExpiryDateQuery);
			expiryFilter.Category = FilterCategories.SupportingDocument;
			expiryFilter.MultilingualDescription = ResString.GetMultilingualString("0DB2938A-B740-4B60-9D97-321BE6DB5D06", DeclarationFilterConstants.SupportingDocumentExpiryDate);
		}

		ZQuery GetSupDocTypeQuery(ZString value)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_Code, value);
			return GetDocumentMainQuery(csiQ);
		}

		ZQuery GetSupDocRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);
			return GetDocumentMainQuery(csiQ);
		}

		ZQuery GetSupDocIssueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			AddDateTimeRange(csiQ, comparisonOperator, JoinCondition.And, CusSupportingInfoSchema.CSI_DateOfIssue, fromDate, toDate);
			return GetDocumentMainQuery(csiQ);
		}

		ZQuery GetSupDocExpiryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			AddDateTimeRange(csiQ, comparisonOperator, JoinCondition.And, CusSupportingInfoSchema.CSI_DateOfExpiry, fromDate, toDate);
			return GetDocumentMainQuery(csiQ);
		}

		#endregion

		#region Previous Document Filters

		void AddPreviousDocumentFilters(ModuleFilterCollection filters)
		{
			var classFilter = filters.AddTextFilter(DeclarationFilterConstants.PreviousDocumentClass, GetPreDocClassQuery);
			classFilter.Category = FilterCategories.SupportingDocument;
			classFilter.MultilingualDescription = ResString.GetMultilingualString("B6A206B9-2705-43E5-B861-E47AB7076BD6", DeclarationFilterConstants.PreviousDocumentClass);

			var typeFilter = filters.AddTextFilter(DeclarationFilterConstants.PreviousDocumentType, GetPreDocTypeQuery);
			typeFilter.Category = FilterCategories.SupportingDocument;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("93A38991-7DBD-4DA6-ACCD-2D1602C0F426", DeclarationFilterConstants.PreviousDocumentType);

			var refFilter = filters.AddTextFilter(DeclarationFilterConstants.PreviousDocumentRef, GetPreDocRefQuery);
			refFilter.Category = FilterCategories.SupportingDocument;
			refFilter.MultilingualDescription = ResString.GetMultilingualString("5A94F899-3D3D-4222-856A-967B4625A50F", DeclarationFilterConstants.PreviousDocumentRef);

			var lineNoFilter = filters.AddTextFilterForExactComparison(DeclarationFilterConstants.PreviousDocumentLineNo, GetPreDocLineNoQuery);
			lineNoFilter.Category = FilterCategories.SupportingDocument;
			lineNoFilter.MultilingualDescription = ResString.GetMultilingualString("AE7CCA68-4D8F-48F6-8A08-039FFD36DE60", DeclarationFilterConstants.PreviousDocumentLineNo);
		}

		ZQuery GetPreDocClassQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_SubType, value);
			return GetDocumentMainQuery(csiQ);
		}

		ZQuery GetPreDocTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_Code, value);
			return GetDocumentMainQuery(csiQ);
		}

		ZQuery GetPreDocRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);
			return GetDocumentMainQuery(csiQ);
		}

		ZQuery GetPreDocLineNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var csiQ = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_LineNo, comparisonOperator, ZShort.ParseSafe(value, ZShort.Zero));
			return GetDocumentMainQuery(csiQ);
		}

		#endregion

		ZDBOnlySubQuery GetDocumentSubQuery(string documentType)
		{
			var csiQ = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_Type, documentType);
			return csiQ;
		}

		ZQuery GetDocumentMainQuery(ZDBOnlySubQuery supDocSubQuery)
		{
			var entryLineSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.CusEntryLine), CusEntryLineSchema.CL_ClusterKey);
			entryLineSubQuery.AddSubQuery(supDocSubQuery, JoinCondition.And);

			var invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey);
			invoiceLineSubQuery.AddSubQuery(supDocSubQuery, JoinCondition.And);

			var invoiceHeaderSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			invoiceHeaderSubQuery.AddSubQuery(supDocSubQuery, JoinCondition.And);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey);
			entryInstructionSubQuery.AddSubQuery(supDocSubQuery, JoinCondition.And);

			var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			entryHeaderSubQuery.AddSubQuery(supDocSubQuery, JoinCondition.And);

			var mainQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQuery.AddSubQuery(supDocSubQuery, JoinCondition.And);
			mainQuery.AddSubQuery(invoiceHeaderSubQuery, JoinCondition.Or);
			mainQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, invoiceLineSubQuery, JoinCondition.Or);
			mainQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryLineSubQuery, JoinCondition.Or);
			mainQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryInstructionSubQuery, JoinCondition.Or);
			mainQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryHeaderSubQuery, JoinCondition.Or);

			return mainQuery;
		}

		#endregion

		#region Guarantee Filters

		void AddGuaranteeFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.GuaranteeReference, GetGuaranteeReferenceQueryFilter);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("6697F257-7136-4A92-A4FB-0E45220CD1E1", DeclarationFilterConstants.NumberFilterTypes.GuaranteeReference);
		}

		ZQuery GetGuaranteeReferenceQueryFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var pwQ = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			pwQ.AddToFilter(CusBondDetailSchema.PW_BondNumber, comparisonOperator, value);

			var mainQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQuery.AddSubQuery(pwQ, JoinCondition.And);
			return mainQuery;
		}

		#endregion

		void AddVehicleVINFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.Vin, CusVehicleSchema.CVH_VehicleIdentificationNumber)
				.WithMaxLengthOf<ModuleTextFilter>(CusVehicleSchema.CVH_VehicleIdentificationNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("4A176C75-CCFE-48F2-B7DF-B959619976C0", DeclarationFilterConstants.NumberFilterTypes.Vin);
			filter.SubGroup = CusVehicleSubGroup;
		}

		ZQuery GetRequestedProcedureQuery(SQLComparisonOperator comparisonOperator, ZString filterText) => RequestedProcedureHelper.GetJobDeclarationFromRequestedProcedureQuery(RequestedProcedureHelper.GetRequestedProcedureFilterTextWithWildcards(filterText));

		ZQuery GetPreviousProcedureQuery(SQLComparisonOperator comparisonOperator, ZString filterText) => RequestedProcedureHelper.GetJobDeclarationFromRequestedProcedureQuery(RequestedProcedureHelper.GetPreviousProcedureFilterTextWithWildcards(filterText));

		ZQuery GetAdditionalProcedureQuery(SQLComparisonOperator comparisonOperator, ZString filterText) => RequestedProcedureHelper.GetJobDeclarationFromRequestedProcedureQuery(RequestedProcedureHelper.GetAdditionalProcedureFilterTextWithWildcards(filterText));

		#region Packing Filters

		void AddPackingFilters(ModuleFilterCollection filters)
		{
			var typeFilter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.PackingType, GetPackingTypeQueryFilter, Lookups.PackTypeList);
			typeFilter.Category = FilterCategories.NumbersAndReferences;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("F39C69D9-3009-4FB0-BA89-0C1AAA6ACD91", DeclarationFilterConstants.NumberFilterTypes.PackingType);

			var marksFilter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.PackingMarks, GetPackingMarksQueryFilter);
			marksFilter.Category = FilterCategories.NumbersAndReferences;
			marksFilter.MultilingualDescription = ResString.GetMultilingualString("05410319-3FC4-45D7-9E7A-8F2C62488766", DeclarationFilterConstants.NumberFilterTypes.PackingMarks);
		}

		ZQuery GetPackingTypeQueryFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var packQ = GetPackingSubQuery();
			packQ.AddToFilter(CusDecHouseContainerPackSchema.CW_PackType, comparisonOperator, value);
			return GetPackingMainQuery(packQ);
		}

		ZQuery GetPackingMarksQueryFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var packQ = GetPackingSubQuery();
			packQ.AddToFilter(CusDecHouseContainerPackSchema.CW_MarksAndNos, comparisonOperator, value);
			return GetPackingMainQuery(packQ);
		}

		ZDBOnlySubQuery GetPackingSubQuery() => new ZDBOnlySubQuery(typeof(BasePackage), CusDecHouseContainerPackSchema.CW_CR_HouseContainer);

		ZQuery GetPackingMainQuery(ZDBOnlySubQuery packSubQuery)
		{
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(BasePackingGroup), CusDecHouseContainerPivotSchema.CR_CU_HouseBill);
			pivotSubQuery.AddSubQuery(packSubQuery, JoinCondition.And);

			var billSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.Bill), CusDecHouseBillSchema.CU_JE);
			billSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			var mainQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQuery.AddSubQuery(billSubQuery, JoinCondition.And);
			return mainQuery;
		}

		#endregion

		#region Seal Filers

		void AddSealFilters(ModuleFilterCollection filters)
		{
			var sealNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.SealNumber, GetSealsFilterQuery);
			sealNumberFilter.MultilingualDescription = ResString.GetMultilingualString("6259D8CD-171C-4BEF-83D7-4D801019FB00", DeclarationFilterConstants.NumberFilterTypes.SealNumber);
		}

		ZQuery GetSealsFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var sealsContainerSubQuery = new ZDBOnlySubQuery(typeof(CusContainer), CusContainerSchema.CO_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			sealsContainerSubQuery.AddToFilter(CusContainerSchema.CO_Seal, comparisonOperator, value);
			sealsContainerSubQuery.AddToFilter(JoinCondition.Or, CusContainerSchema.CO_SecondSeal, comparisonOperator, value);

			var containerSubQuery = new ZDBOnlySubQuery(typeof(CusContainer), CusContainerSchema.CO_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			var additionalSealsContainerSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.CusSeal), CusSealSchema.BK_ParentID);
			additionalSealsContainerSubQuery.AddToFilter(CusSealSchema.BK_SealNumber, comparisonOperator, value);
			containerSubQuery.AddSubQuery(additionalSealsContainerSubQuery, JoinCondition.And);

			var equipmentSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.CusEquipment), CusEquipmentSchema.CEQ_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			var sealsEquipmentSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.CusSeal), CusSealSchema.BK_ParentID);
			sealsEquipmentSubQuery.AddToFilter(CusSealSchema.BK_SealNumber, comparisonOperator, value);
			equipmentSubQuery.AddSubQuery(sealsEquipmentSubQuery, JoinCondition.And);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			var sealsEntryInstructionSubQuery = new ZDBOnlySubQuery(typeof(SealNumber), CusCodeDataSchema.CY_ParentID);
			sealsEntryInstructionSubQuery.AddToFilter(CusCodeDataSchema.CY_Data, comparisonOperator, value);
			entryInstructionSubQuery.AddSubQuery(sealsEntryInstructionSubQuery, JoinCondition.And);

			result.AddSubQuery(sealsContainerSubQuery, JoinCondition.Or);
			result.AddSubQuery(containerSubQuery, JoinCondition.Or);
			result.AddSubQuery(equipmentSubQuery, JoinCondition.Or);
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		protected override bool ShowDeclarantFilter => true;

		protected ModuleFilterSubGroup CusVehicleSubGroup => cusVehicleSubGroup ??= new CusVehicleFilterSubGroup();
		ModuleFilterSubGroup cusVehicleSubGroup;

		class CusVehicleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				var invoiceHeaderSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				var invoicelineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);

				var vehicleQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusVehicle), CusVehicleSchema.CVH_ParentID);
				vehicleQuery.AddToFilter(filter);
				invoicelineSubQuery.AddSubQuery(vehicleQuery, JoinCondition.And);
				invoiceHeaderSubQuery.AddSubQuery(invoicelineSubQuery, JoinCondition.And);
				query.AddSubQuery(invoiceHeaderSubQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
