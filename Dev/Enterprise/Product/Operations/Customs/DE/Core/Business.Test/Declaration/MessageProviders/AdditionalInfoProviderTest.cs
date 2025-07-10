using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AdditionalInfoProviderTest : Customs.Business.Testing.DataProviderTestCase<IReference>
	{
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
			additionalInfo.CSI_Code = "3LNA";
			AssertEquals(null, Provider.Qualifier);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("REFERENCE", Provider.ReferenceNumber);
		}

		public void TestReferenceNumber_Export_NoAttribute()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CreateCode();
			AssertEquals("Not mapped, no attribute", null, Provider.ReferenceNumber);
		}

		public void TestReferenceNumber_Export_WithAttribute()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference);
			AssertEquals("REFERENCE", Provider.ReferenceNumber);
		}

		public void TestReferenceNumber_Export_SubTypeINF()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			CreateCode();
			AssertEquals("Mapped without attribute", "REFERENCE", Provider.ReferenceNumber);
		}

		public void TestDetail()
		{
			AssertEquals("REFERENCE2", Provider.Detail);
		}

		public void TestDetail_Export_NoAttribute()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CreateCode();
			AssertEquals("Not mapped, no attribute", null, Provider.Detail);
		}

		public void TestDetail_Export_WithAttribute()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail);
			AssertEquals("REFERENCE2", Provider.Detail);
		}

		public void TestDetail_Export_SubTypeAUT()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			CreateCode();
			AssertEquals("Mapped without attribute", "REFERENCE2", Provider.Detail);
		}

		public void TestComplement()
		{
			AssertEquals("DESCRIPTION", Provider.Complement);
		}

		public void TestCurrency()
		{
			AssertEquals("EUR", Provider.Currency);
		}

		public void TestCurrency_Export_NoAttribute()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CreateCode();
			AssertEquals("Not mapped, no attribute", null, Provider.Currency);
		}

		public void TestCurrency_Export_WithAttribute()
		{
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CreateCode(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value);
			additionalInfo.CSI_RX_NKCurrency = "EUR";
			AssertEquals("EUR", Provider.Currency);
		}

		public void TestAmount_Default()
		{
			testValue = 1.8M;
			AssertEquals("Default", "1.8", Provider.Amount.ToString());
		}

		public void TestAmount_Normalize()
		{
			testValue = 12.0000M;
			AssertEquals("Normalize", "12", Provider.Amount.ToString());
		}

		public void TestAmount_Round()
		{
			testValue = 12.0055M;
			AssertEquals("Round", "12.01", Provider.Amount.ToString());
		}

		public void TestAmount_FromConstructor()
		{
			var provider = AdditionalInfoProvider.NewOrNull(12.0055M, additionalInfo);
			AssertEquals("12.01", provider.Amount.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var header = declaration.Invoices.AddNew();

			additionalInfo = header.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "3LNACF";
		}
		AdditionalInfo additionalInfo;
		decimal testValue;

		protected override IReference GetProvider()
		{
			additionalInfo.CSI_ReferenceNumber = "REFERENCE";
			additionalInfo.CSI_ReferenceNumber2 = "REFERENCE2";
			additionalInfo.CSI_Description = "DESCRIPTION";
			additionalInfo.CSI_RX_NKCurrency = "EUR";
			additionalInfo.CSI_Value = testValue;
			return AdditionalInfoProvider.NewOrNull(additionalInfo);
		}

		void CreateCode(string attributeName = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, "TD44E");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, "3LNACF", "3LNACF", new DateTime(1900, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 29));

			if (!string.IsNullOrEmpty(attributeName))
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Header);
				helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			}
			Factory.Save();
		}
	}
}
