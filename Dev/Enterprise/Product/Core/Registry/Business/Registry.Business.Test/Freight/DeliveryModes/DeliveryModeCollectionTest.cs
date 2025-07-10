using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeliveryModeCollection))]
	sealed class DeliveryModeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DeliveryModeCollection>
	{
		void AssertHasDefaultItem(DeliveryModeCollection collection, string code, MultilingualString description)
		{
			var found = collection[code];

			AssertNotNull(string.Format("Default values must contain {0}", code), found);
			AssertEquals(string.Format("UserDefinedCode should equal {0}", code), code, found.UserDefinedCode);
			AssertEquals(string.Format("Description should equal {0}", description), description, found.Description);
			AssertEquals(string.Format("UserDefinedDescription should equal {0}", description, code), description, found.UserDefinedDescription);
		}

		public void TestGetDefault()
		{
			var defaultValue = DeliveryModeCollection.GetDefault();
			AssertEquals(4, defaultValue.Count);

			AssertHasDefaultItem(defaultValue, Constants.DeliveryModes.Codes.CFS_CFS, Constants.DeliveryModes.Descriptions.CFS_CFS);
			AssertHasDefaultItem(defaultValue, Constants.DeliveryModes.Codes.CFS_CFS, Constants.DeliveryModes.Descriptions.CFS_CFS);
			AssertHasDefaultItem(defaultValue, Constants.DeliveryModes.Codes.CFS_CFS, Constants.DeliveryModes.Descriptions.CFS_CFS);
			AssertHasDefaultItem(defaultValue, Constants.DeliveryModes.Codes.CFS_CFS, Constants.DeliveryModes.Descriptions.CFS_CFS);
		}

		public void TestAllowNew()
		{
			AssertEquals("Must allow new rows", true, this.Collection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DeliveryModeCollection GetCollectionToTest()
		{
			return new DeliveryModeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeliveryMode();
		}

		#endregion
	}
}
