using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class JobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestDeclarationDescription()
	{
		Declaration.JE_MessageType = "IMP";
		Declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		EntryInstruction.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo;
		EntryInstruction.CEI_Description = "DESCR";

		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = EntryInstruction.PK;

		var testItem = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		AssertEquals("JobDeclarationMessageSendingObject.DeclarationDescription should be", "DESCR", testItem.DeclarationDescription);
	}

	public void TestValidation()
	{
		var testItem = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		AssertNotNull("Validation", testItem.Validation);
		AssertEquals("Validation type", ExpectedValidationType, testItem.Validation.GetType());
	}

	protected virtual Type ExpectedValidationType => typeof(JobDeclarationMessageSendingObjectValidation);

	public void TestIsEntryInAmendingStatus()
	{
		var testItem = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		EntryHeader.CH_EntryStatus = "AMG";
		AssertEquals("When EntryHeader is in Amending Status IsEntryInAmendingStatus is true", true, testItem.IsEntryInAmendingStatus);

		EntryHeader.CH_EntryStatus = "AWO";
		AssertEquals("When EntryHeader is in Amending Status IsEntryInAmendingStatus is false", false, testItem.IsEntryInAmendingStatus);
	}

	public virtual void TestStatusAllowSending()
	{
		var testItem = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		EntryHeader.CH_EntryStatus = "";
		EntryHeader.CH_Status = "";
		AssertEquals("[CHStatus: Empty, CH_EntryStatus: Empty] StatusAllowsSending", true, testItem.StatusAllowsSending);

		EntryHeader.CH_Status = "AWO";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: Empty] StatusAllowsSending", false, testItem.StatusAllowsSending);

		EntryHeader.CH_Status = "ERO";
		AssertEquals("[CHStatus: ERO, CH_EntryStatus: Empty] StatusAllowsSending", true, testItem.StatusAllowsSending);

		EntryHeader.CH_Status = "ACO";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: Empty] StatusAllowsSending", false, testItem.StatusAllowsSending);

		EntryHeader.CH_EntryStatus = "NBR";
		EntryHeader.CH_Status = "ACO";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: NBR] StatusAllowsSending", true, testItem.StatusAllowsSending);

		EntryHeader.CH_EntryStatus = "NBR";
		EntryHeader.CH_Status = "AWO";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: NBR] StatusAllowsSending", false, testItem.StatusAllowsSending);

		EntryHeader.CH_Status = "FFT";
		AssertEquals("[CHStatus: FFT, CH_EntryStatus: NBR] StatusAllowsSending", true, testItem.StatusAllowsSending);
	}

	public void TestMessageTypeCaption()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(sendingObject.MessageTypeInfo);
		AssertEquals("Caption", "Msg. Type", resourceStringData.Caption);
	}

	public void TestMessageTypeReadOnly()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("ReadOnly", false, sendingObject.MessageTypeInfo.ReadOnly);
	}

	public void TestVOCReasonCaption()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(sendingObject.VOCReasonInfo);
		AssertEquals("Caption", "Reason", resourceStringData.Caption);
	}

	public void TestIsAmend()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("By default, IsAmend is false", false, sendingObject.IsAmend);

		sendingObject.MessageType = "AMD";
		AssertEquals("When Messagetype is AMD, IsAmend is true", true, sendingObject.IsAmend);
	}

	public void TestVOCReasonReadOnly()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "";
		AssertEquals("ReadOnly is true by default", true, sendingObject.VOCReasonInfo.ReadOnly);

		sendingObject.MessageType = "CAN";
		AssertEquals("When message type is CAN, ReadOnly must be false", false, sendingObject.VOCReasonInfo.ReadOnly);

		sendingObject.MessageType = "AMD";
		AssertEquals("When message type is AMG, ReadOnly must be false", false, sendingObject.VOCReasonInfo.ReadOnly);
	}

	public void TestVOCReasonMaxLength()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("MaxLength", 1, sendingObject.VOCReasonInfo.MaxLength);
	}

	public void TestCancellationLegislativeReferenceCaption()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(sendingObject.CancellationAndAmendmentLegislativeReferenceInfo);
		AssertEquals("Caption", "Reference", resourceStringData.Caption);
	}

	public void TestCancellationLegislativeReferenceReadOnly()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "";
		AssertEquals("ReadOnly is true by default", true, sendingObject.CancellationAndAmendmentLegislativeReferenceInfo.ReadOnly);

		sendingObject.MessageType = "CAN";
		AssertEquals("When MessageType is CAN, ReadOnly is false", false, sendingObject.CancellationAndAmendmentLegislativeReferenceInfo.ReadOnly);

		sendingObject.MessageType = "AMD";
		AssertEquals("When MessageType is AMD, ReadOnlyis false", false, sendingObject.CancellationAndAmendmentLegislativeReferenceInfo.ReadOnly);
	}

	public void TestCancellationLegislativeReferenceMaxLength()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("MaxLength", 1, sendingObject.CancellationAndAmendmentLegislativeReferenceInfo.MaxLength);
	}

	public void TestCleanUpVOCReasonAndCancellationLegislativeReferenceIfNoLongerApplicable()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		sendingObject.VOCReason = "A";
		sendingObject.CancellationAndAmendmentLegislativeReference = "1";

		sendingObject.MessageType = "";
		CombineAssertions(() =>
		{
			AssertEquals("VOCReason", "", sendingObject.VOCReason);
			AssertEquals("CancellationAndAmendmentLegislativeReference", "", sendingObject.CancellationAndAmendmentLegislativeReference);
		});
	}

	public void TestIsCancel()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		AssertEquals("IsCancel", true, sendingObject.IsCancel);

		sendingObject.MessageType = "";
		AssertEquals("IsCancel", false, sendingObject.IsCancel);
	}

	public void TestCombinedCustomsMessageSubTypeCaption()
	{
		var sendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(sendingObject.CombinedCustomsMessageSubTypeInfo);
		AssertEquals("Caption", "Msg. Sub Type", resourceStringData.Caption);
	}

	public abstract void TestCombinedCustomsMessageSubType();

	public void TestCH_BGMReference()
	{
		EntryHeader.CH_BGMReference = "BGMReference001";
		var testItem = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		AssertEquals("CH_BGMReference", "BGMReference001", testItem.CH_BGMReference);
	}

	public void TestJobDeclarationMessageSendingObjectParent()
	{
		var jobDeclarationMessageSendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		AssertSame("JobDeclarationMessageSendingObjectParent", JobDeclarationMessageSendingObjectParent, jobDeclarationMessageSendingObject.JobDeclarationMessageSendingObjectParent);
	}

	public void TestCanBeSentOnlyInFallbackMode()
	{
		var jobDeclarationMessageSendingObject = (JobDeclarationMessageSendingObject)GetNewBusinessObject();

		EntryInstruction.CEI_Style = "COD";
		AssertEquals("When Style is COD, CanBeSentOnlyInFallbackMode", false, jobDeclarationMessageSendingObject.CanBeSentOnlyInFallbackMode);

		EntryInstruction.CEI_SubStyle = "D";
		AssertEquals("When Style is COD and Sub Style is D, CanBeSentOnlyInFallbackMode", true, jobDeclarationMessageSendingObject.CanBeSentOnlyInFallbackMode);

		EntryInstruction.CEI_Style = "DSE";
		EntryInstruction.CEI_SubStyle = "A";
		AssertEquals("When Style is DSE and Sub Style is A, CanBeSentOnlyInFallbackMode", true, jobDeclarationMessageSendingObject.CanBeSentOnlyInFallbackMode);

		EntryInstruction.CEI_Style = "";
		EntryInstruction.CEI_SubStyle = "";
		AssertEquals("When Style is Empty and Sub Style is Empty, CanBeSentOnlyInFallbackMode", false, jobDeclarationMessageSendingObject.CanBeSentOnlyInFallbackMode);
	}

	public void TestISadCustomsMessageGeneratorValuesProviderMembers()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		Declaration.JE_CustomsOffice = "IT137100";
		Declaration.JE_CustomsProfile = "1234-DEC1";
		Declaration.JE_GS_NKCusAgent = "XXX";

		ConfigureDataForISadCustomsMessageGeneratorValuesProviderMembersTest();

		var customsMessage = (IOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		CombineAssertions(() =>
		{
			AssertEquals("ApplicationReference", ExpectedApplicationReference, customsMessage.GetApplicationReference());
			AssertEquals("SubType", ExpectedMessageSubType, customsMessage.GetSubType());
			AssertSame("Parent", EntryHeader, customsMessage.Parent);
		});
	}

	protected virtual void ConfigureDataForISadCustomsMessageGeneratorValuesProviderMembersTest()
	{
	}

	protected virtual string ExpectedApplicationReference => "1234:XXX:IT137100";

	public void TestIEntryMessageSendingObjectInfo()
	{
		EntryHeader.CH_BGMReference = "A00";
		entryHeader.CH_Status = ITMessageStatusList.Codes.ErrorOriginal;
		entryHeader.CH_EntryStatus = ITEntryStatusList.Codes.Registered;
		CombineAssertions(() =>
		{
			var entryMessageSendingObjectInfo = (IEntryMessageSendingObjectInfo)GetNewBusinessObject();
			AssertEquals("EntryStatusAllowsSending", true, entryMessageSendingObjectInfo.EntryStatusAllowsSending);
			AssertEquals("EntryReference", "A00", entryMessageSendingObjectInfo.EntryReference);
			AssertEquals("EntryMessageStatus", ITMessageStatusList.Codes.ErrorOriginal, entryMessageSendingObjectInfo.EntryMessageStatus);
			AssertEquals("EntryCustomsStatus", ITEntryStatusList.Codes.Registered, entryMessageSendingObjectInfo.EntryCustomsStatus);
		});
	}

	protected abstract ZString ExpectedMessageSubType { get; }

	protected override BusinessObject GetNewBusinessObject()
	{
		return (JobDeclarationMessageSendingObject)Activator.CreateInstance(GetExpectedBusinessObjectType(), EntryHeader, JobDeclarationMessageSendingObjectParent);
	}

	protected JobDeclaration Declaration
	{
		get
		{
			if (declaration == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.CustomsEntryInstructions.AddNew();
			}
			return declaration;
		}
	}
	JobDeclaration declaration;

	protected CusEntryHeader EntryHeader
	{
		get
		{
			if (entryHeader == null)
			{
				entryHeader = Declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryHeader.CH_CEI_Instruction = Declaration.CustomsEntryInstructions[0].PK;
				InvoiceLine.JI_CL = entryLine.PK;
			}
			return entryHeader;
		}
	}
	CusEntryHeader entryHeader;

	protected JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew());
	JobComInvoiceLine invoiceLine;

	protected JobDeclarationMessageSendingObjectParent JobDeclarationMessageSendingObjectParent => jobDeclarationMessageSendingObjectParent ?? (jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(Declaration));
	JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent;

	protected CusEntryInstruction EntryInstruction => Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();
}
