using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DocREFACCInfoProviderTest : DocD99BCUSRESInfoProviderTest
	{
		protected override D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES)
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			CMRREFACCMessage message = (CMRREFACCMessage)entryHeader.Messages.AddNew(typeof(CMRREFACCMessage));
			message.EM_SystemCreateTimeUtc = new ZDateTime(2010, 01, 02, 12, 12, 12);
			message.EM_LinkedObject = entryHeader;
			return new REFACCInfoProvider(cUSRES, message);
		}

		protected override DocD99BCUSRESInfoProvider GetWrapper(D99BCUSRESInfoProvider infoProvider, BusinessObjectFactory factory)
		{
			return DocREFACCInfoProvider.New((REFACCInfoProvider)infoProvider, factory);
		}

		public override void TestPaymentFinalisedDate()
		{
			AssertEquals(Env.Time.GetLocalTimeFromUtc(new DateTime(2010, 01, 02, 12, 12, 12, 0)), Wrapper.PaymentFinalisedDate);
		}

		public void TestCharges()
		{
			AssertEquals("Count", 1, Wrapper.Charges.Count);
			AssertEquals("Item description", "TOTAL REFUNDABLE AMOUNT", Wrapper.Charges[0].Description);
		}
	}
}
