using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeliveryModeCollection))]
	sealed class DeliveryModeCollectionTestCase : RegistryBusinessObjectCollectionTestCase<DeliveryModeCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override DeliveryModeCollection GetCollectionToTest()
		{
			return new DeliveryModeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeliveryMode();
		}

		public void TestUserDefinedCodeDescription()
		{
			var collection = new DeliveryModeCollection();

			var mode = collection.AddNew();
			mode.Code = Constants.DeliveryModes.Codes.CFS_CFS;
			mode.Description = Constants.DeliveryModes.Descriptions.CFS_CFS;
			mode.UserDefinedCode = "DR/DR";
			mode.UserDefinedDescription = (NoResString)"DOOR/DOOR";
			mode.IsSystemDefined = true;

			using (FreightDataRegistry.Instance.ContainerDeliveryModeOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pairList = collection.ToCodeDescription();
				AssertEquals(1, pairList.Count);
				AssertEquals(true, pairList.ContainsCode(Constants.DeliveryModes.Codes.CFS_CFS));

				var userDefinedPairList = collection.ToUserDefinedCodeDescription();
				AssertEquals(1, userDefinedPairList.Count);
				AssertEquals(true, userDefinedPairList.ContainsCode("DR/DR"));
			}

			using (FreightDataRegistry.Instance.ContainerDeliveryModeOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var pairList = collection.ToCodeDescription();
				AssertEquals(1, pairList.Count);
				AssertEquals(true, pairList.ContainsCode(Constants.DeliveryModes.Codes.CFS_CFS));

				var userDefinedPairList = collection.ToUserDefinedCodeDescription();
				AssertEquals(1, userDefinedPairList.Count);
				AssertEquals(true, userDefinedPairList.ContainsCode(Constants.DeliveryModes.Codes.CFS_CFS));
			}
		}
	}
}
