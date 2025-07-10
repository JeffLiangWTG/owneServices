using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GenericChargeModule))]
	public class GenericChargeModuleTest : ZModuleBasherTest
	{
		public void TestAllowExcelExport()
		{
			using (GenericChargeModuleForTest module = new GenericChargeModuleForTest())
			{
				AssertEquals("This is unsupported because there is no index on the PK which is required by " + nameof(FilteredBusinessObjectReader), false, module.ModuleDecisionProvider.AllowExcelExport);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var fieldInfo = typeof(GenericChargeFilterBusinessObject).GetField("elementType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				using (var module = new GenericChargeModuleForTest())
				{
					Env.Security.ChargeCodes.IsAllowed = true;
					Env.Security.GLAccounts.IsAllowed = false;

					var bizO = module.GetNewFilterBusinessObjectForTest();
					var result = fieldInfo.GetValue(bizO) as GenericChargeFilterBusinessObject.ElementType?;
					AssertEquals("Filter bizO for ChargeCodes only.", GenericChargeFilterBusinessObject.ElementType.ChargeCode, result);

					Env.Security.ChargeCodes.IsAllowed = false;
					Env.Security.GLAccounts.IsAllowed = true;

					bizO = module.GetNewFilterBusinessObjectForTest();
					result = fieldInfo.GetValue(bizO) as GenericChargeFilterBusinessObject.ElementType?;
					AssertEquals("Filter bizO for GL only.", GenericChargeFilterBusinessObject.ElementType.GeneralLedger, result);

					Env.Security.ChargeCodes.IsAllowed = true;
					Env.Security.GLAccounts.IsAllowed = true;

					bizO = module.GetNewFilterBusinessObjectForTest();
					result = fieldInfo.GetValue(bizO) as GenericChargeFilterBusinessObject.ElementType?;
					AssertEquals("Filter bizO with all rights on.", GenericChargeFilterBusinessObject.ElementType.All, result);

					Env.Security.ChargeCodes.IsAllowed = false;
					Env.Security.GLAccounts.IsAllowed = false;

					bizO = module.GetNewFilterBusinessObjectForTest();
					result = fieldInfo.GetValue(bizO) as GenericChargeFilterBusinessObject.ElementType?;
					AssertEquals("Filter bizO with no rights on.", GenericChargeFilterBusinessObject.ElementType.None, result);
				}
			}
		}

		public void TestGetSecurityCheckpointForPopups()
		{
			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				using (var module = new GenericChargeModuleForTest())
				{
					Env.Security.ChargeCodes.IsAllowed = true;
					Env.Security.GLAccounts.IsAllowed = false;

					var result = module.GetSecurityCheckpointForPopups();
					AssertEquals("Security Checkpoint for ChargeCodes only.", 1, result.Length);
					AssertEquals("Security Checkpoint for ChargeCodes only.", Env.Security.ChargeCodes, result[0]);

					Env.Security.ChargeCodes.IsAllowed = false;
					Env.Security.GLAccounts.IsAllowed = true;

					result = module.GetSecurityCheckpointForPopups();
					AssertEquals("Security Checkpoint for GL only.", 1, result.Length);
					AssertEquals("Security Checkpoint for GL only.", Env.Security.GLAccounts, result[0]);

					Env.Security.ChargeCodes.IsAllowed = true;
					Env.Security.GLAccounts.IsAllowed = true;

					result = module.GetSecurityCheckpointForPopups();
					AssertEquals("Security Checkpoints with all relative rights.", 2, result.Length);
					AssertContainsExactElementsInAnyOrder("Security Checkpoints with all relative rights.", new SecurityCheckpoint[] { Env.Security.ChargeCodes, Env.Security.GLAccounts }, result);

					Env.Security.ChargeCodes.IsAllowed = false;
					Env.Security.GLAccounts.IsAllowed = false;

					result = module.GetSecurityCheckpointForPopups();
					AssertEquals("Security Checkpoints with all relative rights.", 2, result.Length);
					AssertContainsExactElementsInAnyOrder("Security Checkpoints with all relative rights.", new SecurityCheckpoint[] { Env.Security.ChargeCodes, Env.Security.GLAccounts }, result);
				}
			}
		}

		public void TestFilterControlPopupMessage()
		{
			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				Env.Security.ChargeCodes.IsAllowed = true;
				Env.Security.GLAccounts.IsAllowed = false;

				using (var module = new GenericChargeModuleForTest())
				{
					module.ResultCountMessageForTest.UpdateResultCountMessage(0);
					AssertEquals("No results found and no GL right.", @"There are no records that match your search.
Note: GL Accounts are not listed as you do not have security right to Maintain -> Account -> GL Accounts", module.GetWarningMessageForTest());
					module.ResultCountMessageForTest.UpdateResultCountMessage(3);
					AssertEquals("3 results found and no GL right.", "Found 3 records that match your search criteria.", module.GetWarningMessageForTest());
				}

				Env.Security.ChargeCodes.IsAllowed = false;
				Env.Security.GLAccounts.IsAllowed = true;

				using (var module = new GenericChargeModuleForTest())
				{
					module.ResultCountMessageForTest.UpdateResultCountMessage(0);
					AssertEquals("No results found and no ChargeCode right.", @"There are no records that match your search.
Note: Charge Codes are not listed as you do not have security right to Maintain -> Account -> Charge Codes", module.GetWarningMessageForTest());
					module.ResultCountMessageForTest.UpdateResultCountMessage(3);
					AssertEquals("3 results found and no ChargeCode right.", "Found 3 records that match your search criteria.", module.GetWarningMessageForTest());
				}

				Env.Security.ChargeCodes.IsAllowed = true;
				Env.Security.GLAccounts.IsAllowed = true;

				using (var module = new GenericChargeModuleForTest())
				{
					module.ResultCountMessageForTest.UpdateResultCountMessage(0);
					AssertEquals("No results found and with both rights.", "There are no records that match your search.", module.GetWarningMessageForTest());
					module.ResultCountMessageForTest.UpdateResultCountMessage(3);
					AssertEquals("3 results found and with both rights.", "Found 3 records that match your search criteria.", module.GetWarningMessageForTest());
				}

				Env.Security.ChargeCodes.IsAllowed = false;
				Env.Security.GLAccounts.IsAllowed = false;

				using (var module = new GenericChargeModuleForTest())
				{
					module.ResultCountMessageForTest.UpdateResultCountMessage(0);
					AssertEquals("No results found and no ChargeCode right.", @"There are no records that match your search.
No Charge Codes nor GL Accounts are listed as you do not have security right to Maintain -> Account -> Charge Codes or Maintain -> Account -> GL Accounts", module.GetWarningMessageForTest());
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GenericCharge;
		}

		// This module does its own thing for showing Delete forms, bypassing the thread-safety measures of the base class. This could result in defects if the module is used in a background thread.
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		class GenericChargeModuleForTest : GenericChargeModule
		{
			public new IModuleDecisionProvider ModuleDecisionProvider
			{
				get { return base.ModuleDecisionProvider; }
			}

			public FilterBusinessObject GetNewFilterBusinessObjectForTest()
			{
				return GetNewFilterBusinessObject();
			}

			public ZString GetWarningMessageForTest()
			{
				return (EmbeddedControl as GenericChargeFilterControl).PopupMessageCacheForTest;
			}

			public ResultCountMessage ResultCountMessageForTest
			{
				get { return ResultCountMessage; }
			}
		}
	}
}
