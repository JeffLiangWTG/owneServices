using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ProductSupportingInfo))]
	public class ProductSupportingInfoTest : CusSupportingInfoTest<ProductSupportingInfo>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("PRD", product.CSI_Type);
		}

		public void TestCSI_Tariff()
		{
			product.CSI_Tariff = "76356398";
			AssertEquals("76356398", product.CSI_Tariff);

			product.CSI_Tariff = "7635.44.55";
			AssertEquals("76354455", product.CSI_Tariff);
		}

		public void TestFormattedTariff()
		{
			product.FormattedTariff = "7635.44.55";
			AssertEquals("7635.44.55", product.FormattedTariff);
			AssertEquals("76354455", product.CSI_Tariff);
		}

		public void TestITariffFormatProvider()
		{
			AssertType<EU.Business.TariffFormatterEleven>(((ITariffFormatProvider)product).TariffFormatter);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Product", product.HumanReadableName);
		}

		protected override IEnumerable<ProductSupportingInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
			product = entryInstruction.Products.AddNew();

			yield return product;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			product = entryInstruction.Products.AddNew();
		}

		ProductSupportingInfo product;
	}
}
