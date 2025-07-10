using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteeLookups : EU.Business.Declaration.CommonGuaranteeLookups
	{
		public NctsGuaranteeLookups(NctsGuarantee parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList BondTypeListCore
		{
			get
			{
				var header = Parent.NctsHeader;

				if (header != null && header.IsPhase5Departure)
				{
					return RefCusCodeListTypes.GetCachedList(
						Factory,
						header.DefaultDataGroupingCode,
						EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL251,
						ZDateTime.Today);
				}

				return base.BondTypeListCore;
			}
		}

		protected new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

		protected override ZGuid? PrimaryGuaranteeHolderAddress => Parent.NctsHeader?.Principal?.OrganisationPK;

		public override CusGuaranteeHeaderCollection ReferenceNumbers
		{
			get
			{
				var guarantees = base.ReferenceNumbers;
				if (Parent.NctsHeader is NctsHeader nctsHeader)
				{
					if (nctsHeader.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
					{
						guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeType, "Property", (ZString)EUGuaranteeTypeList.Codes.TST));
						guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeRule, "Property1", (ZString)EU.Business.PermitRuleCodeList.Codes.TSP));
						var valueFrom = arrivalMovementHeader.GoodsLocation.AdditionalIdentifier;
						guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeRule, "Property2", valueFrom));
					}
					else if (nctsHeader.IsPhase5Departure)
					{
						if (guarantees.FilterBusinessObjectDefaults.ContainsDefaultFor($"{CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeType}:Property"))
						{
							guarantees.FilterBusinessObjectDefaults.Remove($"{CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeType}:Property");
						}

						guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeSubType, "Property", Parent.PW_BondType));
					}
				}

				return guarantees;
			}
		}

		protected override CusGuaranteeHeaderCollection GetGuaranteeHeaderCollection()
		{
			CusGuaranteeHeaderCollection result = null;
			var header = Parent.NctsHeader;
			if (header != null)
			{
				var countryCodes = header.GetC0009CountryCodes();
				if (countryCodes.Length > 0) 
				{
					if (GuaranteeReferencesTypeFilter.Count > 0)
					{
						result = new CusGuaranteeHeaderCollectionFiltered(Factory, countryCodes, GuaranteeTypeFilter, GuaranteeReferencesTypeFilter);
					}
					else
					{
						result = new CusGuaranteeHeaderCollectionFiltered(Factory, countryCodes, GuaranteeTypeFilter);
					}
				}
			}
			return result ?? new CusGuaranteeHeaderCollection(Factory, ZQuery.NoResultQuery);
		}

		public CodeDescriptionPairList LiabilityApplicablePercentageCodeList => Factory.GetCachedValue<LiabilityApplicablePercentageCodeList>();

		protected override IReadOnlyList<ZString> GuaranteeTypeFilter => Parent.NctsHeader.IsPhase5Arrival
			? new ZString[] { EUGuaranteeTypeList.Codes.TST }
			: (Parent.NctsHeader.IsPhase5 ? new ZString[] { EUGuaranteeTypeList.Codes.COD, EUGuaranteeTypeList.Codes.TRA } : Array.Empty<ZString>());

		protected override IReadOnlyList<ZString> GuaranteeReferencesTypeFilter => Parent.NctsHeader.IsPhase5Departure ? new ZString[] { AdditionalCustomsReferenceTypeList.Codes.D1, AdditionalCustomsReferenceTypeList.Codes.D2, AdditionalCustomsReferenceTypeList.Codes.D3 } : Array.Empty<ZString>();
	}
}
