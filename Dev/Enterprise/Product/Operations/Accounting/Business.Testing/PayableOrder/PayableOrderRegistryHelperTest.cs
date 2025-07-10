using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	public class PayableOrderRegistryHelperTest : TestCaseWithFactory
	{
		public void TestBlankSupplierRestrictionMessage()
		{
			var expectedString = "Approval of orders with 'blank' Supplier has been restricted in the Registry. Please specify Supplier for this Order.";
			AssertEquals(expectedString, PayableOrderRegistryHelper.BlankSupplierRestrictionMessage);
		}

		public void TestOverriddenSupplierRestrictionMessage()
		{
			var expectedString = "Approval of orders with overridden Supplier details has been restricted in the Registry. Please select a valid Supplier Organization for this Order.";
			AssertEquals(expectedString, PayableOrderRegistryHelper.OverriddenSupplierRestrictionMessage);
		}

		public void TestTemporaryOrganizationRestrictionMessage()
		{
			var expectedString = "Approval of orders with 'Temporary' Supplier details has been restricted in the Registry. Please modify the Supplier for this Order.";
			AssertEquals(expectedString, PayableOrderRegistryHelper.TemporaryOrganizationRestrictionMessage);
		}

		public void TestGetRegistryValue()
		{
			SetOrderApprovalRegistryRestrictions(true);
			AssertRegistryValues(true);
			SetOrderApprovalRegistryRestrictions(false);
			AssertRegistryValues(false);
		}

		void AssertRegistryValues(bool value)
		{
			AssertEquals(value, PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.CreatorApproval));
			AssertEquals(value, PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.BlankSuppliers));
			AssertEquals(value, PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.OverriddenSupplier));
			AssertEquals(value, PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.SupplierIsTempOrg));
		}

		void SetOrderApprovalRegistryRestrictions(bool value)
		{
			var codeDescriptionList = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			codeDescriptionList.Cast<CodeDescriptionBool>().ToList().ForEach(x => x.Bool = value);
			AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionList);
		}
	}
}
