using System;
using CargoWise.Customs.DE.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXTINFMessageHeaderProvider))]
	sealed class EXTINFMessageHeaderProviderTest : ExitMessageHeaderProviderAbstractTest<EXTINFMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTINFMessageHeaderProvider(null));
		}

		protected override IExitMessageHeader GetMessageHeaderProvider() => new EXTINFMessageHeaderProvider(exitReport);
	}
}
