using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(MockRegistryItemDependentNumericRegistryDataType))]
	sealed class RegistryItemDependentNumericRegistryDataTypeTest : RegistryDataTypeTestCase<MockRegistryItemDependentNumericRegistryDataType>
	{
		public void TestValidationWithInclusiveBounds()
		{
			IntRegistryItem item1 = new IntRegistryItem("A", null, null, null, RegistryStorageFlags.All);
			IntRegistryItem item2 = new IntRegistryItem("B", null, null, null, RegistryStorageFlags.All);

			item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			MockRegistryItemDependentNumericRegistryDataType dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, item2);
			try
			{
				dataType.Validate(null, 4, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expecting exception due to validation.");
			}
			catch (RegistryValidationException)
			{
			}

			dataType.Validate(null, 5, Guid.Empty, Guid.Empty, Guid.Empty);
			dataType.Validate(null, 6, Guid.Empty, Guid.Empty, Guid.Empty);
			dataType.Validate(null, 9, Guid.Empty, Guid.Empty, Guid.Empty);
			dataType.Validate(null, 10, Guid.Empty, Guid.Empty, Guid.Empty);

			try
			{
				dataType.Validate(null, 11, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expecting exception due to validation.");
			}
			catch (RegistryValidationException)
			{
			}

			Assert(true);
		}

		public void TestValidationWithNonInclusiveBounds()
		{
			IRegistryItem item1 = new IntRegistryItem("A", null, null, null, RegistryStorageFlags.All);
			IRegistryItem item2 = new IntRegistryItem("B", null, null, null, RegistryStorageFlags.All);

			item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			AssertEquals(10, item2.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(5, item1.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			MockRegistryItemDependentNumericRegistryDataType dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, item2, false, false);
			try
			{
				dataType.Validate(null, 4, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expecting exception due to validation.");
			}
			catch (RegistryValidationException)
			{
			}

			try
			{
				dataType.Validate(null, 5, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expecting exception due to validation.");
			}
			catch (RegistryValidationException)
			{
			}

			dataType.Validate(null, 6, Guid.Empty, Guid.Empty, Guid.Empty);
			dataType.Validate(null, 9, Guid.Empty, Guid.Empty, Guid.Empty);

			try
			{
				dataType.Validate(null, 10, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expecting exception due to validation..");
			}
			catch (RegistryValidationException)
			{
			}

			try
			{
				dataType.Validate(null, 11, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expecting exception due to validation");
			}
			catch (RegistryValidationException)
			{
			}

			Assert(true);
		}

		public void TestLowerAndUpperBoundRegistryItemValue()
		{
			IntRegistryItem item1 = new IntRegistryItem("A", null, null, null, RegistryStorageFlags.All);
			IntRegistryItem item2 = new IntRegistryItem("B", null, null, null, RegistryStorageFlags.All);

			MockRegistryItemDependentNumericRegistryDataType dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, item2);
			AssertEquals("GetLowerBoundRegistryItemValue()", 0, dataType.GetLowerBoundRegistryItemValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetUpperBoundRegistryItemValue()", 0, dataType.GetUpperBoundRegistryItemValue(Guid.Empty, Guid.Empty, Guid.Empty));

			item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals("GetLowerBoundRegistryItemValue()", 5, dataType.GetLowerBoundRegistryItemValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetUpperBoundRegistryItemValue()", 10, dataType.GetUpperBoundRegistryItemValue(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGetErrorMessage()
		{
			IntRegistryItem item1 = new IntRegistryItem("A", (NoResString)"", (NoResString)"Item1", (NoResString)"", RegistryStorageFlags.All);
			IntRegistryItem item2 = new IntRegistryItem("B", (NoResString)"", (NoResString)"Item2", (NoResString)"", RegistryStorageFlags.All);

			item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			MockRegistryItemDependentNumericRegistryDataType dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, null, true, true);
			AssertEquals("GetErrorMessage()", "Number must be greater than or equal to Item1 (5).", dataType.GetErrorMessage(Guid.Empty, Guid.Empty, Guid.Empty));

			dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, null, false, true);
			AssertEquals("GetErrorMessage()", "Number must be greater than Item1 (5).", dataType.GetErrorMessage(Guid.Empty, Guid.Empty, Guid.Empty));

			dataType = new MockRegistryItemDependentNumericRegistryDataType(null, item2, true, true);
			AssertEquals("GetErrorMessage()", "Number must be less than or equal to Item2 (10).", dataType.GetErrorMessage(Guid.Empty, Guid.Empty, Guid.Empty));

			dataType = new MockRegistryItemDependentNumericRegistryDataType(null, item2, true, false);
			AssertEquals("GetErrorMessage()", "Number must be less than Item2 (10).", dataType.GetErrorMessage(Guid.Empty, Guid.Empty, Guid.Empty));

			dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, item2, true, true);
			AssertEquals("GetErrorMessage()", "Number must be greater than or equal to Item1 (5) and less than or equal to Item2 (10).",
				dataType.GetErrorMessage(Guid.Empty, Guid.Empty, Guid.Empty));

			dataType = new MockRegistryItemDependentNumericRegistryDataType(item1, item2, false, false);
			AssertEquals("GetErrorMessage()", "Number must be greater than Item1 (5) and less than Item2 (10).",
				dataType.GetErrorMessage(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override MockRegistryItemDependentNumericRegistryDataType GetNewDataType()
		{
			IntRegistryItem item1 = new IntRegistryItem("A", null, null, null, RegistryStorageFlags.System);
			IntRegistryItem item2 = new IntRegistryItem("B", null, null, null, RegistryStorageFlags.System);

			item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1001);
			item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2001);

			return new MockRegistryItemDependentNumericRegistryDataType(item1, item2);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(-1000, Encoding.Unicode.GetBytes("-1000")),
				new ValidSampleAndBinaryValueInDB(0, Encoding.Unicode.GetBytes("0")),
				new ValidSampleAndBinaryValueInDB(2000, Encoding.Unicode.GetBytes("2000"))
			};
		}
	}
}
