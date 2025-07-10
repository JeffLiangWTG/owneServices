using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityServiceEhubID))]
	sealed class GlobalTrackingShipmentVisibilityServiceEhubIDTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeAndDescriptionReadOnly()
		{
			var serviceEhubID = new GlobalTrackingShipmentVisibilityServiceEhubID();
			AssertEquals(true, serviceEhubID.CodeInfo.ReadOnly);
			AssertEquals(true, serviceEhubID.ServiceInfo.ReadOnly);
			AssertEquals(false, serviceEhubID.EhubIDInfo.ReadOnly);
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
			var result = new GlobalTrackingShipmentVisibilityServiceEhubID();
			result.Code = "CA";
			result.Service = (NoResString)"CA";
			result.EhubID = "CONTAINER_TRACKING";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}