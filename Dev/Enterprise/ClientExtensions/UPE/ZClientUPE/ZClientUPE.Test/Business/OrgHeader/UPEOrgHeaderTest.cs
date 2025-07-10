using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEOrgHeader))]
	internal class UPEOrgHeaderTest : OrgHeaderTest
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEOrgHeader>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public new void TestRequiresSecurityOverrideToUpdateCountry()
		{
			var companyFR = Factory.NewWithValidTestData<GlbCompany>();
			companyFR.GC_Code = "AAA";
			companyFR.GC_Name = "AAA Company";
			companyFR.GC_RN_NKCountryCode = "FR";
			GlbBranch branchFR = companyFR.Branches.AddNew();
			branchFR.GB_Code = "ABC";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TTT";
			staff.GS_LoginName = "testuser";
			Factory.Save();
			using (Env.SetTemporaryUserContext("testuser", branchFR.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				UPEDataRegistry.Instance.EnableUPECustomisations = true;
				Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = true;
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "ORG1";
				org1.OH_RL_NKClosestPort = "AUSYD";
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);
				org1.OH_RL_NKClosestPort = "FRPAR";
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);
				Factory.Save();
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);
				org1.OH_RL_NKClosestPort = "AUSYD";
				Factory.Save();
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);
				Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = false;
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "ORG2";
				org2.OH_RL_NKClosestPort = "AUSYD";
				Assert("New org, has no right, different countries > should require security override", org2.RequiresSecurityOverrideToUpdateCountry);
				org2.OH_RL_NKClosestPort = "FRPAR";
				Assert("New org, has no right, same country > should not require security override", !org2.RequiresSecurityOverrideToUpdateCountry);
				Factory.Save();
				Assert("Existing org, has no right, same country > should not require security override", !org2.RequiresSecurityOverrideToUpdateCountry);
				org2.OH_RL_NKClosestPort = "AUSYD";
				Assert("Existing org, has no right, different countries, UNLOCO has changes > require security override", org2.RequiresSecurityOverrideToUpdateCountry);
				Factory.Save();
				Assert("Existing org, has no right, different countries, UNLOCO has no changes > should not require security override", !org2.RequiresSecurityOverrideToUpdateCountry);
			}
		}

		#region New Properties
		public void TestAccountClass()
		{
			AssertEquals("", Organisation.AccountClass);
			SetAccountClassForTest("TES");
			AssertEquals("TES", Organisation.AccountClass);
		}

		public void TestAccountNumber()
		{
			AssertEquals("Pre-condition", ZString.Empty, Organisation.AccountNumber);
			Organisation.AccountNumber = "88912";
			AssertEquals("88912", Organisation.AccountNumber);
			AssertEquals("OrgCusCode object should be created when a non-empty account number exists", 1, Organisation.CustomsCodes.Count);
			Organisation.AccountNumber = "";
			AssertEquals("", Organisation.AccountNumber);
			AssertEquals("OrgCusCode object should be deleted when the account number is empty", 0, Organisation.CustomsCodes.Count);
		}

		public void TestIsDeliveryHandledByUPSForThisAlternateBroker()
		{
			AssertEquals("Default is true", true, Organisation.IsDeliveryHandledByUPSForThisAlternateBroker);
			Organisation.IsDeliveryHandledByUPSForThisAlternateBroker = true;
			AssertEquals(true, Organisation.IsDeliveryHandledByUPSForThisAlternateBroker);
			Organisation.IsDeliveryHandledByUPSForThisAlternateBroker = false;
			AssertEquals(false, Organisation.IsDeliveryHandledByUPSForThisAlternateBroker);
		}

		public void TestIsITFChargableForThisImporter()
		{
			AssertEquals("Default is true", true, Organisation.IsITFChargableForThisImporter);
			Organisation.IsITFChargableForThisImporter = false;
			AssertEquals(false, Organisation.IsITFChargableForThisImporter);
			Organisation.IsITFChargableForThisImporter = true;
			AssertEquals(true, Organisation.IsITFChargableForThisImporter);
		}

		public void TestIsHighClaimer()
		{
			AssertEquals("Default is false", false, Organisation.IsHighClaimer);
			Organisation.IsHighClaimer = true;
			AssertEquals(true, Organisation.IsHighClaimer);
			Organisation.IsHighClaimer = false;
			AssertEquals(false, Organisation.IsHighClaimer);
		}

		public void TestDefaultContactName()
		{
			OrgContact contact = Organisation.Contacts.AddNew();
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Payables.Code;
			contact.OC_ContactName = "Jack Bauer";
			AssertEquals("Jack Bauer", Organisation.DefaultPayablesContactName);
			UPEOrgHeader nullHeader = Factory.GetNull<UPEOrgHeader>();
			AssertEquals("Should return empty string if Null BizO", "", nullHeader.DefaultPayablesContactName);
		}

		public void TestDefaultImportAirFreightAgentContactName()
		{
			OrgContact contact = Organisation.Contacts.AddNew();
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact.OC_ContactName = "John Doe";
			AssertEquals("John Doe", Organisation.DefaultImportAirFreightAgentContactName);
			UPEOrgHeader nullHeader = Factory.GetNull<UPEOrgHeader>();
			AssertEquals("Should return empty string if Null BizO", "", nullHeader.DefaultImportAirFreightAgentContactName);
		}

		public void TestHasUPSAccountClass()
		{
			AssertHasUPSAccountClass("", false);
			AssertHasUPSAccountClass("1", true);
			AssertHasUPSAccountClass("2", true);
			AssertHasUPSAccountClass("3", true);
			AssertHasUPSAccountClass("4", true);
			AssertHasUPSAccountClass("Xo", false);
			AssertHasUPSAccountClass("5", true);
			AssertHasUPSAccountClass("6", true);
			AssertHasUPSAccountClass("7", true);
			AssertHasUPSAccountClass("8", true);
			AssertHasUPSAccountClass("9", true);
			AssertHasUPSAccountClass("10", true);
			AssertHasUPSAccountClass("11", true);
			AssertHasUPSAccountClass("12", true);
			AssertHasUPSAccountClass("13", true);
			AssertHasUPSAccountClass("14", true);
			AssertHasUPSAccountClass("15", true);
		}

		public void TestIsStandardAccount()
		{
			AssertIsStandardAccount("", false);
			AssertIsStandardAccount("1", true);
			AssertIsStandardAccount("2", false);
			AssertIsStandardAccount("3", true);
			AssertIsStandardAccount("4", true);
			AssertIsStandardAccount("5", false);
			AssertIsStandardAccount("6", false);
			AssertIsStandardAccount("7", false);
			AssertIsStandardAccount("8", false);
			AssertIsStandardAccount("9", false);
			AssertIsStandardAccount("10", false);
			AssertIsStandardAccount("11", false);
			AssertIsStandardAccount("12", false);
			AssertIsStandardAccount("13", false);
			AssertIsStandardAccount("14", false);
			AssertIsStandardAccount("15", false);
		}

		public void TestIsPreferredAccount()
		{
			AssertIsPreferredAccount("", false);
			AssertIsPreferredAccount("1", false);
			AssertIsPreferredAccount("2", true);
			AssertIsPreferredAccount("3", false);
			AssertIsPreferredAccount("4", false);
			AssertIsPreferredAccount("5", false);
			AssertIsPreferredAccount("6", false);
			AssertIsPreferredAccount("7", false);
			AssertIsPreferredAccount("8", false);
			AssertIsPreferredAccount("9", false);
			AssertIsPreferredAccount("10", false);
			AssertIsPreferredAccount("11", false);
			AssertIsPreferredAccount("12", false);
			AssertIsPreferredAccount("13", false);
			AssertIsPreferredAccount("14", false);
			AssertIsPreferredAccount("15", false);
		}

		public void TestIsCreditCardAccount()
		{
			AssertIsCreditCardAccount("", false);
			AssertIsCreditCardAccount("1", false);
			AssertIsCreditCardAccount("2", false);
			AssertIsCreditCardAccount("3", false);
			AssertIsCreditCardAccount("4", false);
			AssertIsCreditCardAccount("5", true);
			AssertIsCreditCardAccount("6", true);
			AssertIsCreditCardAccount("7", false);
			AssertIsCreditCardAccount("8", false);
			AssertIsCreditCardAccount("9", false);
			AssertIsCreditCardAccount("10", false);
			AssertIsCreditCardAccount("11", false);
			AssertIsCreditCardAccount("12", false);
			AssertIsCreditCardAccount("13", false);
			AssertIsCreditCardAccount("14", false);
			AssertIsCreditCardAccount("15", false);
		}

		public void TestIsOnFileCODAccount()
		{
			AssertIsOnFileCODAccount("", false);
			AssertIsOnFileCODAccount("1", false);
			AssertIsOnFileCODAccount("2", false);
			AssertIsOnFileCODAccount("3", false);
			AssertIsOnFileCODAccount("4", false);
			AssertIsOnFileCODAccount("5", false);
			AssertIsOnFileCODAccount("6", false);
			AssertIsOnFileCODAccount("7", false);
			AssertIsOnFileCODAccount("8", false);
			AssertIsOnFileCODAccount("9", false);
			AssertIsOnFileCODAccount("10", false);
			AssertIsOnFileCODAccount("11", true);
			AssertIsOnFileCODAccount("12", true);
			AssertIsOnFileCODAccount("13", false);
			AssertIsOnFileCODAccount("14", true);
			AssertIsOnFileCODAccount("15", true);
		}

		public void TestIsARAccount()
		{
			AssertIsARAccount("", false);
			AssertIsARAccount("1", false);
			AssertIsARAccount("2", false);
			AssertIsARAccount("3", false);
			AssertIsARAccount("4", false);
			AssertIsARAccount("5", false);
			AssertIsARAccount("6", false);
			AssertIsARAccount("7", true);
			AssertIsARAccount("8", true);
			AssertIsARAccount("9", true);
			AssertIsARAccount("10", false);
			AssertIsARAccount("11", false);
			AssertIsARAccount("12", false);
			AssertIsARAccount("13", false);
			AssertIsARAccount("14", false);
			AssertIsARAccount("15", false);
		}

		public void TestIsStandardLegalAccount()
		{
			AssertIsStandardLegalAccount("", false);
			AssertIsStandardLegalAccount("1", false);
			AssertIsStandardLegalAccount("2", false);
			AssertIsStandardLegalAccount("3", false);
			AssertIsStandardLegalAccount("4", false);
			AssertIsStandardLegalAccount("5", false);
			AssertIsStandardLegalAccount("6", false);
			AssertIsStandardLegalAccount("7", false);
			AssertIsStandardLegalAccount("8", false);
			AssertIsStandardLegalAccount("9", false);
			AssertIsStandardLegalAccount("10", false);
			AssertIsStandardLegalAccount("11", false);
			AssertIsStandardLegalAccount("12", false);
			AssertIsStandardLegalAccount("13", true);
			AssertIsStandardLegalAccount("14", false);
			AssertIsStandardLegalAccount("15", false);
		}

		public void TestIsCODAccount()
		{
			AssertIsCODAccount("", false);
			AssertIsCODAccount("1", false);
			AssertIsCODAccount("2", false);
			AssertIsCODAccount("3", false);
			AssertIsCODAccount("4", false);
			AssertIsCODAccount("5", false);
			AssertIsCODAccount("6", false);
			AssertIsCODAccount("7", false);
			AssertIsCODAccount("8", false);
			AssertIsCODAccount("9", false);
			AssertIsCODAccount("10", true);
			AssertIsCODAccount("11", false);
			AssertIsCODAccount("12", false);
			AssertIsCODAccount("13", false);
			AssertIsCODAccount("14", false);
			AssertIsCODAccount("15", false);
		}

		public void TestShouldReceiveCommercialInvoice()
		{
			Organisation.ShouldReceiveCommercialInvoice = true;
			Assert(Organisation.ShouldReceiveCommercialInvoice);
			Assert(Organisation.MiscServ.OM_CustomFlag4);
			Organisation.ShouldReceiveCommercialInvoice = false;
			Assert(!Organisation.ShouldReceiveCommercialInvoice);
			Assert(!Organisation.MiscServ.OM_CustomFlag4);
		}

		public void TestMiscServType()
		{
			AssertEquals(typeof(UPEOrgMiscServ), Organisation.MiscServ.GetType());
		}

		void AssertHasUPSAccountClass(ZString accountClass, bool hasUPSAccountClass)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(hasUPSAccountClass, Organisation.HasUPSAccountClass);
		}

		void AssertIsStandardAccount(ZString accountClass, bool isStandardAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isStandardAccount, Organisation.IsStandardAccount);
		}

		void AssertIsPreferredAccount(ZString accountClass, bool isPreferredAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isPreferredAccount, Organisation.IsPreferredAccount);
		}

		void AssertIsCreditCardAccount(ZString accountClass, bool isCreditCardAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isCreditCardAccount, Organisation.IsCreditCardAccount);
		}

		void AssertIsOnFileCODAccount(ZString accountClass, bool isOnFileCODAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isOnFileCODAccount, Organisation.IsOnFileCODAccount);
		}

		void AssertIsARAccount(ZString accountClass, bool isARAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isARAccount, Organisation.IsARAccount);
		}

		void AssertIsStandardLegalAccount(ZString accountClass, bool isStandardLegalAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isStandardLegalAccount, Organisation.IsStandardLegalAccount);
		}

		void AssertIsCODAccount(ZString accountClass, bool isCODAccount)
		{
			SetAccountClassForTest(accountClass);
			AssertEquals(isCODAccount, Organisation.IsCODAccount);
		}

		void SetAccountClassForTest(ZString accountClass)
		{
			if (Organisation.CompanyData.OB_OJ_ARDebtorGroup.IsEmpty)
			{
				Organisation.OH_IsDebtor = true;
				Organisation.CompanyData.OB_OJ_ARDebtorGroup = Factory.New(typeof(OrgDebtorGroup)).PK;
			}

			Organisation.CompanyData.ARDebtorGroup.OJ_Code = accountClass;
		}

		#endregion
		#region Letter of Authority
		public void TestLetterOfAuthorityExpirationDate()
		{
			Organisation.LetterOfAuthorityExpirationDate = new ZDateTime(2005, 1, 2);
			AssertEquals(new ZDateTime(2005, 1, 2), Organisation.LetterOfAuthorityExpirationDate);
			AssertEquals(new ZDateTime(2005, 1, 2), Organisation.MiscServ.OM_CustomDate1);
		}

		public void TestLetterOfAuthorityDaysToExpiration()
		{
			Organisation.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(1);
			AssertEquals("Due in 1 day", 1, Organisation.LetterOfAuthorityDaysToExpiration);
			Organisation.LetterOfAuthorityExpirationDate = ZDateTime.Now.AddDays(-2);
			AssertEquals("Expired 2 days ago", -2, Organisation.LetterOfAuthorityDaysToExpiration);
		}

		public void TestLetterOfAuthorityDocumentDelivered()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPEOrgHeader organisation = Factory.NewWithValidTestData<UPEOrgHeader>();
			Factory.Save();
			AssertLetterOfAuthorityDocumentDelivered("Delivery date not available until document is delivered", organisation, false);
			ZString subject = new UPEDocumentMenuItemLoader(Factory).LoadLetterOfAuthority().SU_EmailSubjectLine;
			organisation.Logs.AddNew(Events.DocumentDelivered, "---" + subject + "---");
			Factory.Save();
			AssertLetterOfAuthorityDocumentDelivered("Delivery date populated when document is delivered", organisation, true);
		}

		public void TestIsLOAReceivedAuthorisingUPStoClearGoods()
		{
			UPEOrgHeader organisation = Factory.NewWithValidTestData<UPEOrgHeader>();
			organisation.MiscServ.OM_CustomAttrib3 = "ABRA-KADABRA";
			Factory.Save();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(false, organisation.IsLOAReceivedAuthorisingUPStoClearGoods);
			organisation.MiscServ.OM_CustomAttrib3 = "Y";
			Factory.Save();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(true, organisation.IsLOAReceivedAuthorisingUPStoClearGoods);
		}

		public void TestDateLOAReceivedAuthorisingUPStoClearGoods()
		{
			UPEOrgHeader organisation = Factory.NewWithValidTestData<UPEOrgHeader>();
			organisation.DateLOAReceivedAuthorisingUPStoClearGoods = new ZDateTime(2007, 7, 7);
			Factory.Save();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(new ZDateTime(2007, 7, 7), organisation.DateLOAReceivedAuthorisingUPStoClearGoods);
		}

		public void TestDateLOAReceivedAuthorisingUPStoClearGoodsIsEarlier7Days()
		{
			UPEOrgHeader organisation = Factory.NewWithValidTestData<UPEOrgHeader>();
			organisation.DateLOAReceivedAuthorisingUPStoClearGoods = ZDateTime.Now.AddDays(-8);
			Factory.Save();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(false, organisation.DateLOAReceivedAuthorisingUPStoClearGoodsIsEarlier7Days);
			organisation.DateLOAReceivedAuthorisingUPStoClearGoods = ZDateTime.Now.AddDays(-7);
			Factory.Save();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(true, organisation.DateLOAReceivedAuthorisingUPStoClearGoodsIsEarlier7Days);
		}

		public void TestHasUncompletedDeclarations()
		{
			var organisation = Factory.NewWithValidTestData<UPEOrgHeader>();
			var importerPK = organisation.PK;
			var declaration1 = Factory.NewWithValidTestData<UPEJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<UPEJobDeclaration>();
			declaration1.JE_OH_Importer = organisation.PK;
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			organisation.ResetJobNumbers();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(1, organisation.UncompletedJobNumbers.Count);
			declaration2.JE_OH_Importer = organisation.PK;
			declaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			declaration2.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			organisation.ResetJobNumbers();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(0, organisation.UncompletedJobNumbers.Count);
			declaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			declaration2.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			((UPEOrgMiscServ)(declaration1.Importer.MiscServ)).LOAReceivedAuthorisingUPStoClearGoods = true;
			Factory.Save();
			organisation.ResetJobNumbers();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(0, organisation.UncompletedJobNumbers.Count);
			declaration1.CurrentQueue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			declaration2.CurrentQueue.P4_CustomsQueue = ZString.Empty;
			((UPEOrgMiscServ)(declaration1.Importer.MiscServ)).LOAReceivedAuthorisingUPStoClearGoods = false;
			Factory.Save();
			organisation.ResetJobNumbers();
			organisation = Factory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(1, organisation.UncompletedJobNumbers.Count);
		}

		void AssertLetterOfAuthorityDocumentDelivered(ZString message, UPEOrgHeader organisation, bool expectedValue)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			UPEOrgHeader reloadedOrganisation = newFactory.Load<UPEOrgHeader>(organisation.PK);
			AssertEquals(message, expectedValue, reloadedOrganisation.LetterOfAuthorityDocumentDelivered.IsValid);
		}

		#endregion
		#region ICustomLabelsProvider
		public void TestGetCustomFields_ForConsignee()
		{
			Organisation.OH_IsConsignee = true;
			SetupPreReleaseFlag();
			CustomLabelInfoList list = Organisation.GetCustomFields(Organisation, Factory);
			AssertEquals("Delivery handled by UPS for this Alternate Broker", list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag1).Caption);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag1).Styles);
			AssertEquals("ITF Chargable", list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag2).Caption);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag2).Styles);
			AssertEquals("High Claimer", list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag3).Caption);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag3).Styles);
			AssertEquals("Letter of Authority Expiration Date", list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomDate1).Caption);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomDate1).Styles);
			AssertEquals("Should Receive Commercial Invoice", list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag4).Caption);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag4).Styles);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName("IsPreReleaseFeeApplicable").Styles);
			AssertEquals("Pre-Release Notification", list.GetFieldByPropertyName("IsPreReleaseFeeApplicable").Caption);
		}

		public void TestGetCustomFields_ForNonConsignee()
		{
			SetupPreReleaseFlag();
			Organisation.OH_IsConsignee = false;
			CustomLabelInfoList list = Organisation.GetCustomFields(Organisation, Factory);
			AssertEquals("Delivery handled by UPS for this Alternate Broker", list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag1).Caption);
			AssertEquals(CustomLabelStyles.ShowByDefault, list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag1).Styles);
			AssertEquals("Should not show consignee fields", true, (list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomFlag2).Styles & CustomLabelStyles.ShowByDefault) == 0);
			AssertEquals("Should not show consignee fields", true, (list.GetFieldByPropertyName(OrgMiscServ.Schema.OM_CustomDate1).Styles & CustomLabelStyles.ShowByDefault) == 0);
			ICustomFieldProvider customFieldProvider = Organisation;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Info.Type.Name == nameof(ZBool));
			AssertNotNull("Pre-Release Notification Flag is setup.", customProperty);
			AssertNull("Pre-Release Notification Flag should not be visible", list.GetFieldByPropertyName("IsPreReleaseFeeApplicable"));
		}

		#endregion
		#region IUPEDocumentSupportable
		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(UPEOrgHeaderDocumentSupporter), Organisation.DocumentSupporter.GetType());
		}

		#endregion
		#region Implementation
		UPEOrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<UPEOrgHeader>();
					fOrganisation.OH_Code = "DummyCode";
				}

				return fOrganisation;
			}

			set
			{
				fOrganisation = value;
			}
		}

		UPEOrgHeader fOrganisation;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		void SetupPreReleaseFlag()
		{
			var testTemplate = Factory.New<ProcessTaskTemplate>();
			testTemplate.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			testTemplate.P0_Name = "test template";
			var preReleaseFlag = testTemplate.GenCustomColumnDefinitions.AddNew();
			preReleaseFlag.XC_Name = UPEOrgHeader.PreReleaseNotificationFieldName;
			preReleaseFlag.XC_Type = AddOnColumnDataType.Codes.Boolean;
			Factory.Save();
			ICustomFieldProvider customFieldProvider = Organisation;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Info.Type.Name == nameof(ZBool));
			AssertContains(UPEOrgHeader.PreReleaseNotificationFieldName, customProperty.Identifier, ignoreCase: true);
			customProperty.TrySetValue(Organisation, ZBool.False);
			Factory.Save();
		}
		#endregion
	}
}
