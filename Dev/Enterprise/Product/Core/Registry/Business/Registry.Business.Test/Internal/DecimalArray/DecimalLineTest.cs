using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Internal.Testing
{
	[TestedType(typeof(DecimalLine))]
	sealed class DecimalLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDecimalPlaces()
		{
			AssertEquals("DecimalPlaces", 2, Line.DecimalPlaces);
			Collection.DataType.DecimalPlaces = 3;
			AssertEquals("DecimalPlaces", 3, Line.DecimalPlaces);
		}

		public void TestValidateNumber()
		{
			Line.Number = 0m;
			AssertNoErrors(Line.NumberInfo);

			Collection.DataType.LowerBound = 1m;
			Line.ValidateNumber();
			AssertHasError(Line.NumberInfo, "Please enter a '" + Line.NumberInfo.HumanReadableName + "' greater than or equal to 1.");

			Line.Number = 4m;
			AssertNoErrors(Line.NumberInfo);

			Collection.DataType.UpperBound = 3m;
			Line.ValidateNumber();
			AssertHasError(Line.NumberInfo, "Please enter a '" + Line.NumberInfo.HumanReadableName + "' less than or equal to 3.");

			Line.Number = 2m;
			AssertNoErrors(Line.NumberInfo);
		}

		public void TestParentCollection()
		{
			AssertEquals("ParentCollection", Collection, Line.ParentCollection);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Collection.AddNew();
		}

		DecimalLine Line
		{
			get
			{
				if (line == null)
				{
					line = (DecimalLine)GetNewBusinessObject();
				}
				return line;
			}
		}

		DecimalLineCollection Collection
		{
			get
			{
				if (collection == null)
				{
					DecimalArrayRegistryDataType dataType = new DecimalArrayRegistryDataType();
					collection = new DecimalLineCollection(dataType);
				}
				return collection;
			}
		}

		DecimalLine line;
		DecimalLineCollection collection;
	}
}
