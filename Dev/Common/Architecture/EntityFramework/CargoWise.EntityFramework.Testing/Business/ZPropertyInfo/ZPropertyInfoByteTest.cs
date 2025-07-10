using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoByteTest : TestCaseWithDummy
	{
		public void TestOriginalValue()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_Byte = 12;
			Factory.Save();
			bizO.Z0_Byte = 13;
			AssertEquals((Byte)12, bizO.Z0_ByteInfo.OriginalValue);
		}

		public void TestSetValueFromString()
		{
			Dummy.Z0_Byte = 0;
			ZPropertyInfoByte info = (ZPropertyInfoByte)Dummy.Z0_ByteInfo;
			AssertEquals(true, info.SetValueFromString("255"));
			AssertEquals((byte)255, Dummy.Z0_Byte);

			AssertEquals(false, info.SetValueFromString("256"));
			AssertEquals((byte)255, Dummy.Z0_Byte);
		}
	}
}
