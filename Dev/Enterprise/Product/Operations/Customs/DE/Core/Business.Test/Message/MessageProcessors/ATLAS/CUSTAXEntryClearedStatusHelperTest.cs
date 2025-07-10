using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.EntryStatus;

namespace Enterprise.Customs.DE.Business.Testing.Message.MessageProcessors.ATLAS
{
	sealed class CUSTAXEntryClearedStatusHelperTest : TestCase
	{
		public void TestIsCleared_WithTX1_ShouldBeCleared()
		{
			AssertEquals(true, CUSTAXEntryClearedStatusHelper.IsCleared(TX1));
		}

		public void TestIsCleared_WithTX2_ShouldBeCleared()
		{
			AssertEquals(true, CUSTAXEntryClearedStatusHelper.IsCleared(TX2));
		}

		public void TestIsCleared_WithTX3_ShouldBeCleared()
		{
			AssertEquals(true, CUSTAXEntryClearedStatusHelper.IsCleared(TX3));
		}

		public void TestIsCleared_WithTX4_ShouldNotBeCleared()
		{
			AssertEquals(false, CUSTAXEntryClearedStatusHelper.IsCleared(TX4));
		}

		public void TestIsCleared_WithTX5_ShouldNotBeCleared()
		{
			AssertEquals(true, CUSTAXEntryClearedStatusHelper.IsCleared(TX5));
		}

		public void TestIsCleared_WithTX6_ShouldBeCleared()
		{
			AssertEquals(true, CUSTAXEntryClearedStatusHelper.IsCleared(TX6));
		}

		public void TestIsCleared_WithTX7_ShouldNotBeCleared()
		{
			AssertEquals(false, CUSTAXEntryClearedStatusHelper.IsCleared(TX7));
		}

		public void TestIsCleared_WithTX8_ShouldBeCleared()
		{
			AssertEquals(true, CUSTAXEntryClearedStatusHelper.IsCleared(TX8));
		}
	}
}
