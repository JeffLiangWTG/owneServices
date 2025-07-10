using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ProductSupportingInfoCollection))]
	class ProductSupportingInfoCollectionTest : CusSupportingInfoCollectionTest<ProductSupportingInfo>
	{
		public void TestMaxCount()
		{
			var collection = GetCusSupportingInfoCollection();
			AssertEquals("Max Count should be 999", 999, collection.MaxCount);
		}

		protected override CusSupportingInfoCollection<ProductSupportingInfo> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			return entryInstruction.Products;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (ProductSupportingInfo)base.GetNewElementToAddToTheCollection();
			result.CSI_Type = "PRD";

			return result;
		}
	}
}
