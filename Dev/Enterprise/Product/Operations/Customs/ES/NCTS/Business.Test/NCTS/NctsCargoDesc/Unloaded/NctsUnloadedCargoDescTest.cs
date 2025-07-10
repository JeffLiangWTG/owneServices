using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUnloadedCargoDesc))]
	sealed class NctsUnloadedCargoDescTest : NctsCommonCargoDescAbstractTest<NctsHeader>
	{
		public new void TestCorrectTypeDecideForLoad()
		{
			var goodsItem = GetNewBusinessObject();
			Factory.Save();

			AssertType("GoodsItem", goodsItem.GetType(), new BusinessObjectFactory().Load<NctsUnloadedCargoDesc>(goodsItem.PK));
		}

		public void TestValidation()
		{
			AssertType<NctsUnloadedCargoDescValidation>(UnloadedCargoDesc.Validation);
		}

		public void TestNctsAdditionalInfoCollection()
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(UnloadedCargoDesc.AdditionalInfos);
		}

		protected override BusinessObject GetNewBusinessObject() => UnloadedCargoDesc;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => UnloadedCargoDesc;

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => UnloadedCargoDesc;

		NctsUnloadedCargoDesc GetNewNctsUnloadedCargoDesc(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

			return arrivalCargoDesc.UnloadedGoodsItem;
		}

		NctsUnloadedCargoDesc UnloadedCargoDesc => unloadedCargoDesc ?? (unloadedCargoDesc = GetNewNctsUnloadedCargoDesc(Factory));
		NctsUnloadedCargoDesc unloadedCargoDesc;

		protected override ZString CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
