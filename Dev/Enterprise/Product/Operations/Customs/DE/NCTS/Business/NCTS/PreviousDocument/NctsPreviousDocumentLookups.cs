using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPreviousDocumentLookups : EU.NCTS.Business.NctsPreviousDocumentPhase5Lookups
	{
		public NctsPreviousDocumentLookups(NctsPreviousDocument parent)
			: base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				var result = base.SubTypeList;
				if (Parent.IsProcedureN337)
				{
					var officeCode = Parent.Parent?.Header?.MovementHeader?.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfDeparture)?.CY_Data ?? ZString.Empty;
					result = Factory.GetCachedSubTypeList_ATNEU(officeCode);
				}
				return result;
			}
		}

		public override CodeDescriptionPairList UnitOfQuantityList => UnitList;

		public override CodeDescriptionPairList UnitOfQuantity2List => UnitList;

		CodeDescriptionPairList UnitList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today, includeParentDataGrouping: false);
	}
}
