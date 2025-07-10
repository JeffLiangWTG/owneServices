using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondStatusCalculator))]
	sealed class CusUnderbondStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 3, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.UBMREQ, Calculator.InterestedMessageTypes[0]);
			AssertEquals("InterestedMessageTypes[1]", CMRMessage.CMRMessageTypes.UBMREQE, Calculator.InterestedMessageTypes[1]);
			AssertEquals("InterestedMessageTypes[2]", CMRMessage.CMRMessageTypes.UBMREQR, Calculator.InterestedMessageTypes[2]);
		}

		public void TestAutoUnderbondSendingIsCancelled()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08116326240";
			mAWB.CM_FlightNo = "QF123";
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			Factory.Save();
			var calculator = new CusUnderbondStatusCalculatorForTest(underbond1);

			CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond1);
			_ = manager.GenerateOriginalMessages(underbond1);
			Factory.Save();
			calculator.DeriveStatus();

			CMRUBMREQRMessage accepted = Factory.New<CMRUBMREQRMessage>();
			accepted.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+1111A::95'
LOC+4+2222O::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
RFF+MB:08116326240'
UNT+16+000002'".Replace("\r\n", "");
			underbond1.Messages.Add(accepted);
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			calculator.DeriveStatus();
			Assert("Should not be set to auto underbond", underbond1.C4_Status.IsEmpty);
		}

		public void TestGetStatusFromInboundMessage()
		{
			AssertEquals(CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived, Calculator.GetStatusFromInboundMessage(UBMREQRMessage));
		}

		protected override BusinessObject GetNewBusinessObject() => new CusUnderbondStatusCalculator(CusUnderbond.New(Factory));

		CMRUBMREQRMessage ubmreqrMessage;
		CMRUBMREQRMessage UBMREQRMessage
		{
			get
			{
				if (ubmreqrMessage == null)
				{
					ubmreqrMessage = Factory.New<CMRUBMREQRMessage>();
					ubmreqrMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3E65 FG0F 9GFF:1+32'
DTM+9:20050627174622089471:ZZZ'
FTX+AAH+++AAA374MU00000058/SYD3'
TDT+20+32123++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000010++BM:185:95'
RFF+AAQ:112233'
UNT+16+000001'
".Replace("\r\n", "");
				}
				return ubmreqrMessage;
			}
		}

		CusUnderbondStatusCalculator calculator;
		CusUnderbondStatusCalculator Calculator => calculator ?? (calculator = (CusUnderbondStatusCalculator)GetNewBusinessObject());

		internal class CusUnderbondStatusCalculatorForTest : CusUnderbondStatusCalculator
		{
			public CusUnderbondStatusCalculatorForTest(CusUnderbond underbond) : base(underbond)
			{
			}

			internal new void DeriveStatus() => base.DeriveStatus();
		}
	}
}
