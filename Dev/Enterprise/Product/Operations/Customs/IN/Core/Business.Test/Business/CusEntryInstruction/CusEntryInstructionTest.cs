using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : CusEntryInstructionAbstractTest
{
	public void TestContainers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "HXU1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "HXU2";
		var container3 = declaration.CusContainers.AddNew();
		container3.CO_ContainerNumber = "HXU3";
		var container1ForBinding = instruction.ContainersForInstructionForBindingOnly.First(x => x.Container.CO_ContainerNumber == "HXU1");
		container1ForBinding.IsForEntry = true;
		var container3ForBinding = instruction.ContainersForInstructionForBindingOnly.First(x => x.Container.CO_ContainerNumber == "HXU3");
		container3ForBinding.IsForEntry = true;
		AssertEquals(2, instruction.Containers.Count);
		AssertContainsExactElementsInExactOrder(new[] { container1, container3 }, instruction.Containers.Select(x => x.Container));
	}

	[TestDate(2024, 06, 13, 08, 08, 01)]
	public void TestLocalReferenceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals(ZString.Empty, instruction.LocalReferenceNumber);
		AssertEquals(ZDateTime.Empty, instruction.LocalReferenceNumberDate);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		AssertEquals(ZString.Empty, instruction.LocalReferenceNumber);
		AssertEquals(ZDateTime.Empty, instruction.LocalReferenceNumberDate);

		Factory.Save();
		AssertNotEquals(ZString.Empty, instruction.LocalReferenceNumber);
		AssertEquals(new ZDateTime(2024, 06, 13, 08, 08, 01).ToLocalBranchTime(), instruction.LocalReferenceNumberDate);

		entryHeader.CH_BGMReference = "12345";
		Factory.Save();
		AssertEquals("12345", instruction.LocalReferenceNumber);
	}

	public void TestMessageStatusAttributes()
	{
		AssertCaptions(Instruction.MessageStatusInfo, "Message Status", "Message", "Message");
		AssertEquals("Max Length", 3, Instruction.MessageStatusInfo.MaxLength);

		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;

		CombineAssertions(() =>
		{
			AssertReadOnlyProperty(instruction.MessageStatusInfo, true);
			instruction.StatusOverride = true;
			AssertReadOnlyProperty(instruction.MessageStatusInfo, false);
		});
	}

	public void TestCustomsStatusAttributes()
	{
		AssertCaptions(Instruction.CustomsStatusInfo, "Customs Status", "Customs", "Customs");
		AssertEquals("Max Length", 3, Instruction.CustomsStatusInfo.MaxLength);

		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;

		CombineAssertions(() =>
		{
			AssertReadOnlyProperty(instruction.CustomsStatusInfo, true);
			instruction.StatusOverride = true;
			AssertReadOnlyProperty(instruction.CustomsStatusInfo, false);
		});
	}

	public void TestStatusOverrideAttributes()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.StatusOverrideInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Override", resData.Caption);
			AssertEquals("FullDescription", "Override Message Status and Customs Status", resData.FullDescription);
		});

		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		AssertReadOnlyProperty(instruction.StatusOverrideInfo, true);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		AssertReadOnlyProperty(instruction.StatusOverrideInfo, false);
	}

	public void TestMessageStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		instruction.StatusOverride = true;
		instruction.MessageStatus = "ACP";
		AssertEquals("MessageStatus should be ACP", "ACP", instruction.MessageStatus);
		AssertEquals("CH_Status should be ACP", "ACP", entryHeader.CH_Status);
	}

	public void TestCustomsStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		instruction.StatusOverride = true;
		instruction.CustomsStatus = "REG";
		AssertEquals("CustomsStatus should be REG", "REG", instruction.CustomsStatus);
		AssertEquals("CH_EntryStatus should be REG", "REG", entryHeader.CH_EntryStatus);
	}

	public void TestStatusOverride()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;

		instruction.StatusOverride = true;
		instruction.MessageStatus = "ACP";
		instruction.CustomsStatus = "REG";
		instruction.StatusOverride = false;
		AssertEquals("Not in database and untick override, MessageStatus", ZString.Empty, instruction.MessageStatus);
		AssertEquals("Not in database and untick override, CustomsStatus", ZString.Empty, instruction.CustomsStatus);

		instruction.StatusOverride = true;
		instruction.MessageStatus = "ACP";
		instruction.CustomsStatus = "REG";
		Factory.Save();
		Assert("Status Override untick after saving", !instruction.StatusOverride);

		instruction.StatusOverride = true;
		instruction.MessageStatus = "ERR";
		instruction.CustomsStatus = "FAL";
		instruction.StatusOverride = false;
		AssertEquals("Before save and untick override, MessageStatus", "ACP", instruction.MessageStatus);
		AssertEquals("Before save and untick override, CustomsStatus", "REG", instruction.CustomsStatus);

		instruction.StatusOverride = true;
		instruction.CustomsStatus = "FAL";
		instruction.CEI_OH_Carrier = ZGuid.Invalid;
		AssertExceptionThrown<ZSaveException>(Factory.Save);
		Assert("Status Override unchanged after Save failed", instruction.StatusOverride);
		AssertEquals("Save failed, CustomsStatus", "FAL", instruction.CustomsStatus);

		instruction.StatusOverride = false;
		AssertEquals("Untick override, MessageStatus", "ACP", instruction.MessageStatus);
		AssertEquals("Untick override, CustomsStatus", "REG", instruction.CustomsStatus);

		instruction.StatusOverride = true;
		instruction.MessageStatus = "ERR";
		instruction.CustomsStatus = "FAL";
		instruction.CEI_OH_Carrier = ZGuid.Empty;
		Factory.Save();
		Assert("Status Override untick after saving", !instruction.StatusOverride);
		AssertEquals("After save, MessageStatus", "ERR", instruction.MessageStatus);
		AssertEquals("After save, CustomsStatus", "FAL", instruction.CustomsStatus);

		instruction.StatusOverride = true;
		instruction.MessageStatus = "ACP";
		instruction.CustomsStatus = "REG";
		instruction.StatusOverride = false;
		AssertEquals("Untick override, MessageStatus", "ERR", instruction.MessageStatus);
		AssertEquals("Untick override, CustomsStatus", "FAL", instruction.CustomsStatus);
	}

	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Instruction, "INCusEntryInstruction");
	}

	public void TestCEI_WeightUQ()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_WeightUQInfo);
		AssertEquals("Caption", "UOM", info.Caption);
	}

	public void TestGrossWeight_Captions()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.GrossWeightInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Info string data", info);
			AssertEquals("Caption", "Gross Weight", info.Caption);
			AssertEquals("MediumCaption", "Gr. Weight", info.MediumCaption);
			AssertEquals("ShortCaption", "Gr. Wt.", info.ShortCaption);
		});
	}

	public void TestGrossWeight()
	{
		CombineAssertions(() =>
		{
			var header = Header;
			header.CH_CEI_Instruction = Instruction.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.CustomsEntryHeaders.Add(header);

			AssertEquals("When invoice line not present", ZDecimal.Zero, instruction.GrossWeight);

			var invoiceLine1 = CreateNewInvoiceLine();
			AssertEquals("When one invoice line is present with zero Gross Weight", ZDecimal.Zero, instruction.GrossWeight);

			invoiceLine1.JI_Weight = 10;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("When one invoice line is present", 10m, instruction.GrossWeight);

			instruction.CEI_WeightUQ = Core.Constants.Weight.Grams;
			var invoiceLine2 = CreateNewInvoiceLine();
			invoiceLine2.JI_Weight = 10000.1234;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("When more than one invoice line is present", 20000.123m, instruction.GrossWeight);
		});
	}

	public void TestNetWeight_Captions()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.NetWeightInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Info string data", info);
			AssertEquals("Caption", "Net Weight", info.Caption);
			AssertEquals("MediumCaption", "Net Weight", info.MediumCaption);
			AssertEquals("ShortCaption", "Net. Wt.", info.ShortCaption);
		});
	}

	public void TestNetWeight()
	{
		CombineAssertions(() =>
		{
			var header = Header;
			header.CH_CEI_Instruction = Instruction.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.CustomsEntryHeaders.Add(header);

			AssertEquals("When invoice line not present", ZDecimal.Zero, instruction.NetWeight);

			var invoiceLine1 = CreateNewInvoiceLine();
			AssertEquals("When one invoice line is present with zero Net Weight", ZDecimal.Zero, instruction.NetWeight);

			invoiceLine1.JI_NetWeight = 10;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("When one invoice line is present", 10m, instruction.NetWeight);

			instruction.CEI_WeightUQ = Core.Constants.Weight.Grams;
			var invoiceLine2 = CreateNewInvoiceLine();
			invoiceLine2.JI_NetWeight = 10000.1234;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("When more than one invoice line is present", 20000.123m, instruction.NetWeight);
		});
	}

	public void TestCEI_TotalContainer()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var info = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_TotalContainerInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", info);
			AssertEquals("Caption", "Total Container", info.Caption);
			AssertEquals("MediumCaption", "Total Cont.", info.MediumCaption);
			AssertEquals("ShortCaption", "Tot. Cont.", info.ShortCaption);
			AssertEquals("Max Length", 2, Instruction.CEI_TotalContainerInfo.MaxLength);
		});
	}

	public void TestLookups()
	{
		AssertType<CusEntryInstructionLookups>(Instruction.Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusEntryInstructionValidation>(Instruction.Validation);
	}

	public void TestCEI_Style()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_StyleInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Declaration Type", resData.Caption);
			AssertEquals("MediumCaption", "Dec. Type", resData.MediumCaption);
			AssertEquals("ShortCaption", "Dec. Type", resData.ShortCaption);
			AssertEquals("Max Length", 4, Instruction.CEI_StyleInfo.MaxLength);
		});
	}

	public void TestCEI_SubStyle()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_SubStyleInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "NFEI Category", resData.Caption);
			AssertEquals("MediumCaption", "NFEI", resData.MediumCaption);
			AssertEquals("ShortCaption", "NFEI", resData.ShortCaption);
			AssertEquals("Max Length", 2, Instruction.CEI_SubStyleInfo.MaxLength);
		});
	}

	public void TestCEI_SubStyle_ReadOnly()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import JobDeclaration", false, Instruction.CEI_SubStyle_ReadOnly);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Instruction.CEI_Style = DeclarationTypeList.Codes.NoForeignExchangeInvolved;
		AssertEquals("Export JobDeclaration and CEI_Style = NFEI", false, Instruction.CEI_SubStyle_ReadOnly);

		Instruction.CEI_Style = ZString.Empty;
		AssertEquals("Export JobDeclaration and CEI_Style = Empty", true, Instruction.CEI_SubStyle_ReadOnly);

		Instruction.CEI_Style = DeclarationTypeList.Codes.ForeignExchangeInvolved;
		AssertEquals("Export JobDeclaration and CEI_Style = FEI", true, Instruction.CEI_SubStyle_ReadOnly);
	}

	public void TestClearSubStyleIfNotRequired()
	{
		Instruction.CEI_Style = DeclarationTypeList.Codes.NoForeignExchangeInvolved;
		Instruction.CEI_SubStyle = "AA";
		Instruction.CEI_Style = DeclarationTypeList.Codes.ForeignExchangeInvolved;
		AssertEquals("CEI_Style = FEI", ZString.Empty, Instruction.CEI_SubStyle);
	}

	public void TestShippingBillNumber()
	{
		AssertEquals(ZString.Empty, Instruction.ShippingBillNumber);
		AssertEquals(CusEntryInstruction.Schema.SBNumberMaxLength, Instruction.ShippingBillNumberInfo.MaxLength);
		AssertEquals(true, Instruction.ShippingBillNumberInfo.ReadOnly);

		Instruction.ShippingBillNumberOverride = true;
		AssertEquals(false, Instruction.ShippingBillNumberInfo.ReadOnly);
	}

	public void TestShippingBillDate()
	{
		AssertEquals(ZDateTime.Empty, Instruction.ShippingBillDate);
		AssertEquals(true, Instruction.ShippingBillDateInfo.ReadOnly);

		Instruction.ShippingBillNumberOverride = true;
		AssertEquals(false, Instruction.ShippingBillDateInfo.ReadOnly);
	}

	public void TestShippingBillNumberOverride()
	{
		Instruction.ShippingBillNumberOverride = false;
		AssertEquals(ZString.Empty, Instruction.ShippingBillNumber);
		AssertEquals(ZDateTime.Empty, Instruction.ShippingBillDate);

		Instruction.ShippingBillNumberOverride = true;
		Instruction.ShippingBillNumber = "123";
		Instruction.ShippingBillDate = ZDateTime.BrettsBirthday;
		AssertEquals("123", Instruction.ShippingBillNumber);
		AssertEquals(ZDateTime.BrettsBirthday, Instruction.ShippingBillDate);
		Instruction.ShippingBillNumberOverride = false;
		AssertEquals(ZString.Empty, Instruction.ShippingBillNumber);
		AssertEquals(ZDateTime.Empty, Instruction.ShippingBillDate);

		Instruction.ShippingBillNumberOverride = true;
		Instruction.ShippingBillNumber = "123";
		Instruction.ShippingBillDate = ZDateTime.BrettsBirthday;
		Factory.Save();
		AssertEquals(false, Instruction.ShippingBillNumberOverride);

		Instruction.ShippingBillNumberOverride = true;
		Instruction.ShippingBillNumber = "456";
		Instruction.ShippingBillDate = ZDateTime.BrettsBirthday.AddDays(1);
		Instruction.ShippingBillNumberOverride = false;
		AssertEquals("123", Instruction.ShippingBillNumber);
		AssertEquals(ZDateTime.BrettsBirthday, Instruction.ShippingBillDate);
	}

	public void TestShippingBillEntryNumber_DeleteIfEmptyWhenSave()
	{
		CombineAssertions(() =>
		{
			var entryNum = CreateNewEntryNum(CusEntryNumberTypes.Indian.ShippingBill);
			entryNum.CE_EntryNum = "1234";
			entryNum.CE_IssueDate = ZDateTime.BrettsBirthday;
			AssertEquals("1234", Instruction.ShippingBillNumber);
			AssertEquals(ZDateTime.BrettsBirthday, Instruction.ShippingBillDate);

			Instruction.ShippingBillDate = ZDate.Empty;
			Factory.Save();
			AssertNotNull(Factory.Load<CusEntryNumber>(entryNum.PK));

			Instruction.ShippingBillNumber = ZString.Empty;
			Instruction.ShippingBillDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertNotNull(Factory.Load<CusEntryNumber>(entryNum.PK));

			Instruction.ShippingBillNumber = ZString.Empty;
			Instruction.ShippingBillDate = ZDate.Empty;
			Factory.Save();
			AssertNull(Factory.Load<CusEntryNumber>(entryNum.PK));
		});
	}

	public void TestContainersForInstructionForBindingOnly()
	{
		var container = Declaration.CusContainers.AddNew();
		var result = new CusContainerOnEntryInstruction(Instruction);
		result.Container = container;
		result.IsForEntry = true;

		CombineAssertions(() =>
		{
			AssertEquals(1, Instruction.ContainersForInstructionForBindingOnly.Count);
			AssertType<CusContainerOnEntryInstruction>(Instruction.ContainersForInstructionForBindingOnly.First());
		});
	}

	public void TestCEI_NumberOfPackages()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_NumberOfPackagesInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Total No. Of Packages", resData.Caption);
			AssertEquals("MediumCaption", "Total Pack", resData.MediumCaption);
			AssertEquals("ShortCaption", "Tot. Pk.", resData.ShortCaption);
		});
	}

	public void TestNumberOfPackagesUQ()
	{
		AssertEquals(Core.Constants.PkgUnit.Package, Instruction.NumberOfPackagesUQ);
	}

	public void TestCEI_LoosePackages()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.CEI_LoosePackagesInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Loose Packages", resData.Caption);
			AssertEquals("MediumCaption", "Loose Pack", resData.MediumCaption);
			AssertEquals("ShortCaption", "Loose Pack", resData.ShortCaption);
		});
	}

	public void TestLoosePackagesUQ()
	{
		AssertEquals(Core.Constants.PkgUnit.Package, Instruction.LoosePackagesUQ);
	}

	public void TestRBIWaiverNumberSetValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When EntryNum not present", ZString.Empty, Instruction.RBIWaiverNumber);

			var entryNum = CreateNewEntryNum(CusEntryNumberTypes.Indian.ReservedBankOfIndia);
			entryNum.CE_EntryNum = "123";
			AssertEquals("When CE_EntryNum = 123, RBIWaiverNumber", "123", Instruction.RBIWaiverNumber);

			Instruction.RBIWaiverNumber = "999";
			AssertEquals("When RBIWaiverNumber = 999, CE_EntryNum", "999", entryNum.CE_EntryNum);

			Instruction.RBIWaiverNumber = "";
			Factory.Save();
			AssertNull("When RBIWaiverNumber empty", Factory.Load<CusEntryNumber>(entryNum.PK));
		});
	}

	public void TestRBIWaiverNumber()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.RBIWaiverNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", CusEntryInstruction.Schema.RBIWaiverNumberMaxLength, Instruction.RBIWaiverNumberInfo.MaxLength);
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "RBI Waiver No.", resData.Caption);
			AssertEquals("MediumCaption", "Waiver No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "W. No.", resData.ShortCaption);
		});
	}

	public void TestRBIWaiverDateSetValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When RBIWaiverDate not present", ZDate.Empty, Instruction.RBIWaiverDate);

			var entryNum = CreateNewEntryNum(CusEntryNumberTypes.Indian.ReservedBankOfIndia);
			entryNum.CE_IssueDate = new ZDate(2022, 3, 16);
			AssertEquals("When CE_IssueDate = 20220316", new ZDate(2022, 3, 16), Instruction.RBIWaiverDate);

			Instruction.RBIWaiverDate = new ZDate(2022, 3, 19);
			AssertEquals("When RBIWaiverDate = 20220319", new ZDate(2022, 3, 19), entryNum.CE_IssueDate);

			Instruction.RBIWaiverDate = ZDate.Empty;
			Factory.Save();
			AssertNull("When RBIWaiverDate empty", Factory.Load<CusEntryNumber>(entryNum.PK));
		});
	}

	public void TestRBIWaiverDateCaptions()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Instruction.RBIWaiverDateInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "RBI Waiver Date", resData.Caption);
			AssertEquals("MediumCaption", "Waiver Date", resData.MediumCaption);
			AssertEquals("ShortCaption", "Date", resData.ShortCaption);
		});
	}

	public void TestRBIWaiverEntryNumberWithNumberAndDate()
	{
		CombineAssertions(() =>
		{
			AssertNull("When Waiver number and date are not entered", CusEntryNumber.Load(Instruction, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India));

			Instruction.RBIWaiverDate = new ZDate(2022, 3, 16);
			Instruction.RBIWaiverNumber = "1234";
			AssertNotNull("When Waiver number and date are present", CusEntryNumber.Load(Instruction, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India));

			Instruction.RBIWaiverDate = ZDate.Empty;
			AssertNotNull("When Waiver number filled and date is empty", CusEntryNumber.Load(Instruction, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India));

			Instruction.RBIWaiverDate = new ZDate(2022, 3, 16);
			Instruction.RBIWaiverNumber = ZString.Empty;
			AssertNotNull("When Waiver number empty and date is filled", CusEntryNumber.Load(Instruction, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India));

			Instruction.RBIWaiverDate = ZDate.Empty;
			Factory.Save();
			AssertNull("When Waiver number and date are empty", CusEntryNumber.Load(Instruction, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India));
		});
	}

	public void TestRBIWaiverEntryNumberWhenDeleteInstruction()
	{
		CombineAssertions(() =>
		{
			Instruction.RBIWaiverDate = new ZDate(2022, 3, 16);
			Instruction.RBIWaiverNumber = "1234";
			Factory.Save();
			var entryNum = CusEntryNumber.Load(Instruction, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India);
			AssertNotNull("When Waiver number and date are present", entryNum);

			Instruction.Delete();
			Factory.Save();
			AssertNull("When Waiver number and date are empty", Factory.Load<CusEntryNumber>(entryNum.PK));
		});
	}

	public void TestContainersForInstructionForBindingOnlyType()
	{
		AssertType(typeof(CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>), Instruction.ContainersForInstructionForBindingOnly);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		base.GetNewBusinessObject();
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		return instruction;
	}

	void AssertCaptions(ZPropertyInfo info, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(info);
		AssertNotNull("Res String data", resData);
		AssertEquals("Caption", expectedCaption, resData.Caption);
		AssertEquals("MediumCaption", expectedMediumCaption, resData.MediumCaption);
		AssertEquals("ShortCaption", expectedShortCaption, resData.ShortCaption);
	}

	void AssertReadOnlyProperty(ZPropertyInfo info, bool expectedReadOnly)
	{
		ZString message = $"The readonly of {info.Name} should be {expectedReadOnly}.";
		AssertEquals(message, expectedReadOnly, info.ReadOnly);
	}

	public void TestSupportingDocumentCollection()
	{
		var collection = Factory.New<CusEntryInstruction>().SupportingDocuments;
		AssertType<SupportingDocumentCollection>(collection);
	}

	public void TestSWControlCollection()
	{
		var collection = Factory.New<CusEntryInstruction>().SWControls;
		AssertType<SWControlCollection>(collection);
	}

	[TestDate(2024, 6, 13)]
	public void TestIDateOfValuationProvider()
	{
		IDateOfValuationProvider provider = Factory.New<CusEntryInstruction>();
		AssertEquals("Entry Instruction defaults to Today", ZDateTime.Today, provider.DateOfValuation);

		provider = Instruction;
		AssertEquals("Declaration without ValuationDate set", new ZDate(2024, 6, 13), provider.DateOfValuation);
		Declaration.JE_ValuationDate = new ZDate(2022, 3, 16);
		AssertEquals("Declaration with ValuationDate set", new ZDate(2022, 3, 16), provider.DateOfValuation);
	}

	public void TestICusSupportingInfoTypeSupporter()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		Integration.Customs.ICusSupportingInfoTypeSupporter supporter = entryInstruction;
		var supportingInfoTypes = supporter.GetCusSupportingInfoTypes();
		AssertEquals(typeof(SupportingDocument), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals(typeof(SWControl), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SingleWindowControl]);

		AssertType<CusSupportingInfoTypeSupporterFetchStrategy>(supporter.GetFetchStrategies().First());
	}

	public void TestGetSequenceNumberGenerator()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var supportingInfoParent = (ICusSupportingInfoWithSerialNoParent)entryInstruction;

		var expectedValues = new[]
		{
			entryInstruction.SupportingDocumentLineNumberGenerator,
			entryInstruction.SWControlsLineNumberGenerator
		};

		var actualValues = supportingInfoParent.GetCusSupportingInfoTypes().Keys.Select(type => supportingInfoParent.GetSequenceNumberGenerator(type));

		AssertEquals("Count", expectedValues.Length, actualValues.Count());
		AssertContainsExactElementsInExactOrder("Sequence Number Generators", expectedValues, actualValues);
	}

	CusEntryNumber CreateNewEntryNum(ZString entryType)
	{
		var entryNum = Factory.New<CusEntryNumber>();
		entryNum.CE_ParentID = Instruction.PK;
		entryNum.CE_ParentTable = Instruction.TableName;
		entryNum.CE_EntryType = entryType;
		entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		return entryNum;
	}

	JobComInvoiceLine CreateNewInvoiceLine()
	{
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine;
	}

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryHeader Header => header ??= Factory.New<CusEntryHeader>();
	CusEntryHeader header;
}
