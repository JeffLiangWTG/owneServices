using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Testing
{
	class UniquePropertiesValidationTest : TestCaseWithDummy
	{
		public void TestCheckPropertiesAreUniqueInCollection()
		{
			using (dummy1.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			{
				dummy1.Z0_Number = 0;
				dummy1.Z0_Decimal = 0m;
				dummy2.Z0_Number = 1;
				dummy2.Z0_Decimal = 1m;

				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(Dummy.Collection, dummy1.Z0_NumberInfo, dummy1.Z0_DecimalInfo);
				AssertNoErrors(dummy1.Z0_NumberInfo);
				AssertNoErrors(dummy1.Z0_DecimalInfo);

				dummy1.Z0_Number = 1;
				dummy1.Z0_Decimal = 0m;
				dummy2.Z0_Number = 1;
				dummy2.Z0_Decimal = 0m;
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(Dummy.Collection, "You can have your own message here.", dummy1.Z0_NumberInfo, dummy1.Z0_DecimalInfo);
				AssertHasError(dummy1.Z0_NumberInfo, "You can have your own message here.");
				AssertHasError(dummy1.Z0_DecimalInfo, "You can have your own message here.");

				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(Dummy.Collection, dummy1.Z0_NumberInfo, dummy1.Z0_DecimalInfo);
				AssertHasError(dummy1.Z0_NumberInfo, "The following properties (nUmBeR, dEcImAl) have been duplicated and must be unique.");
				AssertHasError(dummy1.Z0_DecimalInfo, "The following properties (nUmBeR, dEcImAl) have been duplicated and must be unique.");
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCheckSinglePropertyThrowsException()
		{
			using (dummy1.SuspendValidationTesting())
			{
				dummy1.Z0_Number = 0;
				dummy1.Z0_Decimal = 0m;
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(Dummy.Collection, dummy1.Z0_NumberInfo);
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

			public override ZPropertyInfo Z0_DecimalInfo
			{
				get
				{
					ZPropertyInfo result = base.Z0_DecimalInfo;
					result.HumanReadableName = "dEcImAl";
					return result;
				}
			}
		}
		#endregion
	}
}
