using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class IBODocDataProviderFunctionalityTest : TestCaseWithFactory
	{
		public void TestFormatGeneralFunctionality()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				testBO.Name = "Ben Govett";
				testBO.Phone = "(02) 9634 6124";
				AssertEquals("{Name}", "Ben Govett", formatStringInterpreter.Format(dataProvider, "{Name}"));
				AssertEquals("Phone: {Phone}", "Phone: (02) 9634 6124", formatStringInterpreter.Format(dataProvider, "Phone: {Phone}"));
				AssertEquals("{Name} - Phone: {Phone}", "Ben Govett - Phone: (02) 9634 6124", formatStringInterpreter.Format(dataProvider, "{Name} - Phone: {Phone}"));
				AssertEquals("{FieldNotThereOhBugger}", "{FieldNotThereOhBugger}", formatStringInterpreter.Format(dataProvider, "{FieldNotThereOhBugger}"));
				AssertEquals("{#$&*)@#$SJDFHSD}", "{#$&*)@#$SJDFHSD}", formatStringInterpreter.Format(dataProvider, "{#$&*)@#$SJDFHSD}"));
			}
		}

		public void TestFormatForString()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				testBO.Name = "Ben govett";
				AssertEquals("{Name}", "Ben govett", formatStringInterpreter.Format(dataProvider, "{Name}"));
				AssertEquals("{Name:Upper}", "BEN GOVETT", formatStringInterpreter.Format(dataProvider, "{Name:Upper}"));
				AssertEquals("{Name:Lower}", "ben govett", formatStringInterpreter.Format(dataProvider, "{Name:Lower}"));
				AssertEquals("{Name:Proper}", "Ben Govett", formatStringInterpreter.Format(dataProvider, "{Name:Proper}"));
			}
		}

		public void TestFormatForInt()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				testBO.IntField = 23;
				AssertEquals("{IntField}", "23", formatStringInterpreter.Format(dataProvider, "{IntField}"));
				AssertEquals("{IntField:AnythingNotRecognisedIsIgnored}", "23", formatStringInterpreter.Format(dataProvider, "{IntField:AnythingNotRecognisedIsIgnored}"));
			}
		}

		public void TestFormatForDecimal()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			testBO.DecimalField = 45.67m;
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				AssertEquals("{DecimalField}", "45.67", formatStringInterpreter.Format(dataProvider, "{DecimalField}"));
				AssertEquals("{DecimalField:C2}", "$45.67", formatStringInterpreter.Format(dataProvider, "{DecimalField:C2}"));
				AssertEquals("{DecimalField:D10}", "0000045.67", formatStringInterpreter.Format(dataProvider, "{DecimalField:D10}"));
				AssertEquals("{DecimalField:E4}", "4.5670E+001", formatStringInterpreter.Format(dataProvider, "{DecimalField:E4}"));
				AssertEquals("{DecimalField:F3}", "45.670", formatStringInterpreter.Format(dataProvider, "{DecimalField:F3}"));
				AssertEquals("{DecimalField:N1}", "45.7", formatStringInterpreter.Format(dataProvider, "{DecimalField:N1}"));
				AssertEquals("{DecimalField:P1}", "4,567.0%", formatStringInterpreter.Format(dataProvider, "{DecimalField:P1}").Replace(" ", ""));

				testBO.DecimalField = 4567.8m;
				AssertEquals("{DecimalField}", "4567.8", formatStringInterpreter.Format(dataProvider, "{DecimalField}"));
				AssertEquals("{DecimalField:C2}", "$4,567.80", formatStringInterpreter.Format(dataProvider, "{DecimalField:C2}"));
				AssertEquals("{DecimalField:D10}", "00004567.8", formatStringInterpreter.Format(dataProvider, "{DecimalField:D10}"));
				AssertEquals("{DecimalField:E4}", "4.5678E+003", formatStringInterpreter.Format(dataProvider, "{DecimalField:E4}"));
				AssertEquals("{DecimalField:F3}", "4567.800", formatStringInterpreter.Format(dataProvider, "{DecimalField:F3}"));
				AssertEquals("{DecimalField:N1}", "4,567.8", formatStringInterpreter.Format(dataProvider, "{DecimalField:N1}"));
				AssertEquals("{DecimalField:P1}", "456,780.0%", formatStringInterpreter.Format(dataProvider, "{DecimalField:P1}").Replace(" ", ""));
			}
		}

		public void TestFormatForDateTime()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			testBO.DateTimeField = new ZDateTime(2006, 1, 23, 15, 43, 32);
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				AssertEquals("{DateTimeField:Date}", "23-Jan-06", formatStringInterpreter.Format(dataProvider, "{DateTimeField:Date}"));
				AssertEquals("{DateTimeField:LongDate}", "23rd January 2006", formatStringInterpreter.Format(dataProvider, "{DateTimeField:LongDate}"));
				AssertEquals("{DateTimeField:AmericanDate}", "01/23/06", formatStringInterpreter.Format(dataProvider, "{DateTimeField:AmericanDate}"));
				AssertEquals("{DateTimeField:BritishDate}", "23/01/06", formatStringInterpreter.Format(dataProvider, "{DateTimeField:BritishDate}"));
				AssertEquals("{DateTimeField:Day}", "Monday", formatStringInterpreter.Format(dataProvider, "{DateTimeField:Day}"));
				AssertEquals("{DateTimeField:Time24h}", "15:43", formatStringInterpreter.Format(dataProvider, "{DateTimeField:Time24h}"));
				AssertEquals("{DateTimeField:Time12h}", "3:43p", formatStringInterpreter.Format(dataProvider, "{DateTimeField:Time12h}"));
				AssertEquals("{DateTimeField:Time12hAMPM}", "3:43 pm", formatStringInterpreter.Format(dataProvider, "{DateTimeField:Time12hAMPM}"));
			}
		}

		public void TestFormatForBool()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				AssertEquals("{BoolTrueField}", "Y", formatStringInterpreter.Format(dataProvider, "{BoolTrueField}"));
				AssertEquals("{BoolFalseField}", "N", formatStringInterpreter.Format(dataProvider, "{BoolFalseField}"));

				AssertEquals("{BoolTrueField:X}", "X", formatStringInterpreter.Format(dataProvider, "{BoolTrueField:X}"));
				AssertEquals("{BoolFalseField:X}", "", formatStringInterpreter.Format(dataProvider, "{BoolFalseField:X}"));

				AssertEquals("{BoolTrueField:YN}", "Y", formatStringInterpreter.Format(dataProvider, "{BoolTrueField:YN}"));
				AssertEquals("{BoolFalseField:YN}", "N", formatStringInterpreter.Format(dataProvider, "{BoolFalseField:YN}"));

				AssertEquals("{BoolTrueField:YesNo}", "Yes", formatStringInterpreter.Format(dataProvider, "{BoolTrueField:YesNo}"));
				AssertEquals("{BoolFalseField:YesNo}", "No", formatStringInterpreter.Format(dataProvider, "{BoolFalseField:YesNo}"));

				AssertEquals("{BoolTrueField:TrueFalse}", "True", formatStringInterpreter.Format(dataProvider, "{BoolTrueField:TrueFalse}"));
				AssertEquals("{BoolFalseField:TrueFalse}", "False", formatStringInterpreter.Format(dataProvider, "{BoolFalseField:TrueFalse}"));
			}
		}

		public void TestFormatForChildDataSource()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			using (var formatStringInterpreter = ObjectFactory.Get<IFormatStringInterpreter>())
			{
				AssertEquals("{Child}", "I Gotcha...", formatStringInterpreter.Format(dataProvider, "{Child}"));
			}
		}

		public void TestGetDocDataValue()
		{
			TestBO testBO = new TestBO();
			IBODocDataProvider dataProvider = BODocDataProvider.Get(testBO);
			AssertNotNull(dataProvider.GetDocDataValue("Name", ""));
		}

		public void TestUpdateForDataRefresh_DoesCopyFromUnloadedBlobFields()
		{
			StmData stmData = Factory.New<StmData>();
			stmData.SD_Name = "TestUpdateForDataRefresh_DoesCopyFromUnloadedBlobFields";
			stmData.SD_BinaryValue = new byte[] { 1 };
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			StmData stmData1 = factory1.Load<StmData>(stmData.PK);
			StmData stmData2 = factory2.Load<StmData>(stmData.PK);

			stmData1.SD_Owner = ZGuid.NewZGuid();
			stmData2.SD_Owner = ZGuid.NewZGuid();
			factory1.Save();
			AssertEquals("Binary value should not be set to null by data refresh bus toot toot", new byte[] { 1 }, stmData2.SD_BinaryValue);

			stmData1.SD_BinaryValue = ZBlob.Empty;
			factory1.Save();
			AssertEquals("Binary value should not be set to null by data refresh bus toot toot", ZBlob.Empty, stmData2.SD_BinaryValue);
		}

		#region Test Classes

		class TestBO : NonPersistentBusinessObject
		{
			public ZString Name
			{
				get { return fName; }
				set { fName = value; }
			}
			ZString fName;

			public ZString Phone
			{
				get { return fPhone; }
				set { fPhone = value; }
			}
			ZString fPhone;

			public ZInt IntField
			{
				get { return fIntField; }
				set { fIntField = value; }
			}
			ZInt fIntField;

			public ZDecimal DecimalField
			{
				get { return fDecimalField; }
				set { fDecimalField = value; }
			}
			ZDecimal fDecimalField;

			public ZDateTime DateTimeField
			{
				get { return fDateTimeField; }
				set { fDateTimeField = value; }
			}
			ZDateTime fDateTimeField;

			public ZBool BoolTrueField
			{
				get { return true; }
			}

			public ZBool BoolFalseField
			{
				get { return false; }
			}

			public TestBOChild Child
			{
				get { return new TestBOChild(); }
			}
		}

		class TestBOChild : NonPersistentBusinessObject
		{
			public override string ToString()
			{
				return "I Gotcha...";
			}
		}

		#endregion
	}
}
