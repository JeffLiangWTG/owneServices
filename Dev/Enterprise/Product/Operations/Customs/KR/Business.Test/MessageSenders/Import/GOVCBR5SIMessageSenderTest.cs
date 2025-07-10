using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SIMessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR5SISender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5SISenderForTest(EntriesToSend, Factory) : new GOVCBR5SISender(EntriesToSend, Factory);

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var declaration = Factory.New<JobDeclaration>();
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
	}

	class GOVCBR5SISenderForTest : GOVCBR5SISender
	{
		public GOVCBR5SISenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		protected override Import5SIHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new System.Exception();
	}
}
