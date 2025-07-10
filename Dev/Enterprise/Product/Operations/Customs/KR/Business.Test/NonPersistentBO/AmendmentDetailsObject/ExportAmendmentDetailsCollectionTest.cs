using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportAmendmentDetailsCollection))]
	sealed class ExportAmendmentDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExportAmendmentDetailsCollection>
	{
		protected override ExportAmendmentDetailsCollection GetCollectionToTest() => new ExportAmendmentDetailsCollection(Factory.New<CusEntryHeader>());
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new ExportAmendmentDetails(new Export5ASHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>()), Factory, entry.PK);
		}

		public void TestVersionOfAmendments()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.CusEntryNumber.CE_EntryNum = "6N00220000051X";
			entry.CH_VersionID = 1;
			entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 1).Delete();
			entry.ResetIsCustomsValueCalculated();

			var message5AS1 = SetSnapShotData5AS(entry, ExportAmendmentReasonCodeList.Codes._11);
			AssertEquals("2", message5AS1.EM_ApplicationReference);
			var message5AS2 = SetSnapShotData5AS(entry, ExportAmendmentReasonCodeList.Codes._12);
			AssertEquals("2", message5AS2.EM_ApplicationReference);

			var amendmentDetailsCollection1 = new ExportAmendmentDetailsCollection(entry);
			AssertEquals(1, amendmentDetailsCollection1.Count);
			AssertEquals(ExportAmendmentReasonCodeList.Codes._12, amendmentDetailsCollection1[0].ReasonCode);

			entry.CH_VersionID = 2;

			var message5AS3 = SetSnapShotData5AS(entry, ExportAmendmentReasonCodeList.Codes._13);
			AssertEquals("3", message5AS3.EM_ApplicationReference);

			var amendmentDetailsCollection2 = new ExportAmendmentDetailsCollection(entry);
			AssertEquals(2, amendmentDetailsCollection2.Count);
			AssertEquals(ExportAmendmentReasonCodeList.Codes._12, amendmentDetailsCollection2[0].ReasonCode);
			AssertEquals(ExportAmendmentReasonCodeList.Codes._13, amendmentDetailsCollection2[1].ReasonCode);
		}
		EDIMessage SetSnapShotData5AS(CusEntryHeader entry, ZString reason)
		{
			var declaration = Factory.Load<JobDeclaration>(entry.Declaration.PK);
			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, "5AS");
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ReasonCode = reason;

			new GOVCBR5ASAmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory).Send();
			Factory.Save();

			return entry.Messages.Cast<EDIMessage>().LastOrDefault(x => x.EM_MessageType == "5AS");
		}
	}
}
