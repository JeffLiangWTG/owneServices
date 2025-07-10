using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SupportingDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<ISupportingDocument>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", SupportingDocumentProvider.NewOrNull(null));
		}

		public void TestFullType()
		{
			AssertEquals("3LNACF", Provider.FullType);
		}

		public void TestType()
		{
			AssertEquals("3LNA", Provider.Type);
		}

		public void TestQualifier()
		{
			AssertEquals("CF", Provider.Qualifier);
		}

		public void TestQualifier_Null()
		{
			supportingDocument.CSI_FullType = "3LNA";
			AssertEquals(null, Provider.Qualifier);
		}

		public void TestReferenceNumber()
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

		public void TestDocumentLineItemNumber()
		{
			AssertEquals("Map always for import", 1, Provider.DocumentLineItemNumber);
		}

		public void TestDocumentLineItemNumber_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", 0, Provider.DocumentLineItemNumber);
		}

		public void TestDocumentLineItemNumber_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber);
			AssertEquals("Map, if attribute exist", 1, Provider.DocumentLineItemNumber);
		}

		public void TestComplement()
		{
			AssertEquals("COMPLEMENTARY INFORMATION", Provider.Complement);
		}

		public void TestComplement_ExportNoAttribute()
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

		public void TestDetail()
		{
			AssertEquals("REFERENCE2", Provider.Detail);
		}

		public void TestDetail_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not Mapped", null, Provider.Detail);
		}

		public void TestDetail_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail);
			AssertEquals("REFERENCE2", Provider.Detail);
		}

		public void TestIssuingAuthorityName()
		{
			AssertEquals("Additional", Provider.IssuingAuthorityName);
		}

		public void TestIssuingAuthorityName_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", null, Provider.IssuingAuthorityName);
		}

		public void TestIssuingAuthorityName_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Authority);
			AssertEquals("Additional", Provider.IssuingAuthorityName);
		}

		public void TestIssuingDate()
		{
			AssertEquals(new DateTime(2020, 1, 1), Provider.IssuingDate);
		}

		public void TestIssuingDate_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", null, Provider.IssuingDate);
		}

		public void TestIssuingDate_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.IssuingDate);
			AssertEquals(new DateTime(2020, 1, 1), Provider.IssuingDate);
		}

		public void TestValidityDate()
		{
			AssertEquals(new DateTime(2021, 1, 1), Provider.ValidityDate);
		}

		public void TestValidityDate_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", null, Provider.ValidityDate);
		}

		public void TestValidityDate_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ValidityDate);
			AssertEquals(new DateTime(2021, 1, 1), Provider.ValidityDate);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("UQ2", Provider.MeasurementUnitAndQualifier);
		}

		public void TestMeasurementUnitAndQualifier_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", null, Provider.MeasurementUnitAndQualifier);
		}

		public void TestMeasurementUnitAndQualifier_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit);
			AssertEquals("UQ2", Provider.MeasurementUnitAndQualifier);
		}

		public void TestComplementaryUnit()
		{
			AssertEquals("KG", Provider.ComplementaryUnit);
		}

		public void TestComplementaryUnit_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", null, Provider.ComplementaryUnit);
		}

		public void TestComplementaryUnit_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ComplementaryUnit);
			AssertEquals("KG", Provider.ComplementaryUnit);
		}

		public void TestQuantity()
		{
			AssertEquals(1.2345M, Provider.Quantity);
		}

		public void TestCurrency()
		{
			AssertEquals("EUR", Provider.Currency);
		}

		public void TestCurrency_Export_NoAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode();
			AssertEquals("Not mapped", null, Provider.Currency);
		}

		public void TestCurrency_Export_WithAttribute()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value);
			AssertEquals("EUR", Provider.Currency);
		}

		public void TestAmount()
		{
			testValue = 2.7M;
			AssertEquals("Default", "2.7", Provider.Amount.ToString());
		}

		public void TestAmount_Normalize()
		{
			testValue = 12.0000M;
			AssertEquals("Normalize", "12", Provider.Amount.ToString());
		}

		public void TestAmount_Rounding()
		{
			testValue = 12.0055M;
			AssertEquals("Round", "12.01", Provider.Amount.ToString());
		}

		void CreateCode(string attributeName = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocument, "Supporting Document");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "3LNACF", "3LNACF", new DateTime(1900, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 29));
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
			supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_FullType = "3LNACF";
		}
		JobDeclaration declaration;
		SupportingDocument supportingDocument;
		decimal testValue;

		protected override ISupportingDocument GetProvider()
		{
			supportingDocument.CSI_ReferenceNumber = "REFERENCE";
			supportingDocument.CSI_ItemNumber = 1;
			supportingDocument.CSI_Description = "COMPLEMENTARY INFORMATION";
			supportingDocument.CSI_ReferenceNumber2 = "REFERENCE2";
			supportingDocument.CSI_AdditionalDescription = "Additional";
			supportingDocument.CSI_DateOfIssue = new DateTime(2020, 1, 1);
			supportingDocument.CSI_DateOfExpiry = new DateTime(2021, 1, 1);
			supportingDocument.CSI_UnitOfQuantity2 = "UQ2";
			supportingDocument.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
			supportingDocument.CSI_Quantity = 1.2345M;
			supportingDocument.CSI_RX_NKCurrency = "EUR";
			supportingDocument.CSI_Value = testValue;
			return SupportingDocumentProvider.NewOrNull(supportingDocument);
		}
	}
}
