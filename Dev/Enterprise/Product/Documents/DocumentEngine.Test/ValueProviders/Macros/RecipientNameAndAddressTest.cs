using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientNameAndAddress))]
	sealed class RecipientNameAndAddressTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientNameAndAddress();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient name and address>", ValueProviderToTest.IsResponsibleForReplacing("<recipient name and address>", Passes.FirstPass));
			Assert("Should replace <recipientnameandaddress>", ValueProviderToTest.IsResponsibleForReplacing("<recipientnameandaddress>", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Mia Wallace", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Consignee;
			var deliveryContact = new DocDeliveryContact(new BusinessObjectFactory());
			deliveryContact.Name = "Fred";
			deliveryContact.CompanyName = "Foo Corp";
			deliveryContact.Address1 = "Addr1";
			deliveryContact.Address2 = "Addr2";
			deliveryContact.City = "Sydney";
			deliveryContact.PostCode = "2000";
			deliveryContact.State = "NSW";
			deliveryContact.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = deliveryContact;
			AssertEquals("Fred\nFoo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));

			Report.TypeOfContact = ContactType.Consignor;
			AssertEquals("Fred\nFoo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.NotifyParty;
			AssertEquals("Fred\nFoo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.Payables;
			AssertEquals("Fred\nFoo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.TransportServices;
			AssertEquals("Fred\nFoo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));

			AssertEquals(true, ValueProviderToTest.GetReplacement("", Report).ToString().StartsWith("FRED"));

			DocumentsDataRegistry.Instance.ContactNameUpperCase.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(true, ValueProviderToTest.GetReplacement("", Report).ToString().StartsWith("Fred"));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWithContactTypeReceivables()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Receivables;
			var deliveryContact = new DocDeliveryContact(new BusinessObjectFactory());
			deliveryContact.Name = "Mike";
			deliveryContact.CompanyName = "Boo Ltd.";
			deliveryContact.Address1 = "Addr1";
			deliveryContact.Address2 = "Addr2";
			deliveryContact.City = "Sydney";
			deliveryContact.PostCode = "2000";
			deliveryContact.State = "NSW";
			deliveryContact.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = deliveryContact;
			AssertEquals("BOO LTD.\nADDR1\nADDR2\nSYDNEY NSW 2000\nATTENTION: MIKE", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWithIncludeCountryParam()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Consignee;
			var contact = new DocDeliveryContact(Factory)
			{
				Name = "CONTACT",
				CompanyName = "WTG",
				Address1 = "Addr1",
				Address2 = "Addr2",
				City = "Sydney",
				PostCode = "2000",
				State = "NSW",
				UNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD")
			};
			((IReportForUnitTesting)Report).DeliveryContact = contact;

			AssertEquals("CONTACT\nWTG\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress>", Report));
			AssertEquals("CONTACT\nWTG\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(N)>", Report));
			AssertEquals("CONTACT\nWTG\nAddr1\nAddr2\nSydney NSW 2000\nAustralia".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(Y)>", Report));
		}

		public void TestRegexMatches()
		{
			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress>"));
			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress( Y )>"));
			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress( N )>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(blah)>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(O)>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(YY)>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(NN)>"));

			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(Y,PBR)>"));
			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(N,FRN)>"));
			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(Y,DocumentLanguage)>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(PBR)>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(DocumentLanguage)>"));
			Assert(ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(,PBR)>"));
			Assert(!ValueProviderToTest.Regex.IsMatch("<RecipientNameAndAddress(YY,PBR)>"));
		}

		public void TestReplacementWithLanguageCode()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Consignee;
			Report.Parent.Language = Core.Constants.Languages.French;

			var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testOrgHeader.MainAddress.CompanyName = "WTG";
			testOrgHeader.MainAddress.Address1 = "Addr1";
			testOrgHeader.MainAddress.Address2 = "Addr2";
			testOrgHeader.MainAddress.City = "Sydney";
			testOrgHeader.MainAddress.Postcode = "2000";
			testOrgHeader.MainAddress.State = "NSW";

			var contact = new DocDeliveryContact(Factory);
			contact.Name = "CONTACT";
			contact.OrgHeaderPK = testOrgHeader.PK;
			contact.UNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var translatedAddress1 = contact.OrgAddress.TranslatedAddresses.AddNew();
			translatedAddress1.OTA_Language = Core.Constants.Languages.PortugueseBrazil;
			translatedAddress1.OTA_CompanyName = "Name pbr";
			translatedAddress1.OTA_Address1 = "Addr1 pbr";
			translatedAddress1.OTA_Address2 = "Addr2 pbr";
			translatedAddress1.OTA_City = "PSydney";
			translatedAddress1.OTA_State = "NSW";
			translatedAddress1.OTA_PostCode = "2110";

			var translatedAddress2 = contact.OrgAddress.TranslatedAddresses.AddNew();
			translatedAddress2.OTA_Language = Core.Constants.Languages.French;
			translatedAddress2.OTA_CompanyName = "Name frn";
			translatedAddress2.OTA_Address1 = "Addr frn";
			translatedAddress2.OTA_Address2 = "Addr2 frn";
			translatedAddress2.OTA_City = "FSydney";
			translatedAddress2.OTA_State = "NSW";
			translatedAddress2.OTA_PostCode = "2220";
			Factory.Save();

			((IReportForUnitTesting)Report).DeliveryContact = contact;

			AssertEquals("CONTACT\nName frn\nAddr frn\nAddr2 frn\nFSydney NSW 2220".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress>", Report));
			AssertEquals("CONTACT\nName frn\nAddr frn\nAddr2 frn\nFSydney NSW 2220".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(N)>", Report));
			AssertEquals("CONTACT\nName pbr\nAddr1 pbr\nAddr2 pbr\nPSydney NSW 2110\nAUSTRÁLIA".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(Y," + Core.Constants.Languages.PortugueseBrazil + ")>", Report));
			AssertEquals("CONTACT\nName pbr\nAddr1 pbr\nAddr2 pbr\nPSydney NSW 2110".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(N," + Core.Constants.Languages.PortugueseBrazil + ")>", Report));
			AssertEquals("CONTACT\nName frn\nAddr frn\nAddr2 frn\nFSydney NSW 2220\nAUSTRALIE".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(Y,DocumentLanguage)>", Report));
		}

		public void TestInvalidInputValueReportAnError()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Consignee;
			var deliveryContact = new DocDeliveryContact(new BusinessObjectFactory());
			deliveryContact.Name = "Contact";
			deliveryContact.CompanyName = "Corp";
			deliveryContact.Address1 = "Addr1";
			deliveryContact.Address2 = "Addr2";
			deliveryContact.City = "Sydney";
			deliveryContact.PostCode = "2000";
			deliveryContact.State = "NSW";
			((IReportForUnitTesting)Report).DeliveryContact = deliveryContact;

			ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(Y,DocumentLanguage)>", Report);
			AssertEquals("Expect no error", "ReportErrorManager has no errors", Report.ErrorManager.ToString());

			ValueProviderToTest.GetReplacement("<RecipientNameAndAddress(,DocumentLanguage)>", Report);
			var expectedErrorMessage = "Severity: [Warning (without error report)] Message: [Error in RecipientNameAndAddress Macro: Parameter IncludeCountryIfSameToCurrent is missing. When LanguageCode is specified, the parameter IncludeCountryIfSameToCurrent must be specified as well]";
			AssertEquals("report.ErrorManager.ToString()", expectedErrorMessage, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		protected override List<FieldInfo> FieldCollection => new List<FieldInfo> { typeof(RecipientNameAndAddress).GetField("factory", BindingFlags.Instance | BindingFlags.NonPublic) };

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Receivables;
			var mike = new DocDeliveryContact(new BusinessObjectFactory());
			mike.Name = "Mike";
			mike.CompanyName = "Boo Ltd.";
			mike.Address1 = "Addr1";
			mike.Address2 = "Addr2";
			mike.City = "Sydney";
			mike.PostCode = "2000";
			mike.State = "NSW";
			mike.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = mike;
		}
	}
}
