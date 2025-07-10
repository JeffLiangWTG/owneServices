using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	class EntryActionCodeListTest : TestCaseWithFactory
	{
		public void TestGetMessageCodeNumber()
		{
			#region DeltaC
			AssertEquals(1, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT));
			AssertEquals(2, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL));
			AssertEquals(3, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MAP));
			AssertEquals(4, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANA));
			AssertEquals(5, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAA));
			AssertEquals(6, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.EAV));
			AssertEquals(7, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV));
			AssertEquals(8, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.CMP));
			AssertEquals(9, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.RPS));
			AssertEquals(10, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.REC));
			AssertEquals(11, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAR));
			AssertEquals(12, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANR));
			AssertEquals(2, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.D2M, false));
			AssertEquals(-1, EntryActionCodeList.GetMessageCodeNumber("UNKNOWCODE"));
			#endregion
			#region DeltaD
			AssertEquals(1, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL, false));
			AssertEquals(2, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.D2M, false));
			AssertEquals(3, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT, false));
			AssertEquals(4, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDV, false));
			AssertEquals(5, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.RPS, false));
			AssertEquals(6, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDA, false));
			AssertEquals(7, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAA, false));
			AssertEquals(8, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANN, false));
			AssertEquals(9, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.REC, false));
			AssertEquals(10, EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV, false));
			#endregion
			AssertEquals(-1, EntryActionCodeList.GetMessageCodeNumber("UNKNOWCODE", false));
		}
	}
}
