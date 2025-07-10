using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplateLanguageSelectorTest : TestCaseWithFactory
	{
		public void TestSelectContactLanguage_SpecificDocument()
		{
			var stmMenuItem = Factory.NewWithValidTestData<StmMenuItem>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "test1";
			contact1.OC_Language = Core.SharedConstants.Languages.ChineseSimplified;
			contact1.OC_IsActive = true;
			var document1 = contact1.Documents.AddNew();
			document1.OD_SU_MenuItem = stmMenuItem.PK;
			document1.OD_DefaultContact = true;

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "test2";
			contact2.OC_Language = Core.SharedConstants.Languages.English;
			contact2.OC_IsActive = true;
			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.Consignee.Code;
			document2.OD_DefaultContact = true;

			var contact3 = orgHeader.Contacts.AddNew();
			contact3.OC_ContactName = "test3";
			contact3.OC_Language = Core.SharedConstants.Languages.French;
			contact3.OC_IsActive = true;
			var document3 = contact3.Documents.AddNew();
			document3.OD_DocumentGroup = ContactType.All.Code;
			document3.OD_DefaultContact = true;

			var defaultLanguagesCollection = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 1 }
			};

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguagesCollection))
			{
				var orgHeaderContact = new OrgHeaderContact(orgHeader, null);
				var defaultLanguage = TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignee.Code, "", stmMenuItem);

				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, defaultLanguage);
			}
		}

		public void TestSelectLanguage_ChildContactType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "test1";
			contact1.OC_Language = Core.SharedConstants.Languages.ChineseSimplified;
			contact1.OC_IsActive = true;
			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.ImportSeaFreightAgent.Code;
			document1.OD_DefaultContact = true;

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "test1";
			contact2.OC_Language = Core.SharedConstants.Languages.French;
			contact2.OC_IsActive = true;
			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.All.Code;
			document2.OD_DefaultContact = true;

			var defaultLanguagesCollection = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 1 }
			};

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguagesCollection))
			{
				var orgHeaderContact = new OrgHeaderContact(orgHeader, null);
				var defaultLanguage = TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.ImportFreightAgent, "SEA");

				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, defaultLanguage);

				defaultLanguage = TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.ExportFreightAgent, "SEA");
				AssertEquals(Core.SharedConstants.Languages.French, defaultLanguage);
			}
		}

		public void TestSelectLanguage()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Core.SharedConstants.Languages.German;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UKB";
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.OH_Language = Core.SharedConstants.Languages.Dutch;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderContact = new OrgHeaderContact(org, null);
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: current default English spelling", Core.SharedConstants.Languages.EnglishBritish, DataRegistry.Instance.EnglishSpelling);
				AssertEquals("Not the correct language selected", Core.SharedConstants.Languages.EnglishBritish, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));

				// Company Language
				var resultsCompany = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, Order = 1 }
				};

				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsCompany))
				{
					var selectedLanguage = ZString.Empty;

					AssertNoExceptionThrown(() => selectedLanguage = TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to English.", Core.SharedConstants.Languages.English, selectedLanguage);
				}

				// Branch Language
				var resultsBranch = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, Order = 1 }
				};

				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsBranch))
				{
					var selectedLanguage = ZString.Empty;

					AssertNoExceptionThrown(() => selectedLanguage = TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to English.", Core.SharedConstants.Languages.English, selectedLanguage);
				}

				company.GC_OH_OrgProxy = orgProxy.PK;
				branch.GB_OH_OrgProxy = branchProxy.PK;

				Factory.Save();

				// Company Language
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsCompany))
				{
					AssertEquals("Language should have been changed to German.", Core.SharedConstants.Languages.German, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to German.", GlbCompany.CurrentCompany.OrgProxy.OH_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
				}

				// Branch Language
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsBranch))
				{
					AssertEquals("Language should have been changed to Dutch.", Core.SharedConstants.Languages.Dutch, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to Dutch.", GlbBranch.CurrentBranch.OrgProxy.OH_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
				}

				// Organization Language
				var resultsOrganization = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization, Order = 1 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, Order = 3 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 2 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsOrganization))
				{
					org.OH_Language = Core.SharedConstants.Languages.PortugueseBrazil;
					AssertEquals("Language should have been changed to PortugueseBrazil.", Core.SharedConstants.Languages.PortugueseBrazil, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to PortugueseBrazil.", org.OH_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
				}

				// Address Language
				var resultsAddress = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 1 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, Order = 2 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsAddress))
				{
					org.MainAddress.OA_Language = Core.SharedConstants.Languages.French;
					AssertEquals("Language should have been changed to French.", Core.SharedConstants.Languages.French, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to French.", org.MainAddress.OA_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
				}

				// System Language
				Env.Registry.RawRegistry.EnglishSpelling.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.SharedConstants.Languages.EnglishAmerican);
				var resultsSystem = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.System, Order = 1 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsSystem))
				{
					AssertEquals("Language should have been changed to American English.", Core.SharedConstants.Languages.EnglishAmerican, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
					AssertEquals("Language should have been changed to American English.", DataRegistry.Instance.EnglishSpelling, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
				}
			}
		}

		public void TestSelectLanguageForContactFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderContact = new OrgHeaderContact(org, null);
			Factory.Save();

			var resultsContact = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 1 }
			};
			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsContact))
			{
				//Contact Priority 4 => Document group is ALL and is not official
				var contact4 = org.Contacts.AddNew();
				contact4.OC_ContactName = "Jeff";
				contact4.OC_Language = Core.SharedConstants.Languages.Albanian;
				contact4.OC_IsActive = true;
				var document4 = contact4.Documents.AddNew();
				document4.OD_DocumentGroup = ContactType.All.Code;
				document4.OD_DefaultContact = false;
				Factory.Save();

				AssertEquals(Core.SharedConstants.Languages.Albanian, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignee.Code));

				//Contact Priority 3 => Document group is ALL and is official
				var contact3 = org.Contacts.AddNew();
				contact3.OC_ContactName = "Nero";
				contact3.OC_Language = Core.SharedConstants.Languages.Arabic;
				contact3.OC_IsActive = true;
				var document3 = contact3.Documents.AddNew();
				document3.OD_DocumentGroup = ContactType.All.Code;
				document3.OD_DefaultContact = true;
				Factory.Save();

				AssertEquals(Core.SharedConstants.Languages.Arabic, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignee.Code));

				//Contact Priority 2 => Match the specific document group and is not official
				var contact2 = org.Contacts.AddNew();
				contact2.OC_ContactName = "James";
				contact2.OC_Language = Core.SharedConstants.Languages.Bangla;
				contact2.OC_IsActive = true;
				var document2 = contact2.Documents.AddNew();
				document2.OD_DocumentGroup = ContactType.Consignee.Code;
				document2.OD_DefaultContact = false;
				Factory.Save();

				AssertEquals(Core.SharedConstants.Languages.Bangla, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignee.Code));

				//Contact Priority 1 => Match the specific document group and is official
				var contact1 = org.Contacts.AddNew();
				contact1.OC_ContactName = "Kelvin";
				contact1.OC_Language = Core.SharedConstants.Languages.ChineseSimplified;
				contact1.OC_IsActive = true;
				var document1 = contact1.Documents.AddNew();
				document1.OD_DocumentGroup = ContactType.Consignee.Code;
				document1.OD_DefaultContact = true;
				Factory.Save();

				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignee.Code));
			}
		}

		public void TestSelectLanguate_ContactsIsOfficialButNotMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderContact = new OrgHeaderContact(org, null);
			org.MainAddress.OA_Language = Core.SharedConstants.Languages.English;
			Factory.Save();

			var resultsContact = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 1 }
			};

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsContact))
			{
				//Contact Is official but not match will fall back to Address language
				var contact6 = org.Contacts.AddNew();
				contact6.OC_ContactName = "Test";
				contact6.OC_Language = Core.SharedConstants.Languages.French;
				contact6.OC_IsActive = true;

				var document6 = contact6.Documents.AddNew();
				document6.OD_DocumentGroup = ContactType.CTO.Code;
				document6.OD_DeliverBy = Core.Constants.ContactNotifyModes.DoNotDeliver;
				document6.OD_DefaultContact = true;

				var contact5 = org.Contacts.AddNew();
				contact5.OC_ContactName = "Walker";
				contact5.OC_Language = Core.SharedConstants.Languages.Afrikaans;
				contact5.OC_IsActive = true;
				var document5 = contact5.Documents.AddNew();
				document5.OD_DocumentGroup = ContactType.CTO.Code;
				document5.OD_DefaultContact = true;
				Factory.Save();

				AssertEquals(Core.SharedConstants.Languages.English, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignee.Code));
			}
		}

		public void TestSelectLanguage_InactiveContact()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Core.SharedConstants.Languages.German;
			company.GC_OH_OrgProxy = orgProxy.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UKB";
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.OH_Language = Core.SharedConstants.Languages.Dutch;
			branch.GB_OH_OrgProxy = branchProxy.PK;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderContact = new OrgHeaderContact(org, null);
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: current default English spelling", Core.SharedConstants.Languages.EnglishBritish, DataRegistry.Instance.EnglishSpelling);
				AssertEquals("Not the correct language selected", Core.SharedConstants.Languages.EnglishBritish, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));

				var resultsContact = new DocumentDeliveryDefaultLanguagesCollection {
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
					new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 1 }
				};
				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resultsContact))
				{
					var contact1 = org.Contacts.AddNew();
					contact1.OC_ContactName = "Nando";
					contact1.OC_Language = Core.SharedConstants.Languages.Finnish;
					contact1.OC_IsActive = true;
					var document1 = contact1.Documents.AddNew();
					document1.OD_DocumentGroup = "ALL";
					document1.OD_DefaultContact = false;
					Factory.Save();

					AssertEquals("Language should have been changed to Finnish.", Core.SharedConstants.Languages.Finnish, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignor.Code));
					AssertEquals("Language should have been changed to Finnish.", contact1.OC_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignor.Code));

					var contact2 = org.Contacts.AddNew();
					contact2.OC_ContactName = "Joe";
					contact2.OC_Language = Core.SharedConstants.Languages.Italian;
					contact2.OC_IsActive = false;
					var document2 = contact2.Documents.AddNew();
					document2.OD_DocumentGroup = ContactType.Consignor.Code;
					document2.OD_DefaultContact = false;
					Factory.Save();

					AssertEquals("Language should have been changed to Finnish.", Core.SharedConstants.Languages.Finnish, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignor.Code));
					AssertEquals("Language should have been changed to Finnish.", contact1.OC_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignor.Code));

					contact2.OC_IsActive = true;

					AssertEquals("Language should have been changed to Italian.", Core.SharedConstants.Languages.Italian, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignor.Code));
					AssertEquals("Language should have been changed to Italian.", contact2.OC_Language, TemplateLanguageSelector.SelectLanguage(orgHeaderContact, ContactType.Consignor.Code));
				}
			}
		}

		public void TestSelectLanguageWithSelectedAddress()
		{
			var defaultLanguagesCollection = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 1 }
			};

			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull(org.MainAddress);

			org.MainAddress.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			var address1 = org.Addresses.AddNew();
			address1.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			var address2 = org.Addresses.AddNew();
			address2.OA_Language = Core.SharedConstants.Languages.French;

			var orgHeaderContact = new OrgHeaderContact(org, address2);

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguagesCollection))
			{
				AssertEquals("Language should be French since address2 is selected", Core.SharedConstants.Languages.French, TemplateLanguageSelector.SelectLanguage(orgHeaderContact));
			}
		}
	}
}
