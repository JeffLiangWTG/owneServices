using System;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(StampDutyRecharge))]
	class StampDutyRechargeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("StampDutyRechargeOrganizationType", Constants.StampDutyRechargeOrganizationType.All, BizObj.StampDutyRechargeOrganizationType);
			AssertEquals("StampDutyRechargeTransactionType", Constants.StampDutyRechargeTransactionType.All, BizObj.StampDutyRechargeTransactionType);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.StampDutyRechargeOrganizationType = BizObj.StampDutyRechargeTransactionType = String.Empty;
			AssertHasErrors("StampDutyRechargeOrganizationType", BizObj.StampDutyRechargeOrganizationTypeInfo);
			AssertHasErrors("StampDutyRechargeTransactionType", BizObj.StampDutyRechargeTransactionTypeInfo);

			BizObj.StampDutyRechargeOrganizationType = BizObj.StampDutyRechargeTransactionType = "ABC";
			AssertHasErrors("StampDutyRechargeOrganizationType", BizObj.StampDutyRechargeOrganizationTypeInfo);
			AssertHasErrors("StampDutyRechargeTransactionType", BizObj.StampDutyRechargeTransactionTypeInfo);

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();
			AssertHasErrors("StampDutyRechargeOrganizationType", BizObj.StampDutyRechargeOrganizationTypeInfo);
			AssertHasErrors("StampDutyRechargeTransactionType", BizObj.StampDutyRechargeTransactionTypeInfo);

			BizObj.StampDutyRechargeOrganizationType = Constants.StampDutyRechargeOrganizationType.LocalOrganizations;
			BizObj.StampDutyRechargeTransactionType = Constants.StampDutyRechargeTransactionType.ARInvoice;
			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();
			AssertNoErrors("StampDutyRechargeOrganizationType", BizObj.StampDutyRechargeOrganizationTypeInfo);
			AssertNoErrors("StampDutyRechargeTransactionType", BizObj.StampDutyRechargeTransactionTypeInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new StampDutyRecharge();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new StampDutyRecharge BizObj
		{
			get { return (StampDutyRecharge)base.BizObj; }
		}

		#endregion
	}
}
