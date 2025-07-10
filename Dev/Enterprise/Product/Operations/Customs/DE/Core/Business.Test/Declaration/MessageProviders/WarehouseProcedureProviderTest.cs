using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class WarehouseProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<WarehouseProcedureProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", WarehouseProcedureProvider.NewOrNull(null));
		}

		public void TestAccessViaAtlasFlag()
		{
			AssertEquals("1", dataProvider.AccessViaAtlasFlag);
		}

		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				warehouseProcedure.Status = false;
				AssertEquals("Status is false", string.Empty, dataProvider.MRN);
				warehouseProcedure.Status = true;
				AssertEquals("Status is true", string.Empty, dataProvider.MRN);
				warehouseProcedure.CSI_ReferenceNumber = "123456789012345678";
				AssertEquals("Status is true and length of CSI_ReferenceNumber is 18", "123456789012345678", dataProvider.MRN);
			});
		}

		public void TestRegistrationNumber()
		{
			AssertEquals("REFNUMBER", dataProvider.RegistrationNumber);
		}

		public void TestReferencedSequenceNumber()
		{
			AssertEquals(3, dataProvider.ReferencedSequenceNumber);
		}

		public void TestUsualProcessingFlag()
		{
			AssertEquals("1", dataProvider.UsualProcessingFlag);
		}

		public void TestComplement()
		{
			AssertEquals("Complement", dataProvider.Complement);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("980012", dataProvider.HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("34", dataProvider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			AssertEquals("00", dataProvider.TaricCode);
		}

		public void TestNationalAdditionalCode()
		{
			AssertEquals("1", dataProvider.NationalAdditionalCode);
		}

		public void TestDebitAmount()
		{
			var debitAmount = dataProvider.DebitAmount;
			CombineAssertions(() =>
			{
				AssertEquals(72345.56m, debitAmount.Quantity);
				AssertEquals("KGM", debitAmount.MeasurementUnit);
			});
		}

		public void TestCommercialAmount()
		{
			var commercialAmount = dataProvider.CommercialAmount;
			CombineAssertions(() =>
			{
				AssertEquals(12345.56m, commercialAmount.Quantity);
				AssertEquals("GRM", commercialAmount.MeasurementUnit);
			});
		}

		public void TestCommercialAmount_null()
		{
			warehouseProcedure.UsualProcessingFlag = false;
			AssertEquals(null, dataProvider.CommercialAmount);
		}

		public void TestCommodityCode()
		{
			AssertEquals("98001234001", dataProvider.CommodityCode);
		}

		protected override IEnumerable<Expression<Func<WarehouseProcedureProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.CommercialAmount;
			yield return x => x.DebitAmount;
		}

		protected override void SetUp()
		{
			base.SetUp();
			warehouseProcedure = Factory.New<PreviousDocument>();
			warehouseProcedure.Status = true;
			warehouseProcedure.CSI_ReferenceNumber = "REFNUMBER";
			warehouseProcedure.CSI_LineNo = 3;
			warehouseProcedure.UsualProcessingFlag = true;
			warehouseProcedure.CSI_Description = "Complement";
			warehouseProcedure.CSI_Tariff = "98001234001";
			warehouseProcedure.CSI_Quantity = 12345.56;
			warehouseProcedure.CSI_UnitOfQuantity = "GRM";
			warehouseProcedure.CSI_Quantity2 = 72345.56;
			warehouseProcedure.CSI_UnitOfQuantity2 = "KGM";
			dataProvider = WarehouseProcedureProvider.NewOrNull(warehouseProcedure);
		}
		PreviousDocument warehouseProcedure;
		IWarehouseProcedure dataProvider;

		protected override WarehouseProcedureProvider GetProvider() => (WarehouseProcedureProvider)dataProvider;
	}
}
