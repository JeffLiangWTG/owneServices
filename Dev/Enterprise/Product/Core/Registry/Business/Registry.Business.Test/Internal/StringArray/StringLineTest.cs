using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Internal.Testing
{
	[TestedType(typeof(StringLine))]
	sealed class StringLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateValue()
		{
			Line.Value = "Hello World";
			AssertNoErrorContaining(Line.ValueInfo, MandatoryValidation.MustBeEntered);

			Line.Value = ZString.Empty;
			AssertHasErrorContaining(Line.ValueInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestDataType()
		{
			AssertEquals(DataType, Line.DataType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Collection.AddNew();
		}

		StringArrayRegistryDataType DataType
		{
			get { return dataType ?? (dataType = new DelimitedStringArrayRegistryDataType()); }
		}
		StringArrayRegistryDataType dataType;

		StringLine Line
		{
			get { return line ?? (line = (StringLine)GetNewBusinessObject()); }
		}
		StringLine line;

		StringLineCollection Collection
		{
			get { return collection ?? (collection = new StringLineCollection(DataType)); }
		}
		StringLineCollection collection;
	}
}
