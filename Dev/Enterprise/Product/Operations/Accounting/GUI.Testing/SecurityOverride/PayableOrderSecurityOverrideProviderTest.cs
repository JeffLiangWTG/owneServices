using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI.Testing
{
	public class PayableOrderSecurityOverrideProviderTest : TestCaseWithFactory
	{
		ISecurityOverrideProvider Provider;

		public void TestIsUserInitiatorAndNotAllowedToApprove()
		{
			var initiator = Factory.NewWithValidTestData<GlbStaff>();
			initiator.GS_LoginName = "Uday Jadhav";
			initiator.GS_Code = "UJ";

			var order = Factory.New<AccPayableOrderHeader>();
			Provider = new PayableOrderSecurityOverrideProvider(order);
			Factory.Save();

			SetRegistryAndAssert(true);
			SetRegistryAndAssert(false);

			using (Env.SetTemporaryUserContext(initiator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetRegistryAndAssert(false);
				SetRegistryAndAssert(false);
			}
		}

		void SetRegistryAndAssert(bool value)
		{
			SetOrderApprovalRegistryRestrictions(value);
			AssertEquals(value, Provider.IsUserInitiatorAndNotAllowedToApprove);
		}

		void SetOrderApprovalRegistryRestrictions(bool value)
		{
			var codeDescriptionList = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			codeDescriptionList
				.Cast<CodeDescriptionBool>()
				.FirstOrDefault(x => x.Code == PayableOrderRestrictionList.Codes.CreatorApproval).Bool = value;
			AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionList);
		}
	}
}
