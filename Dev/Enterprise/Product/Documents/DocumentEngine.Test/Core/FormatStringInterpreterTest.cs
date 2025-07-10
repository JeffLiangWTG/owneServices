using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class FormatStringInterpreterTest : TestCaseWithFactory
	{
		public void TestFormatTrimTrailingZeros()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.DecimalField = 45.67m;
			CombineAssertions(delegate
			{
				AssertEquals("{DecimalField:T}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T}"));
				AssertEquals("{DecimalField:T0}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T0}"));
				AssertEquals("{DecimalField:T1}", "45.7", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T1}"));
				AssertEquals("{DecimalField:T2}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T2}"));
				AssertEquals("{DecimalField:T3}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T3}"));
				AssertEquals("{DecimalField:T4}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T4}"));
			});

			testBO.DecimalField = 4567.854m;
			CombineAssertions(delegate
			{
				AssertEquals("{DecimalField.T}", "4567.854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T}"));
				AssertEquals("{DecimalField.T0}", "4567.854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T0}"));
				AssertEquals("{DecimalField.T1}", "4567.9", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T1}"));
				AssertEquals("{DecimalField.T2}", "4567.85", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T2}"));
				AssertEquals("{DecimalField.T3}", "4567.854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T3}"));
				AssertEquals("{DecimalField.T4}", "4567.854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T4}"));
			});

			testBO.DecimalField = 2020.000m;
			CombineAssertions(delegate
			{
				AssertEquals("{DecimalField.T}", "2020", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T}"));
				AssertEquals("{DecimalField.T0}", "2020", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T0}"));
				AssertEquals("{DecimalField.T1}", "2020", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T1}"));
			});

			var currentCulture = Culture.Current;
			try
			{
				Culture.Set(Culture.GetCultureForLanguage(Core.Constants.Languages.French));

				CombineAssertions(delegate
				{
					AssertEquals("{DecimalField.T}", "2020", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T}"));
					AssertEquals("{DecimalField.T0}", "2020", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T0}"));
					AssertEquals("{DecimalField.T1}", "2020", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T1}"));
				});

				testBO.DecimalField = 4567.854m;
				CombineAssertions(delegate
				{
					AssertEquals("{DecimalField.T}", "4567,854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T}"));
					AssertEquals("{DecimalField.T0}", "4567,854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T0}"));
					AssertEquals("{DecimalField.T1}", "4567,9", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T1}"));
					AssertEquals("{DecimalField.T2}", "4567,85", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T2}"));
					AssertEquals("{DecimalField.T3}", "4567,854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T3}"));
					AssertEquals("{DecimalField.T4}", "4567,854", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T4}"));
				});
			}
			finally
			{
				Culture.Set(currentCulture);
			}
		}

		public void TestFormatGeneralFunctionality()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.Name = "Ben Govett";
			testBO.Phone = "(02) 9634 6124";
			AssertEquals("{Name}", "Ben Govett", FormatStringInterpreter.Format(dataProvider, "{Name}"));
			AssertEquals("Phone: {Phone}", "Phone: (02) 9634 6124", FormatStringInterpreter.Format(dataProvider, "Phone: {Phone}"));
			AssertEquals("{Name} - Phone: {Phone}", "Ben Govett - Phone: (02) 9634 6124", FormatStringInterpreter.Format(dataProvider, "{Name} - Phone: {Phone}"));
			AssertEquals("{FieldNotThereOhBugger}", "{FieldNotThereOhBugger}", FormatStringInterpreter.Format(dataProvider, "{FieldNotThereOhBugger}"));
			AssertEquals("{#$&*)@#$SJDFHSD}", "{#$&*)@#$SJDFHSD}", FormatStringInterpreter.Format(dataProvider, "{#$&*)@#$SJDFHSD}"));
		}

		public void TestFormatForString()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.Name = "Ben govett";
			AssertEquals("{Name}", "Ben govett", FormatStringInterpreter.Format(dataProvider, "{Name}"));
			AssertEquals("{Name:Upper}", "BEN GOVETT", FormatStringInterpreter.Format(dataProvider, "{Name:Upper}"));
			AssertEquals("{Name:Lower}", "ben govett", FormatStringInterpreter.Format(dataProvider, "{Name:Lower}"));
			AssertEquals("{Name:Proper}", "Ben Govett", FormatStringInterpreter.Format(dataProvider, "{Name:Proper}"));
		}

		public void TestFormatForInt()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.IntField = 23;
			AssertEquals("{IntField}", "23", FormatStringInterpreter.Format(dataProvider, "{IntField}"));
			AssertEquals("{IntField:AnythingNotRecognisedIsIgnored}", "23", FormatStringInterpreter.Format(dataProvider, "{IntField:AnythingNotRecognisedIsIgnored}"));
		}

		public void TestFormatForDecimal()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.DecimalField = 45.67m;
			AssertEquals("{DecimalField}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField}"));
			AssertEquals("{DecimalField:C2}", "$45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:C2}"));
			AssertEquals("{DecimalField:D10}", "0000045.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:D10}"));
			AssertEquals("{DecimalField:E4}", "4.5670E+001", FormatStringInterpreter.Format(dataProvider, "{DecimalField:E4}"));
			AssertEquals("{DecimalField:F3}", "45.670", FormatStringInterpreter.Format(dataProvider, "{DecimalField:F3}"));
			AssertEquals("{DecimalField:N1}", "45.7", FormatStringInterpreter.Format(dataProvider, "{DecimalField:N1}"));
			AssertEquals("{DecimalField:P1}", "4,567.0%", FormatStringInterpreter.Format(dataProvider, "{DecimalField:P1}").Replace(" ", ""));
			AssertEquals("{DecimalField:T2}", "45.67", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T2}"));

			testBO.DecimalField = 4567.8m;
			AssertEquals("{DecimalField}", "4567.8", FormatStringInterpreter.Format(dataProvider, "{DecimalField}"));
			AssertEquals("{DecimalField:C2}", "$4,567.80", FormatStringInterpreter.Format(dataProvider, "{DecimalField:C2}"));
			AssertEquals("{DecimalField:D10}", "00004567.8", FormatStringInterpreter.Format(dataProvider, "{DecimalField:D10}"));
			AssertEquals("{DecimalField:E4}", "4.5678E+003", FormatStringInterpreter.Format(dataProvider, "{DecimalField:E4}"));
			AssertEquals("{DecimalField:F3}", "4567.800", FormatStringInterpreter.Format(dataProvider, "{DecimalField:F3}"));
			AssertEquals("{DecimalField:N1}", "4,567.8", FormatStringInterpreter.Format(dataProvider, "{DecimalField:N1}"));
			AssertEquals("{DecimalField:P1}", "456,780.0%", FormatStringInterpreter.Format(dataProvider, "{DecimalField:P1}").Replace(" ", ""));
			AssertEquals("{DecimalField.T2}", "4567.8", FormatStringInterpreter.Format(dataProvider, "{DecimalField:T2}"));
		}

		public void TestFormatForDateTime()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.DateTimeField = new ZDateTime(2006, 1, 23, 15, 43, 32);
			AssertEquals("{DateTimeField:Date}", "23-Jan-06", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:Date}"));
			AssertEquals("{DateTimeField:LongDate}", "23rd January 2006", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:LongDate}"));
			AssertEquals("{DateTimeField:AmericanDate}", "01/23/06", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:AmericanDate}"));
			AssertEquals("{DateTimeField:BritishDate}", "23/01/06", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:BritishDate}"));
			AssertEquals("{DateTimeField:Day}", "Monday", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:Day}"));
			AssertEquals("{DateTimeField:Time24h}", "15:43", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:Time24h}"));
			AssertEquals("{DateTimeField:Time12h}", "3:43p", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:Time12h}"));
			AssertEquals("{DateTimeField:Time12hAMPM}", "3:43 pm", FormatStringInterpreter.Format(dataProvider, "{DateTimeField:Time12hAMPM}"));

			//			AssertEquals("{DateTimeField}", "23-Jan-06", dataProvider.ToString("{DateTimeField}"));
		}

		public void TestFormatForDateTimeOffset()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			testBO.DateTimeOffsetField = new ZDateTimeOffset(2006, 1, 23, 15, 43, 32, TimeSpan.FromHours(9));
			AssertEquals("{DateTimeOffsetField:Date}", "23-Jan-06", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:Date}"));
			AssertEquals("{DateTimeOffsetField:LongDate}", "23rd January 2006", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:LongDate}"));
			AssertEquals("{DateTimeOffsetField:AmericanDate}", "01/23/06", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:AmericanDate}"));
			AssertEquals("{DateTimeOffsetField:BritishDate}", "23/01/06", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:BritishDate}"));
			AssertEquals("{DateTimeOffsetField:Day}", "Monday", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:Day}"));
			AssertEquals("{DateTimeOffsetField:Time24h}", "15:43", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:Time24h}"));
			AssertEquals("{DateTimeOffsetField:Time12h}", "3:43p", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:Time12h}"));
			AssertEquals("{DateTimeOffsetField:Time12hAMPM}", "3:43 pm", FormatStringInterpreter.Format(dataProvider, "{DateTimeOffsetField:Time12hAMPM}"));

			//			AssertEquals("{DateTimeField}", "23-Jan-06", dataProvider.ToString("{DateTimeField}"));
		}

		public void TestFormatForBool()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			AssertEquals("{BoolTrueField}", "Y", FormatStringInterpreter.Format(dataProvider, "{BoolTrueField}"));
			AssertEquals("{BoolFalseField}", "N", FormatStringInterpreter.Format(dataProvider, "{BoolFalseField}"));

			AssertEquals("{BoolTrueField:X}", "X", FormatStringInterpreter.Format(dataProvider, "{BoolTrueField:X}"));
			AssertEquals("{BoolFalseField:X}", "", FormatStringInterpreter.Format(dataProvider, "{BoolFalseField:X}"));

			AssertEquals("{BoolTrueField:YN}", "Y", FormatStringInterpreter.Format(dataProvider, "{BoolTrueField:YN}"));
			AssertEquals("{BoolFalseField:YN}", "N", FormatStringInterpreter.Format(dataProvider, "{BoolFalseField:YN}"));

			AssertEquals("{BoolTrueField:YesNo}", "Yes", FormatStringInterpreter.Format(dataProvider, "{BoolTrueField:YesNo}"));
			AssertEquals("{BoolFalseField:YesNo}", "No", FormatStringInterpreter.Format(dataProvider, "{BoolFalseField:YesNo}"));

			AssertEquals("{BoolTrueField:TrueFalse}", "True", FormatStringInterpreter.Format(dataProvider, "{BoolTrueField:TrueFalse}"));
			AssertEquals("{BoolFalseField:TrueFalse}", "False", FormatStringInterpreter.Format(dataProvider, "{BoolFalseField:TrueFalse}"));

			using (var mockRes = Res.UseMockData())
			{
				const string huo = "或";
				mockRes.SetResourceGetter(delegate(string key)
				{ return new ResourceStringData(key, huo); });

				AssertEquals("{BoolTrueField:YesNo}", huo, FormatStringInterpreter.Format(dataProvider, "{BoolTrueField:YesNo}"));
				AssertEquals("{BoolFalseField:YesNo}", huo, FormatStringInterpreter.Format(dataProvider, "{BoolFalseField:YesNo}"));

				AssertEquals("{BoolTrueField:TrueFalse}", huo, FormatStringInterpreter.Format(dataProvider, "{BoolTrueField:TrueFalse}"));
				AssertEquals("{BoolFalseField:TrueFalse}", huo, FormatStringInterpreter.Format(dataProvider, "{BoolFalseField:TrueFalse}"));
			}
		}

		public void TestFormatForChildDataSource()
		{
			TestDataProvider testBO = new TestDataProvider();
			var dataProvider = (IBODocDataProvider)testBO;
			AssertEquals("{Child}", "I Gotcha...", FormatStringInterpreter.Format(dataProvider, "{Child}"));
		}

		public void TestInterpretExpressionWhichUsesFieldWithDot()
		{
			var book = new BookForTest();
			book.Author = new PersonForTest { FirstName = "Steve", LastName = "Jobs" };

			using (var interpreter = new FormatStringInterpreter())
			{
				AssertEquals("", @"""Steve"" == ""Steve""", interpreter.Interpret(BODocDataProvider.Get(book), @"""{Author.FirstName}"" == ""Steve"""));
			}
		}

		public void TestInvalidFormatStringThrowsNoExceptions()
		{
			var book = new BookForTest()
			{
				Author = new PersonForTest { FirstName = "Steve", LastName = "Jobs" },
				Title = "Apple",
				ISBN = "ABC123456"
			};

			using (var interpreter = new FormatStringInterpreter())
			{
				AssertEquals(@"Steve ate {Title (ABC123456)", interpreter.Interpret(BODocDataProvider.Get(book), @"{Author.FirstName} ate {Title ({ISBN})"));
			}

			using (var interpreter = new FormatStringInterpreter())
			{
				AssertEquals(@"Steve {{ {} {AAAA} $Apple!@#$%^&*(: ABC123456", interpreter.Interpret(BODocDataProvider.Get(book), @"{Author.FirstName} {{ {} {AAAA} ${Title}!@#$%^&*(: { ISBN}"));
			}
		}

		public void TestFormatStringWithBraces()
		{
			var book = new BookForTest()
			{
				Author = new PersonForTest { FirstName = "Steve", LastName = "Jobs" },
				Title = "Apple",
				ISBN = "ABC123456"
			};

			using (var interpreter = new FormatStringInterpreter())
			{
				AssertEquals(@"Steve ate (Apple) (ABC123456)", interpreter.Interpret(BODocDataProvider.Get(book), @"{Author.FirstName} ate ({Title}) ({ISBN})"));
			}

			using (var interpreter = new FormatStringInterpreter())
			{
				AssertEquals(@"{Steve} ate {Apple} {ABC123456}", interpreter.Interpret(BODocDataProvider.Get(book), @"{{Author.FirstName}} ate {{Title}} {{ISBN}}"));
			}
		}
	}
}
