using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class ExitControlDocRequestMenuCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var exitControlDocRequestMenuItem = new ExitControlDocRequestMenuCreator(Factory.New<CusExitHeader>()).Create();
			AssertEquals("Download EAL Clearance Document - Text", "Download EAL Clearance Document", exitControlDocRequestMenuItem.Text);
		}

		public void TestDownloadEAL_Click_Validations()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = ZString.Empty;
			exitHeader.CXH_CustomsProfile = ZString.Empty;

			Factory.Save();

			var exitControlDocRequestMenuItem = new ExitControlDocRequestMenuCreator(exitHeader).Create();

			CombineAssertions(() =>
			{
				exitControlDocRequestMenuItem.PerformClick();
				AssertEquals("Message informing no exit reports", "No reports exist – Please add exit reports before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = "AAA";
				var report1 = exitHeader.CusExitReports.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;
				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_MovementReference = "BBB";
				var report2 = exitHeader.CusExitReports.AddNew();
				report2.CER_CXC_Consignment = consignment2.PK;

				Factory.Save();

				exitControlDocRequestMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = ZString.Empty;
				exitControlDocRequestMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = "INVALID";
				exitControlDocRequestMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = CertificateName;
				exitControlDocRequestMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				exitHeader.Reload();
				exitControlDocRequestMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDownloadEAL_Click_NoDocumentNeeded()
		{
			var mrnCode = "AAA";

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
			exitHeader.CXH_CustomsProfile = CertificateName;

			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = mrnCode;
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_CXC_Consignment = consignment1.PK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var docManagerInfo = ((IDocManagerSupport)report1).DocManagerInfo;
				docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_EAL_CLR.pdf", "CLR");
				docManagerInfo.Save();

				var exitControlDocRequestMenuItem = new ExitControlDocRequestMenuCreator(exitHeader).Create();

				var newEntryNumber = CusEntryNumber.LoadOrCreate(report1, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = "ABCDEFGHIJKLMNOP";
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				exitHeader.Factory.Save();

				exitControlDocRequestMenuItem.PerformClick();
				AssertContains("All Documents for AEAT already exist for the exit detail so nothing will be sent", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDownloadEAL_Click_OneDeclaration_AllDocumentsNeeded()
		{
			var mrnCode = "AAA";

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
			exitHeader.CXH_CustomsProfile = CertificateName;

			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_CXC_Consignment = consignment1.PK;

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var exitControlDocRequestMenuItem = new ExitControlDocRequestMenuCreator(exitHeader).Create();

				consignment1.CXC_MovementReference = ZString.Empty;
				exitHeader.Factory.Save();
				exitControlDocRequestMenuItem.PerformClick();
				AssertContains("EAL Document Capture Request can not be sent when there is no MRN so nothing will be sent and no message will be shown", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

				consignment1.CXC_MovementReference = mrnCode;
				exitHeader.Factory.Save();
				exitControlDocRequestMenuItem.PerformClick();
				AssertContains("EAL Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent and no message will be shown", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(report1, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = "ABCDEFGHIJKLMNOP";
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				exitHeader.Factory.Save();

				CheckFactoryHasNoPendingChanges("Before sending inbox notification request");
				exitControlDocRequestMenuItem.PerformClick();
				CheckFactoryHasNoPendingChanges("After sending inbox notification request");
				AssertContains("New document request created", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDownloadEAL_Click_MultipleDeclarations_AllDocumentsNeeded()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
			exitHeader.CXH_CustomsProfile = CertificateName;

			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "AAA";
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_CXC_Consignment = consignment1.PK;
			var newEntryNumber = CusEntryNumber.LoadOrCreate(report1, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "ABCDEFGHIJKLMNOP";

			newEntryNumber.CE_EntryIsSystemGenerated = true;
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "BBB";
			var report2 = exitHeader.CusExitReports.AddNew();
			report2.CER_CXC_Consignment = consignment2.PK;
			var newEntryNumber2 = CusEntryNumber.LoadOrCreate(report2, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
			newEntryNumber2.CE_EntryNum = "BCDEFGHIJKLMNOPQ";
			newEntryNumber2.CE_EntryIsSystemGenerated = true;

			exitHeader.Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var exitControlDocRequestMenuItem = new ExitControlDocRequestMenuCreator(exitHeader).Create();

				CheckFactoryHasNoPendingChanges("Before sending inbox notification request");
				exitControlDocRequestMenuItem.PerformClick();
				CheckFactoryHasNoPendingChanges("After sending inbox notification request");
				AssertContains("New document request created for each report", "2 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void CheckFactoryHasNoPendingChanges(ZString messagePrefix)
		{
			var mainFactoryChangeSet = Factory.GetChanges();

			bool mainFactoryHasChanges =
				mainFactoryChangeSet.GetChangedObjects().Any()
				|| mainFactoryChangeSet.GetAddedObjects().Any();

			AssertEquals(messagePrefix + " [All changes should be made in the sending factory] Does Main Factory have changes?", false, mainFactoryHasChanges);
		}

		public GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.New<GlbStaff>();
					staff.GS_Code = "AH";
					staff.GS_LoginName = "ahtest";
					var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
					var cert = wrapper.ESBPasswordCollection.AddNew();
					cert.GP_Name = CertificateName;
					cert.GP_MailBoxID = "Test";
					cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
					cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				}

				return staff;
			}
		}
		GlbStaff staff;

		const string CertificateName = "TestCert1";
	}
}
