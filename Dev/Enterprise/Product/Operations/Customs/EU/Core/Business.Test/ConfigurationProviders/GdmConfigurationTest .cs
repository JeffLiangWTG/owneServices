using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(GdmConfiguration))]
	public abstract class GdmConfigurationAbstractTest<G> : TestCaseWithFactory
	where G : GdmConfiguration
	{
		public abstract void TestWhatIsAWaiver();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (G)Activator.CreateInstance(typeof(G));
		}

		protected G configuration;
	}

	[TestedType(typeof(GdmConfiguration))]
	sealed class GdmConfigurationBaseTest : GdmConfigurationAbstractTest<GdmConfiguration>
	{
		public override void TestWhatIsAWaiver()
		{
			AssertEquals(@"^(?i)y.{3}$", configuration.WhatIsAWaiver);
		}

		public void TestIsAWaiver()
		{
			AssertEquals(true, configuration.IsAWaiver("y123"));
			AssertEquals(true, configuration.IsAWaiver("Y123"));
			AssertEquals(false, configuration.IsAWaiver("Y12"));
			AssertEquals(false, configuration.IsAWaiver("Y1234"));
			AssertEquals(false, configuration.IsAWaiver("a123"));
		}
	}
}
