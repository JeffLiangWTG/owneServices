using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B3AutoSenderTest : TestCaseWithFactory
	{
		public void TestNotification()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			company.GC_OH_OrgProxy = org.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AAA";
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "T1";
			staff1.GS_EmailAddress = "t1@wisetechglobal.com";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "T2";
			staff2.GS_EmailAddress = "t2@wisetechglobal.com";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_DeclarationReference = "S00000001";
				var cusEntryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_DeclarationReference = "S00000002";
				declaration2.JE_SystemCreateUser = staff1.GS_Code;
				var cusEntryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_DeclarationReference = "S00000003";
				declaration3.JE_SystemCreateUser = staff2.GS_Code;
				var cusEntryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				Factory.Save();

				var cusEntryHeadersForTesting = new CusEntryHeaderForTesting[]
				{
					new CusEntryHeaderForTesting() { Declaration = declaration1, CusEntryHeader = cusEntryHeader1, NeedToSendB3MessageReturn = false },
					new CusEntryHeaderForTesting() { Declaration = declaration2, CusEntryHeader = cusEntryHeader2, NeedToSendB3MessageReturn = true, SendB3MessageReturn = false, SendB3MessageErrorMessage = "Send Entry Message Failed" },
					new CusEntryHeaderForTesting() { Declaration = declaration3, CusEntryHeader = cusEntryHeader3, NeedToSendB3MessageReturn = true, SendB3MessageReturn = true }
				};

				var logger = new DummyLogger();
				B3AutoSenderForTesting sender = new B3AutoSenderForTesting(logger, cusEntryHeadersForTesting);
				sender.Process();

				var expectedLogs = new string[]
				{
					"Information - CAD Auto Sending executing for Branch: AAA.",
					"Warning - Failed to automatically send CAD for Declaration S00000002 due to Send Entry Message Failed.",
					"Information - CAD Auto Sending for Declartion: Declaration S00000003 succeeded."
				};

				foreach (var log in logger)
				{
					AssertCollectionContains("CAD Auto Sending Log", log, expectedLogs);
				}

				var email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Recipients.Contains("t1@wisetechglobal.com"));
				AssertEquals("CAD Auto Sending Warning should be created", "CAD Auto Sending Warning", email1.Subject);
				AssertContains("Body", "Send Entry Message Failed", email1.Body);

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(MailDBItemsSchema.MI_Subject, "CAD Auto Sending Warning");
				query.AddToFilter(MailDBItemsSchema.MI_Body, SQLComparisonOperator.Contains, declaration2.HumanReadableName);
				var loadedEmails = newFactory.Load<MailItem>(query);
				AssertEquals(1, loadedEmails.Length);
				var loadedEmail = loadedEmails[0];
				AssertContains("t1@wisetechglobal.com", loadedEmail.AllRecipients);
			}
		}

		public void TestNoMessageIsGeneratedWhenServiceProviderInterfaceIsConfiguredInRegistry()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var entryHeader = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "", "");
				Factory.Save();

				var cusEntryHeadersForTesting = new CusEntryHeaderForTesting[] { new CusEntryHeaderForTesting() { Declaration = entryHeader.Declaration, CusEntryHeader = entryHeader } };
				var logger = new DummyLogger();
				var sender = new B3AutoSenderForTesting(logger, cusEntryHeadersForTesting);
				sender.Process();
				AssertContains("because this job is configured to submit through a designated service provider interface", string.Join("\r\n", logger));
			}
		}

		public void TestGetCandidateCusEntryHeaders()
		{
			var entryPK1 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "", "").PK;
			var entryPK2 = CreateCusEntryHeader("LVS", "MSI", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "", "").PK;
			var entryPK3 = CreateCusEntryHeader("LVS", "F", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "", "").PK;
			var entryPK4 = CreateCusEntryHeader("LVS", "MSI", "SNT", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "", "").PK;
			var entryPK5 = CreateCusEntryHeader("LVS", "MSI", "", ZDateTime.Now.AddMonths(-4), true, ZDateTime.Empty, "B3C", "", "").PK;
			var entryPK6 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, false, ZDateTime.Empty, "B3C", "", "").PK;
			var entryPK7 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Now, "B3C", "", "").PK;
			var entryPK8 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "REL", "", "").PK;
			var entryPK9 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "CLR", "").PK;
			var entryPK10 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "CNF", "").PK;
			var entryPK11 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "B3C", "", "AWO").PK;
			var entryPK12 = CreateCusEntryHeader("IMP", "AB", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var entryPK13 = CreateCusEntryHeader("LVS", "MSI", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var entryPK14 = CreateCusEntryHeader("LVS", "MSI", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "39", "").PK;

			var cadEntryPK1 = CreateCusEntryHeader("IMP", "10-1", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK2 = CreateCusEntryHeader("IMP", "10-2", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK3 = CreateCusEntryHeader("IMP", "13-1", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK4 = CreateCusEntryHeader("IMP", "13-2", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK5 = CreateCusEntryHeader("IMP", "20-1", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK6 = CreateCusEntryHeader("IMP", "21-1", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK7 = CreateCusEntryHeader("IMP", "21-2", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK8 = CreateCusEntryHeader("IMP", "21-3", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK9 = CreateCusEntryHeader("IMP", "21-4", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK10 = CreateCusEntryHeader("IMP", "21-5", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK11 = CreateCusEntryHeader("IMP", "21-6", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK12 = CreateCusEntryHeader("IMP", "30-1", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			var cadEntryPK13 = CreateCusEntryHeader("IMP", "30-2", "", ZDateTime.Now, true, ZDateTime.Empty, "CAD", "", "").PK;
			Factory.Save();

			var logger = new DummyLogger();
			var sender = new B3AutoSenderForTesting(logger);
			var entryHeaderPKs = sender.GetCandidateCusEntryHeaders_Exposed().Select(bo => new ZGuid(bo[CusEntryHeaderSchema.Constants.PK]));
			CombineAssertions(() =>
			{
				Assert("entry1 should be included", entryHeaderPKs.Contains(entryPK1));
				Assert("entry2 should be included", entryHeaderPKs.Contains(entryPK2));
				Assert("entry3 should NOT be included", !entryHeaderPKs.Contains(entryPK3));
				Assert("entry4 should NOT be included", !entryHeaderPKs.Contains(entryPK4));
				Assert("entry5 should NOT be included", !entryHeaderPKs.Contains(entryPK5));
				Assert("entry6 should NOT be included", !entryHeaderPKs.Contains(entryPK6));
				Assert("entry7 should NOT be included", !entryHeaderPKs.Contains(entryPK7));
				Assert("entry8 should NOT be included", !entryHeaderPKs.Contains(entryPK8));
				Assert("entry9 should NOT be included", !entryHeaderPKs.Contains(entryPK9));
				Assert("entry10 should NOT be included", !entryHeaderPKs.Contains(entryPK10));
				Assert("entry11 should NOT be included", !entryHeaderPKs.Contains(entryPK11));
				Assert("entry12 should be included", entryHeaderPKs.Contains(entryPK12));
				Assert("entry13 should be included", entryHeaderPKs.Contains(entryPK13));
				Assert("entry14 should NOT be included", !entryHeaderPKs.Contains(entryPK14));

				Assert("cadEntry1 should be included", entryHeaderPKs.Contains(cadEntryPK1));
				Assert("cadEntry2 should be included", entryHeaderPKs.Contains(cadEntryPK2));
				Assert("cadEntry3 should be included", entryHeaderPKs.Contains(cadEntryPK3));
				Assert("cadEntry4 should be included", entryHeaderPKs.Contains(cadEntryPK4));
				Assert("cadEntry5 should be included", entryHeaderPKs.Contains(cadEntryPK5));
				Assert("cadEntry6 should be included", entryHeaderPKs.Contains(cadEntryPK6));
				Assert("cadEntry7 should be included", entryHeaderPKs.Contains(cadEntryPK7));
				Assert("cadEntry8 should be included", entryHeaderPKs.Contains(cadEntryPK8));
				Assert("cadEntry9 should be included", entryHeaderPKs.Contains(cadEntryPK9));
				Assert("cadEntry10 should be included", entryHeaderPKs.Contains(cadEntryPK10));
				Assert("cadEntry11 should be included", entryHeaderPKs.Contains(cadEntryPK11));
				Assert("cadEntry12 should be included", entryHeaderPKs.Contains(cadEntryPK12));
				Assert("cadEntry13 should be included", entryHeaderPKs.Contains(cadEntryPK13));
			});
		}

		CusEntryHeader CreateCusEntryHeader(ZString declarationMessageType, ZString declarationMessageSubType, ZString declarationMessageStatus,
			ZDateTime entryAuthorisationDate, ZBool b3AutoSend, ZDateTime k84AccountingDate,
			ZString entryMessageType, ZString entryStatus, ZString status)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = declarationMessageType;
			declaration.JE_MessageSubType = declarationMessageSubType;
			declaration.JE_MessageStatus = declarationMessageStatus;
			declaration.JE_EntryAuthorisationDate = entryAuthorisationDate;
			declaration.CA_B3AutoSend = b3AutoSend;
			declaration.CA_K84AccountingDate = k84AccountingDate;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_MessageType = entryMessageType;
			cusEntryHeader.CH_EntryStatus = entryStatus;
			cusEntryHeader.CH_Status = status;

			return cusEntryHeader;
		}

		#region Implementation

		class B3AutoSenderForTesting : B3AutoSender
		{
			public B3AutoSenderForTesting(ILogger serviceLogger)
				: base(serviceLogger)
			{
			}

			public B3AutoSenderForTesting(ILogger serviceLogger, CusEntryHeaderForTesting[] declarations)
				: base(serviceLogger)
			{
				declarationsForTestingDict = new Dictionary<ZGuid, CusEntryHeaderForTesting>();
				foreach (CusEntryHeaderForTesting declarationForTesting in declarations)
				{
					declarationsForTestingDict.Add(declarationForTesting.Declaration.PK, declarationForTesting);
				}
			}

			readonly Dictionary<ZGuid, CusEntryHeaderForTesting> declarationsForTestingDict;

			protected override DynamicBusinessObjectCollection GetCandidateCusEntryHeaders()
			{
				if (declarationsForTestingDict != null)
				{
					var sql = ZString.Format("SELECT CH_PK FROM dbo.CusEntryHeader INNER JOIN dbo.JobDeclaration ON JE_PK=CH_JE WHERE JE_PK IN ('{0}')",
						string.Join("','", declarationsForTestingDict.Keys));
					var factory = new BusinessObjectFactory();
					var cusEntryHeaderPKs = new DynamicBusinessObjectCollection(factory);
					cusEntryHeaderPKs.Load(sql);
					return cusEntryHeaderPKs;
				}

				return base.GetCandidateCusEntryHeaders();
			}

			public DynamicBusinessObjectCollection GetCandidateCusEntryHeaders_Exposed()
			{
				return GetCandidateCusEntryHeaders();
			}

			protected override bool NeedToSendB3Message(JobDeclaration declaration)
			{
				return declarationsForTestingDict[declaration.PK].NeedToSendB3MessageReturn;
			}

			protected override bool SendMessage(JobDeclaration declaration, CusEntryHeader cusEntryheader)
			{
				var declarationsForTesting = declarationsForTestingDict[declaration.PK];
				if (!string.IsNullOrEmpty(declarationsForTesting.SendB3MessageErrorMessage))
				{
					GetUserNotification(declaration).ShowError(declarationsForTesting.SendB3MessageErrorMessage, "Send Entry Message Error");
				}
				return declarationsForTesting.SendB3MessageReturn;
			}
		}

		class CusEntryHeaderForTesting
		{
			public JobDeclaration Declaration;
			public CusEntryHeader CusEntryHeader;
			public bool NeedToSendB3MessageReturn = true;
			public bool SendB3MessageReturn = true;
			public string SendB3MessageErrorMessage = string.Empty;
		}

		#endregion
	}
}
