using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class PreviousDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<IPreviousDocument>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", PreviousDocumentProvider.NewOrNull(null));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestFullType() => AssertEquals("1234567", Provider.FullType);

		public void TestType() => AssertEquals("1234", Provider.Type);

		public void TestQualifier() => AssertEquals("567", Provider.Qualifier);

		public void TestQualifier_Null()
		{
			previousDocument.CSI_Code = "1234";
			AssertEquals(null, Provider.Qualifier);
		}

		public void TestReferenceNumber_Import()
		{
			AssertEquals("REFERENCE", Provider.ReferenceNumber);
		}

		public void TestReferenceNumber_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped, no attribute", null, Provider.ReferenceNumber);
		}

		public void TestReferenceNumber_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference);
			AssertEquals("REFERENCE", Provider.ReferenceNumber);
		}

		public void TestGoodsItemNumber_Import()
		{
			AssertEquals(1, Provider.GoodsItemNumber);
		}

		public void TestGoodsItemNumber_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped, no attribute", 0, Provider.GoodsItemNumber);
		}

		public void TestGoodsItemNumber_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber);
			AssertEquals(1, Provider.GoodsItemNumber);
		}

		public void TestMeasurementUnitAndQualifier_Import()
		{
			AssertEquals("KG", Provider.MeasurementUnitAndQualifier);
		}

		public void TestMeasurementUnitAndQualifier_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped, no attribute", null, Provider.MeasurementUnitAndQualifier);
		}

		public void TestMeasurementUnitAndQualifier_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit);
			AssertEquals("KG", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity() => AssertEquals(1.8M, Provider.Quantity);

		public void TestComplement_Import()
		{
			AssertEquals("COMPLEMENTARY INFORMATION", Provider.Complement);
		}

		public void TestComplement_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped, no attribute", null, Provider.Complement);
		}

		public void TestComplement_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement);
			AssertEquals("COMPLEMENTARY INFORMATION", Provider.Complement);
		}

		void CreateCode(string attributeName = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "DC40E");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "1234567", "N380", new DateTime(1900, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 29));
			if (!string.IsNullOrEmpty(attributeName))
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			}
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			previousDocument = header.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "1234567";
		}
		PreviousDocument previousDocument;
		JobDeclaration declaration;

		protected override IPreviousDocument GetProvider()
		{
			previousDocument.CSI_ReferenceNumber = "REFERENCE";
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_UnitOfQuantity = "KG";
			previousDocument.CSI_Quantity = 1.8M;
			previousDocument.CSI_Description = "COMPLEMENTARY INFORMATION";
			return PreviousDocumentProvider.NewOrNull(previousDocument);
		}
	}
}
