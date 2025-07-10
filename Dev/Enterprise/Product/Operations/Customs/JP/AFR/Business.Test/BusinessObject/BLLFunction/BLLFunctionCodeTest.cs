using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BLLFunctionCodeTest : TestCase
	{
		public void TestCaption()
		{
			AssertEquals("Register Split", BLLFunctionCode.RegisterSplit.GetCaption());
			AssertEquals("Register Switch", BLLFunctionCode.RegisterSwitch.GetCaption());
			AssertEquals("Register Merge", BLLFunctionCode.RegisterMerge.GetCaption());
			AssertEquals("Cancel Split", BLLFunctionCode.CancelSplit.GetCaption());
			AssertEquals("Cancel Switch", BLLFunctionCode.CancelSwitch.GetCaption());
			AssertEquals("Cancel Merge", BLLFunctionCode.CancelMerge.GetCaption());
		}
	}
}
