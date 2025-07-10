using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class RecipientAddressFormatterTest : TestCaseWithFactory
	{
		public void TestIcelandicWords()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_RL_NKClosestPort = "ISREY";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.Kennitala, "Test");

			var formatter = new RecipientAddressFormatter(new RecipientNameAndAddress().Regex);
			const string contactName = "THE ACCOUNTS PAYABLE MANAGER";
			var contact = new DocDeliveryContact(Factory);
			contact.OrgHeaderPK = orgProxy.PK;

			var result = formatter.GetDeliveryContactNameLine(contactName, contact, false, false);
			AssertEquals("Icelandic words should be valid.", "b/t: bókhaldsdeildar", result);

			var report = DocumentEngineTestHelper.GetNewReportWithNoExceptionOnErrors(Factory, NewStyleTemplate);
			((IReportForUnitTesting)report).DeliveryContact = contact;

			AssertEquals("Icelandic words should be valid.", "#1\r\nKennitala greiðanda Test", formatter.Format("<RecipientNameAndAddress>", report, false));
		}

		public void TestPrefixForAddressContactNameLine()
		{
			var formatter = new RecipientAddressFormatter(null);
			const string contactName = "LW";
			var contact = new DocDeliveryContact(Factory);

			//Test AttentionPrefixForAddressContactNameLineText
			var result = formatter.GetDeliveryContactNameLine(contactName, contact, false, false);
			AssertEquals("Do not allow to have a space.", "ATTENTION: LW", result);

			DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "", "parameter"));
			result = formatter.GetDeliveryContactNameLine(contactName, contact, false, false);
			AssertEquals("Do not allow to have a space.", "LW", result);

			DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "{0}: ATTENTION", "parameter"));
			result = formatter.GetDeliveryContactNameLine(contactName, contact, false, false);
			AssertEquals("Do not allow to have a space.", "LW: ATTENTION", result);

			DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "ATTENTION {0} ATTENTION", "parameter"	));
			result = formatter.GetDeliveryContactNameLine(contactName, contact, false, false);
			AssertEquals("Do not allow to have a space.", "ATTENTION LW ATTENTION", result);

			var invalidInputArray = new[] { "ATTENTION {1}", "ATTENTION {0} & {1}", "ATTENTION {010}", "ATTENTION {A}", "ATTENTION {}", "ATTENTION {X0X}", "ATTENTION {0X0}000", "ATTENTION {*} XX", "ATTENTION 000{_}" };
			for (var i = 1; i < invalidInputArray.Length; i++)
			{
				DocumentsDataRegistry.Instance.AttentionPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", invalidInputArray[i], "parameter"));
				AssertExceptionThrown<FormatException>("FormatException with hint should be thrown", $"The input value '{invalidInputArray[i]}' is invalid for address formatting, only '{{0}}' can be treated as a replacement string, check your input in Registry -> Documents -> Address Formatting -> 'Attention' Contact Prefix.", () => formatter.GetDeliveryContactNameLine(contactName, contact, false, false));
			}

			//Test RedirectToPrefixForAddressContactNameLineText
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);

			result = formatter.GetDeliveryContactNameLine(contactName, contact, true, false);
			AssertEquals("Do not allow to have a space.", "REDIRECT TO: LW", result);
			DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "", "parameter"));
			result = formatter.GetDeliveryContactNameLine(contactName, contact, true, false);
			AssertEquals("Do not allow to have a space.", "LW", result);
			DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "{0}: REDIRECT TO", "parameter"));
			result = formatter.GetDeliveryContactNameLine(contactName, contact, true, false);
			AssertEquals("Do not allow to have a space.", "LW: REDIRECT TO", result);
			DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "REDIRECT {0} TO", "parameter"));
			result = formatter.GetDeliveryContactNameLine(contactName, contact, true, false);
			AssertEquals("Do not allow to have a space.", "REDIRECT LW TO", result);

			DocumentsDataRegistry.Instance.RedirectToPrefixForAddressContactNameLineText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("y", "{1}: REDIRECT TO", "parameter"));
			AssertExceptionThrown<FormatException>("FormatException with hint should be thrown", @"The input value '{1}: REDIRECT TO' is invalid for address formatting, only '{0}' can be treated as a replacement string, check your input in Registry -> Documents -> Address Formatting -> 'Redirect To' Contact Prefix.", () => formatter.GetDeliveryContactNameLine(contactName, contact, true, false));
		}

		public void TestFormat()
		{
			var report = DocumentEngineTestHelper.GetNewReportWithNoExceptionOnErrors(Factory, NewStyleTemplate);

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
			((IReportForUnitTesting)report).DeliveryContact = contact;

			var formatter = new RecipientAddressFormatter(new RecipientNameAndAddress().Regex);
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR1\r\nADDR2\r\nSYDNEY NSW 2000", formatter.Format("<RecipientNameAndAddress>", report, false));
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR1\r\nADDR2\r\nSYDNEY NSW 2000", formatter.Format("<RecipientNameAndAddress(N)>", report, false));
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR1\r\nADDR2\r\nSYDNEY NSW 2000\r\nAUSTRALIA", formatter.Format("<RecipientNameAndAddress(Y)>", report, false, true));

			//Test RedirectToPrefixForAddressContactNameLineText
			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactWithRedirectTo);
			((IReportForUnitTesting)report).MostOfficialContact = contact;
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR1\r\nADDR2\r\nSYDNEY NSW 2000", formatter.Format("<RecipientNameAndAddress>", report, false));

			var contact2 = new DocDeliveryContact(Factory)
			{
				Name = "CONTACT2",
				CompanyName = "WTG",
				Address1 = "Addr3",
				Address2 = "Addr4",
				City = "Sydney",
				PostCode = "2000",
				State = "NSW",
				UNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD")
			};
			((IReportForUnitTesting)report).MostOfficialContact = contact2;
			AssertEquals("WTG\r\nREDIRECT TO: CONTACT\r\nWTG\r\nADDR1\r\nADDR2\r\nSYDNEY NSW 2000", formatter.Format("<RecipientNameAndAddress>", report, false));

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR1\r\nADDR2\r\nSYDNEY NSW 2000", formatter.Format("<RecipientNameAndAddress>", report, false));

			DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR3\r\nADDR4\r\nSYDNEY NSW 2000", formatter.Format("<RecipientNameAndAddress>", report, false));
		}

		public void TestFormatAddress()
		{
			var report = DocumentEngineTestHelper.GetNewReportWithNoExceptionOnErrors(Factory, NewStyleTemplate);

			var contact = new DocDeliveryContact(Factory)
			{
				Name = "CONTACT",
				CompanyName = "WTG",
				Address1 = "Addr1",
				Address2 = "Addr2",
				City = "Sydney",
				PostCode = "2000",
				UNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD")
			};
			((IReportForUnitTesting)report).DeliveryContact = contact;

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			if (country != null)
			{
				country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityPostcodeCountry;
			}

			AssertEquals("OrgAllowMixedCase", false, Env.Registry.OrgAllowMixedCase);
			var formatter = new RecipientAddressFormatter(new RecipientNameAndAddress().Regex);
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nADDR1\r\nADDR2\r\nSYDNEY\r\n2000", formatter.Format("<RecipientNameAndAddress>", report, false));

			Env.Registry.SetOrgAllowMixedCase(true);
			AssertEquals("OrgAllowMixedCase", true, Env.Registry.OrgAllowMixedCase);
			AssertEquals("WTG\r\nATTENTION: CONTACT\r\nAddr1\r\nAddr2\r\nSydney\r\n2000", formatter.Format("<RecipientNameAndAddress>", report, false));
		}

		public void TestFormatAddress_TranslatedAddress()
		{
			var report = DocumentEngineTestHelper.GetNewReportWithNoExceptionOnErrors(Factory, NewStyleTemplate);

			var testContact = new DocDeliveryContact(Factory)
			{
				CompanyName = "WTG",
				Address1 = "Addr1",
				Address2 = "Addr2",
				City = "Sydney",
				State = "NSW",
				PostCode = "2100",
				AdditionalAddress = "Addr3",
				UNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"),
				Language = Core.Constants.Languages.English
			};

			var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testOrgHeader.MainAddress.CompanyName = testContact.CompanyName;
			testOrgHeader.MainAddress.Address1 = testContact.Address1;
			testOrgHeader.MainAddress.Address2 = testContact.Address2;
			testOrgHeader.MainAddress.City = testContact.City;
			testOrgHeader.MainAddress.Postcode = testContact.PostCode;
			testOrgHeader.MainAddress.State = testContact.State;
			testOrgHeader.MainAddress.OA_AdditionalAddressInformation = testContact.AdditionalAddress;

			testContact.OrgHeaderPK = testOrgHeader.PK;

			var translatedAddress1 = testContact.OrgAddress.TranslatedAddresses.AddNew();
			translatedAddress1.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress1.OTA_CompanyName = "慧咨科技";
			translatedAddress1.OTA_Address1 = "地址1";
			translatedAddress1.OTA_Address2 = "地址2";
			translatedAddress1.OTA_City = "悉尼";
			translatedAddress1.OTA_State = "新南威尔士";
			translatedAddress1.OTA_PostCode = "2100";
			translatedAddress1.OTA_AdditionalAddressInformation = "地址3";

			((IReportForUnitTesting)report).DeliveryContact = testContact;

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				var formatter = new RecipientAddressFormatter(new RecipientNameAndIntendedRecipientAddress().Regex);
				var expectedAddressTranslated = "慧咨科技\r\n地址3\r\n地址1\r\n地址2\r\n悉尼 新南威尔士 2100";
				var actualAddressTranslated = formatter.Format(string.Format("<RecipientNameAndIntendedRecipientAddress({0})>", testContact.OrgHeaderPK), report, false);
				AssertEquals("Get translated address from RecipientNameAndIntendedRecipientAddress macro", expectedAddressTranslated, actualAddressTranslated);

				formatter = new RecipientAddressFormatter(new IntendedRecipientAddress().Regex);
				actualAddressTranslated = formatter.Format(string.Format("<IntendedRecipientAddress({0})>", testContact.OrgHeaderPK), report, true);
				AssertEquals("Get translated address from IntendedRecipientAddress macro", expectedAddressTranslated, actualAddressTranslated);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}
	}
}
