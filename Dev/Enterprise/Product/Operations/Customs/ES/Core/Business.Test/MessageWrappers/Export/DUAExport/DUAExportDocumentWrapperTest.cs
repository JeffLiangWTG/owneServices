using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DUAExportDocumentWrapperTest : WrapperHelperTest<DUAExportDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new DUAExportDocumentWrapper(null));
		}

		public void TestQuantity()
		{
			doc.CSI_Quantity = SupportingDocumentData.Quantity;
			AssertEquals("Expected filled Quantity", SupportingDocumentData.Quantity, wrapper.Quantity);
		}

		public void TestQtyUnit()
		{
			doc.CSI_ParentTableCode = "JE";
			doc.CSI_DataModel = "ES";
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				doc.CSI_UnitOfQuantity = SupportingDocumentData.QtyUnitCW1;
				AssertEquals("Expected filled QtyUnit with mapped value", SupportingDocumentData.QtyUnitCustoms, wrapper.QtyUnit);

				doc.CSI_UnitOfQuantity = SupportingDocumentData.QtyUnitNotMapped;
				AssertEquals("Expected filled QtyUnit with original value because the value is not mapped", SupportingDocumentData.QtyUnitNotMapped, wrapper.QtyUnit);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			doc = Factory.New<SupportingDocument>();
			wrapper = new DUAExportDocumentWrapper(doc);
		}

		SupportingDocument doc;
		DUAExportDocumentWrapper wrapper;

		protected override DUAExportDocumentWrapper GetProvider() => wrapper;
	}
}
