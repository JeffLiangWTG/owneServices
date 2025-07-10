using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestAddInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LevyType = "101";
			entryInstruction.CEI_PackageUQ = "99";
			entryInstruction.CustomsMessageRemarks = "Customs Message Remarsk";

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);

			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("LevyType")).Value == new ZString?("101"));
			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("PackageUQ")).Value == new ZString?("99"));
			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?(DataTransfer.Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks)).Value == new ZString?("Customs Message Remarsk"));
		}

		public void TestAddInfos_EmptyCustomsMessageRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LevyType = "101";
			entryInstruction.CEI_PackageUQ = "99";
			entryInstruction.CustomsMessageRemarks = ZString.Empty;

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);

			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("LevyType")).Value == new ZString?("101"));
			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("PackageUQ")).Value == new ZString?("99"));
			var cusMessageRemarksAddInfo = result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?(DataTransfer.Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks));
			Assert(cusMessageRemarksAddInfo.Value == new ZString?(ZString.Empty));
		}

		public void TestAddInfos_NullCustomsMessageRemarks_StillEmptyDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LevyType = "101";
			entryInstruction.CEI_PackageUQ = "99";

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);

			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("LevyType")).Value == new ZString?("101"));
			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("PackageUQ")).Value == new ZString?("99"));
			var cusMessageRemarksAddInfo = result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?(DataTransfer.Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks));
			Assert(cusMessageRemarksAddInfo.Value == new ZString?(ZString.Empty));
		}

		public void TestAddInfos_CIQRequires()
		{
			var testItem = Business.Testing.CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(testItem.EntryInstruction);

			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("CIQRequires")).Value == new ZString?("N"));

			testItem.InvoiceLine.JI_CIQTariff = "111";
			testItem.EntryInstruction.CEI_CIQRequires = true;
			result = writer.GetDataObject(testItem.EntryInstruction);
			Assert(result.AddInfoCollection.First(addinfo => addinfo.Key == new ZString?("CIQRequires")).Value == new ZString?("Y"));
		}

		public void TestAddInfos_BillOfLading()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			entryInstruction.BillOfLading = "BILL001";
			entryInstruction.BillOfLadingDate = ZDateTime.Today;

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);

			var addinfoBL = result.AddInfoCollection.First(addinfo => addinfo.Key.Value == DataTransfer.Constants.AddInfoKeys.EntryInstruction.BillOfLading);
			AssertNotNull(addinfoBL);
			AssertEquals("BILL001", addinfoBL.Value);

			var addinfoBLDate = result.AddInfoCollection.First(addinfo => addinfo.Key.Value == DataTransfer.Constants.AddInfoKeys.EntryInstruction.BillOfLadingDate);
			AssertNotNull(addinfoBLDate);
			ZDateTime.TryParseISO8601Date(addinfoBLDate.Value, out var resultDate);
			AssertEquals(ZDateTime.Today, resultDate);
		}

		public void TestAddInfoGroups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_CIQRequires = true;

			var ciqReqDoc1 = entryInstruction.CIQRequiredDocuments.AddNew();
			ciqReqDoc1.XC_DocumentType = "13";
			ciqReqDoc1.XC_NumberOfCopies = 1;
			ciqReqDoc1.XC_NumberOfOriginals = 2;

			Factory.Save();

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);

			AssertEquals(1, result.AddInfoGroupCollection.Count);
			Assert(result.AddInfoGroupCollection.Any(group =>
				group.Type.Code == new ZString?("RQD")
				&& group.AddInfoCollection.Count == 3
				&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("DocumentType") && innerAddinfo.Value == new ZString?("13"))
				&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("NumberOfCopies") && innerAddinfo.Value == new ZString?("1"))
				&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("NumberOfOriginals") && innerAddinfo.Value == new ZString?("2"))
			));
		}

		public void TestAddInfoGroup_Attachments()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "TST");
			var attachment = instruction.Attachments.AddNew();
			attachment.EDoc = eDoc.UniqueKey;
			attachment.AttachmentType = "00000001";
			attachment.AttachmentNumber = "123456789012";
			Factory.Save();
			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(instruction);
			AssertEquals(1, result.AddInfoGroupCollection.Count);
			Assert(result.AddInfoGroupCollection.Any(group =>
				group.Type.Code == new ZString?(DataTransfer.Constants.EntryInstruction.Codes.EIA)
				&& group.Type.Description == new ZString?(DataTransfer.Constants.EntryInstruction.Descriptions.EIA)
				&& group.AddInfoCollection.Count == 2
				&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?(DataTransfer.Constants.Attachment.Keys.AttachmentFileName) && innerAddinfo.Value == new ZString?("Test1.pdf"))
				&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?(DataTransfer.Constants.Attachment.Keys.AttachmentType) && innerAddinfo.Value == new ZString?("00000001"))
			));
		}

		public void TestAddInfoGroup_Attachments_EntryLineLinks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var instruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.EDoc = eDoc.UniqueKey;
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.CargoAttributes.AddNew(CargoAttributeList.Codes._31);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			((AttachmentInvoiceLineLink)invoiceLine1.AttachmentLinks.First()).IsLinked = true;
			((AttachmentInvoiceLineLink)invoiceLine2.AttachmentLinks.First()).IsLinked = true;

			var writer = new CNEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(instruction);

			var addInfoGroupEIA = result.AddInfoGroupCollection.Single(addInfoGroup => addInfoGroup.Type.Code.Equals(Constants.EntryInstruction.Codes.EIA));
			CombineAssertions("", () =>
			{
				var entryLineLink = addInfoGroupEIA.AddInfoCollection.FirstOrDefault(addInfo => addInfo.Key.Equals(Constants.Attachment.Keys.EntryLineLinks));
				AssertNotNull("AddInfo should exist: EntryLineLinks", entryLineLink);
				AssertEquals("Value: Numbers of All linked EntryLine.", "1,2", entryLineLink.Value);
			});
		}
	}
}
