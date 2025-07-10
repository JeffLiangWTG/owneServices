namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class DataExportWizardSettingsTest : XmlSerializableTestCase<DataExportWizardSettings>
	{
		public void TestWriteXml()
		{
			var settings = new DataExportWizardSettings
			{
				Delimiter = "|", AppendExtraNewLineToEndOfFile = false, EncodingType = ExportWizard.Constants.EncodingTypes.UTF8,
				TextQualifier = "&",
				FileNameExpression = "\"File\" + obj.Date.ToString(\"yyyy-MM-dd\")",
				Mappings =
				[
					new() { MappingName = "TEST1" },
					new() { MappingName = "TEST2", MapAs = "Description" },
					new()
					{
						MappingName = "TEST3",
						MapAs = "Description3",
						CustomMapList = "CustomMapping",
						Header = "Header3",
						Width = 20,
						Alignment = ExportWizardMapping.FieldAlignment.Right
					},
					new() { MappingName = "TEST4", RowTypeIdentifier = "53F8FD49-C623-4AA1-A790-ADB385BC737D" }
				]
			};

			string result = Serialize(settings);
			Assert(result.Contains("<Delimiter>|</Delimiter>"));
			Assert(result.Contains("<TextQualifier>&amp;</TextQualifier>"));
			Assert(result.Contains("<FilterIndex>0</FilterIndex>"));
			Assert(result.Contains("<IncludeHeaders>True</IncludeHeaders>"));
			Assert(result.Contains("<AppendExtraNewLineToEndOfFile>False</AppendExtraNewLineToEndOfFile>"));
			Assert(result.Contains("<EncodingType>UTF-8</EncodingType>"));
			Assert(result.Contains("<FileNameExpression>\"File\" + obj.Date.ToString(\"yyyy-MM-dd\")</FileNameExpression>"));
			Assert(result.Contains("<Mapping><Name>TEST1</Name><MapAs /><CustomMapList /><Header /><Width>0</Width><Alignment>Left</Alignment><RowTypeName /><Expression /><ConditionExpr /><RowTypeIdentifier /></Mapping>"));
			Assert(result.Contains("<Mapping><Name>TEST2</Name><MapAs>Description</MapAs><CustomMapList /><Header /><Width>0</Width><Alignment>Left</Alignment><RowTypeName /><Expression /><ConditionExpr /><RowTypeIdentifier /></Mapping>"));
			Assert(result.Contains("<Mapping><Name>TEST3</Name><MapAs>Description3</MapAs><CustomMapList>CustomMapping</CustomMapList><Header>Header3</Header><Width>20</Width><Alignment>Right</Alignment><RowTypeName /><Expression /><ConditionExpr /><RowTypeIdentifier /></Mapping>"));
			Assert(result.Contains("<Mapping><Name>TEST4</Name><MapAs /><CustomMapList /><Header /><Width>0</Width><Alignment>Left</Alignment><RowTypeName /><Expression /><ConditionExpr /><RowTypeIdentifier>53F8FD49-C623-4AA1-A790-ADB385BC737D</RowTypeIdentifier></Mapping>"));
		}

		public void TestReadXml()
		{
			string xml = @"
				<DataExportWizardSettings>
					<Mapping><Name>TEST1</Name></Mapping>
					<Mapping><Name>TEST2</Name><MapAs>Code</MapAs></Mapping>
					<Mapping><Name>TEST3</Name><MapAs>Description3</MapAs><CustomMapList>CustomMapping</CustomMapList><Header>Header3</Header></Mapping>
					<Mapping><Name>TEST4</Name><RowTypeIdentifier>53F8FD49-C623-4AA1-A790-ADB385BC737D</RowTypeIdentifier></Mapping>
				</DataExportWizardSettings>";
			DataExportWizardSettings settings = Deserialize(xml);

			AssertEquals(",", settings.Delimiter);
			AssertEquals("\"", settings.TextQualifier);
			AssertEquals(0, settings.FilterIndex);
			AssertEquals(true, settings.IncludeHeaders);
			AssertEquals(true, settings.AppendExtraNewLineToEndOfFile);
			AssertEquals("ASCII", settings.EncodingType);

			AssertEquals(4, settings.Mappings.Count);
			AssertEquals("TEST1", settings.Mappings[0].MappingName);
			AssertEquals("", settings.Mappings[0].MapAs);
			AssertEquals("TEST2", settings.Mappings[1].MappingName);
			AssertEquals("Code", settings.Mappings[1].MapAs);
			AssertEquals("TEST3", settings.Mappings[2].MappingName);
			AssertEquals("Description3", settings.Mappings[2].MapAs);
			AssertEquals("CustomMapping", settings.Mappings[2].CustomMapList);
			AssertEquals("Header3", settings.Mappings[2].Header);
			AssertEquals("53F8FD49-C623-4AA1-A790-ADB385BC737D", settings.Mappings[3].RowTypeIdentifier);

			xml = @"
				<DataExportWizardSettings>
					<Delimiter>&amp;</Delimiter>
					<TextQualifier>'</TextQualifier>
					<FilterIndex>1</FilterIndex>
					<IncludeHeaders>False</IncludeHeaders>
					<AppendExtraNewLineToEndOfFile>False</AppendExtraNewLineToEndOfFile>
					<EncodingType>UTF-8</EncodingType>
					<FileNameExpression>""File"" + obj.Date.ToString(""yyyy-MM-dd"")</FileNameExpression>
				</DataExportWizardSettings>";
			settings = Deserialize(xml);

			AssertEquals("&", settings.Delimiter);
			AssertEquals("\'", settings.TextQualifier);
			AssertEquals(1, settings.FilterIndex);
			AssertEquals(false, settings.IncludeHeaders);
			AssertEquals(false, settings.AppendExtraNewLineToEndOfFile);
			AssertEquals("UTF-8", settings.EncodingType);
			AssertEquals("\"File\" + obj.Date.ToString(\"yyyy-MM-dd\")", settings.FileNameExpression);
			AssertEquals(0, settings.Mappings.Count);
		}
	}
}
