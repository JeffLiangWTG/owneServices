using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSQuerySendingObjectTest : TestCaseWithFactory
	{
		public void TestGetMenuOptionsWithEntry()
		{
			var sendingObject = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);

			var menuOptions = sendingObject.GetMenuOptions();

			AssertEquals("No menu options", 0, menuOptions.Count);

			CreateEntry(declaration);
			cusEntryHeader1.MovementReferenceNumberSetter("MRN123456789", ZDateTime.Now);

			sendingObject = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);

			menuOptions = sendingObject.GetMenuOptions();

			var entry1 = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			var entry2 = declaration.ActiveEntryHeaders[1] as CusEntryHeader;

			AssertEquals("2 menu options", 2, menuOptions.Count);
			Assert("Menu 1", menuOptions.Any(x => x.Value == $"{entry1.EntryTypeFriendlyName} MRN123456789"));
			Assert("Menu 2", menuOptions.Any(x => x.Value == $"{entry2.EntryTypeFriendlyName} (No MRN)"));
		}

		public void TestGetMenuOptionsWithPrevDoc()
		{
			var sendingObject = new CDSQueryDUCRSendingObject(declaration);

			var menuOptions = sendingObject.GetMenuOptions();

			AssertEquals("No menu options", 0, menuOptions.Count);

			CreateEntry(declaration);

			sendingObject = new CDSQueryDUCRSendingObject(declaration);

			menuOptions = sendingObject.GetMenuOptions();

			var entry1 = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			var entry2 = declaration.ActiveEntryHeaders[1] as CusEntryHeader;

			AssertEquals("2 menu options", 2, menuOptions.Count);
			Assert("Menu 1", menuOptions.Any(x => x.Value == $"{entry1.EntryTypeFriendlyName} UNITTEST/00001"));
			Assert("Menu 2", menuOptions.Any(x => x.Value == $"{entry2.EntryTypeFriendlyName} UNITTEST/00001"));

			var prevDoc2 = declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "DCR";
			prevDoc2.CSI_ReferenceNumber = "UNITTEST/00002";

			_ = declaration.DoMerge();
			sendingObject = new CDSQueryDUCRSendingObject(declaration);

			menuOptions = sendingObject.GetMenuOptions();
			AssertEquals("4 menu options", 4, menuOptions.Count);
			Assert("Menu 1", menuOptions.Any(x => x.Value == $"{entry1.EntryTypeFriendlyName} UNITTEST/00001"));
			Assert("Menu 2", menuOptions.Any(x => x.Value == $"{entry1.EntryTypeFriendlyName} UNITTEST/00002"));
			Assert("Menu 3", menuOptions.Any(x => x.Value == $"{entry2.EntryTypeFriendlyName} UNITTEST/00001"));
			Assert("Menu 4", menuOptions.Any(x => x.Value == $"{entry2.EntryTypeFriendlyName} UNITTEST/00002"));
		}

		public void TestGetMenuOptionsSingleEntryMultipleOptions()
		{
			var sendingObject = new CDSQueryDUCRSendingObject(declaration);

			var doc2 = declaration.PreviousDocuments.AddNew();
			doc2.CSI_Code = "DCR";
			doc2.CSI_ReferenceNumber = "UNITTEST/00002";

			_ = declaration.DoMerge();

			var entry1 = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			var menuOptions = sendingObject.GetMenuOptions();
			AssertEquals("2 menu options", 2, menuOptions.Count);
			Assert("Menu 1", menuOptions.Any(x => x.Value == $"{entry1.EntryTypeFriendlyName} UNITTEST/00001"));
			Assert("Menu 2", menuOptions.Any(x => x.Value == $"{entry1.EntryTypeFriendlyName} UNITTEST/00002"));
		}

		public void TestGetEntryHeader()
		{
			var sendingObject = new CDSQueryDUCRSendingObject(declaration);

			var otherPK = ZGuid.NewZGuid();

			var entry1 = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			AssertEquals("Expect Entry1", entry1, sendingObject.GetEntryHeader(ZGuid.Empty));
			AssertEquals("Expect Entry1", entry1, sendingObject.GetEntryHeader(cusEntryHeader1.PK));
			AssertNull("Not expected", sendingObject.GetEntryHeader(otherPK));

			CreateEntry(declaration);
			sendingObject = new CDSQueryDUCRSendingObject(declaration);
			var entry2 = declaration.ActiveEntryHeaders[1] as CusEntryHeader;
			var sourcePK2 = sendingObject.GetMenuOptions().FirstOrDefault(x => x.Value.Contains(entry2.EntryTypeFriendlyName)).Key;
			AssertEquals("Expect Entry2", entry2, sendingObject.GetEntryHeader(sourcePK2));
		}

		[TestDate(2015, 8, 22)]
		public void TestCanSend()
		{
			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 12, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 22);
			password.IsTokenForCDS = true;

			var notificationCollection = new Customs.Business.MessageSendingNotificationCollection();
			var sendingObjectDUCR = new CDSQueryDUCRSendingObject(declaration);
			var sendingObjectMRN = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);

			AssertEquals("Can send DUCR", expected: true, sendingObjectDUCR.CanSend(ZGuid.Empty, notificationCollection));
			AssertEquals("No notifications", 0, notificationCollection.Count);

			password.Badge = "ABC";
			AssertEquals("Cannot send as no credential exists", expected: false, sendingObjectDUCR.CanSend(ZGuid.Empty, notificationCollection));
			AssertContains("Expect credential error", "No company-level CDS credentials exist", notificationCollection.ErrorNotificationsAsString());

			notificationCollection.Clear();
			password.Badge = declaration.JE_CustomsProfile;
			password.Status = PasswordStatusList.Codes.Valid;

			AssertEquals("Cannot send MRN", expected: false, sendingObjectMRN.CanSend(ZGuid.Empty, notificationCollection));
			AssertEquals("1 notification", 1, notificationCollection.Count);
			AssertEquals("Expect no value error", "This entry does not have a value for its MRN", notificationCollection[0].Message);

			CreateEntry(declaration);
			cusEntryHeader1.MovementReferenceNumberSetter("MRN123456789", ZDateTime.Now);

			sendingObjectDUCR = new CDSQueryDUCRSendingObject(declaration);
			sendingObjectMRN = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);
			notificationCollection.Clear();

			var ducrOptions = sendingObjectDUCR.GetMenuOptions().ToList();
			CombineAssertions("DUCR Options", () =>
			{
				ducrOptions.ForEach(x => AssertEquals(x.Value, expected: true, sendingObjectDUCR.CanSend(x.Key, notificationCollection)));
			});

			var mrnOptions = sendingObjectMRN.GetMenuOptions().ToList();
			CombineAssertions("MRN Options", () =>
			{
				mrnOptions.ForEach(x => AssertEquals(x.Value, x.Value.StartsWith($"{cusEntryHeader1.EntryTypeFriendlyName} "), sendingObjectMRN.CanSend(x.Key, notificationCollection)));
			});
		}

		public void TestGetContextCollection()
		{
			IQueryDataProvider sendingObjectDUCRQDP = new CDSQueryDUCRSendingObject(declaration);
			IQueryDataProvider sendingObjectMRNQDP = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);

			var contextList = sendingObjectDUCRQDP.GetContextCollection(ZGuid.Empty);

			AssertEquals("Should have 4 for DUCR", 4, contextList.Count());
			var ent = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.EntryNumberType);
			var en = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.EntryNumber);
			var nt = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.NotificationType);
			var qs = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.QueryString);

			AssertNotNull("Could not find EntryNumberType", ent);
			AssertNotNull("Could not find EntryNumber", en);
			AssertNotNull("Could not find NotificationType", nt);
			AssertNotNull("Could not find QueryString", qs);

			AssertEquals("Check ENT", CDSDISQueryHelper.Constants.EntryNumberTypes.DUCR, ent.Value);
			AssertEquals("Check EN", prevDoc1.CSI_ReferenceNumber, en.Value);
			AssertEquals("Check NT", CDSDISQueryHelper.Constants.NotificationTypes.Status, nt.Value);

			contextList = sendingObjectMRNQDP.GetContextCollection(ZGuid.Empty);

			AssertEquals("Should have 2 for MRN", 2, contextList.Count());
			ent = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.EntryNumberType);
			en = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.EntryNumber);
			nt = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.NotificationType);
			qs = contextList.FirstOrDefault(x => x.Type == CDSDISQueryHelper.Constants.ContextTypes.QueryString);

			AssertNull("Should not find EntryNumberType", ent);
			AssertNull("Should not find EntryNumber", en);
			AssertNotNull("Could not find NotificationType", nt);
			AssertNotNull("Could not find QueryString", qs);
			AssertEquals("Check NT", CDSDISQueryHelper.Constants.NotificationTypes.Status, nt.Value);

			CreateEntry(declaration);
			cusEntryHeader1.MovementReferenceNumberSetter("MRN123456789", ZDateTime.Now);

			sendingObjectDUCRQDP = new CDSQueryDUCRSendingObject(declaration);
			sendingObjectMRNQDP = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);
			var ucrOptions = (sendingObjectDUCRQDP as CDSQueryDUCRSendingObject).GetMenuOptions().ToList();
			var mrnOptions = (sendingObjectMRNQDP as CDSQueryMRNSendingObject).GetMenuOptions().ToList();

			CombineAssertions("DUCR Options", () =>
			{
				ucrOptions.ForEach(x =>
				{
					contextList = sendingObjectDUCRQDP.GetContextCollection(x.Key);
					AssertEquals(x.Value, 4, contextList.Count());
				});
			});

			CombineAssertions("MRN Options", () =>
			{
				mrnOptions.ForEach(x =>
				{
					contextList = sendingObjectMRNQDP.GetContextCollection(x.Key);
					int count = x.Value.StartsWith($"{cusEntryHeader1.EntryTypeFriendlyName} ") ? 4 : 2;
					AssertEquals(x.Value, count, contextList.Count());
				});
			});
		}

		public void TestIQueryDataProvider()
		{
			IQueryDataProvider sendingObjectUCR = new CDSQueryUCRSendingObject(declaration);

			var contextList = sendingObjectUCR.GetContextCollection(ZGuid.Empty);
			AssertNotNull("GetContextCollection() should not return null", contextList);
			AssertEquals("Should have 4 for DUCR", 4, contextList.Count());

			AssertEquals("DataContextType should be 2 - CustomsDeclaration", DataContextType.CustomsDeclaration, sendingObjectUCR.ContextType);
			AssertEquals("Expecting JobNumber for Reference", declaration.JobNumber, sendingObjectUCR.ContextReference);
			AssertEquals("Credential Key", declaration.GetCredentialsKey(), sendingObjectUCR.CredentialKey);
		}

		[TestDate(2020, 02, 10, 14, 15, 16, 789)]
		public void TestMRNSO()
		{
			var so = new MRNSOHelper(declaration, QueryNotificationType.Status);
			cusEntryHeader1.MovementReferenceNumberSetter("MRN123456789", ZDateTime.Now);

			var results = so.GetEntryNumbers(cusEntryHeader1);
			AssertEquals("1 Item", 1, results.Count);
			var (entryPK, sourcePK, number) = results[0];

			CombineAssertions("MRN SO", () =>
			{
				AssertEquals("EntryNumberType:", "MRN", so.EntryNumberType);
				AssertEquals("EntryPK:", cusEntryHeader1.PK, entryPK);
				AssertEquals("EntryNumber:", "MRN123456789", number);
				AssertEquals("NotificationType:", "status", so.NotificationType);
				AssertEquals("CredentialCore:", "BOBCAT.GB132435465768.FAN", so.GetCredentialKeyCore());
			});
		}

		public void TestMRNSO_Full()
		{
			var so = new MRNSOHelper(declaration, QueryNotificationType.Full);
			cusEntryHeader1.MovementReferenceNumberSetter("MRN123456789", ZDateTime.Now);

			var results = so.GetEntryNumbers(cusEntryHeader1);
			AssertEquals("1 Item", 1, results.Count);
			var res = results[0];

			CombineAssertions("MRN SO", () =>
			{
				AssertEquals("EntryNumberType:", "MRN", so.EntryNumberType);
				AssertEquals("EntryPK:", cusEntryHeader1.PK, res.entryPK);
				AssertEquals("EntryNumber:", "MRN123456789", res.number);
				AssertEquals("NotificationType:", "full", so.NotificationType);
				AssertEquals("CredentialCore:", "BOBCAT.GB132435465768.FAN", so.GetCredentialKeyCore());
			});
		}

		public void TestDUCRSO()
		{
			var so = new DUCRSOHelper(declaration);

			var results = so.GetEntryNumbers(cusEntryHeader1);
			AssertEquals("1 Item", 1, results.Count);
			var (entryPK, sourcePK, number) = results[0];

			CombineAssertions("DUCR SO", () =>
			{
				AssertEquals("EntryNumberType:", "DUCR", so.EntryNumberType);
				AssertEquals("EntryPK:", cusEntryHeader1.PK, entryPK);
				AssertEquals("EntryNumber:", prevDoc1.CSI_ReferenceNumber, number);
				AssertEquals("NotificationType:", "status", so.NotificationType);
				AssertEquals("CredentialCore:", "BOBCAT.GB132435465768.FAN", so.GetCredentialKeyCore());
			});
		}

		public void TestUCRSO()
		{
			var so = new UCRSOHelper(declaration);

			var results = so.GetEntryNumbers(cusEntryHeader1);
			AssertEquals("1 Item", 1, results.Count);
			var (entryPK, sourcePK, number) = results[0];

			CombineAssertions("UCR SO", () =>
			{
				AssertEquals("EntryNumberType:", "UCR", so.EntryNumberType);
				AssertEquals("EntryPK:", cusEntryHeader1.PK, entryPK);
				AssertEquals("EntryNumber:", cusEntryHeader1.CH_BGMReference, number);
				AssertEquals("NotificationType:", "status", so.NotificationType);
				AssertEquals("CredentialCore:", "BOBCAT.GB132435465768.FAN", so.GetCredentialKeyCore());
			});
		}

		public void TestInventorySO()
		{
			var so = new InventorySOHelper(declaration);
			cusEntryHeader1.CH_MasterUCR = "MUCR123456";

			var results = so.GetEntryNumbers(cusEntryHeader1);
			AssertEquals("1 Item", 1, results.Count);
			var (entryPK, sourcePK, number) = results[0];

			CombineAssertions("Inventory/MUCR SO", () =>
			{
				AssertEquals("EntryNumberType:", "INVENTORY-REFERENCE", so.EntryNumberType);
				AssertEquals("EntryPK:", cusEntryHeader1.PK, entryPK);
				AssertEquals("EntryNumber:", cusEntryHeader1.CH_MasterUCR, number);
				AssertEquals("NotificationType:", "status", so.NotificationType);
				AssertEquals("CredentialCore:", "BOBCAT.GB132435465768.FAN", so.GetCredentialKeyCore());
			});
		}

		public void TestListQuerySO()
		{
			var msg = Factory.New<CDSDISQueryMessage>();
			msg.EM_MessageOwner = "GB132435465768.FAN";

			var so = new ListSOHelper(msg);

			var results = so.GetEntryNumbers(cusEntryHeader1);
			AssertEquals("1 Item", 1, results.Count);
			var (entryPK, sourcePK, number) = results[0];

			CombineAssertions("ListQuery SO", () =>
			{
				AssertEquals("EntryNumberType:", ZString.Empty, so.EntryNumberType);
				AssertEquals("EntryPK:", ZGuid.Empty, entryPK);
				AssertEquals("EntryNumber:", ZString.Empty, number);
				AssertEquals("NotificationType:", "list", so.NotificationType);
				AssertEquals("CredentialCore:", $"{GBExtensions.GetEnterpriseCode()}.GB132435465768.FAN", so.GetCredentialKeyCore());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "BOB";
			registrationKey.ServerCodeForTest = "CAT";

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_CustomsProfile = "FAN";
			_ = declaration.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB132435465768");

			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "H1";

			var inv1 = declaration.Invoices.AddNew();
			var line1 = inv1.InvoiceLines.AddNew();
			line1.JI_CEI = cei1.PK;

			prevDoc1 = declaration.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "DCR";
			prevDoc1.CSI_ReferenceNumber = "UNITTEST/00001";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			_ = declaration.DoMerge();
			Factory.Save();

			cusEntryHeader1 = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
		}

		void CreateEntry(JobDeclaration dec)
		{
			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "H2";

			var inv2 = dec.Invoices.AddNew();
			var line2 = inv2.InvoiceLines.AddNew();
			line2.JI_CEI = cei2.PK;

			_ = dec.DoMerge();
			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader1;
		PreviousDocument prevDoc1;

		class MRNSOHelper : CDSQueryMRNSendingObject
		{
			public MRNSOHelper(JobDeclaration jobDeclaration, QueryNotificationType queryNotificationType)
				: base(jobDeclaration, queryNotificationType) { }
			public new ZString EntryNumberType => base.EntryNumberType;
			public new List<(ZGuid entryPK, ZGuid sourcePK, ZString number)> GetEntryNumbers(CusEntryHeader entry) => base.GetEntryNumbers(entry).Select(x => (x.EntryPK, x.SourcePK, x.Number)).ToList();
			public new ZString NotificationType => base.NotificationType;
			public new ZString GetCredentialKeyCore() => base.GetCredentialKeyCore();
		}
		class DUCRSOHelper : CDSQueryDUCRSendingObject
		{
			public DUCRSOHelper(JobDeclaration jobDeclaration) : base(jobDeclaration) { }
			public new ZString EntryNumberType => base.EntryNumberType;
			public new List<(ZGuid entryPK, ZGuid sourcePK, ZString number)> GetEntryNumbers(CusEntryHeader entry) => base.GetEntryNumbers(entry).Select(x => (x.EntryPK, x.SourcePK, x.Number)).ToList();
			public new ZString NotificationType => base.NotificationType;
			public new ZString GetCredentialKeyCore() => base.GetCredentialKeyCore();
		}
		class UCRSOHelper : CDSQueryUCRSendingObject
		{
			public UCRSOHelper(JobDeclaration jobDeclaration) : base(jobDeclaration) { }
			public new ZString EntryNumberType => base.EntryNumberType;
			public new List<(ZGuid entryPK, ZGuid sourcePK, ZString number)> GetEntryNumbers(CusEntryHeader entry) => base.GetEntryNumbers(entry).Select(x => (x.EntryPK, x.SourcePK, x.Number)).ToList();
			public new ZString NotificationType => base.NotificationType;
			public new ZString GetCredentialKeyCore() => base.GetCredentialKeyCore();
		}
		class InventorySOHelper : CDSQueryInventorySendingObject
		{
			public InventorySOHelper(JobDeclaration jobDeclaration) : base(jobDeclaration) { }
			public new ZString EntryNumberType => base.EntryNumberType;
			public new List<(ZGuid entryPK, ZGuid sourcePK, ZString number)> GetEntryNumbers(CusEntryHeader entry) => base.GetEntryNumbers(entry).Select(x => (x.EntryPK, x.SourcePK, x.Number)).ToList();
			public new ZString NotificationType => base.NotificationType;
			public new ZString GetCredentialKeyCore() => base.GetCredentialKeyCore();
		}
		class ListSOHelper : CDSQueryListSendingObject
		{
			public ListSOHelper(CDSDISQueryMessage queryMessage) : base(queryMessage) { }
			public new ZString EntryNumberType => base.EntryNumberType;
			public new List<(ZGuid entryPK, ZGuid sourcePK, ZString number)> GetEntryNumbers(CusEntryHeader entry) => base.GetEntryNumbers(entry).Select(x => (x.EntryPK, x.SourcePK, x.Number)).ToList();
			public new ZString NotificationType => base.NotificationType;
			public new ZString GetCredentialKeyCore() => base.GetCredentialKeyCore();
		}
	}
}
