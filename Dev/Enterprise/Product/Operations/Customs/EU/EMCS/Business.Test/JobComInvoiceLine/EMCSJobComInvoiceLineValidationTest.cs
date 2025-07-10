using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobComInvoiceLineValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckJI_Procedure()
		{
			line.Validation.ValidateJI_Procedure();
			AssertNoNotifications(line.JI_ProcedureInfo);
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			line.JI_ZZF_NKTaxType = "XXX";
			AssertNoMessageErrors("JI_ZZF_NKTaxType is not used for EMCS.", line.JI_InvoiceQuantityInfo);
		}

		public void TestCheckJI_NetWeight()
		{
			line.Validation.ValidateJI_NetWeight();
			AssertHasMessageErrorContaining(line.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeZero);

			line.JI_NetWeight = -1m;
			AssertHasMessageErrorContaining(line.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			line.JI_NetWeight = 1m;
			AssertNoMessageErrorContaining(line.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(line.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_Weight()
		{
			line.Validation.ValidateJI_Weight();
			AssertHasMessageErrorContaining(line.JI_WeightInfo, MandatoryValidation.ValueCannotBeZero);

			line.JI_Weight = -1m;
			AssertHasMessageErrorContaining(line.JI_WeightInfo, MandatoryValidation.ValueCannotBeNegative);

			line.JI_Weight = 1m;
			AssertNoMessageErrorContaining(line.JI_WeightInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(line.JI_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			line.JI_CustomsUnitQty = EMCSCustomsQuantityTypeList.Codes.FifteenLitre;
			line.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageErrorContaining(line.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			line.JI_CustomsQuantity = -1M;
			AssertHasMessageErrorContaining(line.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			line.JI_CustomsQuantity = 12345678901234567889.111M;
			AssertEquals(1, line.JI_CustomsQuantityInfo.GetErrors().Count(x => x.Message.Contains("is too large")));
			AssertHasErrorContaining(line.JI_CustomsQuantityInfo, "the maximum value allowed for Customs Qty is 999,999,999,999.999.");

			line.JI_CustomsQuantity = 999999999999.999M;
			AssertNoErrors(line.JI_CustomsQuantityInfo);
			AssertNoMessageErrorContaining(line.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(line.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "Excise Movement Control System (EMCS) CN-Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "29024100", "o-Xylol", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			CombineAssertions(() =>
			{
				line.JI_Tariff = "12345678";
				AssertHasMessageErrorContaining("Invalid Code", line.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());

				line.JI_Tariff = "29024100";
				AssertNoMessageErrorContaining("Valid Code", line.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			line = invoice.JobComInvoiceLines.AddNew();
		}
		EMCSJobComInvoiceLine line;
		EMCSJobDeclaration declaration;
	}
}
