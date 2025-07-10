using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeader))]
	sealed class CusExitHeaderTest : EU.ExitControl.Business.Testing.CusExitHeaderAbstractTest<CusExitHeader>
	{
		public void TestCusExitReports()
		{
			AssertType<CusExitReportCollection<CusExitReport>>(exitHeader.CusExitReports);
		}

		public void TestCusExitConsignments()
		{
			AssertType<CusExitConsignmentCollection<CusExitConsignment>>(exitHeader.CusExitConsignments);
		}

		public void TestCusExitContainers()
		{
			AssertType<CusExitContainerCollection<CusExitContainer>>(exitHeader.CusExitContainers);
		}

		public void TestLookups()
		{
			AssertType<CusExitHeaderValidation>(exitHeader.Validation);
		}

		public void TestValidation()
		{
			AssertType<CusExitHeaderValidation>(exitHeader.Validation);
		}

		public void TestDefaultBrokerOnSaving()
		{
			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = "AH3";
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = true;

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					var exitHeader = Factory.New<CusExitHeader>();
					Factory.Save();
					AssertNull("Do not default BM_GS_NKCusAgent if current user is not Broker", exitHeader.CustomsAgent);

					staffCurrentUser.GS_IsSystemAccount = false;
					var exitHeader2 = Factory.New<CusExitHeader>();
					Factory.Save();
					AssertEquals("Default BM_GS_NKCusAgent if current user is Broker and CusAgent is empty", GlbStaff.CurrentUser.GS_Code, exitHeader2.CXH_GS_NKCustomsAgent);

					var staff = Factory.New<GlbStaff>();
					staff.GS_Code = "AAA";
					staff.GS_LoginName = "AAA User";
					var exitHeader3 = Factory.New<CusExitHeader>();
					exitHeader3.CXH_GS_NKCustomsAgent = staff.GS_Code;
					Factory.Save();
					AssertEquals("BM_GS_NKCusAgent stays as is if it is not empty", staff, exitHeader3.CustomsAgent);
				});
			}
		}

		public void TestCXH_CustomsProfileDefaulted()
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

			CombineAssertions(() =>
			{
				exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("CXH_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, exitHeader.CXH_CustomsProfile);

				exitHeader.CXH_GS_NKCustomsAgent = staff2.GS_Code;
				AssertEquals("CXH_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", exitHeader.CXH_CustomsProfile);

				exitHeader.CXH_GS_NKCustomsAgent = staff3.GS_Code;
				AssertEquals("CXH_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, exitHeader.CXH_CustomsProfile);

				exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
				exitHeader.CXH_CustomsProfile = "TestCert2";
				AssertEquals("CXH_CustomsProfile has value when broker not empty", "TestCert2", exitHeader.CXH_CustomsProfile);

				exitHeader.CXH_GS_NKCustomsAgent = ZString.Empty;
				AssertEquals("CXH_CustomsProfile has been cleared when broker is empty", ZString.Empty, exitHeader.CXH_CustomsProfile);

				exitHeader.CXH_CustomsProfile = "TESTCERT1";
				exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("CXH_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", exitHeader.CXH_CustomsProfile);

				exitHeader.CXH_GS_NKCustomsAgent = ZString.Empty;
				exitHeader.CXH_CustomsProfile = "TestCert2";
				exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("CXH_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, exitHeader.CXH_CustomsProfile);
			});
		}

		public void TestCusExitConsignmentPackages()
		{
			AssertType<CusExitConsignmentPackageCollection<CusExitConsignmentPackage>>("CusExitConsignmentPackages Type", exitHeader.CusExitConsignmentPackages);
		}

		public void TestTrainingEntry_GenAddOnColumnAndDefault()
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();

			CombineAssertions(() =>
			{
				AssertEquals("TrainingEntry should be true by default", true, header.TrainingEntry);

				header.TrainingEntry = false;
				AssertEquals("TrainingEntry should be false when set", false, header.TrainingEntry);
			});
		}

		public void TestTrainingEntryPersistance()
		{
			var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_ExitControl_TrainingEntry");

			var header = Factory.NewWithValidTestData<CusExitHeader>();
			header.TrainingEntry = true;

			CombineAssertions(() =>
			{
				AssertEquals("TrainingEntry", true, header.TrainingEntry);
				AssertNotNull("TrainingEntry is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
			});
		}

		public void TestReadOnlyOnSaving()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("When there are no report in the exitheader it is not readonly", false, exitHeader.ReadOnly);

				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = "AAA";
				var report1 = exitHeader.CusExitReports.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;
				report1.CER_Status = "EXR";
				exitHeader.CXH_JobReference = "AA";
				Factory.Save();
				AssertEquals("When there is one report with status EXR exitHeader is readonly", true, exitHeader.ReadOnly);
				AssertEquals("When there is one report with status EXR report1 is readonly", true, report1.ReadOnly);

				report1.CER_Status = ZString.Empty;
				exitHeader.CXH_JobReference = "BB";
				Factory.Save();
				AssertEquals("When there is one report with status CLP exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("When there is one report with status CLP report1 is not readonly", false, report1.ReadOnly);

				report1.CER_Status = "COX";
				exitHeader.CXH_JobReference = "CC";
				Factory.Save();
				AssertEquals("When there is one report with status COX exitHeader is readonly", true, exitHeader.ReadOnly);
				AssertEquals("When there is one report with status COX report1 is readonly", true, report1.ReadOnly);

				report1.CER_Status = "ERR";
				exitHeader.CXH_JobReference = "DD";
				Factory.Save();
				AssertEquals("When there is one report with status ERR exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("When there is one report with status ERR report1 is not readonly", false, report1.ReadOnly);

				report1.CER_Status = "REF";
				exitHeader.CXH_JobReference = "EE";
				Factory.Save();
				AssertEquals("When there is one report with status REF exitHeader is readonly", true, exitHeader.ReadOnly);
				AssertEquals("When there is one report with status REF report1 is readonly", true, report1.ReadOnly);

				report1.CER_Status = "RCE";
				exitHeader.CXH_JobReference = "FF";
				Factory.Save();
				AssertEquals("When there is one report with status RCE exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("When there is one report with status RCE report1 is not readonly", false, report1.ReadOnly);

				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_MovementReference = "BBB";
				var report2 = exitHeader.CusExitReports.AddNew();
				report2.CER_CXC_Consignment = consignment2.PK;
				exitHeader.CXH_JobReference = "GG";
				Factory.Save();
				AssertEquals("When there are 2 reports with status not EXR, COX or REF exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("When there are 2 reports with status not EXR, COX or REF report1 is not readonly", false, report1.ReadOnly);
				AssertEquals("When there are 2 reports with status not EXR, COX or REF report2 is not readonly", false, report2.ReadOnly);

				report1.CER_Status = "EXR";
				report2.CER_Status = "COX";
				exitHeader.CXH_JobReference = "HH";
				Factory.Save();
				AssertEquals("When there are 2 reports with status EXR, COX or REF exitHeader is readonly", true, exitHeader.ReadOnly);
				AssertEquals("When there are 2 reports with status EXR, COX or REF report1 is readonly", true, report1.ReadOnly);
				AssertEquals("When there are 2 reports with status EXR, COX or REF report2 is readonly", true, report2.ReadOnly);

				var log = exitHeader.Logs.AddNew(AutoEvents.UnlockForEdit);
				exitHeader.CXH_JobReference = "II";
				Factory.Save();
				AssertEquals("When there are 2 reports with status EXR, COX or REF but with UCK log as the last one, exitHeader is not readonly", false, exitHeader.ReadOnly);
				AssertEquals("When there are 2 reports with status EXR, COX or REF but with UCK log as the last one, report1 is readonly", true, report1.ReadOnly);
				AssertEquals("When there are 2 reports with status EXR, COX or REF but with UCK log as the last one, report2 is readonly", true, report2.ReadOnly);
			});
		}

		#region ICustomsFileParent

		public void TestICustomsFileParent_GetDeclarationTypeFromInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			var exitHeader = (CusExitHeader)GetNewBusinessObject();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;

			((ICustomsFileParent)exitHeader).DeclarationTypeInfo.SetValueFromString("AAA");

			AssertEquals("Should get value from the value of Declaration", "AAA", ((ICustomsFileParent)exitHeader).DeclarationType);
		}

		public void TestICustomsFileParent_DeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "AAA";

			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitHeader>();
				AssertEquals("When parent is null, should get empty string returned", ZString.Empty, ((ICustomsFileParent)exitHeader).DeclarationType);

				exitHeader = (CusExitHeader)GetNewBusinessObject();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				AssertEquals("When parent is Declaration, should get value from JE_MessageType", "AAA", ((ICustomsFileParent)exitHeader).DeclarationType);
			});
		}

		public void TestICustomsFileParent_DeclarationTypeInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitHeader>();
				AssertType<ZPropertyInfoString>("When parent is null, should get a ZPropertyInfoString returned", ((ICustomsFileParent)exitHeader).DeclarationTypeInfo);

				exitHeader = (CusExitHeader)GetNewBusinessObject();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				AssertEquals("When parent is Declaration, should get JE_MessageTypeInfo returned", declaration.JE_MessageTypeInfo, ((ICustomsFileParent)exitHeader).DeclarationTypeInfo);
			});
		}

		public void TestICustomsFileParent_BranchPk()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GB_Branch = branch.PK;

			var customsFileParent = (ICustomsFileParent)exitHeader;

			AssertEquals("Should get value from CXH_GB_Branch", exitHeader.CXH_GB_Branch, customsFileParent.BranchPk);
		}

		public void TestICustomsFileParent_IsLocked()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var log = exitHeader.Logs.AddNew(AutoEvents.UnlockForEdit);

			AssertEquals("Should not be locked as there is no active LCK log.", false, ((ICustomsFileParent)exitHeader).IsLocked);

			log = exitHeader.Logs.AddNew(AutoEvents.LockForEdit);
			AssertEquals("Should be locked as there is an active LCK log.", true, ((ICustomsFileParent)exitHeader).IsLocked);

			log.Cancel();
			AssertEquals("Should not be locked as there is no active LCK log.", false, ((ICustomsFileParent)exitHeader).IsLocked);

			exitHeader.ReadOnly = true;
			AssertEquals("Should be locked as ReadOnly is set to true.", true, ((ICustomsFileParent)exitHeader).IsLocked);
		}

		public void TestICustomsFileParent_LockFile()
		{
			var exitHeader = Factory.New<CusExitHeader>();

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
			var exitHeader = Factory.New<CusExitHeader>();

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
				var exitHeader = Factory.New<CusExitHeader>();

				AssertEquals("Prereq: exitHeader is not readonly", false, exitHeader.ReadOnly);

				var returnedMessage = exitHeader.LockExitHeader(string.Empty);
				AssertEquals("Message informing lock is done", "This tab page has been locked for edit\r\nExit Control\r\n\r\nYou can click the Exit Control - Unlock Exit Control to unlock it.", returnedMessage);

				Factory.Save();
				AssertEquals("exitHeader is readonly", true, exitHeader.ReadOnly);
			});
		}

		public void TestUnlockExitHeader()
		{
			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitHeader>();
				exitHeader.SetReadOnlyIncludingChildren(true);

				AssertEquals("Prereq: exitHeader is readonly", true, exitHeader.ReadOnly);

				var returnedMessage = exitHeader.UnlockExitHeader(string.Empty);
				AssertEquals("Message informing unlock is done", "The Exit Control is unlocked.", returnedMessage);

				Factory.Save();
				AssertEquals("exitHeader is not readonly", false, exitHeader.ReadOnly);
			});
		}

		public void TestIsUCC6() => AssertEquals(true, Factory.New<CusExitHeader>().IsUCC6);

		protected override bool ExpectedShouldHaveSeqNumInContainersOrEquipmentsAndSeals => true;
	}
}
