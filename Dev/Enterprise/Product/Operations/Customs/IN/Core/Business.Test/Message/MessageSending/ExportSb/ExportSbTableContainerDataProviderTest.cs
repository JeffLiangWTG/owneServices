using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableContainerDataProviderTest : ExportSbTableContainerDataProviderAbstractClassBase
{
	public override void TestContainerNumber()
	{
		AssertEquals("TBA", CreateDataProvider().ContainerNumber);
	}

	public override void TestContainerSize()
	{
		AssertEquals("TBA", CreateDataProvider().ContainerSize);
	}

	public override void TestCustomHouseCode()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestExciseSealNo()
	{
		AssertEquals("TBA", CreateDataProvider().ExciseSealNo);
	}

	public override void TestJobDate()
	{
		AssertNull(CreateDataProvider().JobDate);

		header.CH_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestMovementDocumentNumber()
	{
		container1.CO_MovementDocumentNum = "MDN0509";
		AssertEquals("MDN0509", CreateDataProvider().MovementDocumentNumber);
	}

	public override void TestMovementDocumentType()
	{
		container1.CO_MovementDocumentType = "MDT";
		AssertEquals("MDT", CreateDataProvider().MovementDocumentType);
	}

	public override void TestSealDate()
	{
		AssertEquals("TBA", CreateDataProvider().SealDate);
	}

	public override void TestSealDeviceId()
	{
		container1.CO_SealDeviceID = "SDID";
		AssertEquals("SDID", CreateDataProvider().SealDeviceId);
	}

	public override void TestSealTypeIndicator()
	{
		AssertEquals("TBA", CreateDataProvider().SealTypeIndicator);
	}

	protected override TableContainerDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableContainer.First();
	}

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new ExportSbCACHE01AdditionalDataProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();
		entryLine1 = header.MergedLines.AddNew();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.ActiveEntryHeaders.Add(header);
		invoice1 = declaration.Invoices.AddNew();
		invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		header.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine1.JI_CEI = entryInstruction.PK;
		container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "HXU1";

		entryInstruction.ContainersForInstructionForBindingOnly.First(x => x.Container.CO_ContainerNumber == "HXU1").IsForEntry = true;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice1;
	JobComInvoiceLine invoiceLine1;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine1;
	CusContainer container1;
}
