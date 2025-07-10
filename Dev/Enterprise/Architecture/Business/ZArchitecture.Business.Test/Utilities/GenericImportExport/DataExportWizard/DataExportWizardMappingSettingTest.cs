namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DataExportWizardMappingSettingTest : XmlSerializableTestCase<DataExportWizardMappingSetting>
	{
		public void TestWriteXml()
		{
			DataExportWizardMappingSetting mappingSetting = new DataExportWizardMappingSetting();
			mappingSetting.MappingName = "TEST";
			mappingSetting.MapAs = "Description";
			string result = Serialize(mappingSetting);
			Assert(result.Contains("<Name>TEST</Name>"));
			Assert(result.Contains("<MapAs>Description</MapAs>"));
		}

		public void TestReadXml()
		{
			string xml = "<DataExportWizardMappingSetting><Name>TEST1</Name></DataExportWizardMappingSetting>";
			DataExportWizardMappingSetting mappingSetting = Deserialize(xml);
			AssertEquals("TEST1", mappingSetting.MappingName);
			AssertEquals("", mappingSetting.MapAs);

			xml = "<DataExportWizardMappingSetting><Crap1>qwerty</Crap1><Name>TEST2</Name><Crap2>asdfgh</Crap2><MapAs>Desc</MapAs><Crap3/></DataExportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST2", mappingSetting.MappingName);
			AssertEquals("Desc", mappingSetting.MapAs);

			xml = "<DataExportWizardMappingSetting><Name>TEST3</Name><MapAs>Code</MapAs></DataExportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST3", mappingSetting.MappingName);
			AssertEquals("Code", mappingSetting.MapAs);

			xml = "<DataExportWizardMappingSetting><Name>TEST3</Name><MapAs>Code</MapAs><Width>30</Width><Alignment>Right</Alignment></DataExportWizardMappingSetting>";
			mappingSetting = Deserialize(xml);
			AssertEquals("TEST3", mappingSetting.MappingName);
			AssertEquals("Code", mappingSetting.MapAs);
			AssertEquals(30, mappingSetting.Width);
			AssertEquals(ExportWizardMapping.FieldAlignment.Right, mappingSetting.Alignment);
		}
	}
}
