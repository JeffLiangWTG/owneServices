using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class ExitControlMessageProcessorTest : TestCase
	{
		public void TestExitControlMessageProcessors()
		{
			AssertEquals(5, messageProcessors.Count);
		}

		public void TestEXTCTL()
		{
			AssertEquals(typeof(EXTCTLMessageProcessor), messageProcessors["DEXTL"]);
		}

		public void TestDEXTJE()
		{
			AssertEquals(typeof(EXTREJMessageProcessor), messageProcessors["DEXTJ"]);
		}

		public void TestDEXTSE()
		{
			AssertEquals(typeof(EXTSTAMessageProcessor), messageProcessors["DEXTS"]);
		}

		public void TestDEXTDE()
		{
			AssertEquals(typeof(EXTDATMessageProcessor), messageProcessors["DEXTD"]);
		}

		public void TestDEERRG()
		{
			AssertEquals(typeof(ERRNCKMessageProcessor), messageProcessors["DEERR"]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			messageProcessors = new ExitControlMessageProcessor().ExitControlMessageProcessors;
		}

		Dictionary<string, Type> messageProcessors;
	}
}
