using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class PopulateExitControlManagerTest : TestCaseWithFactory
	{
		public void TestAddOrUpdateExitControl_NewExitHeader()
		{
			var expectedBrokerCode = "XZX";
			var expectedCertificate = "TESTCERT1";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;
			var wrapper = GlbStaffWrapper.Get(staffCurrentUser);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = expectedCertificate;
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitReport);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration);
					
					AssertEquals("1 new exit report created", 1, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

					var exitHeader = GetExitHeaderForDeclarationAES(declaration);
					AssertExitHeader("New ExitHeader", exitHeader, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
										expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedCertificate: expectedCertificate);
				});
			}
		}

		public void TestAddOrUpdateExitControl_ExistingExitHeaderWithoutConsignment()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration);

					AssertEquals("1 exit header updated", 1, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

					var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestAddOrUpdateExitControl_ExistingExitHeaderAndConsignmentWithoutReportToOverwrite()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
				exitConsignment.CXC_LocalReference = "local ref";

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration, false);

					AssertEquals("Nothing was done when overwrite confirmation is false", 0, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertEquals("No unupdatable entries", 0, populateData.NonUpdatableEntriesMRNs.Count());

					populateData = AddOrUpdateExitControlForTest(declaration);

					AssertEquals("1 header updated when overwrite confirmation is true", 1, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertEquals("No unupdatable entries", 0, populateData.NonUpdatableEntriesMRNs.Count());

					var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestAddOrUpdateExitControl_ExistingExitHeaderWithConsignmentAndReportToOverwrite()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
				exitConsignment.CXC_LocalReference = "local ref";

				var exitReport = exitHeader.CusExitReports.AddNew();
				exitReport.CER_CXC_Consignment = exitConsignment.PK;
				exitReport.CER_OfficeOfExit = "AAA";
				exitReport.CER_TransportMode = "AIR";

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration, false);

					AssertEquals("Nothing was done when overwrite confirmation is false", 0, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertEquals("No unupdatable entries", 0, populateData.NonUpdatableEntriesMRNs.Count());

					populateData = AddOrUpdateExitControlForTest(declaration);

					AssertEquals("1 header updated when overwrite confirmation is true", 1, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertEquals("No unupdatable entries", 0, populateData.NonUpdatableEntriesMRNs.Count());

					var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}

		public void TestAddOrUpdateExitControl_ExistingExitHeaderWithExistingSentReport()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";
			var expectedUnchangedConsignmentLRN = "local ref";
			var expectedUnchangedCustomsOffice = "AAA";
			var expectedUnchangedInlandMOT = "AIR";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration.PK;
				exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
				exitConsignment.CXC_LocalReference = expectedUnchangedConsignmentLRN;

				var exitReport = exitHeader.CusExitReports.AddNew();
				exitReport.CER_CXC_Consignment = exitConsignment.PK;
				exitReport.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
				exitReport.CER_TransportMode = expectedUnchangedInlandMOT;
				exitReport.CER_MessageStatus = "SNT";

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration);

					AssertEquals("No exit header created or updated", 0, populateData.ObjectsAddedOrUpdated);
					AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
					AssertContainsExactElementsInAnyOrder("Can't update detail when sent or accepted and No details updated", new ZString[] { "refNum1" }, populateData.NonUpdatableEntriesMRNs);

					var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

					AssertExitHeader("Existing exit header without new data", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference,
									expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
				});
			}
		}

		public void TestAddOrUpdateExitControl_ExistingExitHeaderWithConsignmentNotAssociated()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";
			var expectedUnchangedConsignmentLRN = "local ref";
			var expectedUnchangedCustomsOffice = "AAA";
			var expectedUnchangedInlandMOT = "AIR";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_JobReference = expectedHeaderReference;

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
				exitConsignment.CXC_LocalReference = expectedUnchangedConsignmentLRN;

				var exitReport = exitHeader.CusExitReports.AddNew();
				exitReport.CER_CXC_Consignment = exitConsignment.PK;
				exitReport.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
				exitReport.CER_TransportMode = expectedUnchangedInlandMOT;

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration);

					AssertEquals("No exit header created or updated", 0, populateData.ObjectsAddedOrUpdated);
					AssertContainsExactElementsInAnyOrder("Can't update exit control when it is not associated to the declaration", new ZString[] { "A movement for MRN refNum1 already exists in Exit Control Reference" }, populateData.ErrorMessages);
					AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

					GetExitHeaderForDeclarationAES(declaration, false);

					AssertExitHeader("Existing exit header without new data", exitHeader, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN },
									ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty, expectedBroker: expectedBrokerCode,
									expectedHeaderReference: expectedHeaderReference, expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
				});
			}
		}

		public void TestAddOrUpdateExitControl_ExistingExitHeaderWithConsignmentNotAssociatedExpectedDeclaration()
		{
			var expectedBrokerCode = "XZX";
			var expectedHeaderReference = "Reference";
			var expectedUnchangedConsignmentLRN = "local ref";
			var expectedUnchangedCustomsOffice = "AAA";
			var expectedUnchangedInlandMOT = "AIR";
			var expectedDeclarationReference = "DecRef";

			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = expectedBrokerCode;
			staffCurrentUser.GS_LoginName = "Current User";
			staffCurrentUser.GS_IsSystemAccount = false;

			var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
			expectedCarrierOrgHeader.OH_Code = "Carrier";
			var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
			expectedCarrierAddress.OA_Address1 = "Carrier Address";

			var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
			expectedSupplierOrgHeader.OH_Code = "Supplier";
			var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
			expectedSupplierAddress.OA_Address1 = "Supplier Address}";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
				declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitReport);
				declaration.JE_CustomsOffice = "LV009998";
				declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
				declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
				declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

				var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
				declaration2.JE_DeclarationReference = expectedDeclarationReference;
				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_ParentID = declaration2.PK;
				exitHeader.CXH_ParentTableCode = declaration2.TablePrefix;
				exitHeader.CXH_JobReference = expectedHeaderReference;

				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				exitConsignment.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
				exitConsignment.CXC_LocalReference = expectedUnchangedConsignmentLRN;

				var exitReport = exitHeader.CusExitReports.AddNew();
				exitReport.CER_CXC_Consignment = exitConsignment.PK;
				exitReport.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
				exitReport.CER_TransportMode = expectedUnchangedInlandMOT;

				Factory.Save();

				CombineAssertions(() =>
				{
					var populateData = AddOrUpdateExitControlForTest(declaration);

					AssertEquals("No exit header created or updated", 0, populateData.ObjectsAddedOrUpdated);
					AssertContainsExactElementsInAnyOrder("Can't update exit control when it is not associated to the declaration", new ZString[] { "A movement for MRN refNum1 already exists in Job Number DecRef" }, populateData.ErrorMessages);
					AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

					GetExitHeaderForDeclarationAES(declaration, false);

					AssertExitHeader("Existing exit header without new data", exitHeader, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN },
									ZGuid.Empty, ZGuid.Empty, declaration2.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference,
									expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
				});
			}
		}

		public void TestAddOrUpdateExitControl_MultipleHeaders_NewExitHeader()
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			Factory.Save();

			CombineAssertions(() =>
			{
				var populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("2 new exit reports created", 2, populateData.ObjectsAddedOrUpdated);
				AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
				AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

				var exitHeader = GetExitHeaderForDeclarationAES(declaration);
				AssertExitHeader("New ExitHeader", exitHeader, 2, new ZString[] { ExpectedMRN1ForNewExitReport, ExpectedMRN2ForNewExitReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK);
			});
		}

		public void TestAddOrUpdateExitControl_MultipleHeaders_ExistingExitHeaderWithoutConsignments()
		{
			var expectedHeaderReference = "Reference";

			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = expectedHeaderReference;

			Factory.Save();

			CombineAssertions(() =>
			{
				var populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("2 new exit reports created", 2, populateData.ObjectsAddedOrUpdated);
				AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
				AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

				var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

				AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 2, new ZString[] { ExpectedMRN1ForNewExitReport, ExpectedMRN2ForNewExitReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
			});
		}

		public void TestAddOrUpdateExitControl_MultipleHeaders_ExistingExitHeaderAndConsignmentsWithoutReports()
		{
			var expectedHeaderReference = "Reference";

			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = expectedHeaderReference;

			var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
			exitConsignment1.CXC_LocalReference = "local ref 1";

			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitReport;
			exitConsignment2.CXC_LocalReference = "local ref 2";

			Factory.Save();

			CombineAssertions(() =>
			{
				var populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("2 headers updated when overwrite confirmation is true and 1 header created", 3, populateData.ObjectsAddedOrUpdated);
				AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
				AssertEquals("No unupdatable entries", 0, populateData.NonUpdatableEntriesMRNs.Count());
				
				var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

				AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 3, new ZString[] { ExpectedMRN1ForNewExitReport, ExpectedMRN2ForNewExitReport, ExpectedMRN3ForNewExitReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);

				exitHeaderAfterGeneration.CusExitReports[0].CER_Status = "REF";
				exitHeaderAfterGeneration.CusExitReports[1].CER_Status = "EXR";
				exitHeaderAfterGeneration.CusExitReports[2].CER_Status = "COX";
				Factory.Save();
				populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("No exit header updated when sent or accepted", 0, populateData.ObjectsAddedOrUpdated);
				AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
				AssertContainsExactElementsInAnyOrder("Can't update detail when sent or accepted and No details updated", new ZString[] { "refNum1", "refNum2", "refNum3" }, populateData.NonUpdatableEntriesMRNs);
			});
		}

		public void TestAddOrUpdateExitControl_MultipleHeaders_ExistingExitHeaderWithConsignmentsAndReports()
		{
			var expectedHeaderReference = "Reference";
			var expectedUnchangedConsignmentLRN2 = "local ref2";
			var expectedUnchangedCustomsOffice = "AAA";
			var expectedUnchangedInlandMOT = "AIR";

			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader.CXH_JobReference = expectedHeaderReference;

			var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
			exitConsignment1.CXC_LocalReference = "local ref 1";

			var exitReport1 = exitHeader.CusExitReports.AddNew();
			exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
			exitReport1.CER_OfficeOfExit = "AAA";
			exitReport1.CER_TransportMode = "AIR";

			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment2.CXC_MovementReference = ExpectedMRN2ForNewExitReport;
			exitConsignment2.CXC_LocalReference = expectedUnchangedConsignmentLRN2;

			var exitReport2 = exitHeader.CusExitReports.AddNew();
			exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
			exitReport2.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
			exitReport2.CER_TransportMode = expectedUnchangedInlandMOT;
			exitReport2.CER_MessageStatus = "SNT";

			Factory.Save();

			CombineAssertions(() =>
			{
				var populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("1 detail update and 1 created when overwrite confirmation is true", 2, populateData.ObjectsAddedOrUpdated);
				AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
				AssertContainsExactElementsInAnyOrder("Can't update detail when sent or accepted", new ZString[] { "refNum2" }, populateData.NonUpdatableEntriesMRNs);
				
				var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);

				AssertExitHeader("Existing exit header with new data", exitHeaderAfterGeneration, 3, new ZString[] { ExpectedMRN1ForNewExitReport, ExpectedMRN2ForNewExitReport, ExpectedMRN3ForNewExitReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, expectedUnchangedConsignmentLRN2, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference,
									expectedCustomsOffices: new ZString[] { ExpectedCustomsOfficeForNewExitReport, expectedUnchangedCustomsOffice, ExpectedCustomsOfficeForNewExitReport },
									expectedInlandMOTs: new ZString[] { ExpectedInlandMOTForNewExitReport, expectedUnchangedInlandMOT, ExpectedInlandMOTForNewExitReport });

				exitHeaderAfterGeneration.CusExitReports[0].CER_Status = "REF";
				exitHeaderAfterGeneration.CusExitReports[1].CER_Status = "EXR";
				exitHeaderAfterGeneration.CusExitReports[2].CER_Status = "COX";
				Factory.Save();
				populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("No exit header updated when sent or accepted", 0, populateData.ObjectsAddedOrUpdated);
				AssertEquals("No error messages", 0, populateData.ErrorMessages.Count());
				AssertContainsExactElementsInAnyOrder("Can't update detail when sent or accepted", new ZString[] { "refNum1", "refNum2", "refNum3" }, populateData.NonUpdatableEntriesMRNs);
			});
		}

		public void TestAddOrUpdateExitControl_MultipleHeaders_ExistingExitHeaderWithConsignmentWrongAssociation_Update()
		{
			var expectedHeaderReference1 = "Reference1";
			var expectedHeaderReference2 = "Reference2";
			var expectedHeaderReference3 = "Reference3";
			var expectedUnchangedConsignmentLRN1 = "local ref 1";
			var expectedUnchangedConsignmentLRN2 = "local ref 2";
			var expectedUnchangedCustomsOffice = "AAA";
			var expectedUnchangedInlandMOT = "AIR";
			var expectedDeclarationReference = "DecRef";

			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_DeclarationReference = expectedDeclarationReference;
			var exitHeader1 = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader1.CXH_ParentID = declaration2.PK;
			exitHeader1.CXH_ParentTableCode = declaration2.TablePrefix;
			exitHeader1.CXH_JobReference = expectedHeaderReference1;

			var exitConsignment1 = exitHeader1.CusExitConsignments.AddNew();
			exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
			exitConsignment1.CXC_LocalReference = expectedUnchangedConsignmentLRN1;

			var exitReport1 = exitHeader1.CusExitReports.AddNew();
			exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
			exitReport1.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
			exitReport1.CER_TransportMode = expectedUnchangedInlandMOT;

			var exitHeader2 = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader2.CXH_JobReference = expectedHeaderReference2;

			var exitConsignment2 = exitHeader2.CusExitConsignments.AddNew();
			exitConsignment2.CXC_MovementReference = ExpectedMRN3ForNewExitReport;
			exitConsignment2.CXC_LocalReference = expectedUnchangedConsignmentLRN2;

			var exitReport2 = exitHeader2.CusExitReports.AddNew();
			exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
			exitReport2.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
			exitReport2.CER_TransportMode = expectedUnchangedInlandMOT;

			var exitHeader3 = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader3.CXH_ParentID = declaration.PK;
			exitHeader3.CXH_ParentTableCode = declaration.TablePrefix;
			exitHeader3.CXH_JobReference = expectedHeaderReference3;

			var exitConsignment3 = exitHeader3.CusExitConsignments.AddNew();
			exitConsignment3.CXC_MovementReference = ExpectedMRN2ForNewExitReport;
			exitConsignment3.CXC_LocalReference = "local ref 3";

			Factory.Save();

			CombineAssertions(() =>
			{
				var populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("1 header updated when overwrite confirmation is true", 1, populateData.ObjectsAddedOrUpdated);
				AssertContainsExactElementsInAnyOrder("Can't update exit control when it is not associated to the declaration", new ZString[] { "A movement for MRN refNum1 already exists in Job Number DecRef", "A movement for MRN refNum3 already exists in Exit Control Reference2" }, populateData.ErrorMessages);
				AssertEquals("No unupdatable entries", 0, populateData.NonUpdatableEntriesMRNs.Count());

				var exitHeaderAfterGeneration = GetExitHeaderForDeclarationAES(declaration);
				AssertEquals("There is no new exitHeader created since there was already one associated to the declaration (exitHeader3)", exitHeader3, exitHeaderAfterGeneration);

				AssertExitHeader("New ExitHeader", exitHeaderAfterGeneration, 1, new ZString[] { ExpectedMRN2ForNewExitReport },
									new ZString[] { ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference3);

				AssertExitHeader("Existing exit header 1 without new data", exitHeader1, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN1 },
								ZGuid.Empty, ZGuid.Empty, declaration2.PK, expectedHeaderReference: expectedHeaderReference1,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });

				AssertExitHeader("Existing exit header 2 without new data", exitHeader2, 1, new ZString[] { ExpectedMRN3ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN2 },
								ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty, expectedHeaderReference: expectedHeaderReference2,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
			});
		}

		public void TestAddOrUpdateExitControl_MultipleHeaders_ExistingExitHeaderWithConsignmentWrongAssociation()
		{
			var expectedHeaderReference1 = "Reference1";
			var expectedHeaderReference2 = "Reference2";
			var expectedUnchangedConsignmentLRN1 = "local ref 1";
			var expectedUnchangedConsignmentLRN2 = "local ref 2";
			var expectedUnchangedCustomsOffice = "AAA";
			var expectedUnchangedInlandMOT = "AIR";
			var expectedDeclarationReference = "DecRef";

			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitReport;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_DeclarationReference = expectedDeclarationReference;
			var exitHeader1 = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader1.CXH_ParentID = declaration2.PK;
			exitHeader1.CXH_ParentTableCode = declaration2.TablePrefix;
			exitHeader1.CXH_JobReference = expectedHeaderReference1;

			var exitConsignment1 = exitHeader1.CusExitConsignments.AddNew();
			exitConsignment1.CXC_MovementReference = ExpectedMRN1ForNewExitReport;
			exitConsignment1.CXC_LocalReference = expectedUnchangedConsignmentLRN1;

			var exitReport1 = exitHeader1.CusExitReports.AddNew();
			exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
			exitReport1.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
			exitReport1.CER_TransportMode = expectedUnchangedInlandMOT;

			var exitHeader2 = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader2.CXH_JobReference = expectedHeaderReference2;

			var exitConsignment2 = exitHeader2.CusExitConsignments.AddNew();
			exitConsignment2.CXC_MovementReference = ExpectedMRN3ForNewExitReport;
			exitConsignment2.CXC_LocalReference = expectedUnchangedConsignmentLRN2;

			var exitReport2 = exitHeader2.CusExitReports.AddNew();
			exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
			exitReport2.CER_OfficeOfExit = expectedUnchangedCustomsOffice;
			exitReport2.CER_TransportMode = expectedUnchangedInlandMOT;

			Factory.Save();

			CombineAssertions(() =>
			{
				var populateData = AddOrUpdateExitControlForTest(declaration);

				AssertEquals("1 header created", 1, populateData.ObjectsAddedOrUpdated);
				AssertContainsExactElementsInAnyOrder("Can't update exit control when it is not associated to the declaration", new ZString[] { "A movement for MRN refNum1 already exists in Job Number DecRef", "A movement for MRN refNum3 already exists in Exit Control Reference2" }, populateData.ErrorMessages);
				AssertNull("No unupdatable entries", populateData.NonUpdatableEntriesMRNs);

				var newExitHeader = GetExitHeaderForDeclarationAES(declaration);
				AssertExitHeader("New ExitHeader", newExitHeader, 1, new ZString[] { ExpectedMRN2ForNewExitReport },
									new ZString[] { ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK);

				AssertExitHeader("Existing exit header 1 without new data", exitHeader1, 1, new ZString[] { ExpectedMRN1ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN1 },
								ZGuid.Empty, ZGuid.Empty, declaration2.PK, expectedHeaderReference: expectedHeaderReference1,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });

				AssertExitHeader("Existing exit header 2 without new data", exitHeader2, 1, new ZString[] { ExpectedMRN3ForNewExitReport }, new ZString[] { expectedUnchangedConsignmentLRN2 },
								ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty, expectedHeaderReference: expectedHeaderReference2,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
			});
		}

		void AssertExitHeader(string message, CusExitHeader header, int expectedConsignmentReportCount, ZString[] expectedMRNCodes, ZString[] expectedLRNCodes, ZGuid expectedExporterID, ZGuid expectedCarrierID, ZGuid expectedParentID, string expectedBroker = "", string expectedParentTableCode = "JE",
									string expectedCertificate = "", string expectedHeaderReference = ExpectedReferenceForNewExitHeader, ZString[] expectedCustomsOffices = null, ZString[] expectedInlandMOTs = null)
		{
			AssertEquals(message + " Header.CXH_GS_NKCustomsAgent", expectedBroker, header.CXH_GS_NKCustomsAgent);
			AssertEquals(message + " Header.CXH_CustomsProfile", expectedCertificate, header.CXH_CustomsProfile);
			AssertEquals(message + " Header.CXH_OH_Exporter", expectedExporterID, header.CXH_OH_Exporter);
			AssertEquals(message + " Header.CXH_OA_Carrier", expectedCarrierID, header.CXH_OA_Carrier);
			AssertEquals(message + " Header.CXH_ParentID", expectedParentID, header.CXH_ParentID);
			AssertEquals(message + " Header.CXH_ParentTableCode", expectedParentTableCode, header.CXH_ParentTableCode);
			AssertEquals(message + " Header.CXH_JobReference", expectedHeaderReference, header.CXH_JobReference);

			AssertExitConsignments(expectedConsignmentReportCount, header.CusExitConsignments, expectedMRNCodes, expectedLRNCodes);

			AssertExitReports(expectedConsignmentReportCount, header.CusExitReports, expectedMRNCodes, expectedCustomsOffices, expectedInlandMOTs);
		}

		void AssertExitConsignments(int expectedConsignmentCount, ICusExitConsignmentCollection<CusExitConsignment> exitConsignments, ZString[] expectedMRNCodes, ZString[] expectedLRNCodes)
		{
			AssertEquals("ExitConsignments count is correct", expectedConsignmentCount, exitConsignments.Count);
			AssertContainsExactElementsInAnyOrder("ExitConsignments mrn codes are correct", expectedMRNCodes, exitConsignments.Select(x => x.CXC_MovementReference).ToArray());
			AssertContainsExactElementsInAnyOrder("ExitConsignments lrn codes are correct", expectedLRNCodes, exitConsignments.Select(x => x.CXC_LocalReference).ToArray());
		}

		void AssertExitReports(int expectedReportCount, ICusExitReportCollection<CusExitReport> exitReports, ZString[] expectedMRNCodes, ZString[] expectedCustomsOffices = null, ZString[] expectedInlandMOTs = null)
		{
			AssertEquals("ExitReports count is correct", expectedReportCount, exitReports.Count);
			AssertContainsExactElementsInAnyOrder("ExitReports mrn codes are correct", expectedMRNCodes, exitReports.Select(x => x.Consignment.CXC_MovementReference).ToArray());
			if (expectedCustomsOffices == null)
			{
				AssertEquals("ExitReports Customs Office codes are correct", false, exitReports.Any(x => x.CER_OfficeOfExit != ExpectedCustomsOfficeForNewExitReport));
			}
			else
			{
				AssertContainsExactElementsInAnyOrder("ExitReports Customs Office codes are correct", expectedCustomsOffices, exitReports.Select(x => x.CER_OfficeOfExit).ToArray());
			}

			if (expectedInlandMOTs == null)
			{
				AssertEquals("ExitReports Inland MOT codes are correct", false, exitReports.Any(x => x.CER_TransportMode != ExpectedInlandMOTForNewExitReport));
			}
			else
			{
				AssertContainsExactElementsInAnyOrder("ExitReports Inland MOT codes are correct", expectedInlandMOTs, exitReports.Select(x => x.CER_TransportMode).ToArray());
			}
		}

		CusExitHeader GetExitHeaderForDeclarationAES(JobDeclaration declaration, bool exitHeaderShouldExist = true)
		{
			var query = new ZQuery(CusExitHeaderSchema.CXH_ParentID, declaration.PK);
			var exitHeaders = Factory.Load<CusExitHeader>(query);

			if (exitHeaderShouldExist)
			{
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);
				return exitHeaders[0];
			}
			else
			{
				AssertEquals("No exitHeaders associated to the declaration", 0, exitHeaders.Length);
				return null;
			}
		}

		const string ExpectedMRN1ForNewExitReport = "refNum1";
		const string ExpectedMRN2ForNewExitReport = "refNum2";
		const string ExpectedMRN3ForNewExitReport = "refNum3";
		const string ExpectedLRN1ForNewExitConsignment = "ES00001";
		const string ExpectedLRN2ForNewExitConsignment = "ES00002";
		const string ExpectedLRN3ForNewExitConsignment = "ES00003";
		const string ExpectedReferenceForNewExitHeader = "JD0001";
		const string ExpectedCustomsOfficeForNewExitReport = "ES009999";
		const string ExpectedInlandMOTForNewExitReport = "SEA";

		public GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.GetStaffAccount();
				}

				return staff;
			}
		}
		GlbStaff staff;

		PopulateExitControlData AddOrUpdateExitControlForTest(JobDeclaration declaration, bool overwriteConfirmationReturn = true) => new PopulateExitControlManager().AddOrUpdateExitControl(declaration, declaration.CustomsEntryHeaders.Where(x => x.IsAcceptedExport && x.IsExportUCC6), (code) => overwriteConfirmationReturn);
	}
}
