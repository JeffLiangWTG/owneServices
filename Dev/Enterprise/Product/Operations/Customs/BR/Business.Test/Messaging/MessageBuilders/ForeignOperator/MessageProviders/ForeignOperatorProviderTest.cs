using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.ProductCatalog;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ForeignOperatorProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "001";
			manufacturer.OH_FullName = "FOREIGN OPERATOR";
			manufacturer.MainAddress.OA_Address1 = "PAULISTA AVENUE";
			manufacturer.MainAddress.OA_Address2 = "COMPLEMENTARY";
			manufacturer.MainAddress.OA_City = "SAO PAULO";
			manufacturer.MainAddress.OA_RN_NKCountryCode = "BR";
			manufacturer.MainAddress.OA_PostCode = "04248000";
			manufacturer.MainAddress.OA_State = "SP";

			var customsCodeTIN = manufacturer.CustomsCodes.AddNew();
			customsCodeTIN.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.TIN;
			customsCodeTIN.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			customsCodeTIN.OK_CustomsRegNo = "50178";

			var customsFOICode = manufacturer.CustomsCodes.AddNew();
			customsFOICode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode;
			customsFOICode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			customsFOICode.OK_CustomsRegNo = "56789";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";
			owner.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer.PK;
			foreignOperator.BFR_AuthorityIdentifier = "TEST123";

			var foreignOperatorMessageSending = new ForeignOperatorMessageSendingObject(foreignOperator);
			foreignOperatorMessageSending.Action = ActionList.Codes.Deactivate;

			var dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);
			CombineAssertions(() =>
			{
				AssertEquals("Sequence", 1, dataProvider.Sequence);
				AssertEquals("RootCpfCnpj", "75400331", dataProvider.RootCpfCnpj);
				AssertEquals("Code", "TEST123", dataProvider.Code);
				AssertEquals("Version", string.Empty, dataProvider.Version);
				AssertEquals("RegistrationNumber", customsCodeTIN.OK_CustomsRegNo, dataProvider.RegistrationNumber);
				AssertEquals("Name", manufacturer.OH_FullName, dataProvider.Name);
				AssertEquals("Situation", "DESATIVADO", dataProvider.Situation);
				AssertEquals("Address", manufacturer.MainAddress.Address1 + " " + manufacturer.MainAddress.Address2, dataProvider.Address);
				AssertEquals("City", manufacturer.MainAddress.OA_City, dataProvider.City);
				AssertEquals("Country", "BR", dataProvider.Country);
				AssertEquals("SubDivisionCountryCode", "BR-SP", dataProvider.SubDivisionCountryCode);
				AssertEquals("ZipCode", manufacturer.MainAddress.OA_PostCode, dataProvider.ZipCode);
				AssertEquals("InternalCode", customsFOICode.OK_CustomsRegNo, dataProvider.InternalCode);
				AssertEquals("Email", string.Empty, dataProvider.Email);
				AssertEquals("ReferenceDate", string.Empty, dataProvider.ReferenceDate);
				AssertNull(dataProvider.AdditionalIdentification);
			});

			foreignOperator.BFR_OH_ForeignOperator = ZGuid.Empty;
			dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);
			CombineAssertions(() =>
			{
				AssertEquals("Sequence", 1, dataProvider.Sequence);
				AssertEquals("RootCpfCnpj", "75400331", dataProvider.RootCpfCnpj);
				AssertEquals("Code", "TEST123", dataProvider.Code);
				AssertEquals("Version", string.Empty, dataProvider.Version);
				AssertEquals("RegistrationNumber", string.Empty, dataProvider.RegistrationNumber);
				AssertEquals("Name", string.Empty, dataProvider.Name);
				AssertEquals("Situation", "DESATIVADO", dataProvider.Situation);
				AssertEquals("Address", string.Empty, dataProvider.Address);
				AssertEquals("City", string.Empty, dataProvider.City);
				AssertEquals("Country", string.Empty, dataProvider.Country);
				AssertEquals("SubDivisionCountryCode", string.Empty, dataProvider.SubDivisionCountryCode);
				AssertEquals("ZipCode", string.Empty, dataProvider.ZipCode);
				AssertEquals("InternalCode", string.Empty, dataProvider.InternalCode);
				AssertEquals("Email", string.Empty, dataProvider.Email);
				AssertEquals("ReferenceDate", string.Empty, dataProvider.ReferenceDate);
				AssertNull(dataProvider.AdditionalIdentification);
			});
		}

		public void TestEmail()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "001";
			manufacturer.OH_FullName = "FOREIGN OPERATOR";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer.PK;

			var foreignOperatorMessageSending = new ForeignOperatorMessageSendingObject(foreignOperator);

			var dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);

			AssertEquals("Email", string.Empty, dataProvider.Email);

			var contact = manufacturer.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var allocation1 = contact.Allocations.AddNew();
			allocation1.PC_Type = OrgConstants.ContactAllocationType.CUS;
			dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);
			AssertEquals("Email", string.Empty, dataProvider.Email);

			var allocation2 = contact.Allocations.AddNew();
			allocation2.PC_Type = OrgConstants.ContactAllocationType.BRForeignOperator;
			dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);
			AssertEquals("Email", "test@test.com", dataProvider.Email);
		}

		public void TestInternalCode()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "002";
			manufacturer.OH_FullName = "FOREIGN OPERATOR";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer.PK;

			var foreignOperatorMessageSending = new ForeignOperatorMessageSendingObject(foreignOperator);

			var dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);
			AssertEquals("InternalCode", string.Empty, dataProvider.InternalCode);

			var customsFOICode = manufacturer.CustomsCodes.AddNew();
			customsFOICode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode;
			customsFOICode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			customsFOICode.OK_CustomsRegNo = "56789";

			dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);
			AssertEquals("InternalCode", "56789", dataProvider.InternalCode);
		}

		public void TestSituation()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "002";
			manufacturer.OH_FullName = "FOREIGN OPERATOR";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer.PK;

			var foreignOperatorMessageSending = new ForeignOperatorMessageSendingObject(foreignOperator);
			var dataProvider = new ForeignOperatorProvider(foreignOperatorMessageSending);

			foreignOperatorMessageSending.Action = ActionList.Codes.Activate;
			AssertEquals("Situation should be ", Constants.Situation.Active, dataProvider.Situation);

			foreignOperatorMessageSending.Action = ActionList.Codes.CreateNewVersion;
			AssertEquals("Situation should be ", Constants.Situation.Active, dataProvider.Situation);

			foreignOperatorMessageSending.Action = ActionList.Codes.Deactivate;
			AssertEquals("Situation should be ", Constants.Situation.Deactive, dataProvider.Situation);
		}
	}
}

