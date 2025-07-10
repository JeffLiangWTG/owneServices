using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(CusExitControlHeader))]
	class CusExitControlHeaderTest : EU.Business.Testing.CusExitControlHeaderTest
	{
		public void TestLookups()
		{
			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			AssertType<CusExitControlHeaderLookups>(exitHeader.Lookups);
		}

		public void TestValidation()
		{
			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			AssertType<CusExitControlHeaderValidation>(exitHeader.Validation);
		}

		#region ICustomsFileParent

		public void TestICustomsFileParent_GetDeclarationTypeFromInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			exitHeader.CEH_ParentID = declaration.PK;

			((ICustomsFileParent)declaration).DeclarationTypeInfo.SetValueFromString("AAA");

			AssertEquals("Should get value from the value of Declaration.", "AAA", ((ICustomsFileParent)exitHeader).DeclarationType);
		}

		public void TestICustomsFileParent_DeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "AAA";

			var shipment = Factory.New<ForwardingShipment>();

			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitControlHeader>();
				AssertEquals("When parent is null, should get empty string returned", ZString.Empty, ((ICustomsFileParent)exitHeader).DeclarationType);

				exitHeader = (CusExitControlHeader)GetNewBusinessObject();
				exitHeader.CEH_ParentID = declaration.PK;
				AssertEquals("When parent is Declaration, should get value from JE_MessageType", "AAA", ((ICustomsFileParent)exitHeader).DeclarationType);

				exitHeader.CEH_ParentID = shipment.PK;
				exitHeader.CEH_ParentTableCode = "JS";
				AssertEquals("When parent is Shipment and it is not export, should get empty string returned", ZString.Empty, ((ICustomsFileParent)exitHeader).DeclarationType);

				shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				shipment.JS_RL_NKDestination = "MYPKG";
				AssertEquals("When parent is Shipment and it is export, should get EXP returned", "EXP", ((ICustomsFileParent)exitHeader).DeclarationType);
			});
		}

		public void TestICustomsFileParent_DeclarationTypeInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			var shipment = Factory.New<ForwardingShipment>();

			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitControlHeader>();
				AssertType<ZPropertyInfoString>("When parent is null, should get a ZPropertyInfoString returned", ((ICustomsFileParent)exitHeader).DeclarationTypeInfo);

				exitHeader = (CusExitControlHeader)GetNewBusinessObject();
				exitHeader.CEH_ParentID = declaration.PK;
				AssertEquals("When parent is Declaration, should get JE_MessageTypeInfo returned", declaration.JE_MessageTypeInfo, ((ICustomsFileParent)exitHeader).DeclarationTypeInfo);

				exitHeader.CEH_ParentID = shipment.PK;
				exitHeader.CEH_ParentTableCode = "JS";
				AssertType<ZPropertyInfoString>("When parent is Shipment, should get a ZPropertyInfoString returned", ((ICustomsFileParent)exitHeader).DeclarationTypeInfo);
			});
		}

		public void TestICustomsFileParent_BranchPk()
		{
			var company = Factory.New<GlbCompany>();
			var declarationBranch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = declarationBranch.PK;

			var shipment = Factory.New<ForwardingShipment>();

			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitControlHeader>();
				AssertEquals("When parent is null, should get currentBranch pk", GlbBranch.CurrentBranch.PK, ((ICustomsFileParent)exitHeader).BranchPk);

				exitHeader = (CusExitControlHeader)GetNewBusinessObject();
				exitHeader.CEH_ParentID = declaration.PK;
				AssertEquals("When parent is Declaration, should get Declaration's branch pk", declarationBranch.PK, ((ICustomsFileParent)exitHeader).BranchPk);

				exitHeader.CEH_ParentID = shipment.PK;
				exitHeader.CEH_ParentTableCode = "JS";
				AssertEquals("When parent is Shipment, should get currentBranch pk", GlbBranch.CurrentBranch.PK, ((ICustomsFileParent)exitHeader).BranchPk);
			});
		}

		public void TestICustomsFileParent_IsLocked()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			exitHeader.CEH_ParentID = declaration.PK;
			var log = exitHeader.Logs.AddNew(AutoEvents.UnlockForEdit);

			AssertEquals("Should not be locked as there is no active LCK log.", false, ((ICustomsFileParent)exitHeader).IsLocked);

			log = exitHeader.Logs.AddNew(AutoEvents.LockForEdit);
			AssertEquals("Should be locked as there is an active LCK log.", true, ((ICustomsFileParent)exitHeader).IsLocked);

			log.Cancel();
			AssertEquals("Should not be locked as there is no active LCK log.", false, ((ICustomsFileParent)exitHeader).IsLocked);
		}

		public void TestICustomsFileParent_LockFile()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			exitHeader.CEH_ParentID = declaration.PK;

			var log1 = exitHeader.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = exitHeader.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = exitHeader.Logs.AddNew(AutoEvents.Attached);

			var fileParent = (ICustomsFileParent)exitHeader;
			fileParent.LockFile(string.Empty);

			Assert("Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);
		}

		public void TestICustomsFileParent_UnlockFile()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			exitHeader.CEH_ParentID = declaration.PK;

			var log1 = exitHeader.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = exitHeader.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = exitHeader.Logs.AddNew(AutoEvents.Attached);

			((ICustomsFileParent)exitHeader).UnlockFile(string.Empty);

			Assert("Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);

			var newLog = exitHeader.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNotNull("Should add a new UCK event.", newLog);
		}

		#endregion

		public void TestLockExitHeader()
		{
			CombineAssertions(() =>
			{
				var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
				var exitDetail1 = exitHeader.CusExitDetails.AddNew();
				exitDetail1.CED_Status = MessageStatusList.Codes.AwaitingResponse;
				exitDetail1.ReadOnly = false;
				var exitDetail2 = exitHeader.CusExitDetails.AddNew();

				AssertEquals("Prereq: exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("Prereq: exitDetail1 is not readonly", false, exitDetail1.ReadOnly);
				AssertEquals("Prereq: exitDetail2 is not readonly", false, exitDetail2.ReadOnly);

				exitHeader.LockExitHeader(string.Empty);

				AssertEquals("exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("exitDetail1 is readonly", true, exitDetail1.ReadOnly);
				AssertEquals("exitDetail2 is not readonly", false, exitDetail2.ReadOnly);
			});
		}

		public void TestUnlockExitHeader()
		{
			CombineAssertions(() =>
			{
				var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
				var exitDetail1 = exitHeader.CusExitDetails.AddNew();
				var exitDetail2 = exitHeader.CusExitDetails.AddNew();
				var exitDetail3 = exitHeader.CusExitDetails.AddNew();
				var exitItem1 = exitDetail1.CusExitItems.AddNew();

				exitHeader.LockExitHeader(string.Empty);
				exitDetail1.CED_Status = MessageStatusList.Codes.AwaitingResponse;
				exitDetail2.CED_Status = MessageProcessorConstants.EntryStatusCodes.Cleared;
				exitDetail3.CED_Status = MessageProcessorConstants.EntryStatusCodes.CustomsDeclarationAccepted;

				exitHeader.UnlockExitHeader(string.Empty);

				AssertEquals("exitHeader is unlocked", false, exitHeader.ReadOnly);

				AssertEquals("exitDetail1 is unlocked when status AWR", false, exitDetail1.ReadOnly);
				AssertEquals("exitItem1 is unlocked when status AWR", false, exitItem1.ReadOnly);

				AssertEquals("exitDetail2 is unlocked when status CLR", false, exitDetail2.ReadOnly);
				AssertEquals("exitDetail3 is unlocked when status CDA", false, exitDetail3.ReadOnly);
			});
		}

		public void TestCodesForUnlocking()
		{
			var exitHeader = (CusExitControlHeader)GetNewBusinessObject();
			AssertEquals("CodesForUnlocking contains AWR", true, exitHeader.CodesForUnlocking.Contains(MessageStatusList.Codes.AwaitingResponse));
			AssertEquals("CodesForUnlocking contains CLR", true, exitHeader.CodesForUnlocking.Contains(MessageProcessorConstants.EntryStatusCodes.Cleared));
			AssertEquals("CodesForUnlocking contains CDA", true, exitHeader.CodesForUnlocking.Contains(MessageProcessorConstants.EntryStatusCodes.CustomsDeclarationAccepted));
		}

		public void TestCEH_CustomsProfileDefaulted()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert2";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "AA";
			staff2.GS_LoginName = "aatest";
			var wrapper2 = GlbStaffWrapper.Get(staff2);
			cert = wrapper2.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert3";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			cert = wrapper2.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert4";
			cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AZ";
			staff3.GS_LoginName = "aztest";

			Factory.Save();

			var exitHeader = Factory.New<CusExitControlHeader>();

			CombineAssertions(() =>
			{
				exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("CEH_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, exitHeader.CEH_CustomsProfile);

				exitHeader.CEH_GS_NKCustomsAgent = staff2.GS_Code;
				AssertEquals("CEH_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", exitHeader.CEH_CustomsProfile);

				exitHeader.CEH_GS_NKCustomsAgent = staff3.GS_Code;
				AssertEquals("CEH_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, exitHeader.CEH_CustomsProfile);

				exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
				exitHeader.CEH_CustomsProfile = "TestCert2";
				AssertEquals("CEH_CustomsProfile has value when broker not empty", "TestCert2", exitHeader.CEH_CustomsProfile);

				exitHeader.CEH_GS_NKCustomsAgent = ZString.Empty;
				AssertEquals("CEH_CustomsProfile has been cleared when broker is empty", ZString.Empty, exitHeader.CEH_CustomsProfile);

				exitHeader.CEH_CustomsProfile = "TESTCERT1";
				exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("CEH_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", exitHeader.CEH_CustomsProfile);

				exitHeader.CEH_GS_NKCustomsAgent = ZString.Empty;
				exitHeader.CEH_CustomsProfile = "TestCert2";
				exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("CEH_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, exitHeader.CEH_CustomsProfile);
			});
		}

		public void TestDeclEmailAddrValue()
		{
			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";

			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					var exitHeader = Factory.New<CusExitControlHeader>();
					AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, exitHeader.DeclEmailAddr);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					var exitHeader = Factory.New<CusExitControlHeader>();
					AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, exitHeader.DeclEmailAddr);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					var exitHeader = Factory.New<CusExitControlHeader>();
					AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, exitHeader.DeclEmailAddr);
				}
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();

			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ReferenceNumber = "Test";
			exitHeader.CEH_ArrivalNotificationDate = ZDate.Today;
			return exitHeader;
		}

		public void TestTypeSafe()
		{
			var exitHeader = GetNewBusinessObject() as CusExitControlHeader;
			AssertType<CusExitDetailCollection>(exitHeader.CusExitDetails);
		}

		protected override ZString? NoVariableDefaultDataGroupingCodeCountry => Core.Constants.CountryCodes.Spain;
	}
}
