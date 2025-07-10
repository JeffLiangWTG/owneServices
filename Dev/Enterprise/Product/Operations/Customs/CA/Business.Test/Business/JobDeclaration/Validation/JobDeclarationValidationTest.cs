using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		class JobDeclarationValidationForTest : JobDeclarationValidation
		{
			public JobDeclarationValidationForTest(JobDeclaration parent) : base(parent)
			{
			}

			public bool ShouldValidateIsCreditLimitExceeded_Exposed
			{
				get
				{
					return ShouldValidateIsCreditLimitExceeded;
				}
			}
		}

		public void TestCheckJE_OH_ImporterUsingDTYCustomsRule()
		{
			var today = new ZDate(2023, 01, 30);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_OH_PermitHolder = importer.PK;
			customsRule.CPH_StartDate = today.AddMonths(-1);
			var rule01 = customsRule.Rules.AddNew();
			rule01.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			rule01.CPR_ValueTo = "10000";
			Factory.Save();

			var messageError = "exceeds maximum amount";
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_DateOfFirstArrival = today;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(false, declaration.IsBrokerToPay);
			AssertEquals(false, declaration.IsImporterToPay);
			AssertEquals(0m, declaration.DisbursementAmount);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var duty1 = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty1.C1_Amount = 5000.5m;
			var duty2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 5000.5m;
			var validation = declaration.Validation;
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Duty Amount (10001.00) exceeds maximum amount defined by Custom Rules.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			customsRule.CPH_PermitDescription = "TEST Rule";
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Duty Amount (10001.00) exceeds maximum amount defined by Custom Rule: TEST Rule.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			duty1.C1_Amount = 5000m;
			duty2.C1_Amount = 5000m;
			validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);
		}

		public void TestShouldValidateIsCreditLimitExceeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var validation = new JobDeclarationValidationForTest(declaration);
			AssertEquals(true, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals(true, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(true, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals(true, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals(false, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals(false, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, validation.ShouldValidateIsCreditLimitExceeded_Exposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals(false, validation.ShouldValidateIsCreditLimitExceeded_Exposed);
		}

		public void TestJE_TotalNoOfPacksCaptionFullDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			declaration.JE_TotalNoOfPacks = 0;
			var totalNoOfPacksProperty = DataBoundResourceStrings.GetDataForProperty(this.declaration.JE_TotalNoOfPacksInfo);
			AssertEquals("JE_TotalNoOfPacks - FullDescription", "Enter the total number of outer packages for this shipment, as distinct from the number of units. The total number of outer packages is what the goods are packed into and the type of packaging in which the goods are contained or wrapped. Example: Drums, Barrels, Pallets etc.", totalNoOfPacksProperty.FullDescription);
			AssertEquals("JE_TotalNoOfPacks - Caption", "No. Packages", totalNoOfPacksProperty.Caption);
			AssertEquals("JE_TotalNoOfPacks - ShortCaption", "Packages", totalNoOfPacksProperty.ShortCaption);
		}

		public void TestJE_TotalWeightUnitCaptionFullDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			declaration.JE_TotalWeightUnit = "KG";
			var weightUnitProperty = DataBoundResourceStrings.GetDataForProperty(this.declaration.JE_TotalWeightUnitInfo);
			AssertEquals("JE_TotalWeightUnit - FullDescription", ZString.Empty, weightUnitProperty.FullDescription);
			AssertEquals("JE_TotalWeightUnit - Caption", "Total Weight UQ", weightUnitProperty.Caption);
			AssertEquals("JE_TotalWeightUnit - ShortCaption", "Weight", weightUnitProperty.ShortCaption);
		}

		public override void TestPackagesActualPackageCount_Validation()
		{
			var error = JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage;

			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			var jobDeclaration = mockDeclaration.Object;
			jobDeclaration.DisableDefaultPackingInformation = true;
			jobDeclaration.JE_TotalNoOfPacks = 100;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			AssertHasMessageError("Should have message error.", jobDeclaration.PackagesActualPackageCountInfo, error);

			var packGroup = jobDeclaration.PackingGroups.AddNew();
			var package = packGroup.Packages.AddNew();
			AssertHasMessageError("Should have message error.", jobDeclaration.PackagesActualPackageCountInfo, error);

			package.CW_PackQty = 100;
			AssertNoMessageError("Should not have message error.", jobDeclaration.PackagesActualPackageCountInfo, error);

			jobDeclaration.JE_TotalNoOfPacks = 10;
			AssertHasMessageError("Should have message error.", jobDeclaration.PackagesActualPackageCountInfo, error);

			package.CW_PackQty = 10;
			AssertNoMessageError("Should not have message error.", jobDeclaration.PackagesActualPackageCountInfo, error);

			jobDeclaration.Packages.RemoveAndDeleteAll();
			AssertHasMessageError("Should have message error.", jobDeclaration.PackagesActualPackageCountInfo, error);

			jobDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			jobDeclaration.Validation.ValidatePackagesActualPackageCount();
			AssertNoMessageError("Should not have message error as the 'ShouldValidatePackagesActualPackageCount' is false.", jobDeclaration.PackagesActualPackageCountInfo, error);
		}

		public void TestCheckJE_DateOfFirstArrival_MandatoryValidation()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_DateOfFirstArrivalInfo);
		}

		public void TestCheckJE_DateOfFirstArrival_GACPGAHeader()
		{
			var message = "Declarations containing goods regulated by Global Affairs Canada may not be accepted if provided more than 30 days in advance of arrival.";

			var now = ZDateTime.Now;

			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_DateOfFirstArrival = now;

			AssertNoMessageError(declaration.JE_DateOfFirstArrivalInfo, message);

			var invoiceHeader = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.CA_GACInd = YesNoList.Codes.Yes;

			AssertNotNull(line.GACPGAHeader);
			AssertNoMessageError(declaration.JE_DateOfFirstArrivalInfo, message);

			declaration.JE_DateOfFirstArrival = now.AddDays(31);
			AssertHasMessageError(declaration.JE_DateOfFirstArrivalInfo, message);
		}

		#region CheckCA_ExamLocationCode

		public void TestCheckExamLocationDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ExamLocationCode = ZString.Empty;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			declaration.Validation.ValidateExamLocationDescription();
			AssertNoMessageError(declaration.ExamLocationDescriptionInfo, JobDeclarationValidation.ExamLocationMustBeEntered);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.Validation.ValidateExamLocationDescription();
			AssertHasMessageError(declaration.ExamLocationDescriptionInfo, JobDeclarationValidation.ExamLocationMustBeEntered);
			declaration.ExamLocationDescription = "XX";
			declaration.Validation.ValidateExamLocationDescription();
			AssertNoMessageError(declaration.ExamLocationDescriptionInfo, JobDeclarationValidation.ExamLocationMustBeEntered);

			declaration.ExamLocationDescription = "123ABC";
			AssertNoWarning(declaration.ExamLocationDescriptionInfo, "The Exam Location has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			declaration.ExamLocationDescription = "123ABC– ";
			AssertHasWarning(declaration.ExamLocationDescriptionInfo, "The Exam Location has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		#endregion

		[TestDate(2016, 02, 18)]
		public void TestCheckEffectiveCCN()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			declaration.Validation.ValidateEffectiveCCN();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.EffectiveCCNInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.EffectiveCCN = "ABCD";
			declaration.Validation.ValidateEffectiveCCN();
			AssertHasMessageError(declaration.EffectiveCCNInfo, ReleaseStatusValidation.InvalidCCNCode);

			declaration.EffectiveCCN = "ABCDE";
			declaration.Validation.ValidateEffectiveCCN();
			AssertNoMessageError(declaration.EffectiveCCNInfo, ReleaseStatusValidation.InvalidCCNCode);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_DeclarationReference = "B00001002";
			declaration2.Company.GC_Name = "GLBCOMP";
			declaration2.Branch.GB_BranchName = "GLBBRNCH";

			var ccn2 = declaration2.ReleaseStatuses.AddNew();
			ccn2.RL_CargoControlNumber = "ABCDE";
			Factory.Save();

			declaration.Validation.ValidateEffectiveCCN();
			AssertHasMessageError(declaration.EffectiveCCNInfo, "Another Declaration already contains the same CCN as this declaration (Declaration: 'B00001002', Company: 'GLBCOMP', Branch: 'GLBBRNCH').");

			declaration2.JE_SystemCreateTimeUtc = new ZDateTime(2009, 02, 18);
			Factory.Save();
			declaration.Validation.ValidateEffectiveCCN();
			AssertNoMessageErrors(declaration.EffectiveCCNInfo);
		}

		[TestDate(2016, 02, 18)]
		public void TestCheckEffectiveCCNPrefix()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.EffectiveCCNPrefix = "";
			declaration.Validation.ValidateEffectiveCCNPrefix();
			AssertNoErrors(declaration.EffectiveCCNPrefixInfo);

			declaration.EffectiveCCNPrefix = "273";
			declaration.Validation.ValidateEffectiveCCNPrefix();
			AssertHasMessageError(declaration.EffectiveCCNPrefixInfo, "The CCN-prefix should be composed of 4 characters");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.EffectiveCCNPrefix = "";
			AssertHasMessageError(declaration.EffectiveCCNPrefixInfo, "You have not entered a value.");

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_SystemCreateTimeUtc = new ZDateTime(2018, 02, 19);
			declaration2.JE_DeclarationReference = "B00001002";
			declaration2.Company.GC_Name = "GLBCOMP";
			declaration2.Branch.GB_BranchName = "GLBBRNCH";

			var ccn2 = declaration2.ReleaseStatuses.AddNew();
			ccn2.RL_CargoControlNumber = "2731890069";
			Factory.Save();

			declaration.ReleaseStatuses.RemoveAndDeleteAll();
			declaration.EffectiveCCNPrefix = "2731";
			declaration.EffectiveCCNSuffix = "890069";
			declaration.Validation.ValidateEffectiveCCNPrefix();
			AssertHasMessageErrorContaining(declaration.EffectiveCCNPrefixInfo, "Another Declaration already contains the same CCN");

			declaration.EffectiveCCNPrefix = "2732";
			AssertNoMessageErrors(declaration.EffectiveCCNPrefixInfo);
		}

		[TestDate(2016, 02, 18)]
		public void TestCheckEffectiveCCNSuffix()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateEffectiveCCNSuffix();
			AssertHasMessageError(declaration.EffectiveCCNSuffixInfo, "You have not entered a value.");

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_SystemCreateTimeUtc = new ZDateTime(2018, 02, 19);
			declaration2.JE_DeclarationReference = "B00001002";
			declaration2.Company.GC_Name = "GLBCOMP";
			declaration2.Branch.GB_BranchName = "GLBBRNCH";

			var ccn2 = declaration2.ReleaseStatuses.AddNew();
			ccn2.RL_CargoControlNumber = "1411142556";
			Factory.Save();

			declaration.ReleaseStatuses.RemoveAndDeleteAll();
			declaration.EffectiveCCNPrefix = "1411";
			declaration.EffectiveCCNSuffix = "142556";
			declaration.Validation.ValidateEffectiveCCNPrefix();
			AssertHasMessageErrorContaining(declaration.EffectiveCCNSuffixInfo, "Another Declaration already contains the same CCN");

			declaration.EffectiveCCNSuffix = "142557";
			AssertNoMessageErrors(declaration.EffectiveCCNSuffixInfo);
		}

		public void TestCheckJE_TotalWeight()
		{
			declaration.JE_TotalWeight = ZDecimal.Zero;
			string messageError = "Total Weight" + MandatoryValidation.ValueCannotBeZero + ".";
			AssertHasMessageError(declaration.JE_TotalWeightInfo, messageError);

			declaration.JE_TotalWeight = 23m;
			AssertNoMessageError(declaration.JE_TotalWeightInfo, messageError);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_TotalWeight = ZDecimal.Zero;
			AssertNoMessageError(declaration.JE_TotalWeightInfo, messageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.JE_TotalWeight = ZDecimal.Zero;
			AssertNoMessageError(declaration.JE_TotalWeightInfo, messageError);
		}

		public void TestJE_OH_ImporterValidationWithOrgOnCreditHoldWhenLVS()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;

			Factory.Save();

			Assert("IsCreditOnHold", org.CreditChecker.IsCreditOnHold());

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = org.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.IsImport());
			AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, "TestOrg is on Credit Hold.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, "TestOrg is on Credit Hold.");
		}

		public void TestCheckJE_TransportMode_B3X()
		{
			var messageError = "You have not entered a Mode of Transportation.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.JE_TransportMode = ZString.Empty;
			AssertHasMessageError(declaration.JE_TransportModeInfo, messageError);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageError(declaration.JE_TransportModeInfo, messageError);
		}

		public void TestCheckJE_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.NeedValidateMessageType = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertHasErrorContaining(declaration.JE_MessageTypeInfo, "Enter a valid Shipment Type");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoErrorContaining(declaration.JE_MessageTypeInfo, "Enter a valid Shipment Type");
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoErrorContaining(declaration.JE_MessageTypeInfo, "You may not change the shipment type because messages have been sent.");
			declaration.CustomsEntryHeaders.AddNew().Messages.AddNew();
			declaration.Validation.ValidateJE_MessageType();
			AssertHasErrorContaining(declaration.JE_MessageTypeInfo, "You may not change the shipment type because messages have been sent.");
		}

		public void TestCheckJE_GB()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "AAA";
			branch1.GB_RL_NKHomePort = "CAYLO";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "BBB";
			branch2.GB_RL_NKHomePort = "AUSYD";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch1.PK;
			AssertNoMessageError(declaration.JE_GBInfo, "The branch location should be Canada.");
			declaration.JE_GB = branch2.PK;
			AssertHasMessageError(declaration.JE_GBInfo, "The branch location should be Canada.");
		}

		public void TestDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Validation.Declaration, declaration);
		}

		public void TestMessageValidation()
		{
			JobDeclaration parent = Factory.New<JobDeclaration>();
			AssertEquals(typeof(ExternalMessageValidation), parent.Validation.MessageValidation.GetType());
		}

		public void TestCheckCalculatedFreightAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 100000m;
			declaration.JE_TransportMode = "ROA";
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 100m;
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.CalculatedFreightAmountInfo, JobDeclarationValidation.TotalFreightIsOverAmount);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100000m, Constants.CurrencyCodes.Canada);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.CalculatedFreightAmountInfo, JobDeclarationValidation.TotalFreightIsOverAmount);
			invoiceLine.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, -10000m, Constants.CurrencyCodes.Canada);
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.CalculatedFreightAmountInfo, JobDeclarationValidation.TotalFreightIsOverAmount);
		}

		public void EffectiveCCN_MessageErrorIfNotEnteredForIM2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.Validation.ValidateEffectiveCCN();
			declaration.Validation.ValidateEffectiveCCNPrefix();
			declaration.Validation.ValidateEffectiveCCNSuffix();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.EffectiveCCNInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.EffectiveCCNPrefixInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.EffectiveCCNSuffixInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			declaration.Validation.ValidateEffectiveCCN();
			declaration.Validation.ValidateEffectiveCCNPrefix();
			declaration.Validation.ValidateEffectiveCCNSuffix();
			AssertNoMessageErrors(declaration.EffectiveCCNInfo);
			AssertNoMessageErrors(declaration.EffectiveCCNPrefixInfo);
			AssertNoMessageErrors(declaration.EffectiveCCNSuffixInfo);
		}

		public void TestInvoiceHeaderNumberOfPacksOfParentPivotValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.ToggleLinkageWithPackage(package1, true);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.ToggleLinkageWithPackage(package2, true);

			var parentPivot = invoiceHeader2.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().First(x => x.CHZ_CW == parentPackage.PK);
			AssertNoMessageErrors(parentPivot.CHZ_NumberOfPacksInfo);
		}

		public void TestInvoiceLineNumberOfPacksOfParentPivotValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.ToggleLinkageWithPackage(package1, true);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.ToggleLinkageWithPackage(package2, true);

			var parentPivot = invoiceLine2.PackagesPivot.Cast<InvoiceLinePackagePivot>().First(x => x.CHC_CW == parentPackage.PK);
			AssertNoMessageErrors(parentPivot.CHC_NumberOfPacksInfo);
		}

		public void TestInvoiceLineNumberOfPacksOfParentPivotWithInvoiceHeaderValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.ToggleLinkageWithPackage(package1, true);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.ToggleLinkageWithPackage(package2, true);

			var parentPivot = invoiceHeader2.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().First(x => x.CHZ_CW == parentPackage.PK);
			AssertNoMessageErrors(parentPivot.CHZ_NumberOfPacksInfo);
		}

		BasePackage CreatePackage(JobDeclaration declaration, ZString packType, int packQty, Bill bill, ZGuid? parentPackagePK = null)
		{
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_HouseBill;
			package.CW_PackType = packType;
			package.CW_PackQty = packQty;
			if (null != parentPackagePK)
			{
				package.CW_CW_Parent = (ZGuid)parentPackagePK;
			}
			return package;
		}

		Bill createBill(JobDeclaration declaration, ZString billType, ZString billNum)
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = billType;
			bill.CU_BillNum = billNum;
			return bill;
		}
	}
}
