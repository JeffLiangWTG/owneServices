using System;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOImportModule))]
	sealed class AirCTOImportModuleTest : CMRModuleTest
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.ImportAirCTOReport, testModule.LicenceCheckPoint);
		}

		public override void TestExceptionsFilter()
		{
			base.TestExceptionsFilter();
			ErrorReporter.Instance.Clear();
		}

		public override void TestTriggersFilter()
		{
			base.TestTriggersFilter();
			ErrorReporter.Instance.Clear();
		}

		public override void TestMilestonesFilter()
		{
			base.TestMilestonesFilter();
			ErrorReporter.Instance.Clear();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.AirCTOImport;

		protected override Type GetTypeOfFilterControl() => typeof(AirCTOFilterControl);

		AirCTOImportModule testModule;
		protected override void SetUp()
		{
			base.SetUp();
			testModule = new AirCTOImportModule();
		}

		protected override void TearDown()
		{
			if (testModule != null)
			{
				testModule.Dispose();
			}

			base.TearDown();
		}
	}
}
