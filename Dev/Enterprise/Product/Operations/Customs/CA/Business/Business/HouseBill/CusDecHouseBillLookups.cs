

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class BillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public BillLookups(Bill houseBill)
			: base(houseBill)
		{
		}

		public Bill HouseBill
		{
			get { return Parent; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		public override CodeDescriptionPairList NoOfPacksPackType_List
		{
			get
			{
				var declaration = Parent.Declaration;
				if (declaration != null && declaration.IsIID)
				{
					return Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);
				}
				else
				{
					return base.NoOfPacksPackType_List;
				}
			}
		}
	}
}
