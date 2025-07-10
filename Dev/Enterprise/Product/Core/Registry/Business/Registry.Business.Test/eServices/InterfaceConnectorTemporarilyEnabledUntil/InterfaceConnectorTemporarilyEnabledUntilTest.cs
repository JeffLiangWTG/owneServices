using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterfaceConnectorTemporarilyEnabledUntil))]
	sealed class InterfaceConnectorTemporarilyEnabledUntilTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("EnabledUntil", ZDateTime.Empty, BizObj.EnabledUntil);
		}

		[TestDate(2016, 2, 8, 13, 14, 15)]
		public void TestValidation()
		{
			BizObj.EnabledUntil = ZDateTime.Now.AddMonths(6);
			AssertNoErrors(BizObj.EnabledUntilInfo);

			BizObj.EnabledUntil = ZDateTime.Now.AddDays(-1);
			AssertHasError(BizObj.EnabledUntilInfo, "The value must be between 08-Feb-16 and 08-Feb-17.");

			BizObj.EnabledUntil = ZDateTime.Now.AddMonths(12).AddDays(1);
			AssertHasError(BizObj.EnabledUntilInfo, "The value must be between 08-Feb-16 and 08-Feb-17.");
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			InterfaceConnectorTemporarilyEnabledUntil result = new InterfaceConnectorTemporarilyEnabledUntil();

			result.EnabledUntil = new ZDateTime(2016, 2, 8);

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			InterfaceConnectorTemporarilyEnabledUntil result = new InterfaceConnectorTemporarilyEnabledUntil();

			result.EnabledUntil = new ZDateTime(2016, 2, 8);

			return result;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new InterfaceConnectorTemporarilyEnabledUntil BizObj
		{
			get { return (InterfaceConnectorTemporarilyEnabledUntil)base.BizObj; }
		}

		#endregion
	}
}
