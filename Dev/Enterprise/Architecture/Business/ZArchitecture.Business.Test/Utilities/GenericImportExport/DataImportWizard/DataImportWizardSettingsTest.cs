namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DataImportWizardSettingsTest : XmlSerializableTestCase<DataImportWizardSettings>
	{
		public void TestWriteXml()
		{
			DataImportWizardSettings settings = new DataImportWizardSettings
			{
				Delimiter = "|", TextQualifier = "&", StartingRow = 5, Mappings =
				[
					new DataImportWizardMappingSetting() { MappingName = "TEST1", FileColumnIndex = 1, DefaultValue = "DEF1", UpdateExisting = true, Delimiter = ".", FileColumnIndexOrder = [0, 1] },
					new DataImportWizardMappingSetting() { MappingName = "TEST2", FileColumnIndex = 2, DefaultValue = "DEF2", ProperCase = true, Delimiter = "|", FileColumnIndexOrder = [1, 2] },
					new DataImportWizardMappingSetting() { MappingName = "TEST3", FileColumnIndex = 3, DefaultValue = "DEF3", WesternCharactersOnly = true, Delimiter = ".", FileColumnIndexOrder = [0, 1] }
				]
			};

			var pairList1 = new CustomMapPairListSetting { Name = "L1", Pairs =
				[
					new CustomMapPairSetting() { Input = "A", Output = "1" },
					new CustomMapPairSetting()  { Input = "B", Output = "2" }
				]
			};
			var pairList2 = new CustomMapPairListSetting { Name = "L2", Pairs = [new CustomMapPairSetting() { Input = "Z", Output = "26" }] };
			settings.CustomMapLists = [pairList1, pairList2];

			string result = Serialize(settings);
			Assert(result.Contains("<Delimiter>|</Delimiter>"));
			Assert(result.Contains("<TextQualifier>&amp;</TextQualifier>"));
			Assert(result.Contains("<StartingRow>5</StartingRow>"));
			Assert(result.Contains("<Mapping><ColumnIndex>1</ColumnIndex><Name>TEST1</Name><DefaultValue>DEF1</DefaultValue><Expression /><MapAs /><ProperCase>False</ProperCase><UpdateExisting>True</UpdateExisting><CustomMapList /><Delimiter>.</Delimiter><ColumnIndexOrder>0</ColumnIndexOrder><ColumnIndexOrder>1</ColumnIndexOrder><WesternCharactersOnly>False</WesternCharactersOnly></Mapping>"));
			Assert(result.Contains("<Mapping><ColumnIndex>2</ColumnIndex><Name>TEST2</Name><DefaultValue>DEF2</DefaultValue><Expression /><MapAs /><ProperCase>True</ProperCase><UpdateExisting>False</UpdateExisting><CustomMapList /><Delimiter>|</Delimiter><ColumnIndexOrder>1</ColumnIndexOrder><ColumnIndexOrder>2</ColumnIndexOrder><WesternCharactersOnly>False</WesternCharactersOnly></Mapping>"));
			Assert(result.Contains("<Mapping><ColumnIndex>3</ColumnIndex><Name>TEST3</Name><DefaultValue>DEF3</DefaultValue><Expression /><MapAs /><ProperCase>False</ProperCase><UpdateExisting>False</UpdateExisting><CustomMapList /><Delimiter>.</Delimiter><ColumnIndexOrder>0</ColumnIndexOrder><ColumnIndexOrder>1</ColumnIndexOrder><WesternCharactersOnly>True</WesternCharactersOnly></Mapping>"));
			Assert(result.Contains("<CustomMapList><Name>L1</Name><Pair><Input>A</Input><Output>1</Output></Pair><Pair><Input>B</Input><Output>2</Output></Pair></CustomMapList>"));
			Assert(result.Contains("<CustomMapList><Name>L2</Name><Pair><Input>Z</Input><Output>26</Output></Pair></CustomMapList>"));
		}

		public void TestReadXml()
		{
			string xml = @"
				<DataImportWizardSettings>
					<Mapping><Name>TEST1</Name><ColumnIndex>1</ColumnIndex><DefaultValue>DEF1</DefaultValue><Delimiter>.</Delimiter><ColumnIndexOrder>0</ColumnIndexOrder><ColumnIndexOrder>1</ColumnIndexOrder><WesternCharactersOnly>True</WesternCharactersOnly></Mapping>
					<Mapping><Name>TEST2</Name><Expression>d*e</Expression><DefaultValue>DEF2</DefaultValue><UpdateExisting>True</UpdateExisting><Delimiter>|</Delimiter><ColumnIndexOrder>1</ColumnIndexOrder><ColumnIndexOrder>2</ColumnIndexOrder></Mapping>
				</DataImportWizardSettings>";
			DataImportWizardSettings settings = Deserialize(xml);

			AssertEquals(",", settings.Delimiter);
			AssertEquals("\"", settings.TextQualifier);
			AssertEquals(1, settings.StartingRow);

			AssertEquals(2, settings.Mappings.Count);
			AssertEquals("TEST1", settings.Mappings[0].MappingName);
			AssertEquals(1, settings.Mappings[0].FileColumnIndex);
			AssertEquals("DEF1", settings.Mappings[0].DefaultValue);
			AssertEquals(false, settings.Mappings[0].UpdateExisting);
			AssertEquals(true, settings.Mappings[0].WesternCharactersOnly);
			AssertEquals("", settings.Mappings[0].Expression);
			AssertEquals(".", settings.Mappings[0].Delimiter);
			AssertEquals(2, settings.Mappings[0].FileColumnIndexOrder.Count);
			AssertEquals(0, settings.Mappings[0].FileColumnIndexOrder[0]);
			AssertEquals(1, settings.Mappings[0].FileColumnIndexOrder[1]);
			AssertEquals("TEST2", settings.Mappings[1].MappingName);
			AssertEquals(-1, settings.Mappings[1].FileColumnIndex);
			AssertEquals("DEF2", settings.Mappings[1].DefaultValue);
			AssertEquals(true, settings.Mappings[1].UpdateExisting);
			AssertEquals(false, settings.Mappings[1].WesternCharactersOnly);
			AssertEquals("d*e", settings.Mappings[1].Expression);
			AssertEquals(2, settings.Mappings[1].FileColumnIndexOrder.Count);
			AssertEquals(1, settings.Mappings[1].FileColumnIndexOrder[0]);
			AssertEquals(2, settings.Mappings[1].FileColumnIndexOrder[1]);

			AssertEquals(0, settings.CustomMapLists.Count);

			xml = @"
				<DataImportWizardSettings>
					<Delimiter>&amp;</Delimiter>
					<TextQualifier>'</TextQualifier>
					<StartingRow>2</StartingRow>
					<CustomMapList><Name>L1</Name><Pair><Input>X</Input><Output>24</Output></Pair></CustomMapList>
					<CustomMapList><Name>L2</Name><Pair><Input>Y</Input><Output>25</Output></Pair></CustomMapList>
				</DataImportWizardSettings>";
			settings = Deserialize(xml);

			AssertEquals("&", settings.Delimiter);
			AssertEquals("\'", settings.TextQualifier);
			AssertEquals(2, settings.StartingRow);
			AssertEquals(0, settings.Mappings.Count);

			AssertEquals(2, settings.CustomMapLists.Count);
			AssertEquals("L1", settings.CustomMapLists[0].Name);
			AssertEquals(1, settings.CustomMapLists[0].Pairs.Count);
			AssertEquals("X", settings.CustomMapLists[0].Pairs[0].Input);
			AssertEquals("24", settings.CustomMapLists[0].Pairs[0].Output);
			AssertEquals("L2", settings.CustomMapLists[1].Name);
			AssertEquals(1, settings.CustomMapLists[1].Pairs.Count);
			AssertEquals("Y", settings.CustomMapLists[1].Pairs[0].Input);
			AssertEquals("25", settings.CustomMapLists[1].Pairs[0].Output);
		}
	}
}
