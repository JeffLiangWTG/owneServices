using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPurgePeriod))]
	sealed class HVLVPurgePeriodTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetDefault()
		{
			var hvlvPurgePeriod = new HVLVPurgePeriod();
			AssertEquals("IsEnable default false", false, hvlvPurgePeriod.IsEnabled);
			AssertEquals("PurgePeriod default 6", 6, hvlvPurgePeriod.PurgePeriod);
		}

		public void TestValidation()
		{
			var bizObj = BizObj as HVLVPurgePeriod;

			AssertEquals("preCondition", false, bizObj.IsEnabled);
			AssertEquals("preCondition", 6, bizObj.PurgePeriod);

			bizObj.PurgePeriod = 5;
			bizObj.RunPreSaveValidation();
			AssertEquals("No errs", false, bizObj.PurgePeriodInfo.HasNotifications());

			bizObj.PurgePeriod = 2;
			bizObj.RunPreSaveValidation();
			AssertEquals("Less than minimum", true, bizObj.PurgePeriodInfo.HasNotifications());

			bizObj.PurgePeriod = 37;
			bizObj.RunPreSaveValidation();
			AssertEquals("Bigger than maximum", true, bizObj.PurgePeriodInfo.HasNotifications());
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new HVLVPurgePeriod();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (HVLVPurgePeriod)BizObj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (HVLVPurgePeriod)BizObj;
		}

		#endregion
	}
}
