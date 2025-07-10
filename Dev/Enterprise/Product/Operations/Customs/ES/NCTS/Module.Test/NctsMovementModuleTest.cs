using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using CusGuaranteeHeader = Enterprise.Customs.ES.Business.CusGuaranteeHeader;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementModule))]
	class NctsMovementModuleTest : ZModuleBasherTest
	{
		public void TestFilterBusinessObject()
		{
			using (var module = new NctsMovementModule())
			{
				AssertType(typeof(NctsMovementFilterStripBusinessObject), module.FilterBusinessObject);
			}
		}

		public void TestGuaranteeCheckTransitStatusMenuItem()
		{
			using (var module = new NctsMovementModuleForTest())
			{
				CombineAssertions(() =>
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);

					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");
					AssertNotNull("checkTransitStatusMenuItem", checkTransitStatusMenuItem);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_Phase4()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				string writtenOffTransitFromCustomsTestFileContent = GetTestFileContent("NotWrittenOffTransitFromCustoms.txt");

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AH";
				staff.GS_LoginName = "ahtest";
				var wrapper = GlbStaffWrapper.Get(staff);
				var cert = wrapper.ESBPasswordCollection.AddNew();
				cert.GP_Name = "TestCert1";
				cert.GP_MailBoxID = "Test";
				cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var nctsHeaderDeparture = Factory.New<NctsHeader>();
					nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
					nctsHeaderDeparture.BH_JobReference = "Departure";

					var nctsHeaderDepartureAndArrival = Factory.New<NctsHeader>();
					nctsHeaderDepartureAndArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
					nctsHeaderDepartureAndArrival.BH_JobReference = "DepartureAndArrival";
					var departureAndArrivalMrn = "20ES00999950012811";
					var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDepartureAndArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = departureAndArrivalMrn;
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					nctsHeaderDepartureAndArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
					nctsHeaderDepartureAndArrival.BH_CustomsProfile = "TestCert1";

					var mockHeaderDepartureAndArrival = new Mock<NctsHeader>(Factory, ((IBusinessObjectFactoryInternals)Factory).RowFactory.LoadFromPK("CusInBondHeader", nctsHeaderDepartureAndArrival.PK));
					mockHeaderDepartureAndArrival.Setup(a => a.CreateRequestAndGetResponse(departureAndArrivalMrn, cert)).Returns(writtenOffTransitFromCustomsTestFileContent);
					mockHeaderDepartureAndArrival.CallBase = true;
					var mockHeaderDepartureAndArrivalObject = mockHeaderDepartureAndArrival.Object;

					var nctsHeaderArrival = Factory.New<NctsHeader>();
					nctsHeaderArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
					nctsHeaderArrival.BH_JobReference = "Arrival";
					newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012812";
					newEntryNumber.CE_EntryIsSystemGenerated = true;

					var nctsHeaders = new NctsHeaderCollection(Factory)
					{
						nctsHeaderDeparture,
						mockHeaderDepartureAndArrivalObject,
						nctsHeaderArrival
					};

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when no movement was selected", "Please select at least one movement", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						var resultList = module.WriteOffResultCollection.Cast<WriteOffResult>();
						AssertEquals("ResultList contains 3 elements", 3, resultList.Count());
						AssertWriteOffResult(resultList.FirstOrDefault(x => x.JobNumber == "Departure"), "Departure header without data", "Departure", "Excluded (not registered)");
						AssertWriteOffResult(resultList.FirstOrDefault(x => x.JobNumber == "DepartureAndArrival"), "DepartureAndArrival header with all data", "DepartureAndArrival", "Not Written Off", "20ES00999950012811", declarationStatus: "Despachado");
						AssertWriteOffResult(resultList.FirstOrDefault(x => x.JobNumber == "Arrival"), "Arrival with mrn", "Arrival", "Excluded (not a departure)", "20ES00999950012812");

						module.DisplayGrid.SelectSingleElement(nctsHeaderDeparture);
						checkTransitStatusMenuItem.PerformClick();
						resultList = module.WriteOffResultCollection.Cast<WriteOffResult>();
						AssertEquals("ResultList contains 1 element when only selecting departure header", 1, resultList.Count());
						AssertWriteOffResult(resultList.FirstOrDefault(x => x.JobNumber == "Departure"), "Departure header without data", "Departure", "Excluded (not registered)");

						module.DisplayGrid.SelectSingleElement(nctsHeaderDepartureAndArrival);
						checkTransitStatusMenuItem.PerformClick();
						resultList = module.WriteOffResultCollection.Cast<WriteOffResult>();
						AssertEquals("ResultList contains 1 element when only selecting departure and arrival header", 1, resultList.Count());
						AssertWriteOffResult(resultList.FirstOrDefault(x => x.JobNumber == "DepartureAndArrival"), "DepartureAndArrival header with all data", "DepartureAndArrival", "Not Written Off", "20ES00999950012811", declarationStatus: "Despachado");

						module.DisplayGrid.SelectSingleElement(nctsHeaderArrival);
						checkTransitStatusMenuItem.PerformClick();
						resultList = module.WriteOffResultCollection.Cast<WriteOffResult>();
						AssertEquals("ResultList contains 1 element when only selecting arrival header", 1, resultList.Count());
						AssertWriteOffResult(resultList.FirstOrDefault(x => x.JobNumber == "Arrival"), "Arrival with mrn", "Arrival", "Excluded (not a departure)", "20ES00999950012812");
					});
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_AddTransaction_Phase4()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				string writtenOffTransitFromCustomsTestFileContent = GetTestFileContent("WrittenOffTransitFromCustoms.txt");

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AH";
				staff.GS_LoginName = "ahtest";
				var wrapper = GlbStaffWrapper.Get(staff);
				var cert = wrapper.ESBPasswordCollection.AddNew();
				cert.GP_Name = "TestCert1";
				cert.GP_MailBoxID = "Test";
				cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

				var mrnCode = "22ES00999950008716";

				var guaranteeReference = "Guarantee1";
				var guaranteeHeader = ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, guaranteeReference, oblTranValue: 0m, oblTranReference: mrnCode);
				guaranteeHeader.CPH_Balance = 0;

				AddTransaction("First-CON", -60m, PermitTransactionStatusList.Codes.Confirmed);
				AddTransaction("Second-CON", 30m, PermitTransactionStatusList.Codes.Confirmed);
				AddTransaction("Third-CON", -90m, PermitTransactionStatusList.Codes.Confirmed);
				AddTransaction("Fourth-PEN", -40m, PermitTransactionStatusList.Codes.Pending);

				var nctsHeaderDepartureAndArrival = Factory.New<NctsHeader>();
				nctsHeaderDepartureAndArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
				nctsHeaderDepartureAndArrival.BH_JobReference = "Reference";
				var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDepartureAndArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = mrnCode;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				nctsHeaderDepartureAndArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
				nctsHeaderDepartureAndArrival.BH_CustomsProfile = "TestCert1";
				nctsHeaderDepartureAndArrival.LocalReferenceNumber = "AH3";

				var guarantee = nctsHeaderDepartureAndArrival.Guarantees.AddNew();
				guarantee.PW_BondNumber = guaranteeReference;
				guarantee.PW_BondAmount = 1000m;

				Factory.Save();

				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var mockHeader = new Mock<NctsHeader>(Factory, ((IBusinessObjectFactoryInternals)Factory).RowFactory.LoadFromPK("CusInBondHeader", nctsHeaderDepartureAndArrival.PK));
					mockHeader.Setup(a => a.CreateRequestAndGetResponse(mrnCode, cert)).Returns(writtenOffTransitFromCustomsTestFileContent);
					mockHeader.CallBase = true;
					var mockHeaderObject = mockHeader.Object;

					var newFactory = new BusinessObjectFactory();
					var nctsHeaders = new NctsHeaderCollection(newFactory);
					nctsHeaders.Add(mockHeaderObject);

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when no movement was selected", "Please select at least one movement", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						var resultList = module.WriteOffResultCollection.Cast<WriteOffResult>();
						AssertEquals("ResultList contains 1 element", 1, resultList.Count());
						AssertWriteOffResult(resultList.FirstOrDefault(), "Reference header with all data", "Reference", "Written Off", mrnCode, declarationStatus: "Ultimado");

						var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
						var guarantee1Transactions = Factory.Load<CusGuaranteeHeader>(query).First(x => x.CPH_Number == guaranteeReference).GetTransactions();
						AssertEquals("5 Original transactions + 1 new one", 6, guarantee1Transactions.Count());

						var transaction = guarantee1Transactions.First(x => x.CPL_Comment.StartsWith("Write-off"));
						AssertEquals("Transaction.CPL_Reference", mrnCode, transaction.CPL_Reference);
						AssertEquals("Transaction.CPL_Comment", "Write-off NCTS Departure AH3", transaction.CPL_Comment);
						AssertEquals("Transaction.CPL_TranValue", 120m, transaction.CPL_TranValue);
						AssertEquals("Transaction.CPL_TransactionDate", new ZDateTime(2022, 04, 20), transaction.CPL_TransactionDate);
						AssertEquals("Transaction.CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
					});
				}

				void AddTransaction(ZString comment, ZDecimal value, ZString status)
				{
					var transaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
					transaction1.CPL_Reference = mrnCode;
					transaction1.CPL_TranValue = value;
					transaction1.CPL_TransactionStatus = status;
					transaction1.CPL_Comment = comment;
					transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_NoDepartureDeclaration_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var nctsHeaderArrival1 = Factory.New<NctsHeader>();
					nctsHeaderArrival1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
					nctsHeaderArrival1.BH_JobReference = "Arrival1";
					var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderArrival1, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012812";
					newEntryNumber.CE_EntryIsSystemGenerated = true;

					var nctsHeaderArrival2 = Factory.New<NctsHeader>();
					nctsHeaderArrival2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
					nctsHeaderArrival2.BH_JobReference = "Arrival2";
					newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderArrival2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012813";
					newEntryNumber.CE_EntryIsSystemGenerated = true;

					var nctsHeaders = new NctsHeaderCollection(Factory)
					{
						nctsHeaderArrival1,
						nctsHeaderArrival2
					};

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when no movement was selected", "Please select at least one movement", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all declarations selected are Arrival", "None of the selected movements match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_DeparturesWithNoMRN_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var nctsHeaderDeparture = Factory.New<NctsHeader>();
					nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
					nctsHeaderDeparture.BH_JobReference = "Departure";

					var nctsHeaderDepartureAndArrival = Factory.New<NctsHeader>();
					nctsHeaderDepartureAndArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
					nctsHeaderDepartureAndArrival.BH_JobReference = "DepartureAndArrival";

					var nctsHeaders = new NctsHeaderCollection(Factory)
					{
						nctsHeaderDeparture,
						nctsHeaderDepartureAndArrival
					};

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when no movement was selected", "Please select at least one movement", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all declarations selected ade Departure or DepartureAndArrival but without mrn", "None of the selected movements match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_DeparturesWithNoGuarantees_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var nctsHeaderDeparture = Factory.New<NctsHeader>();
					nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
					nctsHeaderDeparture.BH_JobReference = "Departure";
					var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDeparture, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012812";
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096", addOBLTransaction: false);
					ModuleTestHelper.AddGuaranteeToHeader(nctsHeaderDeparture, "16ESAGL9990000096");

					var nctsHeaderDepartureAndArrival = Factory.New<NctsHeader>();
					nctsHeaderDepartureAndArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
					nctsHeaderDepartureAndArrival.BH_JobReference = "DepartureAndArrival";
					var departureAndArrivalMrn = "20ES00999950012811";
					newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDepartureAndArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = departureAndArrivalMrn;
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096", withOldEndDate: true);
					ModuleTestHelper.AddGuaranteeToHeader(nctsHeaderDepartureAndArrival, "22ESAGL9990000096");

					var nctsHeaders = new NctsHeaderCollection(Factory)
					{
						nctsHeaderDeparture,
						nctsHeaderDepartureAndArrival
					};

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when no movement was selected", "Please select at least one movement", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all declarations selected ade Departure or DepartureAndArrival with mrn but without a correct guarantee associated", "None of the selected movements match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_DeparturesWithoutBrokerOrCertificate_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mockSettings.Object))
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

				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var nctsHeaderDeparture = Factory.New<NctsHeader>();
					nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
					nctsHeaderDeparture.BH_JobReference = "Departure";
					var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDeparture, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012812";
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096");
					ModuleTestHelper.AddGuaranteeToHeader(nctsHeaderDeparture, "16ESAGL9990000096");

					var nctsHeaderDepartureAndArrival = Factory.New<NctsHeader>();
					nctsHeaderDepartureAndArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
					nctsHeaderDepartureAndArrival.BH_JobReference = "DepartureAndArrival";
					var departureAndArrivalMrn = "20ES00999950012811";
					newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDepartureAndArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = departureAndArrivalMrn;
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096");
					ModuleTestHelper.AddGuaranteeToHeader(nctsHeaderDepartureAndArrival, "22ESAGL9990000096");

					var nctsHeaders = new NctsHeaderCollection(Factory)
					{
						nctsHeaderDeparture,
						nctsHeaderDepartureAndArrival
					};

					var messageError = "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate in the Details tab in the following declarations: Departure, DepartureAndArrival.";

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when no movement was selected", "Please select at least one movement", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all declarations selected ade Departure or DepartureAndArrival with mrn and correct guarantees associated but no broker is declared", messageError, UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeaderDeparture.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
						nctsHeaderDeparture.BH_CustomsProfile = ZString.Empty;
						nctsHeaderDepartureAndArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
						nctsHeaderDepartureAndArrival.BH_CustomsProfile = ZString.Empty;
						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but no certificate is declared", messageError, UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeaderDeparture.BH_CustomsProfile = "INVALID";
						nctsHeaderDepartureAndArrival.BH_CustomsProfile = "INVALID";
						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but certificate declared is invalid", messageError, UnitTestUserNotification.Instance.LastMessage.Text);

						nctsHeaderDeparture.BH_CustomsProfile = cert.GP_Name;
						nctsHeaderDepartureAndArrival.BH_CustomsProfile = cert.GP_Name;
						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but current user has no authorisation for certificate declared", messageError, UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[RequiresSTA]
		public void TestGuaranteeCheckTransitStatusMenuItemFunctionality_CorrectDepartures_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mockSettings.Object))
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

				using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				using (var module = new NctsMovementModuleForTest())
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					var checkTransitStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check transit status");

					var nctsHeaderDeparture = Factory.New<NctsHeader>();
					nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
					nctsHeaderDeparture.BH_JobReference = "Departure";
					var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDeparture, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012812";
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096");
					ModuleTestHelper.AddGuaranteeToHeader(nctsHeaderDeparture, "16ESAGL9990000096");
					nctsHeaderDeparture.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
					nctsHeaderDeparture.BH_CustomsProfile = cert.GP_Name;

					var nctsHeaderDepartureAndArrival = Factory.New<NctsHeader>();
					nctsHeaderDepartureAndArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					nctsHeaderDepartureAndArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
					nctsHeaderDepartureAndArrival.BH_JobReference = "DepartureAndArrival";
					var departureAndArrivalMrn = "20ES00999950012811";
					newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderDepartureAndArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = departureAndArrivalMrn;
					newEntryNumber.CE_EntryIsSystemGenerated = true;
					ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096");
					ModuleTestHelper.AddGuaranteeToHeader(nctsHeaderDepartureAndArrival, "22ESAGL9990000096");
					nctsHeaderDepartureAndArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
					nctsHeaderDepartureAndArrival.BH_CustomsProfile = cert.GP_Name;

					var nctsHeaderArrival = Factory.New<NctsHeader>();
					nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					nctsHeaderArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
					nctsHeaderArrival.BH_JobReference = "Arrival";
					newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
					newEntryNumber.CE_EntryNum = "20ES00999950012812";
					newEntryNumber.CE_EntryIsSystemGenerated = true;

					Factory.Save();

					var nctsHeaders = new NctsHeaderCollection(Factory)
					{
						nctsHeaderDeparture,
						nctsHeaderDepartureAndArrival,
						nctsHeaderArrival
					};

					module.DisplayGrid.SetDataBinding(nctsHeaders, "");
					CombineAssertions(() =>
					{
						module.DisplayGrid.Select();
						module.DisplayGrid.Focus();

						module.DisplayGrid.SelectAllElements();
						checkTransitStatusMenuItem.PerformClick();
						AssertEquals("Messages sent correctly", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("2 Messages sent successfully."));

						AssertEquals("NctsHeader message status has changed to SNT for nctsHeaderDeparture because it is Phase5", LogicalStatusList.Codes.Sent, nctsHeaderDeparture.MovementHeader.BM_MessageStatus);
						AssertEquals("NctsHeader message status has changed to MDS for nctsHeaderDepartureAndArrival because it is Phase4", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeaderDepartureAndArrival.BH_MessageStatus);
						AssertEquals("NctsHeader message status has not changed for nctsHeaderArrival because it is not D or DA", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, nctsHeaderArrival.BH_MessageStatus);

						ModuleTestHelper.AssertMessageSent("Departure", nctsHeaderDeparture);
						ModuleTestHelper.AssertMessageSent("DepartureAndArrival", nctsHeaderDepartureAndArrival);
						AssertEquals("Arrival has no messages", 0, nctsHeaderArrival.Messages.Count);
					});
				}
			}
		}

		public override void TestExceptionsFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		void AssertWriteOffResult(WriteOffResult writeOffResult, string message, string jobNumber, string guaranteeStatus, string mrn = "", string declarationStatus = "-")
		{
			AssertEquals(message + ", JobNumber", jobNumber, writeOffResult.JobNumber);
			AssertEquals(message + ", Mrn", mrn, writeOffResult.Mrn);
			AssertEquals(message + ", GuaranteeStatus", guaranteeStatus, writeOffResult.GuaranteeStatus);
			AssertEquals(message + ", DeclarationStatus", declarationStatus, writeOffResult.DeclarationStatus);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			collection.Add(header);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.NctsMovementModule;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		string GetTestFileContent(string fileName)
		{
			const string testFilePath = "Enterprise.Customs.ES.NCTS.Module.Testing.TestFiles";
			return TestFileReader.GetEmbeddedFileText(testFilePath, fileName);
		}

		TestFileReader TestFileReader => testFileReader ?? (testFileReader = new TestFileReader(typeof(NctsMovementModuleTest)));
		TestFileReader testFileReader;

		class NctsMovementModuleForTest : NctsMovementModule
		{
			public WriteOffResultCollection WriteOffResultCollection;
			protected override void ShowTransitResultsGrid(WriteOffResultCollection writeOffResultCollection) => WriteOffResultCollection = writeOffResultCollection;
		}
	}
}
