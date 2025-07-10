using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconDeclarationLookups : Customs.Business.CusReconDeclarationLookups
	{
		public CusReconDeclarationLookups(CusReconDeclaration parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CustomsStatusList
		{
			get
			{
				return Factory.GetCachedValue("DE.CusReconDeclarationLookups.CustomsStatusList", () =>
				{
					var statusCodeList = new ZString[] { UniversalReferenceConstants.EntryStatus.RC2, UniversalReferenceConstants.EntryStatus.REJ, UniversalReferenceConstants.EntryStatus.TX1,
						UniversalReferenceConstants.EntryStatus.TX2, UniversalReferenceConstants.EntryStatus.TX3, UniversalReferenceConstants.EntryStatus.TX4,
						UniversalReferenceConstants.EntryStatus.TX5, UniversalReferenceConstants.EntryStatus.TX6, UniversalReferenceConstants.EntryStatus.TRA,
						UniversalReferenceConstants.EntryStatus.ERR, UniversalReferenceConstants.EntryStatus.TXR };

					var result = new CodeDescriptionPairList();
					var customsStatusList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
					customsStatusList.Load();
					foreach (ZZRefCusCodeListCombined codeList in customsStatusList)
					{
						if (statusCodeList.Contains(codeList.ZZD_Code))
						{
							result.AddPairIfNotExist(codeList.ZZD_Code, codeList.ZZD_Description);
						}
					}
					result.Sort();
					return result;
				});
			}
		}

		public override CustomsOfficeCodeCollection CustomsOfficeList => CustomsOfficeCodeCollection.GetCachedCollection(
			Factory
			, Core.Constants.CountryCodes.Germany
			, ZDateTime.Today
			, new Dictionary<ZString, ZString[]> { { RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, new ZString[] { bool.TrueString } } });

		public override CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<MonthlyClosingDeclarationTypeList>();

		public override CodeDescriptionPairList DeclarantTypeList => Factory.GetCachedValue<RepresentationTypeList>();

		public override CusAuthorisationHeaderCollection AuthorizationList
		{
			get
			{
				var parent = Parent;
				var declarationType = parent.CRD_DeclarationType;
				var declarantType = parent.CRD_DeclarantType;
				var declarant = parent.DeclarantAddress?.Header;
				var representative = parent.RepresentativeAddress?.Header;
				var buyingAgent = parent.BuyingAgentAddress?.Header;

				List<ZString> types = new List<ZString>();
				List<ZGuid> permitHolders = new List<ZGuid>();
				var ruleCode = ZString.Empty;
				var ruleCodeValue = ZString.Empty;

				if (declarationType == ImportDeclarationTypeList.Codes.VZA)
				{
					types.Add(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);

					GetOtherCollectionParameters();
				}
				else if (declarationType == ImportDeclarationTypeList.Codes.AZ)
				{
					types.Add(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);

					GetOtherCollectionParameters();
				}
				else if (declarationType == MonthlyClosingDeclarationTypeList.Codes.VAV || declarationType == MonthlyClosingDeclarationTypeList.Codes.AAV)
				{
					types.Add(CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
					AddPermitHolders(declarant, null, null);
				}
				else if (declarationType == MonthlyClosingDeclarationTypeList.Codes.VZL || declarationType == MonthlyClosingDeclarationTypeList.Codes.AZL)
				{
					types.Add(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
					types.Add(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);

					if (declarantType == RepresentationTypeList.Codes._1Self || declarantType == RepresentationTypeList.Codes._3Indirect)
					{
						AddPermitHolders(declarant, null, null);
					}
					else if (declarantType == RepresentationTypeList.Codes._2Direct)
					{
						AddPermitHolders(declarant, representative, null);
					}
				}

				return new CusAuthorisationHeaderCollection(Factory, types.ToArray(), permitHolders.ToArray(), parent.CRD_PeriodFrom, ruleCode, ruleCodeValue);

				void GetOtherCollectionParameters()
				{
					ruleCode = CusAuthorisationRuleTypeList.Codes.Usage;
					ruleCodeValue = CusAuthorisationUsageRuleList.Codes.FreeCirculation;

					if (declarantType == RepresentationTypeList.Codes._1Self)
					{
						AddPermitHolders(declarant, null, null);
					}
					else if (declarantType == RepresentationTypeList.Codes._2Direct)
					{
						AddPermitHolders(declarant, representative, null);
					}
					else if (declarantType == RepresentationTypeList.Codes._3Indirect)
					{
						AddPermitHolders(declarant, null, buyingAgent);
					}
				}

				void AddPermitHolders(OrgHeader declarantHeader, OrgHeader representativeHeader, OrgHeader buyingAgentHeader)
				{
					if (declarantHeader != null)
					{
						permitHolders.Add(declarantHeader.PK);
					}
					if (representativeHeader != null)
					{
						permitHolders.Add(representativeHeader.PK);
					}
					if (buyingAgentHeader != null)
					{
						permitHolders.Add(buyingAgentHeader.PK);
					}
				}
			}
		}
	}
}
