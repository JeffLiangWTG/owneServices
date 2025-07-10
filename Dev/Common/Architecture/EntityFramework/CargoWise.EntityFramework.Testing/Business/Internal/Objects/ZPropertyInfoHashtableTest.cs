using System.Collections;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoHashtableTest : TestCaseWithDummy
	{
		public void TestGetEnumerator()
		{
			int count = Hash.Count;
			int outerLoopCount = 0;
			int innerLoopCount = 0;

			foreach (ZPropertyInfo info1 in Hash)
			{
				outerLoopCount++;
				ZPropertyInfo accessedInfo1 = info1; // to supress warning that Info1 variable isn't used ;P

				innerLoopCount = 0;
				foreach (ZPropertyInfo info2 in Hash)
				{
					innerLoopCount++;
					ZPropertyInfo accessedInfo2 = info2; // to supress warning that Info2 variable isn't used ;P
				}
			}

			AssertEquals("Number of values in ZPropertyInfoHash", count, outerLoopCount);
			AssertEquals("Number of values in ZPropertyInfoHash", count, innerLoopCount);
		}

		public void TestContainsKey()
		{
			AssertEquals("Non-existing property", false, Dummy.ZPropertyInfoHash.ContainsKey("splaty"));
			AssertEquals("Existing Non-wrapped property", true, Dummy.ZPropertyInfoHash.ContainsKey(DummyBizoSchema.Z0_Code.Name));
			AssertEquals("Existing Wrapped property", true, Dummy.ZPropertyInfoHash.ContainsKey("Self+" + DummyBizoSchema.Z0_Code.Name));
		}

		public void TestGetPropertyInfos_All()
		{
			string self_Z0_Description = "Self+Z0_Description";
			ZPropertyInfo wrappedProperty = Dummy.ZPropertyInfoHash[self_Z0_Description];
			Dummy.SelfAccessed = false;

			AssertEquals("All properties included in foreach of PropertyInfoTypes.All", true, ContainsProperty(Dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All), DummyBizoSchema.Z0_Description.Name));
			AssertEquals("All properties included in foreach of PropertyInfoTypes.All", true, ContainsProperty(Dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All), self_Z0_Description));
			AssertEquals("Self should not be accessed when enumerating", false, Dummy.SelfAccessed);
		}

		public void TestGetPropertyInfos_NonWrapped()
		{
			string self_Z0_Description = "Self+Z0_Description";
			ZPropertyInfo wrappedProperty = Dummy.ZPropertyInfoHash[self_Z0_Description];
			Dummy.SelfAccessed = false;

			AssertEquals("Only non-wrapped properties in foreach of PropertyInfoType.NonWrapped", true, ContainsProperty(Dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping), DummyBizoSchema.Z0_Description.Name));
			AssertEquals("No wrapped properties in foreach of PropertyInfoType.NonWrapped", false, ContainsProperty(Dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping), self_Z0_Description));
			AssertEquals("Self should not be accessed when enumerating", false, Dummy.SelfAccessed);
		}

		public void TestGetPropertyInfos_Wrapped()
		{
			string self_Z0_Description = "Self+Z0_Description";
			ZPropertyInfo wrappedProperty = Dummy.ZPropertyInfoHash[self_Z0_Description];
			Dummy.SelfAccessed = false;

			AssertEquals("Only wrapped properties in foreach of PropertyInfoType.Wrapped", true, ContainsProperty(Dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping), self_Z0_Description));
			AssertEquals("No non-wrapped properties in foreach of PropertyInfoType.Wrapped", false, ContainsProperty(Dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping), DummyBizoSchema.Z0_Description.Name));
			AssertEquals("Self should not be accessed when enumerating", false, Dummy.SelfAccessed);
		}

		bool ContainsProperty(IEnumerable properties, string propertyName)
		{
			foreach (ZPropertyInfo property in properties)
			{
				if (property.Name == propertyName)
				{
					return true;
				}
			}
			return false;
		}

		#region Implementation

		ZPropertyInfoHashtable Hash;

		protected override void SetUp()
		{
			base.SetUp();
			Hash = new ZPropertyInfoHashtable(Dummy);
		}

		#endregion
	}
}
