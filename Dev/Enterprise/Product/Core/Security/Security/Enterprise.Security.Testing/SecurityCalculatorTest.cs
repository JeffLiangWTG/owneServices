using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Security
{
	sealed class SecurityCalculatorTest : TestCaseWithFactory
	{
		public void TestGetSecurityStateForOneLevel()
		{
			ZGuid staffPk = ZGuid.NewZGuid();

			AssertEquals("GetSecurityStateForOneLevel()", SecurityState.Implicit, Calculator.GetSecurityStateForOneLevel(Securities, CheckPoint1, false, staffPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));

			GlbSecurity security1 = Securities.AddNew();
			security1.GU_GS = staffPk;
			security1.GU_SecurityRight = CheckPoint1.Code;
			security1.GU_SecurityItemIsAllowed = true;
			AssertEquals("GetSecurityStateForOneLevel()", SecurityState.Granted, Calculator.GetSecurityStateForOneLevel(Securities, CheckPoint1, false, staffPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));

			security1.GU_SecurityItemIsAllowed = false;
			AssertEquals("GetSecurityStateForOneLevel()", SecurityState.Denied, Calculator.GetSecurityStateForOneLevel(Securities, CheckPoint1, false, staffPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));

			GlbSecurity security2 = Securities.AddNew();
			security2.GU_GS = staffPk;
			security2.GU_SecurityRight = CheckPoint1.Code;
			security2.GU_SecurityItemIsAllowed = true;
			AssertEquals("GetSecurityStateForOneLevel()", SecurityState.Granted, Calculator.GetSecurityStateForOneLevel(Securities, CheckPoint1, false, staffPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));
		}

		#region Test Objects

		ZSecurity Security
		{
			get
			{
				if (security == null)
				{
					security = (ZSecurity)new ZSecurityFactory().NewSecurityInstance(Securities, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				}
				return security;
			}
		}

		SecurityCheckpoint CheckPoint1
		{
			get
			{
				if (checkPoint1 == null)
				{
					checkPoint1 = new SecurityCheckpoint("DUMMY_CHECK_POINT_1", (NoResString)"", null, Security);
				}
				return checkPoint1;
			}
		}

		GlbSecurityCollection Securities
		{
			get
			{
				if (securities == null)
				{
					securities = new GlbSecurityCollection(Factory);
				}
				return securities;
			}
		}

		DummySecurityCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					calculator = new DummySecurityCalculator();
				}
				return calculator;
			}
		}

		ZSecurity security;
		SecurityCheckpoint checkPoint1;
		GlbSecurityCollection securities;
		DummySecurityCalculator calculator;

		#region class DummySecurityCalculator

		class DummySecurityCalculator : SecurityCalculator
		{
		}

		#endregion

		#endregion
	}
}
