using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class NctsBranchMessageProcessorProviderTest : TestCase
	{
		public void TestNctsMessageProcessors()
		{
			AssertEquals(22, messageProcessors.Count);
		}

		public void TestACK()
		{
			AssertEquals(typeof(AcknowledgementMessageProcessor), messageProcessors["ACK"]);
		}

		public void Test004()
		{
			AssertEquals(typeof(CC004CMessageProcessor), messageProcessors["004"]);
		}

		public void Test009()
		{
			AssertEquals(typeof(CC009CMessageProcessor), messageProcessors["009"]);
		}

		public void Test019()
		{
			AssertEquals(typeof(CC019CMessageProcessor), messageProcessors["019"]);
		}

		public void Test022()
		{
			AssertEquals(typeof(CC022CMessageProcessor), messageProcessors["022"]);
		}

		public void Test025()
		{
			AssertEquals(typeof(CC025CMessageProcessor), messageProcessors["025"]);
		}

		public void Test028()
		{
			AssertEquals(typeof(CC028CMessageProcessor), messageProcessors["028"]);
		}

		public void Test029()
		{
			AssertEquals(typeof(CC029CMessageProcessor), messageProcessors["029"]);
		}

		public void Test035()
		{
			AssertEquals(typeof(CC035CMessageProcessor), messageProcessors["035"]);
		}

		public void Test043()
		{
			AssertEquals(typeof(CC043CMessageProcessor), messageProcessors["043"]);
		}

		public void Test045()
		{
			AssertEquals(typeof(CC045CMessageProcessor), messageProcessors["045"]);
		}

		public void Test051()
		{
			AssertEquals(typeof(CC051CMessageProcessor), messageProcessors["051"]);
		}

		public void Test055()
		{
			AssertEquals(typeof(CC055CMessageProcessor), messageProcessors["055"]);
		}

		public void Test056()
		{
			AssertEquals(typeof(CC056CMessageProcessor), messageProcessors["056"]);
		}

		public void Test057()
		{
			AssertEquals(typeof(CC057CMessageProcessor), messageProcessors["057"]);
		}

		public void Test060()
		{
			AssertEquals(typeof(CC060CMessageProcessor), messageProcessors["060"]);
		}

		public void Test140()
		{
			AssertEquals(typeof(CC140CMessageProcessor), messageProcessors["140"]);
		}

		public void Test182()
		{
			AssertEquals(typeof(CC182CMessageProcessor), messageProcessors["182"]);
		}

		public void Test917()
		{
			AssertEquals(typeof(CC917CMessageProcessor), messageProcessors["917"]);
		}

		public void Test928()
		{
			AssertEquals(typeof(CC928CMessageProcessor), messageProcessors["928"]);
		}

		public void TestNCT()
		{
			AssertEquals(typeof(IENCTS043MessageProcessor), messageProcessors["NCT"]);
		}

		public void TestUniversalEventData()
		{
			AssertEquals(typeof(NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor), messageProcessors["UER"]);
		}

		Dictionary<string, Type> messageProcessors;
		protected override void SetUp()
		{
			base.SetUp();
			messageProcessors = new NctsBranchMessageProcessorProvider().NctsMessageProcessors;
		}
	}
}
