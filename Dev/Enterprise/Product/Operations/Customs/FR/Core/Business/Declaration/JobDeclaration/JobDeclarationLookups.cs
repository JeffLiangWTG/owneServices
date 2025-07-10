using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public new JobDeclaration Declaration => Parent;

		public CodeDescriptionPairList CustomsLanguageList => Factory.GetCachedValue<FRCustomsLanguageList>();

		public override ICodeDescriptionPairList DeclarantTypeList => Factory.GetCachedValue<RepresentationTypeList>();

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		#region Payments

		public virtual CodeDescriptionPairList DefermentAccountNumberList
		{
			get
			{
				var cacheKey = "FR.JobDeclarationLookups.DefermentAccountNumberList" + GetActualClientAndDeclarantCacheKey();

				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();

					if (Parent.IsImport)
					{
						AddDefermentAccountNumberListIfNotEmptyOrNull(result, Parent.Importer, nameof(Parent.Importer));
					}
					else if (Parent.IsExport)
					{
						AddDefermentAccountNumberListIfNotEmptyOrNull(result, Parent.Supplier, nameof(Parent.Supplier));
					}

					AddDefermentAccountNumberListIfNotEmptyOrNull(result, Parent.Declarant?.Header, nameof(Parent.Declarant));

					return result;
				});
			}
		}

		void AddDefermentAccountNumberListIfNotEmptyOrNull(CodeDescriptionPairList defermentAccountNumberList, OrgHeader defermentSource, ZString sourceName)
		{
			var dan = defermentSource?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, Core.Constants.CountryCodes.France) ?? ZString.Empty;
			if (!string.IsNullOrEmpty(dan))
			{
				var pair = new CodeDescriptionPair(dan.ToString(), sourceName + ", " + defermentSource.OH_FullName);
				if (sourceName == nameof(Parent.Importer))
				{
					defermentAccountNumberList.Insert(0, pair);
				}
				else
				{
					defermentAccountNumberList.Add(pair);
				}
			}
		}

		public CodeDescriptionPairList ChargePaymentOrDestinationIDs => UniversalReferenceDataHelper.PaymentDestinationList(Factory, Declaration);

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<MethodOfPaymentList>();

		#endregion

		#region Goods Locations

		public CusAuthorisationHeaderCollection GoodsLocations
		{
			get
			{
				ZString type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation;
				ZString country = Parent.CountryCode;
				var permitHolder = Parent.DeltaAccountOrgHeader?.PK ?? ZGuid.Empty;
				var rulesDetail = new ZString(CusAuthorisationRuleTypeList.Codes.SUB);

				var numbers = new CusAuthorisationHeaderCollectionFiltered(Factory, type, permitHolder);
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.Country, "Property", country, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", type, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", permitHolder, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.RuleDetails, "Property1", rulesDetail, true));
				return numbers;
			}
		}

		public CodeDescriptionPairList SubGoodsLocations => Factory.GetCachedValue("FR.JobDeclarationLookups.SubGoodsLocations" + GetActualClientAndDeclarantCacheKey() + Parent.JE_LocationOfGoods, GetSubGoodsLocations);

		CodeDescriptionPairList GetSubGoodsLocations()
		{
			var list = new CodeDescriptionPairList();
			if (Parent.JE_LocationOfGoodsRelatedCusAuthorisation != null)
			{
				Parent.JE_LocationOfGoodsRelatedCusAuthorisation
					.GetAuthorisationRuleWithCode(CusAuthorisationRuleTypeList.Codes.SUB)
					.ForEach(x => list.AddPair(x.CPR_ValueFrom, x.CPR_Description));
			}
			return list;
		}

		public IEnumerable<CusAuthorisationHeader> AuthorizedLocations => Declaration.DeltaAccountOrgHeader.GetCusAuthorisationHeadersWithType(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation, Parent.CountryCode);

		#endregion

		#region Delta Account Mode List

		public CodeDescriptionPairList DeltaModeList
		{
			get
			{
				var cacheKey = "FR.JobDeclarationLookups.DeltaModeList" + GetActualClientAndDeclarantCacheKey() + Parent.JE_ApplicationCode;
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = GetDeltaModeList();
					return result;
				});
			}
		}

		protected virtual CodeDescriptionPairList GetDeltaModeList() => new DeclarationApplicationCodeList();

		#endregion

		public override CodeDescriptionPairList ApplicationCodeList => GetApplicationCodeList(Parent.IsDeltaIEEnable);

		public CodeDescriptionPairList GetApplicationCodeList(bool isDeltaIE)
		{
			var registry = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.Value.SubmissionType;
			var cacheKey = "FR.JobDeclarationLookups.GetApplicationCodeList," + isDeltaIE + registry;

			return Factory.GetCachedValue(cacheKey, () =>
			{
				var result = new DeclarationApplicationCodeList();
				if (registry == DeclarationApplicationCodeList.Codes.Interface)
				{
					result.RemoveCode(DeclarationApplicationCodeList.Codes.DeltaIE);
					result.RemoveCode(DeclarationApplicationCodeList.Codes.DeltaG);
				}
				else if (registry.IsEmpty || registry == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin)
				{
					result.RemoveCode(DeclarationApplicationCodeList.Codes.Interface);
					if (!isDeltaIE)
					{
						result.RemoveCode(DeclarationApplicationCodeList.Codes.DeltaIE);
					}
				}
				else
				{
					if (!isDeltaIE)
					{
						result.RemoveCode(DeclarationApplicationCodeList.Codes.DeltaIE);
					}
				}

				return result;
			});
		}

		#region VATProcedure

		public List<ZString> VatProcedureSpecialMentionList => Factory.GetCachedValue("VatProcedureSpecialMention_" + DataGroupingForVATCANA, delegate
		{
			var canaList = ZZRefCusCodeListCombined.Loader.Load(Factory, DataGroupingForVATCANA, UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, ZDateTime.Today);

			var list = new List<ZString>();

			if (canaList != null)
			{
				foreach (var item in canaList)
				{
					var attributes = item.Attributes;
					if (attributes != null)
					{
						var cusCodeListAttribute = attributes.Cast<ZZRefCusCodeListAttributeCombined>().FirstOrDefault(x => string.Compare(x.ZZE_ZXE_NKName, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention, System.StringComparison.OrdinalIgnoreCase) == 0);
						if (cusCodeListAttribute != null && !cusCodeListAttribute.ZZE_Value.IsEmpty && !list.Contains(cusCodeListAttribute.ZZE_Value))
						{
							list.Add(cusCodeListAttribute.ZZE_Value);
						}
					}
				}
			}
			return list;
		});
		protected ZString GetActualClientAndDeclarantCacheKey()
		{
			return System.FormattableString.Invariant($"{Declaration.JE_MessageType},{Declaration.JE_OH_Importer},{Declaration.JE_OH_Supplier},{Declaration.JE_OA_DeclarantAddress}");
		}

		#endregion

		#region Guarantees

		public ZString GetGuaranteeCacheKey()
		{
			var parent = Parent;
			var importerAddress = parent.ImporterDocumentaryAddressCode;
			var supplierAddress = parent.SupplierDocumentaryAddressCode;
			var declarantAddress = parent.DeclarantOrgAddress?.AddressCode ?? ZString.Empty;
			var branchAddress = parent.Branch?.OrgProxy?.MainAddress?.AddressCode ?? ZString.Empty;
			var companyAddress = parent.Company?.OrgProxy?.MainAddress?.AddressCode ?? ZString.Empty;
			return System.FormattableString.Invariant($"{parent.JE_DeltaMode},{parent.JE_MessageType},{importerAddress},{supplierAddress},{declarantAddress},{branchAddress},{companyAddress}");
		}

		public CusGuaranteeHeader[] CODCustomsGuarantees => GetCachedCustomsGuarantees("FR.JobDeclarationLookups.CustomsGuarantees,", false, GuaranteeTypeList.Codes.COD);

		public CusGuaranteeHeader[] CODCustomsGuaranteesIncludingExpired => GetCachedCustomsGuarantees("FR.JobDeclarationLookups.CustomsGuaranteesIncludingExpired,", true, GuaranteeTypeList.Codes.COD);

		public CusGuaranteeHeader[] DefermentCustomsGuarantees => GetCachedCustomsGuarantees("FR.JobDeclarationLookups.DefermentCustomsGuarantees,", false, GuaranteeTypeList.Codes.DEF);

		protected CusGuaranteeHeader[] GetCachedCustomsGuarantees(string cacheKeyPrefix, bool ignoreEndDate, string guaranteeType)
		{
			var cacheKey = cacheKeyPrefix + GetGuaranteeCacheKey();

			return Factory.GetCachedValue(cacheKey, () =>
			{
				return Factory.GetCustomsGuarantees(Parent, ignoreEndDate, guaranteeType);
			});
		}

		public virtual CodeDescriptionPairList CustomsGuaranteeNumberList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var parent = Parent;
				var deltaMode = parent.DeltaMode;

				foreach (var guarantee in CODCustomsGuarantees)
				{
					var sourceType = ZString.Empty;
					var sourceName = ZString.Empty;

					if (guarantee.CPH_OH_PermitHolder == parent.ImporterDocumentaryAddress.OrganisationPK && guarantee.AddressCodes.Contains(parent.ImporterDocumentaryAddressCode))
					{
						sourceType = nameof(parent.Importer);
						sourceName = parent.Importer.OH_FullName;
					}
					else if (guarantee.CPH_OH_PermitHolder == parent.SupplierDocumentaryAddress.OrganisationPK && guarantee.AddressCodes.Contains(parent.SupplierDocumentaryAddressCode))
					{
						sourceType = nameof(parent.Supplier);
						sourceName = parent.Supplier.OH_FullName;
					}
					else if (guarantee.CPH_OH_PermitHolder == parent.Branch.GB_OH_OrgProxy)
					{
						sourceType = nameof(parent.Branch);
						sourceName = parent.Branch.OrgProxy.OH_FullName;
					}
					else if (guarantee.CPH_OH_PermitHolder == parent.Company.GC_OH_OrgProxy)
					{
						sourceType = nameof(parent.Company);
						sourceName = parent.Company.OrgProxy.OH_FullName;
					}
					else
					{
						sourceType = nameof(parent.Declarant);
						sourceName = parent.Declarant.Header.OH_FullName;
					}

					var customsGuaranteeFriendlyName = guarantee.GetCustomsGuaranteeFriendlyName(deltaMode);

					if (!customsGuaranteeFriendlyName.IsEmpty && !result.ContainsCode(guarantee.CPH_Number))
					{
						var pair = new CodeDescriptionPair(guarantee.CPH_Number.ToString(), $"{customsGuaranteeFriendlyName}, {sourceType}, {sourceName}");
						if (sourceType == nameof(parent.Importer))
						{
							result.Insert(0, pair);
						}
						else
						{
							result.Add(pair);
						}
					}
				}

				return result;
			}
		}
		#endregion

		public override OrgHeaderCollection Buyers => new OrgHeaderCollection(Factory);

		public CodeDescriptionPairList ExportExitTypeList => Factory.GetCachedValue<ExportExitTypeList>();

		public CodeDescriptionPairList AirRouteTypeList => Factory.GetCachedValue<AirRouteTypeList>();

		public CodeDescriptionPairList RegionOrTerritoryOfDestinationList => Factory.GetCachedValue<FRDomesticOverseasTerritories>();

		public virtual ZString DataGroupingForVATCANA { get; }
	}
}

