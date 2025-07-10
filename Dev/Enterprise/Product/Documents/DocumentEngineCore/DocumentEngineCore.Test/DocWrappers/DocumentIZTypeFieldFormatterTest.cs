using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocumentIZTypeFieldFormatterTest : TestCaseWithFactory
	{
		public void TestToStringWithFormatForString()
		{
			DocumentIZTypeFieldFormatter wrapper = new DocumentIZTypeFieldFormatter(new ZString("Ben govett"));
			AssertEquals("{Name}", "Ben govett", wrapper.ToString(""));
			AssertEquals("{Name:Upper}", "BEN GOVETT", wrapper.ToString("Upper"));
			AssertEquals("{Name:Lower}", "ben govett", wrapper.ToString("Lower"));
			AssertEquals("{Name:Proper}", "Ben Govett", wrapper.ToString("Proper"));
			AssertEquals("{Name:Whatever}", "Ben govett", wrapper.ToString("Whatever"));
		}

		public void TestToStringWithFormatForShort()
		{
			DocumentIZTypeFieldFormatter wrapper = new DocumentIZTypeFieldFormatter(new ZShort(23));
			AssertEquals("{ShortField}", "23", wrapper.ToString(""));
			AssertEquals("{ShortField:C2}", "$23.00", wrapper.ToString("C2"));
			AssertEquals("{ShortField:D10}", "0000000023", wrapper.ToString("D10"));
			AssertEquals("{ShortField:E4}", "2.3000E+001", wrapper.ToString("E4"));
			AssertEquals("{ShortField:F3}", "23.000", wrapper.ToString("F3"));
			AssertEquals("{ShortField:N1}", "23.0", wrapper.ToString("N1"));
			AssertEquals("{ShortField:P1}", "2,300.0%", wrapper.ToString("P1").Replace(" ", ""));
			AssertEquals("{ShortField:AnythingNotRecognisedIsIgnored}", "23", wrapper.ToString("AnythingNotRecognisedIsIgnored"));
		}

		public void TestToStringWithFormatForInt()
		{
			DocumentIZTypeFieldFormatter wrapper = new DocumentIZTypeFieldFormatter(new ZInt(23));
			AssertEquals("{IntField}", "23", wrapper.ToString(""));
			AssertEquals("{IntField:C2}", "$23.00", wrapper.ToString("C2"));
			AssertEquals("{IntField:D10}", "0000000023", wrapper.ToString("D10"));
			AssertEquals("{IntField:E4}", "2.3000E+001", wrapper.ToString("E4"));
			AssertEquals("{IntField:F3}", "23.000", wrapper.ToString("F3"));
			AssertEquals("{IntField:N1}", "23.0", wrapper.ToString("N1"));
			AssertEquals("{IntField:P1}", "2,300.0%", wrapper.ToString("P1").Replace(" ", ""));
			AssertEquals("{IntField:AnythingNotRecognisedIsIgnored}", "23", wrapper.ToString("AnythingNotRecognisedIsIgnored"));
		}

		public void TestToStringWithFormatForDecimal()
		{
			DocumentIZTypeFieldFormatter wrapper = new DocumentIZTypeFieldFormatter(new ZDecimal(45.67m));
			AssertEquals("{DecimalField}", "45.67", wrapper.ToString(""));
			AssertEquals("{DecimalField:C2}", "$45.67", wrapper.ToString("C2"));
			AssertEquals("{DecimalField:D10}", "0000045.67", wrapper.ToString("D10"));
			AssertEquals("{DecimalField:E4}", "4.5670E+001", wrapper.ToString("E4"));
			AssertEquals("{DecimalField:F3}", "45.670", wrapper.ToString("F3"));
			AssertEquals("{DecimalField:N1}", "45.7", wrapper.ToString("N1"));
			AssertEquals("{DecimalField:P1}", "4,567.0%", wrapper.ToString("P1").Replace(" ", ""));
			AssertEquals("{DecimalField:T2}", "45.7", wrapper.ToString("T1"));
			AssertEquals("{DecimalField:T2}", "45.67", wrapper.ToString("T2"));

			wrapper = new DocumentIZTypeFieldFormatter(new ZDecimal(4567.8m));
			AssertEquals("{DecimalField}", "4567.8", wrapper.ToString(""));
			AssertEquals("{DecimalField:C2}", "$4,567.80", wrapper.ToString("C2"));
			AssertEquals("{DecimalField:D10}", "00004567.8", wrapper.ToString("D10"));
			AssertEquals("{DecimalField:E4}", "4.5678E+003", wrapper.ToString("E4"));
			AssertEquals("{DecimalField:F3}", "4567.800", wrapper.ToString("F3"));
			AssertEquals("{DecimalField:N1}", "4,567.8", wrapper.ToString("N1"));
			AssertEquals("{DecimalField:P1}", "456,780.0%", wrapper.ToString("P1").Replace(" ", ""));
			AssertEquals("{DecimalField.T2}", "4567.8", wrapper.ToString("T2"));
		}

		public void TestToStringWithFormatForDateTime()
		{
			DocumentIZTypeFieldFormatter wrapper = new DocumentIZTypeFieldFormatter(new ZDateTime(2006, 1, 23, 15, 43, 32));
			AssertEquals("{DateTimeField,Date}", "23-Jan-06", wrapper.ToString("Date"));
			AssertEquals("{DateTimeField,LongDate}", "23rd January 2006", wrapper.ToString("LongDate"));
			AssertEquals("{DateTimeField,AmericanDate}", "01/23/06", wrapper.ToString("AmericanDate"));
			AssertEquals("{DateTimeField,AmericanDateWithCentury}", "01/23/2006", wrapper.ToString("AmericanDateWithCentury"));
			AssertEquals("{DateTimeField,BritishDate}", "23/01/06", wrapper.ToString("BritishDate"));
			AssertEquals("{DateTimeField,Day}", "Monday", wrapper.ToString("Day"));
			AssertEquals("{DateTimeField,Time24h}", "15:43", wrapper.ToString("Time24h"));
			AssertEquals("{DateTimeField,Time12h}", "3:43p", wrapper.ToString("Time12h"));
			AssertEquals("{DateTimeField,Time12hAMPM}", "3:43 pm", wrapper.ToString("Time12hAMPM"));

			//			AssertEquals("{DateTimeField}", "23-Jan-06", wrapper.ToString(""));
		}

		public void TestToStringWithFormatForDateTimeOffset()
		{
			DocumentIZTypeFieldFormatter wrapper = new DocumentIZTypeFieldFormatter(new ZDateTimeOffset(2006, 1, 23, 15, 43, 32, TimeSpan.FromHours(9)));
			AssertEquals("{DateTimeField,Date}", "23-Jan-06", wrapper.ToString("Date"));
			AssertEquals("{DateTimeField,LongDate}", "23rd January 2006", wrapper.ToString("LongDate"));
			AssertEquals("{DateTimeField,AmericanDate}", "01/23/06", wrapper.ToString("AmericanDate"));
			AssertEquals("{DateTimeField,AmericanDateWithCentury}", "01/23/2006", wrapper.ToString("AmericanDateWithCentury"));
			AssertEquals("{DateTimeField,BritishDate}", "23/01/06", wrapper.ToString("BritishDate"));
			AssertEquals("{DateTimeField,Day}", "Monday", wrapper.ToString("Day"));
			AssertEquals("{DateTimeField,Time24h}", "15:43", wrapper.ToString("Time24h"));
			AssertEquals("{DateTimeField,Time12h}", "3:43p", wrapper.ToString("Time12h"));
			AssertEquals("{DateTimeField,Time12hAMPM}", "3:43 pm", wrapper.ToString("Time12hAMPM"));

			//			AssertEquals("{DateTimeField}", "23-Jan-06", wrapper.ToString(""));
		}

		public void TestToStringWithFormatForBool()
		{
			DocumentIZTypeFieldFormatter wrapperTrue = new DocumentIZTypeFieldFormatter(ZBool.True);
			DocumentIZTypeFieldFormatter wrapperFalse = new DocumentIZTypeFieldFormatter(ZBool.False);

			AssertEquals("{BoolTrueField}", "Y", wrapperTrue.ToString(""));
			AssertEquals("{BoolFalseField}", "N", wrapperFalse.ToString(""));

			AssertEquals("{BoolTrueField,X}", "X", wrapperTrue.ToString("X"));
			AssertEquals("{BoolFalseField,X}", "", wrapperFalse.ToString("X"));

			AssertEquals("{BoolTrueField,YN}", "Y", wrapperTrue.ToString("YN"));
			AssertEquals("{BoolFalseField,YN}", "N", wrapperFalse.ToString("YN"));

			AssertEquals("{BoolTrueField,YesNo}", "Yes", wrapperTrue.ToString("YesNo"));
			AssertEquals("{BoolFalseField,YesNo}", "No", wrapperFalse.ToString("YesNo"));

			AssertEquals("{BoolTrueField,TrueFalse}", "True", wrapperTrue.ToString("TrueFalse"));
			AssertEquals("{BoolFalseField,TrueFalse}", "False", wrapperFalse.ToString("TrueFalse"));
		}
	}
}
