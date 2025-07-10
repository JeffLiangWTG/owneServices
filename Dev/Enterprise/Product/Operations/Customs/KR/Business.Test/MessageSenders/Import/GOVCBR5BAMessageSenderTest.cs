using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BAMessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR5BASender>
	{
		public void TestValidationMode()
		{
			AssertEquals("Pre-condition", "Import", declaration.ValidationMode.ToString());
			var sender = GetMessageSender();
			AssertEquals("Import, AgreedRateForAllLines", declaration.ValidationMode.ToString());
			sender.Send();
			AssertEquals("Import", declaration.ValidationMode.ToString());
		}

		public void TestVersionIDFromEntryNum()
		{
			var entry = GetMessageParents().Cast<CusEntryHeader>().First();
			entry.CH_VersionID = 10;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = messageType;
			entryNum.CE_EntryLineReference = "20";
			MessageSender.Send();
			var message = entry.Messages[0];

			AssertEquals("21", message.EM_ApplicationReference);
		}

		readonly ZString messageType = ElectronicDocumentTypeList.Codes._5BA;

		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5BASenderForTest(EntriesToSend, Factory) : new GOVCBR5BASender(EntriesToSend, Factory);

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader1.AllEntryLines.AddNew();
					var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader2.AllEntryLines.AddNew();
					var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader3.AllEntryLines.AddNew();
					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;
		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
		}
	}

	class GOVCBR5BASenderForTest : GOVCBR5BASender
	{
		public GOVCBR5BASenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		protected override Import5BAHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new System.Exception();
	}
}
