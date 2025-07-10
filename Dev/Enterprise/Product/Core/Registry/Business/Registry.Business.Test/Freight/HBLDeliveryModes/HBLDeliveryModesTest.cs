using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryModes))]
	sealed class HBLDeliveryModesTest : RegistryBusinessObjectTemplateTestCase<HBLDeliveryModes>
	{
		public void TestValidateHBLDeliveryMode()
		{
			var hblDeliveryModes = new HBLDeliveryModes("FCL");
			var mode1 = hblDeliveryModes.Modes.Add("DOOR/DOOR", (NoResString)"DOOR/DOOR", true);
			var mode2 = hblDeliveryModes.Modes.Add("CFS/CFS", (NoResString)"CFS/CFS", false);

			hblDeliveryModes.DefaultHBLDeliveryMode = mode1.Code;
			AssertNoErrors(hblDeliveryModes.DefaultHBLDeliveryModeInfo);

			hblDeliveryModes.DefaultHBLDeliveryMode = mode2.Code;
			AssertHasError(hblDeliveryModes.DefaultHBLDeliveryModeInfo, "Default HBL Delivery Mode must be 'Show In List'.");
		}

		protected override HBLDeliveryModes GetBusinessObjectToClone()
		{
			var result = new HBLDeliveryModes("FCL");
			HBLDeliveryMode mode = result.Modes.AddNew();
			mode.Code = "TEST";
			mode.Description = (NoResString)"Testing";
			mode.ShowInList = true;

			return result;
		}

		protected override HBLDeliveryModes GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
