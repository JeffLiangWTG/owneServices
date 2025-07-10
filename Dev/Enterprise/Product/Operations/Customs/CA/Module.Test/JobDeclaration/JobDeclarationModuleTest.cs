using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		public void TestCopyToNewVersionForB2_IM2()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				var copyToNewB2Menu = module.GetNewActionMenuItems().FindByText("Copy to new version for B2");
				AssertNotNull("Copy to new version for B2 menu item is invisible", copyToNewB2Menu);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information Please select one Declaration to copy.", UnitTestUserNotification.Instance.LastMessage.ToString());
				module.selectedElements = new JobDeclaration[1];
				module.selectedElements[0] = declaration;
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information You may only copy Import or other Import Copy for B2 type declarations to a new version for B2.", UnitTestUserNotification.Instance.LastMessage.ToString());
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information You may only copy declarations that have been accounted for.", UnitTestUserNotification.Instance.LastMessage.ToString());
				declaration.CA_K84AccountingDate = new ZDateTime(2016, 06, 12);

				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006789";

				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information You may only copy declarations that have been accepted (decided).", UnitTestUserNotification.Instance.LastMessage.ToString());
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var nextJob = Factory.New<JobDeclaration>();
				nextJob.JE_DeclarationReference = "B00000003";
				nextJob.CA_OriginalTransactionNo = "12345000067897";
				nextJob.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				nextJob.CA_Version = 2;
				Factory.Save();
				copyToNewB2Menu.PerformClick();
				AssertEquals("Information The new version already exists. If you wish to ignore the existing version then open that version and deactivate it by selecting Actions=>Mark Inactive, then retry the copy to a new version.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestCreatePRECARMAdjustmentEntry()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				var createPRECARMAdjustmentEntryMenu = module.GetNewActionMenuItems().FindByText("Create PRECARM adjustment entry");
				AssertNull("Create PRECARM adjustment entry menu item is invisible", createPRECARMAdjustmentEntryMenu);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				using (var module = new JobDeclarationModuleForTest())
				{
					module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
					var createPRECARMAdjustmentEntryMenu = module.GetNewActionMenuItems().FindByText("Create PRECARM adjustment entry");
					AssertNotNull("Create PRECARM adjustment entry menu item is visible", createPRECARMAdjustmentEntryMenu);

					createPRECARMAdjustmentEntryMenu.PerformClick();
					AssertEquals("Information Please select one Declaration.", UnitTestUserNotification.Instance.LastMessage.ToString());

					var declaration = Factory.New<JobDeclaration>();
					module.selectedElements = new JobDeclaration[1];
					module.selectedElements[0] = declaration;
					createPRECARMAdjustmentEntryMenu.PerformClick();
					AssertEquals("Information You may only create PRECARM adjustment entry to B3 Lodged declarations.", UnitTestUserNotification.Instance.LastMessage.ToString());

					var entryHeader = declaration.ActiveEntryHeaders.AddNew();
					entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
					createPRECARMAdjustmentEntryMenu.PerformClick();
					AssertEquals("Information You may not create PRECARM adjustment entry with B3 accept date older than 4 years.", UnitTestUserNotification.Instance.LastMessage.ToString());

					entryHeader.CH_EntryReleaseDate = ZDateTime.Now.AddYears(-4).AddSeconds(-1);
					createPRECARMAdjustmentEntryMenu.PerformClick();
					AssertEquals("Information You may not create PRECARM adjustment entry with B3 accept date older than 4 years.", UnitTestUserNotification.Instance.LastMessage.ToString());

					entryHeader.CH_EntryReleaseDate = ZDateTime.Now;
					declaration.GetNewRelatedDeclaration(Factory, JobDeclaration.PRECARMAdjustmentDeclarationRelationshipType);
					Factory.Save();

					createPRECARMAdjustmentEntryMenu.PerformClick();
					AssertEquals("Information The PRECARM adjustment entry already exists.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public void TestShowTemplateCopyForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var module = new JobDeclarationModuleForTest())
			using (module.ShowPopup())
			{
				module.CurrentBusinessObjectInGrid_Exposed = declaration;
				module.ShowTemplateCopyForm_Exposed();
				AssertNotEquals("Copy Allowed For Non-IM2", CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, UnitTestUserNotification.Instance.LastMessage?.Text);

				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				module.ShowTemplateCopyForm_Exposed();
				AssertEquals("CopyNotAllowedForIM2", CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, UnitTestUserNotification.Instance.LastMessage?.Text);

				declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				module.ShowTemplateCopyForm_Exposed();
				AssertNotEquals("Copy Allowed For BX3", CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, UnitTestUserNotification.Instance.LastMessage?.Text);
			}
		}

		int referenceNum = 100010;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			CreateChgCodes(factory);
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			var organisations = factory.GetCachedValue("BaseOrganisationDeclarationModuleTest", delegate
				{
					return factory.Load<OrgHeader>(new ZQuery() { MaximumRows = 24 });
				});

			var unlocos = factory.GetCachedValue("BaseUNLOCODeclarationModuleTest", delegate
			{
				return factory.Load<RefUNLOCO>(new ZQuery() { MaximumRows = 11 });
			});

			declaration.JE_OH_ShippingLine = organisations[(i + 2) % organisations.Length].PK;
			declaration.JE_OH_Forwarder = organisations[(i + 3) % organisations.Length].PK;
			declaration.JE_RL_NKPortOfFirstArrival = unlocos[i].RL_Code;
			declaration.CA_K84AccountingDate = ZDateTime.Now;
			declaration.CA_K84StatementDate = ZDateTime.Now;
			declaration.JE_DateOfFirstArrival = ZDateTime.Now;
			declaration.JE_WarehouseReleaseDate = ZDateTime.Now;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.CA_OGDStatus = AVSStatusList.Codes.NotValidated;
			declaration.CA_AccountingAge = 1;
			declaration.ImporterOfRecordAddress.OrganisationPK = organisations[(i + 4) % organisations.Length].PK;

			var document = declaration.DocsAndCartage.RequiredDocuments.AddNew();
			var addinfo = document.AddInfos.AddNew();
			addinfo.EX_ApplicationCode = "CAD";
			addinfo.EX_ReferenceNumber = referenceNum++.ToString();
			addinfo.EX_Status = Common.CA.DIF.StatusList.Codes.AwaitingOriginal;

			string twoDigitsNumber = i.ToString().PadLeft(2, '0');

			declaration.CA_PlaceOfReport = "PR" + twoDigitsNumber;
			declaration.CA_PortOfExit = "PE" + twoDigitsNumber;
			declaration.CA_ReasonForExportCode = twoDigitsNumber;
			declaration.CA_TransportDocumentNumber = "TD" + twoDigitsNumber;
			declaration.JE_CustomsOffice = "PC" + twoDigitsNumber;
			declaration.CA_UnladingOffice = "PU" + twoDigitsNumber;
			declaration.JE_CarrierCode = "CC" + twoDigitsNumber;
			declaration.JE_LocationOfGoods = "SL" + twoDigitsNumber;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN" + twoDigitsNumber;
			declaration.JE_CERSProofOfReportNumber = "POR" + twoDigitsNumber;
			declaration.CA_DeclarationException = "010";

			var b3EntryHeader = declaration.ActiveEntryHeaders.AddNew();
			b3EntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			b3EntryHeader.CH_Status = MessageStatusList.Codes.Sent;
			b3EntryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			b3EntryHeader.PopulateEntrySubmittedDateIfRequired();
			b3EntryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			b3EntryHeader.CH_EntryReleaseDate = new ZDateTime(2020, 8, 19);
			var entryLine = b3EntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 10000 + i;

			var relEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			relEntryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			relEntryHeader.CH_Status = MessageStatusList.Codes.Sent;
			relEntryHeader.PopulateEntrySubmittedDateIfRequired();

			var relEntryLine = relEntryHeader.MergedLines.AddNew();
			relEntryLine.CL_CustomsValue = 10000 + i;

			var g7ExportEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			g7ExportEntryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			g7ExportEntryHeader.CH_Status = MessageStatusList.Codes.Sent;
			g7ExportEntryHeader.PopulateEntrySubmittedDateIfRequired();

			var g7ExportEntryLine = g7ExportEntryHeader.MergedLines.AddNew();
			g7ExportEntryLine.CL_CustomsValue = 10000 + i;

			if (messageType == JobMessageTypeList.Codes.Import)
			{
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 10 + i);
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 20 + i);
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 30 + i);
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 40 + i);
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 50 + i);
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 60 + i);
				CreateTransactionData(declaration, i);

				declaration.CA_BondType = BondTypeList.Codes.SingleTransactionBond;
				declaration.CA_BondNo = "12345671234567123456712345671234567";
				declaration.CA_SuretyCode = "ABC";
			}

			return declaration;
		}

		protected override List<string> FetchHintIgnoreField
		{
			get
			{
				var result = base.FetchHintIgnoreField;
				result.Add("CA_AccountingAge");//will always be zero if both Release and K4Account dates are set
				result.Add("TotalBilledAmount");//this is in a dynamic collection - so fetch hints wont work.
				result.Add("TotalOutstandingAmount");//this is in a dynamic collection - so fetch hints wont work.
				result.Add("TotalInvoicedAmount");//this is in a dynamic collection - so fetch hints wont work.
				return result;
			}
		}

		sealed class JobDeclarationModuleForTest : JobDeclarationModule
		{
			internal new ZController GetNewController(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);

			internal new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();

			internal BusinessObject[] selectedElements;

			internal BusinessObject CurrentBusinessObjectInGrid_Exposed;

			internal IZForm ShowTemplateCopyForm_Exposed() => ShowTemplateCopyForm(CurrentBusinessObjectInGrid);

			protected override BusinessObject[] GetSelectedElements() => selectedElements;

			protected override BusinessObject CurrentBusinessObjectInGrid => CurrentBusinessObjectInGrid_Exposed;
		}
	}
}
