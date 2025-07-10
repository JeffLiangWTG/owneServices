using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LabelValuePairWrapper))]
	sealed class LabelValuePairWrapperTest : GenericWrapperTest
	{
		public void TestSetAllValues()
		{
			LabelValuePairWrapper value = new LabelValuePairWrapper("label", "value", Factory);
			AssertLabelValuePair("Is Not Empty", "label", "value", value);
			AssertEquals(0m, value.ValueAsDecimal);

			value = new LabelValuePairWrapper("label", new LabelValuePairWrapper("Formatted Value", "original value", Factory), Factory);
			AssertLabelValuePair("Formatted values", "label", "Formatted Value", value);
			AssertEquals(0m, value.ValueAsDecimal);

			value = new LabelValuePairWrapper("date", new ZDateTime(2010, 6, 9, 17, 55, 45), Factory);
			AssertLabelValuePair("Date values", "date", "09-Jun-10 17:55", value);
			AssertEquals("Native value", new ZDateTime(2010, 6, 9, 17, 55, 45), value.NativeValue);
			AssertEquals(0m, value.ValueAsDecimal);

			value = new LabelValuePairWrapper(Factory);
			AssertEquals("Empty", LabelValuePairWrapper.Empty.IsEmpty, value.IsEmpty);
			AssertEquals(0m, value.ValueAsDecimal);

			value = new LabelValuePairWrapper("label", "1.23", Factory);
			AssertEquals(1.23m, value.ValueAsDecimal);
		}

		void AssertLabelValuePair(string message, string expectedLabel, string expectedValue, LabelValuePairWrapper actualLabelValuePair)
		{
			AssertEquals(message + " label", expectedLabel, actualLabelValuePair.Label);
			AssertEquals(message + " value", expectedValue, actualLabelValuePair.Value);
		}

		public override void TestWrapperMappingsEmpty()
		{
			LabelValuePairWrapper wrapperEmpty = new LabelValuePairWrapper(ZString.Empty, ZString.Empty, Factory);

			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Label", ZString.Empty, wrapperEmpty.Label);
			AssertEquals("wrapperEmpty.Value", ZString.Empty, wrapperEmpty.Value);
			AssertEquals("wrapperEmpty.LabelAndValue", ZString.Empty, wrapperEmpty.LabelAndValue);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new LabelValuePairWrapper("Label", "Code", Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
LabelValuePair                                  (Default Field: Value)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Label                                   String
LabelAndValue                           String
Value                                   String
ValueAsDate                             DateTime
ValueAsDecimal                          Decimal
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new LabelValuePairWrapper("Label", "Code", Factory);
		}
	}
}
