using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	[TestedType(typeof(LicenceUsageLog))]
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest("AU")]
	sealed class LicenceUsageLogTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLicenceModuleDescription()
		{
			AssertEquals("", UsageLog.LicenceModuleDescription);

			UsageLog.S7_FormCaption = Env.Licence.ShippingManager.Name;
			AssertEquals(Env.Licence.ShippingManager.DisplayName, UsageLog.LicenceModuleDescription);

			UsageLog.S7_FormCaption = "MEH MEH";
			AssertEquals("", UsageLog.LicenceModuleDescription);
		}

		public void TestLicenceTypeAndDescription()
		{
			AssertEquals(LicenceTypes.Codes.NON, UsageLog.LicenceType);
			AssertEquals(LicenceTypes.Descriptions.NON, UsageLog.LicenceTypeDescription);

			UsageLog.S7_MouseClicks = (int)ModuleLicenceType.OPN;
			AssertEquals(LicenceTypes.Codes.OPN, UsageLog.LicenceType);
			AssertEquals(LicenceTypes.Descriptions.OPN, UsageLog.LicenceTypeDescription);

			UsageLog.S7_MouseClicks = (int)ModuleLicenceType.REN;
			AssertEquals(LicenceTypes.Codes.REN, UsageLog.LicenceType);
			AssertEquals(LicenceTypes.Descriptions.REN, UsageLog.LicenceTypeDescription);
		}

		public void TestLocalUsageTime()
		{
			AssertEquals(ZDateTime.Now.Date, UsageLog.LocalUsageTime.Date);

			UsageLog.S7_OpenDateTimeUtc = new ZDateTime(2009, 1, 1, 10, 0, 0);
			using (DisposableEnvironment.ForBranchCodeSlowerThanPK("BNE"))
			{
				AssertEquals(new ZDateTime(2009, 1, 1, 20, 0, 0), UsageLog.LocalUsageTime);
			}
		}

		public void TestParentBranch()
		{
			AssertNull(UsageLog.ParentBranch);

			UsageLog.S7_ParentID = Env.CurrentBranch.PK;
			AssertNull("S7_ParentTableCode not set", UsageLog.ParentBranch);

			UsageLog.S7_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			AssertEquals(Env.CurrentBranch.PK, UsageLog.ParentBranch.PK);

			UsageLog.S7_ParentID = ZGuid.NewZGuid();
			AssertNull("no branch", UsageLog.ParentBranch);
		}

		LicenceUsageLog UsageLog
		{
			get { return usageLog ?? (usageLog = Factory.New<LicenceUsageLog>()); }
		}
		LicenceUsageLog usageLog;
	}
}
