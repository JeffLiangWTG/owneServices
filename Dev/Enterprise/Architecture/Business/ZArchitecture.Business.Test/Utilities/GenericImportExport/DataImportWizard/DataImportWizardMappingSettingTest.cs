namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DataImportWizardMappingSettingTest : XmlSerializableTestCase<DataImportWizardMappingSetting>
	{
		public void TestWriteXml()
		{
			DataImportWizardMappingSetting mappingSetting = new DataImportWizardMappingSetting();
			mappingSetting.MappingName = "TEST";
			mappingSetting.FileColumnIndex = 1;
			mappingSetting.DefaultValue = "DEF1";
			mappingSetting.Expression = "Field[0]+Field[1]";
			mappingSetting.MapAs = "Description";
			mappingSetting.ProperCase = true;
			mappingSetting.CustomMapList = "List1";
			mappingSetting.Delimiter = ".";
			mappingSetting.FileColumnIndexOrder = [1, 2];
			string result = Serialize(mappingSetting);
			Assert(result.Contains("<Name>TEST</Name>"));
			Assert(result.Contains("<ColumnIndex>1</ColumnIndex>"));
			Assert(result.Contains("<DefaultValue>DEF1</DefaultValue>"));
			Assert(result.Contains("<Expression>Field[0]+Field[1]</Expression>"));
			Assert(result.Contains("<MapAs>Description</MapAs>"));
			Assert(result.Contains("<ProperCase>True</ProperCase>"));
			Assert(result.Contains("<CustomMapList>List1</CustomMapList>"));
			Assert(result.Contains("<Delimiter>.</Delimiter>"));
			Assert(result.Contains("<ColumnIndexOrder>1</ColumnIndexOrder><ColumnIndexOrder>2</ColumnIndexOrder>"));
		}

		public void TestReadXmlWithInvalidValue()
		{
			string xml = "<DataImportWizardMappingSetting><Name>TEST1</Name><ColumnIndex>AAA</ColumnIndex></DataImportWizardMappingSetting>";
			AssertNoExceptionThrown(() => Deserialize(xml));
		}

		public void TestReadXml()
		{
			string xml = "<DataImportWizardMappingSetting><Name>TEST1</Name><ColumnIndex>1</ColumnIndex></DataImportWizardMappingSetting>";
			DataImportWizardMappingSetting mappingSetting = Deserialize(xml);
			AssertEquals("TEST1", mappingSetting.MappingName);
			AssertEquals(1, mappingSetting.FileColumnIndex);
			AssertEquals("", mappingSetting.DefaultValue);
			AssertEquals("", mappingSetting.Expression);
			AssertEquals("", mappingSetting.MapAs);
			AssertEquals(false, mappingSetting.ProperCase);
			AssertEquals("", mappingSetting.CustomMapList);
			AssertNullOrEmpty("Delimiter", mappingSetting.Delimiter);
			AssertEquals(0, mappingSetting.FileColumnIndexOrder.Count);

			xml = "<DataImportWizardMappingSetting><Crap1>qwerty</Crap1><Name>TEST2</Name><Crap2>asdfgh</Crap2><ColumnIndex>2</ColumnIndex><Crap3/></DataImportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST2", mappingSetting.MappingName);
			AssertEquals(2, mappingSetting.FileColumnIndex);
			AssertEquals("", mappingSetting.DefaultValue);
			AssertEquals("", mappingSetting.Expression);
			AssertEquals("", mappingSetting.MapAs);
			AssertEquals(false, mappingSetting.ProperCase);
			AssertEquals("", mappingSetting.CustomMapList);
			AssertNullOrEmpty("Delimiter", mappingSetting.Delimiter);
			AssertEquals(0, mappingSetting.FileColumnIndexOrder.Count);

			xml = "<DataImportWizardMappingSetting><Name>TEST3</Name><DefaultValue>DEF3</DefaultValue><ProperCase>True</ProperCase><CustomMapList>List1</CustomMapList></DataImportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST3", mappingSetting.MappingName);
			AssertEquals(-1, mappingSetting.FileColumnIndex);
			AssertEquals("DEF3", mappingSetting.DefaultValue);
			AssertEquals("", mappingSetting.Expression);
			AssertEquals("", mappingSetting.MapAs);
			AssertEquals(true, mappingSetting.ProperCase);
			AssertEquals("List1", mappingSetting.CustomMapList);
			AssertNullOrEmpty("Delimiter", mappingSetting.Delimiter);
			AssertEquals(0, mappingSetting.FileColumnIndexOrder.Count);

			xml = "<DataImportWizardMappingSetting><Name>TEST3</Name><Expression>a+b</Expression><MapAs>Address</MapAs><ProperCase>False</ProperCase></DataImportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST3", mappingSetting.MappingName);
			AssertEquals(-1, mappingSetting.FileColumnIndex);
			AssertEquals("", mappingSetting.DefaultValue);
			AssertEquals("a+b", mappingSetting.Expression);
			AssertEquals("Address", mappingSetting.MapAs);
			AssertEquals(false, mappingSetting.ProperCase);
			AssertEquals("", mappingSetting.CustomMapList);
			AssertNullOrEmpty("Delimiter", mappingSetting.Delimiter);
			AssertEquals(0, mappingSetting.FileColumnIndexOrder.Count);

			xml = "<DataImportWizardMappingSetting><Name>TEST5</Name><Expression>a+b</Expression><MapAs>Address</MapAs><ProperCase>False</ProperCase><Delimiter>|</Delimiter><ColumnIndexOrder>1</ColumnIndexOrder><ColumnIndexOrder>2</ColumnIndexOrder></DataImportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST5", mappingSetting.MappingName);
			AssertEquals(-1, mappingSetting.FileColumnIndex);
			AssertEquals("", mappingSetting.DefaultValue);
			AssertEquals("a+b", mappingSetting.Expression);
			AssertEquals("Address", mappingSetting.MapAs);
			AssertEquals(false, mappingSetting.ProperCase);
			AssertEquals("", mappingSetting.CustomMapList);
			AssertEquals("|", mappingSetting.Delimiter);
			AssertEquals(2, mappingSetting.FileColumnIndexOrder.Count);
			AssertEquals(1, mappingSetting.FileColumnIndexOrder[0]);
			AssertEquals(2, mappingSetting.FileColumnIndexOrder[1]);
		}
	}
}
