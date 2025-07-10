using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CalculateDeliveryDueDateTransportMode))]
	sealed class CalculateDeliveryDueDateTransportModeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeAndDescriptionReadOnly()
		{
			var calculateDeliveryDueDateTransportMode = new CalculateDeliveryDueDateTransportMode();
			AssertEquals(true, calculateDeliveryDueDateTransportMode.CodeInfo.ReadOnly);
			AssertEquals(true, calculateDeliveryDueDateTransportMode.DescriptionInfo.ReadOnly);
			AssertEquals(true, calculateDeliveryDueDateTransportMode.EnglishDescriptionInfo.ReadOnly);
			AssertEquals(false, calculateDeliveryDueDateTransportMode.EnabledInfo.ReadOnly);
		}

		#region Implementation

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CalculateDeliveryDueDateTransportMode();
			result.Code = "AIR";
			result.Description = (NoResString)"AIR";
			result.Enabled = true;
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}


