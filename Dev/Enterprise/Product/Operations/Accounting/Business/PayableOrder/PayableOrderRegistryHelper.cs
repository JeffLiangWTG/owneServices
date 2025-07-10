using System.Linq;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public static class PayableOrderRegistryHelper
	{
		public static string BlankSupplierRestrictionMessage
		{
			get
			{ return Res.GetString("dd67feb1-03b0-42df-9b3b-7b15acb14b17", "Approval of orders with 'blank' Supplier has been restricted in the Registry. Please specify Supplier for this Order."); }
		}

		public static string OverriddenSupplierRestrictionMessage
		{
			get
			{ return Res.GetString("2edcc58e-3bf1-46e7-aa22-ad8b761c9a54", "Approval of orders with overridden Supplier details has been restricted in the Registry. Please select a valid Supplier Organization for this Order."); }
		}

		public static string TemporaryOrganizationRestrictionMessage
		{
			get
			{ return Res.GetString("d5707cf4-63cb-4eb0-920b-2dacf3547d12", "Approval of orders with 'Temporary' Supplier details has been restricted in the Registry. Please modify the Supplier for this Order."); }
		}

		public static bool GetRegistryValue(string code)
		{
			var registryItem = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value.Cast<CodeDescriptionBool>().FirstOrDefault(x => x.Code.Equals(code));
			return (bool)(registryItem?.Bool ?? false);
		}
	}
}
