using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(SupportingDocSendingObject))]
public class SupportingDocSendingObjectTest : Customs.Business.Testing.JobDeclarationSupportingDocSendingObjectTest
{
	public void TestMessageTypeAndSubType()
	{
		var sendingObject = (SupportingDocSendingObject)SupportingDocSendingObject.New(declaration);
		CombineAssertions(() =>
		{
			AssertEquals("MessageTypeForEDIMessage", MessageTypeCodeList.Codes.EBD, sendingObject.MessageTypeForEDIMessage);
			AssertEquals("MessageSubTypeForEDIMessage", ZString.Empty, sendingObject.MessageSubTypeForEDIMessage);
		});
	}

	public void TestGetGlbExternalPasswordPK()
	{
		var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
		var sendingObject = (SupportingDocSendingObject)SupportingDocSendingObject.New(declaration);
		CombineAssertions(() =>
		{
			AssertEquals(credential.PK, sendingObject.GetCredentialPK());
		});
	}

	public void TestDocumentTypeList()
	{
		RefCusCodeTestHelper.CreateDocumentTypeList(Factory);

		var list = sendingObject.DocumentTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidDocumentTypeCode));
			AssertEquals("Invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidDocumentTypeCode));
		});
	}

	public override void TestEntries()
	{
		var cusEntryInstruction1 = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
		var cusEntryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		cusEntryHeader1.CH_CEI_Instruction = cusEntryInstruction1.PK;
		cusEntryInstruction1.CEI_Description = "desc-1111";
		cusEntryHeader1.MovementReferenceNumberSetter("1111");

		var cusEntryInstruction2 = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
		var cusEntryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		cusEntryHeader2.CH_CEI_Instruction = cusEntryInstruction2.PK;
		cusEntryInstruction2.CEI_Description = "desc-2222";
		cusEntryHeader2.MovementReferenceNumberSetter("2222");

		var cusEntryInstruction3 = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
		var cusEntryHeader3 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		cusEntryHeader3.CH_CEI_Instruction = cusEntryInstruction3.PK;
		cusEntryHeader3.CH_BGMReference = "xyz";

		CombineAssertions(() =>
		{
			sendingObject = declaration.GetSupportingDocSendingObject() as SupportingDocSendingObject;
			AssertContainsExactElementsInAnyOrder("Codes", new[] { "12341234", "1111", "2222" }, from e in sendingObject.Entries.ToArray() select e.Code);
			AssertContainsExactElementsInAnyOrder("Descriptions", new[] { "desc-12341234", "desc-1111", "desc-2222" }, from e in sendingObject.Entries.ToArray() select e.Description);
		});
	}

	public void TestLocalReferenceNumber()
	{
		AssertEquals("Caption", "Entry (MRN)", DataBoundResourceStrings.GetDataForProperty(sendingObject.LocalReferenceNumberInfo).Caption);
	}

	public void TestCaseNumber()
	{
		AssertEquals("Caption", "Reference", DataBoundResourceStrings.GetDataForProperty(sendingObject.CaseNumberInfo).Caption);
	}

	public void TestToMessageString()
	{
		SendingObject.EDoc = eDocOnDeclaration.UniqueKey;
		var message = SendingObject.ToMessageString();
		AssertEquals("Produces a XML", true, message.StartsWith("<?xml"));
	}

	SupportingDocSendingObject SendingObject => (SupportingDocSendingObject)sendingObject;

	protected override void SetUp()
	{
		base.SetUp();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_Description = "desc-" + entryHeader.MovementReferenceNumber;
	}
}
