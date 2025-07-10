using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(CustomizedField))]
	class CustomizedFieldTest : DataObjectTestCase<CustomizedField>
	{
		public void TestNew_BooleanValue()
		{
			var field = CustomizedField.New("GreatBoolean", ZBool.True);
			CombineAssertions("Boolean Value", delegate
			{
				AssertEquals("field.Key", "GreatBoolean", field.Key);
				AssertEquals("field.DataType", DataType.Boolean, field.DataType);
				AssertEquals("field.Value", "true", field.Value);
			});
		}

		public void TestNew_ByteValue()
		{
			var field = CustomizedField.New("GreatByte", new ZByte(123));
			CombineAssertions("Byte Value", delegate
			{
				AssertEquals("field.Key", "GreatByte", field.Key);
				AssertEquals("field.DataType", DataType.Byte, field.DataType);
				AssertEquals("field.Value", "123", field.Value);
			});
		}

		public void TestNew_DateTimeValue()
		{
			var field = CustomizedField.New("GreatDate", new ZDateTime(2010, 5, 2));
			CombineAssertions("DateTime Value", delegate
			{
				AssertEquals("field.Key", "GreatDate", field.Key);
				AssertEquals("field.DataType", DataType.DateTime, field.DataType);
				AssertEquals("field.Value", "2010-05-02T00:00:00", field.Value);
			});
		}

		public void TestNew_DateTimeOffsetValue()
		{
			var field = CustomizedField.New("GreatDateTimeOffset", new ZDateTimeOffset(2010, 5, 2, 1, 2, 3, 123, TimeSpan.FromHours(8)));
			CombineAssertions("DateTimeOffset Value", delegate
			{
				AssertEquals("field.Key", "GreatDateTimeOffset", field.Key);
				AssertEquals("field.DataType", DataType.DateTimeOffset, field.DataType);
				AssertEquals("field.Value", "2010-05-02T01:02:03.123+08:00", field.Value);
			});
		}

		public void TestNew_DecimalValue()
		{
			var field = CustomizedField.New("GreatDecimal", new ZDecimal(123.456));
			CombineAssertions("Decimal Value", delegate
			{
				AssertEquals("field.Key", "GreatDecimal", field.Key);
				AssertEquals("field.DataType", DataType.Decimal, field.DataType);
				AssertEquals("field.Value", "123.456", field.Value);
			});
		}

		public void TestNew_GeographyValue()
		{
			var field = CustomizedField.New("GreatGeography", new ZGeography("-121 48"));
			CombineAssertions("Geography Value", delegate
			{
				AssertEquals("field.Key", "GreatGeography", field.Key);
				AssertEquals("field.DataType", DataType.Geography, field.DataType);
				AssertEquals("field.Value", "POINT (-121 48)", field.Value);
			});
		}

		public void TestNew_IntValue()
		{
			var field = CustomizedField.New("GreatInt", new ZInt(123));
			CombineAssertions("Int Value", delegate
			{
				AssertEquals("field.Key", "GreatInt", field.Key);
				AssertEquals("field.DataType", DataType.Integer, field.DataType);
				AssertEquals("field.Value", "123", field.Value);
			});
		}

		public void TestNew_ShortValue()
		{
			var field = CustomizedField.New("GreatShort", new ZShort(123));
			CombineAssertions("Short Value", delegate
			{
				AssertEquals("field.Key", "GreatShort", field.Key);
				AssertEquals("field.DataType", DataType.Short, field.DataType);
				AssertEquals("field.Value", "123", field.Value);
			});
		}

		public void TestNew_StringValue()
		{
			var field = CustomizedField.New("GreatString", new ZString("The great string is really great."));
			CombineAssertions("String Value", delegate
			{
				AssertEquals("field.Key", "GreatString", field.Key);
				AssertEquals("field.DataType", DataType.String, field.DataType);
				AssertEquals("field.Value", "The great string is really great.", field.Value);
			});
		}

		public void TestNew_TimeValue()
		{
			var field = CustomizedField.New("GreatTime", new ZTime(10, 5));
			CombineAssertions("Time Value", delegate
			{
				AssertEquals("field.Key", "GreatTime", field.Key);
				AssertEquals("field.DataType", DataType.Time, field.DataType);
				AssertEquals("field.Value", "10:05", field.Value);
			});
		}

		public void TestNew_MaxLength_StringValue()
		{
			var expectedWord = new ZString('a', 21474836); // should be 2147483647 but there is the limit memory usage in unit test so we use 21474836 for 32 bit.
			var field = CustomizedField.New("GreatString", expectedWord);
			CombineAssertions("String Value", delegate
			{
				AssertEquals("field.Key", "GreatString", field.Key);
				AssertEquals("field.DataType", DataType.String, field.DataType);
				AssertEquals("field.Value", expectedWord, field.Value);
			});
		}

		public void TestCustomizedFieldMaxLength_Matching_JobComInvoiceLineSchema_JI_CustomTextBlob1_Maxlength()
		{
			AssertEquals("The maxlength of CustomizedField does not match maxlength of obComInvoiceLineSchema.JI_CustomTextBlob1", CustomizedField.ValueMaxLength, JobComInvoiceLineSchema.JI_CustomTextBlob1.MaxLength);
		}

		public void TestCustomizedFieldKeyLength()
		{
			AssertEquals("The maxlength of the CustomizedField key should be the max of XV_Name and OT_FieldName", Math.Max(AutoGenCustomAddOnValue.Schema.XV_NameMaxLength, AutoOrgCustomLabels.Schema.OT_FieldNameMaxLength), CustomizedField.KeyMaxLength);
		}

		public void TestCustomFieldsHaveCultureInvariantDecimals()
		{
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			var currentUICulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");

				var customField = CustomizedField.New("Decimal", new ZDecimal(345.543m));
				AssertEquals("customField.DataType", DataType.Decimal, customField.DataType);
				AssertEquals("customField.Key", "Decimal", customField.Key);
				AssertEquals("customField.Value", "345.543", customField.Value);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(CustomizedField.Value)
		};
	}
}
