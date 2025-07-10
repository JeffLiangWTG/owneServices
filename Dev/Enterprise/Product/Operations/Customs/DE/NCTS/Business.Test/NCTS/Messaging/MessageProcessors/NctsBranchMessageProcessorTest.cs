using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsBranchMessageProcessorTest : TestCase
	{
		public void TestNctsMessageProcessors()
		{
			AssertEquals(10, messageProcessors.Count);
		}

		public void TestDETBS()
		{
			AssertEquals(typeof(NctsTBESTAMessageProcessor), messageProcessors["DETBS"]);
		}

		public void TestDETQS()
		{
			AssertEquals(typeof(NctsTRQSTAMessageProcessor), messageProcessors["DETQS"]);
		}

		public void TestDETSP()
		{
			AssertEquals(typeof(NctsDESPERMessageProcessor), messageProcessors["DETSP"]);
		}

		public void TestDETPR()
		{
			AssertEquals(typeof(NctsDEPRELMessageProcessor), messageProcessors["DETPR"]);
		}

		public void TestDETSS()
		{
			AssertEquals(typeof(NctsDESSTAMessageProcessor), messageProcessors["DETSS"]);
		}

		public void TestDETPI()
		{
			AssertEquals(typeof(NctsDEPINCMessageProcessor), messageProcessors["DETPI"]);
		}

		public void TestDETPJ()
		{
			AssertEquals(typeof(NctsDEPREJMessageProcessor), messageProcessors["DETPJ"]);
		}

		public void TestDETPS()
		{
			AssertEquals(typeof(NctsDEPSTAMessageProcessor), messageProcessors["DETPS"]);
		}

		public void TestDETSJ()
		{
			AssertEquals(typeof(NctsDESREJMessageProcessor), messageProcessors["DETSJ"]);
		}

		public void TestDETGA()
		{
			AssertEquals(typeof(NctsGUAACKMessageProcessor), messageProcessors["DETGA"]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageProcessors = new NctsBranchMessageProcessor().NctsMessageProcessors;
		}
		Dictionary<string, Type> messageProcessors;
	}
}
