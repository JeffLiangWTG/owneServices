using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Customs.Manifest.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusReconDeclaration))]
	class CusReconDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2021, 10, 13)]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CRD_ApplicationCode", CusReconDeclarationApplicationCodeList.Codes.CLS, declaration.CRD_ApplicationCode);
				AssertEquals("CRD_PeriodFrom", new ZDate(2021, 10, 1), declaration.CRD_PeriodFrom);
				AssertEquals("CRD_PeriodTo", new ZDate(2021, 10, 31), declaration.CRD_PeriodTo);
			});
		}

		public void TestCRD_MessageStatus_ReadOnly()
		{
			AssertEquals(true, declaration.CRD_MessageStatusInfo.ReadOnly);
		}

		public void TestCRD_ApplicationCode_ReadOnly()
		{
			AssertEquals(true, declaration.CRD_ApplicationCodeInfo.ReadOnly);
		}

		public void TestCRD_CustomsStatus_ReadOnly()
		{
			AssertEquals(true, declaration.CRD_CustomsStatusInfo.ReadOnly);
		}

		public void TestRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZString.Empty, declaration.RegistrationNumber);

				var entryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				entryNumber.CE_EntryNum = "12345678";

				AssertEquals("Has CusEntryNumber", "12345678", declaration.RegistrationNumber);
			});
		}

		public void TestRegistrationNumber_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(nameof(declaration.RegistrationNumber));
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Registration Number", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Reg. Number", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Reg. No.", resourceStringData.ShortCaption);
			});
		}

		public void TestProcedure_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(nameof(declaration.Procedure));
			AssertEquals("Procedure", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestIsFinalized()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No FinalizationFlag note", false, declaration.IsFinalized);

				declaration.CreateFinalizationFlagNote(MonthlyClosingHelper.DeclarationIsFinalizedFlag);
				AssertEquals("Has FinalizationFlag note with value '1'", true, declaration.IsFinalized);

				declaration.CreateFinalizationFlagNote("0");
				AssertEquals("Has FinalizationFlag note with value '0'", false, declaration.IsFinalized);
			});
		}

		public void TestIsFinalized_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(nameof(declaration.IsFinalized));
			AssertEquals("Is Finalized?", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCanSendMessage()
		{
			AssertEquals(true, declaration.CanSendMessage);
		}

		public void TestCanSendMessageSNT()
		{
			declaration.CRD_MessageStatus = MessageStatusList.Codes.Sent;
			AssertEquals(false, declaration.CanSendMessage);
		}

		public void TestCusReconEntries()
		{
			AssertType<CusReconEntryCollection>(declaration.CusReconEntries);
		}

		public void TestGetNewValidation()
		{
			AssertType<CusReconDeclarationValidation>(declaration.Validation);
		}

		public void TestGetNewLookups()
		{
			AssertType<CusReconDeclarationLookups>(declaration.Lookups);
		}

		public void TestMessages()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = declaration;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = declaration;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { message1, message2 }, declaration.Messages.Cast<EDIMessage>());
				AssertEquals("IsManagedForDataRefresh", true, declaration.Messages.IsManagedForDataRefresh);
			});
		}

		public void TestIDocManagerSupport()
		{
			CombineAssertions(() =>
			{
				AssertType<DocManagerInfo>("Type", ((IDocManagerSupport)declaration).DocManagerInfo);
				AssertEquals("DocManagerCode", Core.Constants.DocManagerCodes.GermanyMonthlyClosing, ((IDocManagerSupport)declaration).DocManagerInfo.DocManagerCode);
			});
		}

		[TestDate(2020, 08, 15)]
		public void TestJobReferenceNumberOnSaving()
		{
			var registryItem = DECustomsDataRegistry.Instance.MonthlyClosingJobNumberCustomization;
			var customization = new ManifestJobNumberCustomisation();
			ManifestJobNumberCustomisationTest.Set(customization, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 1, false);
			ManifestJobNumberCustomisationTest.Set(customization, BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits, 2, false);
			ManifestJobNumberCustomisationTest.Set(customization, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 3, false, "4");
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customization))
			{
				CombineAssertions(() =>
				{
					declaration.CRD_JobReferenceNumber = ZString.Empty;
					AssertNoExceptionThrown("Save when CRD_JobReferenceNumber is empty", () => Factory.Save());

					var referenceNumber = declaration.CRD_JobReferenceNumber;
					var expectedReferenceNumber = string.Format("MON{0}0820200000001", GlbCompany.CurrentCompany.GC_Code);
					AssertEquals("CRD_JobReferenceNumber is set based on registry", expectedReferenceNumber, referenceNumber);

					AssertNoExceptionThrown("Save again", () => Factory.Save());
					AssertEquals("CRD_JobReferenceNumber is not overwritten", referenceNumber, declaration.CRD_JobReferenceNumber);
				});
			}
		}

		[UseSnapshotProtection]
		public void TestJobReferenceNumberOnSaving_NoDuplicateReferenceException()
		{
			var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
			declaration.OnSaving();
			var jobReference = declaration.CRD_JobReferenceNumber;
			dbConnection.RollbackTransaction();

			var newFactory = new BusinessObjectFactory();
			var declaration2 = newFactory.New<CusReconDeclaration>();
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Saving declaration2", () => newFactory.Save());
				AssertEquals("declaration and declaration2 have the same JobReferenceNumber", jobReference, declaration2.CRD_JobReferenceNumber);

				dbConnection.BeginTransaction();
				AssertEquals("Before saved, declaration has duplicate JobReferenceNumber", jobReference, declaration.CRD_JobReferenceNumber);
				AssertNoExceptionThrown("Saving declaration", () => Factory.Save());
				AssertNotEquals("After saved, declaration has unique JobReferenceNumber", jobReference, declaration.CRD_JobReferenceNumber);
			});
		}

		public void TestCRD_PeriodFrom_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_PeriodFromInfo);

		public void TestCRD_PeriodTo_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_PeriodToInfo);

		public void TestCRD_DeclarationType_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_DeclarationTypeInfo);

		public void TestCRD_DeclarantType_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_DeclarantTypeInfo);

		public void TestCRD_OA_DeclarantAddress_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_OA_DeclarantAddressInfo);

		public void TestCRD_OA_RepresentativeAddress_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_OA_RepresentativeAddressInfo);

		public void TestCRD_OA_BuyingAgentAddress_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_OA_BuyingAgentAddressInfo);

		public void TestCRD_CPH_ReconClearanceAuthorisation_ReadOnly() => AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(declaration.CRD_CPH_ReconClearanceAuthorisationInfo);

		public void TestCRD_OA_BuyingAgentAddress_Caption()
		{
			var propertyResourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.CRD_OA_BuyingAgentAddressInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Short Caption", "Represented", propertyResourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Represented Party", propertyResourceStringData.MediumCaption);
				AssertEquals("Caption", "Represented Party", propertyResourceStringData.Caption);
			});
		}

		public void TestIsDeclarantImporter_Caption()
		{
			AssertEquals("Declarant is Importer?", DataBoundResourceStrings.GetDataForProperty(declaration.IsDeclarantImporterInfo).Caption);
		}

		public void TestIsDeclarantImporter_ReadOnly()
		{
			CombineAssertions(() =>
			{
				declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
				AssertEquals("CRD_OA_DeclarantAddress empty", false, declaration.CRD_OA_DeclarantAddress.IsValid);
				AssertEquals("CRD_OA_DeclarantAddress empty, IsDeclarantImporter readonly", true, declaration.IsDeclarantImporterInfo.ReadOnly);

				declaration.CRD_OA_DeclarantAddress = ZGuid.Invalid;
				AssertEquals("CRD_OA_DeclarantAddress invalid", false, declaration.CRD_OA_DeclarantAddress.IsValid);
				AssertEquals("CRD_OA_DeclarantAddress invalid, IsDeclarantImporter readonly", true, declaration.IsDeclarantImporterInfo.ReadOnly);

				declaration.CRD_OA_DeclarantAddress = Factory.New<OrgAddress>().PK;
				AssertEquals("CRD_OA_DeclarantAddress valid", true, declaration.CRD_OA_DeclarantAddress.IsValid);
				AssertEquals("CRD_OA_DeclarantAddress valid, IsDeclarantImporter editable", false, declaration.IsDeclarantImporterInfo.ReadOnly);

				declaration.CusReconEntries.AddNew();
				AssertEquals("Declaration is linked to a Simplified Declaration", true, declaration.IsDeclarantImporterInfo.ReadOnly);
			});
		}

		public void TestIsDeclarantImporterAddressChanges()
		{
			var declarantAddress1 = Factory.New<OrgAddress>();
			var declarantAddress2 = Factory.New<OrgAddress>();
			declaration.CRD_OA_DeclarantAddress = declarantAddress1.PK;
			declaration.IsDeclarantImporter = true;

			CombineAssertions(() =>
			{
				AssertEquals("CRD_OA_ImporterAddress same as CRD_OA_DeclarantAddress", declarantAddress1.PK, declaration.CRD_OA_ImporterAddress);

				declaration.CRD_OA_DeclarantAddress = declarantAddress2.PK;
				AssertEquals("Changing CRD_OA_DeclarantAddress to declarantAddress2, IsDeclarantImporter = False", false, declaration.IsDeclarantImporter);
				AssertEquals("Changing CRD_OA_DeclarantAddress to declarantAddress2, CRD_OA_ImporterAddress empty", ZGuid.Empty, declaration.CRD_OA_ImporterAddress);

				declaration.IsDeclarantImporter = true;
				AssertEquals("Changing IsDeclarantImporter to True, CRD_OA_ImporterAddress same as CRD_OA_DeclarantAddress", declarantAddress2.PK, declaration.CRD_OA_ImporterAddress);

				declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
				AssertEquals("Clearing CRD_OA_DeclarantAddress, IsDeclarantImporter = False", false, declaration.IsDeclarantImporter);
				AssertEquals("Clearing CRD_OA_DeclarantAddress, CRD_OA_ImporterAddress empty", ZGuid.Empty, declaration.CRD_OA_ImporterAddress);
			});
		}

		public void TestIsDeclarantImporterTickChanges()
		{
			var orgAddressPK = Factory.New<OrgAddress>().PK;
			declaration.CRD_OA_DeclarantAddress = orgAddressPK;

			CombineAssertions(() =>
			{
				AssertEquals("Initial value", false, declaration.IsDeclarantImporter);

				declaration.IsDeclarantImporter = true;
				AssertEquals("IsDeclarantImporter set to true", declaration.CRD_OA_DeclarantAddress, declaration.CRD_OA_ImporterAddress);

				declaration.IsDeclarantImporter = false;
				AssertEquals("IsDeclarantImporter set to false (CRD_OA_ImporterAddress)", ZGuid.Empty, declaration.CRD_OA_ImporterAddress);
				AssertEquals("IsDeclarantImporter set to false (CRD_OA_DeclarantAddress)", orgAddressPK, declaration.CRD_OA_DeclarantAddress);
			});
		}

		public void TestCRD_CustomsOffice_ReadOnly()
		{
			AssertEquals("CustomsOffice is always read only.", true, declaration.CRD_CustomsOfficeInfo.ReadOnly);
		}

		public void TestCRD_CPH_ReconClearanceAuthorisation_Populate()
		{
			var permitOwner = Factory.NewWithValidTestData<OrgHeader>();
			var authorisationHeader = TestHelper.CreateAuthorisationRecord(permitOwner, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEIPO12345678901");
			Factory.Save();

			declaration.CRD_OA_DeclarantAddress = permitOwner.MainAddress.PK;
			declaration.CRD_PeriodFrom = ZDate.Today;
			declaration.CRD_PeriodTo = ZDate.Today;
			declaration.CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;

			CombineAssertions(() =>
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VAV;
				AssertEquals("Set CRD_DeclarationType", authorisationHeader.PK, declaration.CRD_CPH_ReconClearanceAuthorisation);

				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZ;
				AssertEquals("Clear if not only one AuthorisationHeader was found", declaration.CRD_CPH_ReconClearanceAuthorisation, ZGuid.Empty);

				declaration.CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VAV;
				declaration.CRD_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
				AssertEquals("Set CRD_DeclarantType", authorisationHeader.PK, declaration.CRD_CPH_ReconClearanceAuthorisation);

				declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
				declaration.CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;
				declaration.CRD_OA_DeclarantAddress = permitOwner.MainAddress.PK;
				AssertEquals("Set CRD_OA_DeclarantAddress", authorisationHeader.PK, declaration.CRD_CPH_ReconClearanceAuthorisation);

				declaration.CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;
				declaration.CRD_OA_RepresentativeAddress = permitOwner.MainAddress.PK;
				AssertEquals("Set CRD_OA_RepresentativeAddress", authorisationHeader.PK, declaration.CRD_CPH_ReconClearanceAuthorisation);

				declaration.CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;
				declaration.CRD_OA_BuyingAgentAddress = permitOwner.MainAddress.PK;
				AssertEquals("Set CRD_OA_BuyingAgentAddress", authorisationHeader.PK, declaration.CRD_CPH_ReconClearanceAuthorisation);
			});
		}

		public void TestCRD_CustomsOffice()
		{
			var permit = Factory.New<CusAuthorisationHeader>();
			permit.CPH_Number = "DEEIR5864A1000701";

			CombineAssertions(() =>
			{
				AssertEquals("Empty Customes office", ZString.Empty, declaration.CRD_CustomsOffice);

				declaration.CRD_CPH_ReconClearanceAuthorisation = permit.PK;
				AssertEquals("Initial Customes office value", "DE005864", declaration.CRD_CustomsOffice);

				declaration.CRD_CPH_ReconClearanceAuthorisation = ZGuid.Empty;
				AssertEquals("Clearing CRD_CPH_ReconClearanceAuthorisation", ZString.Empty, declaration.CRD_CustomsOffice);
			});
		}

		public void TestCRD_DeclarantType_Captions()
		{
			var propertyResourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.CRD_DeclarantTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Medium Caption", "Rep. Type", propertyResourceStringData.MediumCaption);
				AssertEquals("Caption", "Representation Type", propertyResourceStringData.Caption);
			});
		}

		public void TestIRelatedJobMembers()
		{
			declaration.CRD_JobReferenceNumber = "MONEDI0820200000001";
			CombineAssertions(() =>
			{
				var declarationAsIRelatedJob = declaration as IRelatedJob;
				AssertEquals("JobNumber", "MONEDI0820200000001", declarationAsIRelatedJob.JobNumber);
				AssertEquals("JobDescription", "Monthly Closing Job - MONEDI0820200000001", declarationAsIRelatedJob.JobDescription);
				AssertEquals("JobStatus", ZString.Empty, declarationAsIRelatedJob.JobStatus);

				var declarationAsIControllerIDProvider = declaration as IControllerIDProvider;
				AssertEquals("ControllerID", ControllerIDs.Customs.DE.MonthlyClosing, declarationAsIControllerIDProvider.ControllerID);
				AssertEquals("BusinessObjectPK", declaration.PK, declarationAsIControllerIDProvider.BusinessObjectPK);
			});
		}

		public void TestLineStatusSummary()
		{
			AssertEquals("Precondition", 0, declaration.CusReconEntries.Count);
			AssertEquals("Lines = 0", declaration.LineStatusSummary);

			var entry1 = declaration.CusReconEntries.AddNew();
			var entry2 = declaration.CusReconEntries.AddNew();

			entry1.CusReconEntryLines.AddNew();
			entry1.CusReconEntryLines.AddNew();
			entry2.CusReconEntryLines.AddNew();

			AssertEquals("Lines = 3 (Open = 3)", declaration.LineStatusSummary);

			var rc2Line1 = entry1.CusReconEntryLines.AddNew();
			rc2Line1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.RC2;
			var rc2Line2 = entry1.CusReconEntryLines.AddNew();
			rc2Line2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.RC2;
			var rc2Line3 = entry2.CusReconEntryLines.AddNew();
			rc2Line3.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.RC2;
			var rc2Line4 = entry2.CusReconEntryLines.AddNew();
			rc2Line4.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.RC2;

			AssertEquals("Lines = 7 (Open = 3, RC2 = 4)", declaration.LineStatusSummary);

			var rejLine1 = entry1.CusReconEntryLines.AddNew();
			rejLine1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.REJ;
			var rejLine2 = entry1.CusReconEntryLines.AddNew();
			rejLine2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.REJ;
			var rejLine3 = entry2.CusReconEntryLines.AddNew();
			rejLine3.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.REJ;
			var rejLine4 = entry2.CusReconEntryLines.AddNew();
			rejLine4.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.REJ;
			var rejLine5 = entry2.CusReconEntryLines.AddNew();
			rejLine5.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.REJ;

			AssertEquals("Lines = 12 (Open = 3, REJ = 5, RC2 = 4)", declaration.LineStatusSummary);

			var errLine1 = entry1.CusReconEntryLines.AddNew();
			errLine1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;
			var errLine2 = entry1.CusReconEntryLines.AddNew();
			errLine2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;
			var errLine3 = entry2.CusReconEntryLines.AddNew();
			errLine3.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;
			var errLine4 = entry2.CusReconEntryLines.AddNew();
			errLine4.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;
			var errLine5 = entry2.CusReconEntryLines.AddNew();
			errLine5.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;
			var errLine6 = entry2.CusReconEntryLines.AddNew();
			errLine6.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;

			AssertEquals("Lines = 18 (Open = 3, REJ = 5, RC2 = 4, ERR = 6)", declaration.LineStatusSummary);

			var tx1Line = entry1.CusReconEntryLines.AddNew();
			tx1Line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX1;
			var tx2Line = entry1.CusReconEntryLines.AddNew();
			tx2Line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX2;
			var tx3Line = entry1.CusReconEntryLines.AddNew();
			tx3Line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX3;
			var tx4Line = entry1.CusReconEntryLines.AddNew();
			tx4Line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX4;
			var tx5Line = entry2.CusReconEntryLines.AddNew();
			tx5Line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			var tx6Line = entry2.CusReconEntryLines.AddNew();
			tx6Line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX6;
			var txrLine = entry2.CusReconEntryLines.AddNew();
			txrLine.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TXR;

			AssertEquals("Lines = 25 (Open = 3, REJ = 5, RC2 = 4, ERR = 6, TX* = 7)", declaration.LineStatusSummary);
		}

		public void TestLineStatusSummary_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(nameof(declaration.LineStatusSummary));
			AssertEquals("Line Status", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestDeclarantCode()
		{
			const string code = "DEC1";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;

			AssertEquals("Precondition", ZGuid.Empty, declaration.CRD_OA_DeclarantAddress);
			AssertNullOrEmpty(declaration.DeclarantCode);

			declaration.CRD_OA_DeclarantAddress = orgHeader.Addresses.MainAddress.PK;
			AssertEquals(code, declaration.DeclarantCode);
		}

		public void TestRepresentativeCode()
		{
			const string code = "REP1";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;

			AssertEquals("Precondition", ZGuid.Empty, declaration.CRD_OA_RepresentativeAddress);
			AssertNullOrEmpty(declaration.RepresentativeCode);

			declaration.CRD_OA_RepresentativeAddress = orgHeader.Addresses.MainAddress.PK;
			AssertEquals(code, declaration.RepresentativeCode);
		}

		public void TestBuyingAgentCode()
		{
			const string code = "BA1";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;

			AssertEquals("Precondition", ZGuid.Empty, declaration.CRD_OA_BuyingAgentAddress);
			AssertNullOrEmpty(declaration.BuyingAgentCode);

			declaration.CRD_OA_BuyingAgentAddress = orgHeader.Addresses.MainAddress.PK;
			AssertEquals(code, declaration.BuyingAgentCode);
		}

		public void TestUnlinkedDeclarationsNumberMessage()
		{
			var entryDate = new ZDate(2024, 04, 18);
			declaration.CRD_PeriodFrom = entryDate;
			declaration.CRD_PeriodTo = entryDate;

			var parentDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = parentDeclaration.CustomsEntryHeaders.AddNew();
			var declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
			var cusReconEntry1 = Factory.New<CusReconEntry>();

			cusReconEntry1.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry1.CRE_GB_Branch = declaration.CRD_GB_Branch;
			cusReconEntry1.CRE_OA_DeclarantAddress = declarantAddress.PK;
			cusReconEntry1.CRE_EntryDate = entryDate;
			cusReconEntry1.CRE_EntryType = MonthlyClosingDeclarationTypeList.Codes.AZL;

			var cusReconEntry2 = Factory.New<CusReconEntry>();
			cusReconEntry2.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry2.CRE_GB_Branch = declaration.CRD_GB_Branch;
			cusReconEntry2.CRE_OA_DeclarantAddress = declarantAddress.PK;
			cusReconEntry2.CRE_EntryDate = entryDate;
			cusReconEntry2.CRE_EntryType = MonthlyClosingDeclarationTypeList.Codes.AZL;
			Factory.Save();

			AssertEquals("2 unlinked Simplified Declarations found", declaration.UnlinkedDeclarationsNumberMessage);

			cusReconEntry2.CRE_CRD = declaration.PK;
			Factory.Save();
			AssertEquals("1 unlinked Simplified Declarations found", declaration.UnlinkedDeclarationsNumberMessage);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.NewWithValidTestData<CusReconDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CusReconDeclaration>();
		}
		CusReconDeclaration declaration;

		void AssertPropertyReadOnlyIfLinkedToSimplifiedDeclaration(ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Declaration is not linked to a Simplified Declaration", false, propertyInfo.ReadOnly);

				declaration.CusReconEntries.AddNew();
				AssertEquals("Declaration is linked to a Simplified Declaration", true, propertyInfo.ReadOnly);
			});
		}
	}
}
