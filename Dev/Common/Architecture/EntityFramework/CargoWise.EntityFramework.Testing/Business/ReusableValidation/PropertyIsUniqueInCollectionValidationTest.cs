using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PropertyIsUniqueInCollectionValidationTest : TestCaseWithDummy
	{
		public void TestCheckPropertyIsUniqueInCollection()
		{
			using (dummy1.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			{
				dummy1.Z0_Number = 0;
				dummy2.Z0_Number = 1;
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, Dummy.Collection);
				AssertNoErrors(dummy1.Z0_NumberInfo);

				dummy1.Z0_Number = 1;
				dummy2.Z0_Number = 1;
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, Dummy.Collection, "You can have your own message here.");
				AssertHasError(dummy1.Z0_NumberInfo, "You can have your own message here.");

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, Dummy.Collection);
				AssertHasError(dummy1.Z0_NumberInfo, "The nUmBeR has been duplicated and must be unique.");

				dummy1.Z0_NumberInfo.ClearAllNotifications();
				dummy2.Delete();
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, Dummy.Collection);
				AssertNoErrors(dummy1.Z0_NumberInfo);
			}
		}

		public void TestCheckPropertyIsUniqueInCollection_WhenValuesAreEmpty()
		{
			using (dummy1.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			{
				dummy1.Z0_Number = 0;
				dummy2.Z0_Number = 0;

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, Dummy.Collection);
				AssertNoErrors(dummy1.Z0_NumberInfo);

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, Dummy.Collection, true);
				AssertHasError(dummy1.Z0_NumberInfo, "The nUmBeR has been duplicated and must be unique.");
			}
		}

		public void TestCheckPropertyIsUniqueInCollection_WithEqualBusinessObjectButDifferentReference_ShouldNotCauseError()
		{
			using (dummy1.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			{
				dummy1.Z0_Number = 1;
				dummy2.Z0_Number = 2;

				var loadedDummy1 = Factory.Load<DummyBusinessObject>(dummy1.PK);
				var collection = new[] { loadedDummy1, dummy2 };

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NumberInfo, collection);
				AssertNoErrors("There should be no validation error because the object in the collection with the matching value is a copy of the same business object, and yet...", dummy1.Z0_NumberInfo);
			}
		}

		public void TestCheckPropertyIsUniqueInCollection_WithDifferentCaseString_ShouldCauseError()
		{
			using (dummy1.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			{
				dummy1.Z0_NVarChar = "STRing";
				dummy2.Z0_NVarChar = "string";

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NVarCharInfo, Dummy.Collection, ignoreStringCase: false);
				AssertNoErrors(dummy1.Z0_NVarCharInfo);

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(dummy1.Z0_NVarCharInfo, Dummy.Collection);
				AssertHasError(dummy1.Z0_NVarCharInfo, "The N Var Char has been duplicated and must be unique.");
			}
		}

		#region Implementation

		DummyBaseBusinessObject dummy1;
		DummyBaseBusinessObject dummy2;

		protected override void SetUp()
		{
			base.SetUp();
			dummy1 = Factory.New<DummyBaseBusinessObjectWithCustomHumanReadableName>();
			dummy2 = Factory.New<DummyBaseBusinessObjectWithCustomHumanReadableName>();
			Dummy.Collection.Add(dummy1);
			Dummy.Collection.Add(dummy2);
		}

		#endregion

		#region class DummyBaseBusinessObjectWithCustomHumanReadableName

		class DummyBaseBusinessObjectWithCustomHumanReadableName : DummyBaseBusinessObject
		{
			public DummyBaseBusinessObjectWithCustomHumanReadableName(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZPropertyInfo Z0_NumberInfo
			{
				get
				{
					ZPropertyInfo result = base.Z0_NumberInfo;
					result.HumanReadableName = "nUmBeR";
					return result;
				}
			}
		}

		#endregion
	}
}
