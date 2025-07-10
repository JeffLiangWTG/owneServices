using System.Reflection;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZReflectionTest : TestCase
	{
		public void TestGetPropertyThrowsAmbiguousMatchExceptionOnNewedPropertyWithDifferentReturnType()
		{
			bool ambiguousMatchExceptionThrown = false;

			try
			{
				PropertyInfo info = Nikon.GetType().GetProperty("Lens");
			}
			catch (AmbiguousMatchException)
			{
				ambiguousMatchExceptionThrown = true;
			}

			Assert(ambiguousMatchExceptionThrown);
		}

		public void TestGetPropertyIncludingNewFindsNewedPropertyWithDifferentReturnType()
		{
			PropertyInfo info1 = ZReflection.GetPropertyIncludingNew(Nikon, "Lens");
			AssertNotNull(info1);
			AssertEquals("Nikkor70_200VR", info1.GetValue(Nikon, null).GetType().Name);

			PropertyInfo info2 = ZReflection.GetPropertyIncludingNew(typeof(NikonCamera), "Lens");
			AssertNotNull(info2);
			AssertEquals("Nikkor70_200VR", info2.GetValue(Nikon, null).GetType().Name);
		}

		public void TestGetPropertyIncludingNewWithCustomBindingFlags()
		{
			PropertyInfo info1 = ZReflection.GetPropertyIncludingNew(Nikon, "PrivateBool", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(info1);
			AssertEquals(true, info1.GetValue(Nikon, null));

			PropertyInfo info2 = ZReflection.GetPropertyIncludingNew(typeof(NikonCamera), "PrivateBool", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(info2);
			AssertEquals(true, info2.GetValue(Nikon, null));
		}

		public void TestGetPropertyIncludingNewWithBadProperty()
		{
			PropertyInfo info1 = ZReflection.GetPropertyIncludingNew(Nikon, "ThisPropertyDoesNotExist");
			AssertNull(info1);

			PropertyInfo info2 = ZReflection.GetPropertyIncludingNew(typeof(NikonCamera), "ThisPropertyDoesNotExist");
			AssertNull(info2);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Nikon = new NikonCamera();
		}

		Camera Nikon;

		#endregion
	}
}
