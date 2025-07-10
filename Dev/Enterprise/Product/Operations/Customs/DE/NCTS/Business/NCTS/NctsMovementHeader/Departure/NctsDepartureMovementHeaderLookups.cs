using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using OfficeCodes_NCTS = Enterprise.Customs.EU.Business.OfficeCodes_NCTS;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDepartureMovementHeaderLookups : NctsDepartureMovementHeaderPhase5Lookups
	{
		public NctsDepartureMovementHeaderLookups(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList LocationOfGoodsCodeList
		{
			get
			{
				var header = Parent.Header;
				var principalOrgPk = header.Principal.Organisation?.PK ?? ZGuid.Empty;
				var departureCustomsOfficeCode = Parent.CustomsOffices.GetFirstElementHaving(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture)?.CY_Data ?? ZString.Empty;
				var rules = CusAuthorizationHelper.GetCachedAuthorizationRules(Factory
					, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit
					, principalOrgPk
					, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location
					, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice
					, departureCustomsOfficeCode
					, ZDate.Today);
				return rules;
			}
		}

		public override CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<NctsDeclarationTypeList>();

		protected override CodeDescriptionPairList ModeOfTransportListCore => Factory.GetCachedValue("DE.NCTS.ModeOfTransportList", () =>
		{
			var list = new CodeDescriptionPairList(base.ModeOfTransportListCore);
			list.RemoveCode(EU.Business.ModeOfTransportList.Codes._7_FixedTransportInstallations);
			return list;
		});

		protected override CodeDescriptionPairList BorderModeOfTransportListCore => Factory.GetCachedValue("DE.NCTS.BorderModeOfTransportList", () =>
		{
			var list = new CodeDescriptionPairList(base.BorderModeOfTransportListCore);
			list.RemoveCode(EU.Business.ModeOfTransportList.Codes._7_FixedTransportInstallations);
			return list;
		});

		protected override bool IsOfficeValidForOfficeCodeList(NctsEuOfficeCode office) => validOfficeCodesForOfficeCodeList.Contains(office.CY_Code);

		static readonly ImmutableHashSet<string> validOfficeCodesForOfficeCodeList = new HashSet<string>
		{
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit
		}.ToImmutableHashSet();

		protected override CodeDescriptionPairList TransportAtBorderTypeOfIdListCore
		{
			get
			{
				if (Parent is NctsDepartureMovementHeader movementHeader && movementHeader.IsInPhase5TransitionPeriod)
				{
					return Factory.GetCachedValue($"DETransportAtBorderTypeOfIdList_{movementHeader.BM_ExportTransportMode}",
						() =>
						{
							var listCore = base.TransportAtBorderTypeOfIdListCore;
							if (!listCore.ContainsCode(NctsTransportTypeOfIdList.Codes._99))
							{
								return listCore;
							}

							var list = new CodeDescriptionPairList();
							list.AddRange(listCore);
							list.RemoveCode(NctsTransportTypeOfIdList.Codes._99);

							return list;
						});
				}
				return base.TransportAtBorderTypeOfIdListCore;
			}
		}
	}
}
