using System;
using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DecimalArrayRegistryDataType))]
	sealed class DecimalArrayRegistryDataTypeTest : RegistryDataTypeTestCase<DecimalArrayRegistryDataType>
	{
		public void TestDefaults()
		{
			AssertEquals("DecimalPlaces", 2, TypeCastDataType.DecimalPlaces);
			AssertEquals("LowerBound", null, TypeCastDataType.LowerBound);
			AssertEquals("UpperBound", null, TypeCastDataType.UpperBound);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Values cannot have more than 1 decimal place.")]
		public void TestValidate_1DecimalPlace()
		{
			TypeCastDataType.DecimalPlaces = 1;
			DataType.Validate(null, new decimal[] { 0m, 1.22m, 3m }, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Values cannot have more than 3 decimal places.")]
		public void TestValidate_3DecimalPlaces()
		{
			TypeCastDataType.DecimalPlaces = 3;
			DataType.Validate(null, new decimal[] { 0.1234m, 1m, 3m }, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Values must be greater than or equal to 1.")]
		public void TestValidate_LowerBound()
		{
			TypeCastDataType.LowerBound = 1m;
			DataType.Validate(null, new decimal[] { 0m, 1m, 3m }, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Values must be less than or equal to 3.")]
		public void TestValidate_UpperBound()
		{
			TypeCastDataType.UpperBound = 3m;
			DataType.Validate(null, new decimal[] { 0m, 1m, 4m }, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#region Implementation

		protected override DecimalArrayRegistryDataType GetNewDataType()
		{
			return new DecimalArrayRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			decimal[] lhsNumbers = (decimal[])lhs;
			decimal[] rhsNumbers = (decimal[])rhs;

			AssertEquals("Length", lhsNumbers.Length, rhsNumbers.Length);

			for (int i = 0; i < lhsNumbers.Length; i++)
			{
				AssertEquals("[" + i + "]", lhsNumbers[i], rhsNumbers[i]);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Array.Empty<decimal>(), Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(new decimal[] { 1m, 2m, 3m }, Encoding.Unicode.GetBytes("1,2,3")),
				new ValidSampleAndBinaryValueInDB(new decimal[] { -1.1m, 2.2m, 3.34m }, Encoding.Unicode.GetBytes("-1.1,2.2,3.34")),
				new ValidSampleAndBinaryValueInDB(new decimal[] { 2.2m }, Encoding.Unicode.GetBytes(", 2.2,3.3a")),
				new ValidSampleAndBinaryValueInDB(new decimal[] { 2.23m }, Encoding.Unicode.GetBytes("2.2300,")),
			};
		}

		DecimalArrayRegistryDataType TypeCastDataType
		{
			get { return DataType; }
		}

		#endregion
	}
}
