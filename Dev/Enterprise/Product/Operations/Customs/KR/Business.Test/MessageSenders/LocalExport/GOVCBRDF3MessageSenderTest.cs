using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRDF3MessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBRDF3MessageSender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;
		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBRDF3MessageSenderForTest(EntriesToSend, Factory) : new GOVCBRDF3MessageSender(EntriesToSend, Factory);
		IEnumerable<CusEntryHeader> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<CusEntryHeader> parents;
		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetLocalExportHasAmendSnapShot();
					entriesToSend = entry.Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;

		public void TestCusEntryNumCreated()
		{
			GetMessageSender().Send();
			foreach (CusEntryHeader parent in Parents)
			{
				var entryNum = parent.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._DF3);
				AssertNotNull("CusEntryNum was created type to DF3.", entryNum);
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entryNum.CE_EntryStatus);
				AssertEquals("CE_EntryNum was set ILocalExportAmendEntryHeader.CustomsReceiptNumber", parent.CH_BGMReference, entryNum.CE_EntryNum);
			}
		}

		public void TestStatusNotUpdated()
		{
			GetMessageSender().Send();
			foreach (CusEntryHeader parent in Parents)
			{
				AssertEquals(parent.CH_StatusInfo.OriginalValue.ToString(), parent.CH_Status);
			}
		}

		public void TestSnapShotNotCreated()
		{
			GetMessageSender().Send();
			foreach (CusEntryHeader parent in Parents)
			{
				AssertEquals(2, parent.Snapshots.Count);

				var originalSnapShot = parent.Snapshots.GetFirstSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, "LDG");
				AssertEquals("This SnapShot is Original SnapShot.", 1u, originalSnapShot.CES_VersionNumber);
				var amendmentSnapShot = parent.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, "LDG");
				AssertEquals("This SnapShot is Amendment SnapShot.", 2u, amendmentSnapShot.CES_VersionNumber);
			}
		}
	}

	class GOVCBRDF3MessageSenderForTest : GOVCBRDF3MessageSender
	{
		public GOVCBRDF3MessageSenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
		: base(entries, factory)
		{
		}

		protected override LocalExportAmendEntryHeader GetMessageDataProvider(CusEntryHeader entry, ZString messageID) => throw new System.Exception();
	}
}
