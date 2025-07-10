using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVEnablePartyScreening))]
	sealed class HVLVEnablePartyScreeningTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetDefault()
		{
			var hvlvEnablePartyScreening = new HVLVEnablePartyScreening();
			AssertEquals("EnableHVLVPartyScreening default false", false, hvlvEnablePartyScreening.EnableHVLVPartyScreening);
			AssertEquals("EnableNewDPSResultForm default true", true, hvlvEnablePartyScreening.EnableNewDPSResultForm);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new HVLVEnablePartyScreening();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (HVLVEnablePartyScreening)BizObj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (HVLVEnablePartyScreening)BizObj;
		}

		#endregion
	}
}
