using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualiserDataSetTest : TestCaseWithFactory
	{
		public void TestModifyingWholeDecimalValuesWith3DecimalPlacesDoNotChangeTheDocument()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("A", "X").Z0_AnotherDecimal = 1.000m;
			dummy.Collection.AddNew("B", "X").Z0_AnotherDecimal = 2.000m;
			dummy.Collection.AddNew("C", "Y").Z0_AnotherDecimal = 3.000m;
			dummy.Collection.AddNew("D", "Y").Z0_AnotherDecimal = 4.000m;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<Collection.AnotherDecimal>]   {D}-[<HideRowIf(1==1)>>]
{A}-[#GroupBy:Collection.Description]
{B}-[Total]   {C}-[<Total Collection.AnotherDecimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected =
@"{C}-[1]   {D}-[Enterprise.DocumentEngine.FlexCelInterface.RowHider>]
{C}-[2]   {D}-[Enterprise.DocumentEngine.FlexCelInterface.RowHider>]
{B}-[Total]   {C}-[3]
{C}-[3]   {D}-[Enterprise.DocumentEngine.FlexCelInterface.RowHider>]
{C}-[4]   {D}-[Enterprise.DocumentEngine.FlexCelInterface.RowHider>]
{B}-[Total]   {C}-[7]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Pre-condition: Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();
				documentPack.SaveVisualizerContentNote();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestReportWithVisualizedCollectionUsedWithTotalMacroThatAddsUpAValueThatIsHidden()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A", "X", 1, 1);
			dummy.Collection.AddNew("B", "X", 2, 2);
			dummy.Collection.AddNew("C", "Y", 3, 3);
			dummy.Collection.AddNew("D", "Y", 4, 4);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Code>]   {D}-[<Collection.Z0_Number>]
{A}-[#GroupBy:Collection.Z0_Description]
{B}-[Total]   {C}-[<Total Collection.Z0_Number>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var expected =
@"{B}-[A]
{B}-[B]
{B}-[Total]   {C}-[3]
{B}-[C]
{B}-[D]
{B}-[Total]   {C}-[7]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Pre-condition: Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();
				documentPack.SaveVisualizerContentNote();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestReportWithVisualizedCollectionUsedInMultipleSectionBodyAreasWithTwoGroupBys()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A", "A1");
			dummy.Collection.AddNew("A", "A2");
			dummy.Collection.AddNew("B", "B1");
			dummy.Collection.AddNew("B", "B2");

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{A}-[#GroupBy:Collection.Z0_Description]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code]
{B}-[<Collection.Z0_Code>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Code>: <Collection.Z0_Description>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected =
@"{B}-[A1]
{B}-[A2]
{B}-[A]
{B}-[B1]
{B}-[B2]
{B}-[B]
{B}-[A: A1]
{B}-[A: A2]
{B}-[B: B1]
{B}-[B: B2]
";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Pre-condition: There should be two sections rendered.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				// Overriding data with exactly the same values from real data.
				var tableName = VisualiserDataSet.GetTableName("Collection");
				var dataSet = report.OverridingDataSet;
				var dataTable = dataSet.Tables.Add(tableName);
				dataTable.Columns.Add("Z0_Code");
				dataTable.Columns.Add("Z0_Description");

				foreach (DummyChildBusinessObject item in dummy.Collection)
				{
					dataTable.Rows.Add(item.Z0_Code, item.Z0_Description);
				}

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There should be two sections rendered.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestReportWithVisualizedCollectionUsedInMultipleSectionBodyAreasWithOneGroupBy()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A");
			dummy.Collection.AddNew("B");

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[1: <Collection.Z0_Code>]
{A}-[#GroupBy:Collection.Z0_Code]
{B}-[Grouped]
{A}-[#SectionBody:Data=Collection]
{B}-[2: <Collection.Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected =
@"{B}-[1: A]
{B}-[Grouped]
{B}-[1: B]
{B}-[Grouped]
{B}-[2: A]
{B}-[2: B]
";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Pre-condition: There should be two sections rendered.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				// Overriding data with exactly the same values from real data.
				var tableName = VisualiserDataSet.GetTableName("Collection");
				var dataSet = report.OverridingDataSet;
				var dataTable = dataSet.Tables.Add(tableName);
				dataTable.Columns.Add("Z0_Code");

				foreach (DummyChildBusinessObject item in dummy.Collection)
				{
					dataTable.Rows.Add(item.Z0_Code);
				}

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There should be two sections rendered.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestReportWithVisualizedCollectionAndRelatedBusinessObject()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var childDummy1 = dummy.Collection.AddNew("A", "AAB", 1);
			var relatedDummy = Factory.New<DummyBODocSupportable>();
			relatedDummy.Z0_Number = 30;
			childDummy1.Z0_Guid = relatedDummy.PK;
			var childDummy2 = dummy.Collection.AddNew("B", "BBB", 2);

			AssertNotNull(childDummy1.RelatedDummy);
			AssertNull(childDummy2.RelatedDummy);

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=CollectionWrapper]
{B}-[CollectionWrapper.Int32Number: <CollectionWrapper.Int32Number>]
{B}-[CollectionWrapper.RelatedDummy.Int32Number: <CollectionWrapper.RelatedDummyWrapper.Int32Number>]
{A}-[#GroupBy:CollectionWrapper.Int32Number]
{A}-[#EndOfReport]");

			var expected =
@"{B}-[CollectionWrapper.Int32Number: 1]
{B}-[CollectionWrapper.RelatedDummy.Int32Number: 30]
{B}-[CollectionWrapper.Int32Number: 2]
{B}-[CollectionWrapper.RelatedDummy.Int32Number: ]
";

			template.SO_DataContext = "UnitTest";
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				DeliverableCollectionView view = new DeliverableCollectionView(documentPack, new DocDeliveryContactCollection(new BusinessObjectFactory()));

				DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(documentPack, view);
				TemplateToVisualiserComponentsConverter convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());
				List<VisualiserComponent> components = convertor.Components;
				AssertNotNull("Has a grid", components.Find(delegate(VisualiserComponent match)
				{ return match is VisualiserComponentGrid; }));

				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertMultilineASCIIEquals("", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestVisualisedDataSetHasSameSchemaWithReportDataSource()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_AnotherDate = ZDateTime.Now;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=CollectionWrapper]
{B}-[DateTime: <CollectionWrapper.Now>]
{B}-[DateTime: <CollectionWrapper.NowOffset>]
{A}-[#EndOfReport]");

			template.SO_DataContext = "UnitTest";
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				DeliverableCollectionView view = new DeliverableCollectionView(documentPack, new DocDeliveryContactCollection(new BusinessObjectFactory()));

				DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(documentPack, view);
				TemplateToVisualiserComponentsConverter convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());
				List<VisualiserComponent> components = convertor.Components;

				AssertEquals(2, report.OverridingDataSet.Tables.Count);

				var wrapperDataTable = report.OverridingDataSet.Tables[1];
				AssertEquals("VisualiserTable_CollectionWrapper", wrapperDataTable.TableName);

				var columnNow = wrapperDataTable.Columns["Now"];
				AssertNotNull(columnNow);
				AssertEquals(typeof(DateTime), columnNow.DataType);

				var columnNowOffset = wrapperDataTable.Columns["NowOffset"];
				AssertNotNull(columnNowOffset);
				AssertEquals(typeof(DateTime), columnNowOffset.DataType);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGridStylesForDecimals()
		{
			StmMenuItem menuCommand = Factory.New<StmMenuItem>();
			menuCommand.SU_MenuName = "DecimalTest";
			var pack = new DocumentPack(menuCommand);
			DeliverableCollectionView view = new DeliverableCollectionView(pack, new DocDeliveryContactCollection(new BusinessObjectFactory()));

			DummyBusinessObject topLevelDataSource = GetNewBizObjDataSource();
			VisualiserDataSet boundDataSet = new VisualiserDataSet();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DecimalGridStyles.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				pack.Add(report);
				pack.Add(new DummyDeliverable());
				DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(pack, view);
				TemplateToVisualiserComponentsConverter convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());
				List<VisualiserComponent> components = convertor.Components;
				AssertNotNull("Has a grid", components.Find(delegate(VisualiserComponent match)
				{ return match is VisualiserComponentGrid; }));

				AssertEquals("Should have 1 grid style from 1 decimal column", 1, report.OverridingDataSet.VisualiserGridStyles.Count);

				report.Factory.Save();
				pack.SaveVisualizerContentNote();
				Factory.Save();
			}

			pack = new DocumentPack(menuCommand);

			excelTemplate = new ExcelTemplateForUnitTesting("DecimalGridStyles.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				pack.Add(report);
				pack.Add(new DummyDeliverable());
				DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(pack, view);
				TemplateToVisualiserComponentsConverter convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());
				List<VisualiserComponent> components = convertor.Components;
				AssertNotNull("Has a grid", components.Find(delegate(VisualiserComponent match)
				{ return match is VisualiserComponentGrid; }));

				AssertEquals("Should have 1 grid style from 1 decimal column", 1, report.OverridingDataSet.VisualiserGridStyles.Count);
			}
		}

		public void TestSerialiseXMLSchemaProperlyStoresBooleanValueTypes()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Boolean", typeof(bool));
			dS.MainTable.Columns.Add("String", typeof(string));
			dS.MainTable.Columns.Add("Decimal", typeof(string));
			dS.MainRow["Boolean"] = true;
			dS.MainRow["String"] = "this is a string field";
			dS.MainRow["Decimal"] = 34.12m;
			using (StringWriter sW = new StringWriter())
			{
				foreach (byte b in dS.SerialiseToByteArray())
				{
					sW.Write((char)b);
				}

				AssertMultilineASCIIEquals("SW.ToString()", SerialiseXMLSchemaProperlyStoresBooleanValueTypesExpectedValue, sW.ToString());
			}
		}
		#region SerialiseXMLSchemaProperlyStoresBooleanValueTypesExpectedValue
		const string SerialiseXMLSchemaProperlyStoresBooleanValueTypesExpectedValue = @"<VisualiserDataSet>
  <xs:schema id=""VisualiserDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""VisualiserDataSet"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
          <xs:element name=""MainBizObject"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Boolean"" type=""xs:boolean"" minOccurs=""0"" />
                <xs:element name=""String"" type=""xs:string"" minOccurs=""0"" />
                <xs:element name=""Decimal"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <MainBizObject>
    <Boolean>true</Boolean>
    <String>this is a string field</String>
    <Decimal>34.12</Decimal>
  </MainBizObject>
</VisualiserDataSet>";
		#endregion

		public void TestDeSerialiseFromXMLUsesSchemaAndSetsBooleanValueTypes()
		{
			using (VisualiserDataSet visualiserData = new VisualiserDataSet())
			{
				visualiserData.DeSerialise(SerialiseXMLSchemaProperlyStoresBooleanValueTypesExpectedValue);
				DataTable mainBizObjTable = visualiserData.Tables[VisualiserDataSet.MainBusinessObjectTableName];
				AssertNotNull(mainBizObjTable);
				AssertEquals("mainBizObjTable.Columns.Count", 3, mainBizObjTable.Columns.Count);
				AssertEquals("mainBizObjTable.Columns[0].DataType", typeof(bool), mainBizObjTable.Columns[0].DataType);
				AssertEquals("mainBizObjTable.Columns[1].DataType", typeof(string), mainBizObjTable.Columns[1].DataType);
				AssertEquals("mainBizObjTable.Columns[2].DataType", typeof(string), mainBizObjTable.Columns[2].DataType);
			}
		}

		public void TestDeletingRowFromTableRemovesTableFromUnTouchedTables()
		{
			VisualiserDataSetForTesting dataSet = new VisualiserDataSetForTesting();

			AssertEquals("Precondition: dataSet.UnTouchedTableNames.Count", 0, dataSet.UnTouchedTableNames.Count);

			DataTable tableWithRows = dataSet.Tables.Add("TableWithRows");
			tableWithRows.Columns.Add("ColumnOne");
			tableWithRows.Columns.Add("ColumnTwo");

			tableWithRows.Rows.Add("foo", "bar");
			tableWithRows.Rows.Add("hello", "world");
			tableWithRows.Rows.Add("turn", "around");

			AssertEquals("dataSet.UnTouchedTableNames.Count", 1, dataSet.UnTouchedTableNames.Count);

			tableWithRows.Rows[1].Delete();

			AssertEquals("dataSet.UnTouchedTableNames.Count", 0, dataSet.UnTouchedTableNames.Count);
		}

		public void TestDeSerialiseChildTableWithNoMainObjectData()
		{
			using (VisualiserDataSet visualiserData = new VisualiserDataSet())
			{
				visualiserData.DeSerialise(XMLForDeSerialiseChildTableWithNoMainObjectDataExpectedValue);
				AssertEquals("visualiserData.Tables.Count", 2, visualiserData.Tables.Count);
				AssertEquals("visualiserData.Tables[1].TableName", VisualiserDataSet.MainBusinessObjectTableName, visualiserData.Tables[1].TableName);
				DataTable linesTable = visualiserData.Tables[0];
				AssertEquals("linesTable.TableName", "VisualiserTable_CommercialInvoiceLines", linesTable.TableName);
				AssertEquals("linesTable.Columns.Count", 11, linesTable.Columns.Count);
				AssertEquals("linesTable.Rows.Count", 10, linesTable.Rows.Count);
				AssertEquals("linesTable.Rows[9]", "TARRED BITUMINISED PAPER ETC Mofieid", linesTable.Rows[9]["Description"]);
			}
		}
		#region XMLForDeSerialiseChildTableWithNoMainObjectDataExpectedValue
		const string XMLForDeSerialiseChildTableWithNoMainObjectDataExpectedValue = @"
<VisualiserDataSet>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>CIGARETTE PAPER IN BOOKLETS ETC</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>45.00</Amount>
    <OrderNumber />
    <Invoice>2FUK</Invoice>
    <InvoiceInvoiceNumber>2FUK</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>688.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>OTHER SELF ADHESIVE PAPER &amp; PAPERBOARD ETC, IN STRIPS OR ROLLS</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>537.00</Amount>
    <OrderNumber />
    <Invoice>2FUK</Invoice>
    <InvoiceInvoiceNumber>2FUK</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>688.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>COASTERS</Description>
    <CountryOfOriginCode>AU</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>400.00</Amount>
    <OrderNumber />
    <Invoice>1BUNG</Invoice>
    <InvoiceInvoiceNumber>1BUNG</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>712.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>CEMENT BOARD</Description>
    <CountryOfOriginCode>AU</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>43.00</Amount>
    <OrderNumber />
    <Invoice>1BUNG</Invoice>
    <InvoiceInvoiceNumber>1BUNG</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>712.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>PRINTING/WRITING ROLLS, PRINTED FOR SELF-RECORDING APPARATUS</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>65.00</Amount>
    <OrderNumber />
    <Invoice>1BUNG</Invoice>
    <InvoiceInvoiceNumber>1BUNG</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>712.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>DIARIES</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>23.00</Amount>
    <OrderNumber />
    <Invoice>1BUNG</Invoice>
    <InvoiceInvoiceNumber>1BUNG</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>712.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>FILTER BLOCKS SLABS ETC</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>68.00</Amount>
    <OrderNumber />
    <Invoice>2FUK</Invoice>
    <InvoiceInvoiceNumber>2FUK</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>688.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>BOBBINS SPOOLS ETC FOR WINDING TEXTILE YARN</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>38.00</Amount>
    <OrderNumber />
    <Invoice>2FUK</Invoice>
    <InvoiceInvoiceNumber>2FUK</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>688.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>SELF-ADHESIVE PRINTED LABELS</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>94.00</Amount>
    <OrderNumber />
    <Invoice>1BUNG</Invoice>
    <InvoiceInvoiceNumber>1BUNG</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>712.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
  <VisualiserTable_CommercialInvoiceLines>
    <PartNumber />
    <Description>TARRED BITUMINISED PAPER ETC Mofieid</Description>
    <CountryOfOriginCode>GB</CountryOfOriginCode>
    <LineQuantity />
    <UnitPriceAmount>0</UnitPriceAmount>
    <Amount>87.00</Amount>
    <OrderNumber />
    <Invoice>1BUNG</Invoice>
    <InvoiceInvoiceNumber>1BUNG</InvoiceInvoiceNumber>
    <InvoiceIncoTermCode>FOB</InvoiceIncoTermCode>
    <InvoiceInvoiceAmount>712.00 NZD</InvoiceInvoiceAmount>
  </VisualiserTable_CommercialInvoiceLines>
</VisualiserDataSet>
";
		#endregion

		public void TestGetColumnName()
		{
			AssertEquals("SmellyFartTexture", VisualiserDataSet.GetColumnName("Rotten.SmellyFart.Texture", "Rotten"));
			AssertEquals("Texture", VisualiserDataSet.GetColumnName("SmellyFart.Texture", "SmellyFart"));
			AssertEquals("SmellyFart", VisualiserDataSet.GetColumnName("SmellyFart", string.Empty));

			AssertEquals("SmellyFart", VisualiserDataSet.GetColumnName("Format(\"{SmellyFart}\")", string.Empty));
			AssertEquals("Total{SmellyFart}", VisualiserDataSet.GetColumnName("Total(\"{SmellyFart}\")", string.Empty));
			AssertEquals("SmellyFart", VisualiserDataSet.GetColumnName("GetDocDataValue(\"SmellyFart\")", string.Empty));

			AssertEquals("ContainerNo", VisualiserDataSet.GetColumnName("Packages.ContainerNo", "Packages"));
			AssertEquals("ContainerTypeCode", VisualiserDataSet.GetColumnName("Packages.Container.Type.Code", "Packages"));
			AssertEquals("ContainerVolumeGoodsUnitCode", VisualiserDataSet.GetColumnName("Packages.Container.VolumeGoods.Unit.Code", "Packages"));
			AssertEquals("ContainerAirVentFlowValueAndUnitCode", VisualiserDataSet.GetColumnName("Packages.Container.AirVentFlow.ValueAndUnitCode", "Packages"));
			AssertEquals("ContainerVolumeGoodsValue", VisualiserDataSet.GetColumnName("Packages.Container.VolumeGoods.Format(\"{Value:T3}\")", "Packages"));
			AssertEquals("ContainerWeightGoodsValue", VisualiserDataSet.GetColumnName("Packages.Container.WeightGoods.Format(\"{Value:T3}\")", "Packages"));
			AssertEquals("ContainerWeightTareValue", VisualiserDataSet.GetColumnName("Packages.Container.WeightTare.Format(\"{Value:T3}\")", "Packages"));
		}

		public void TestConstructorMakesMainTable()
		{
			VisualiserDataSet ds = new VisualiserDataSet();
			Assert(ds.Tables.Contains(VisualiserDataSet.MainBusinessObjectTableName));
		}

		public void TestMainTableHasOneRow()
		{
			VisualiserDataSet ds = new VisualiserDataSet();
			AssertEquals(1, ds.MainTable.Rows.Count);
		}

		public void TestMainRow()
		{
			VisualiserDataSet ds = new VisualiserDataSet();
			AssertSame(ds.MainRow, ds.MainTable.Rows[0]);
		}

		public void TestDeSerialiseFromXML()
		{
			string xml = @"<VisualiserDataSet>
  <MainBizObject>
    <FortyTwo>42</FortyTwo>
    <Another>yet another field</Another>
  </MainBizObject>
</VisualiserDataSet>";

			VisualiserDataSet dS = new VisualiserDataSet();

			AssertEquals(0, dS.MainTable.Columns.Count);

			dS.DeSerialise(xml);

			AssertEquals(2, dS.MainTable.Columns.Count);
			AssertEquals("42", dS.MainRow["FortyTwo"]);
			AssertEquals("yet another field", dS.MainRow["Another"]);
		}

		public void TestSerialiseToByteArray()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("FortyTwo");
			dS.MainTable.Columns.Add("Another");
			dS.MainRow["FortyTwo"] = "42";
			dS.MainRow["Another"] = "yet another field";
			using (StringWriter sW = new StringWriter())
			{
				foreach (byte b in dS.SerialiseToByteArray())
				{
					sW.Write((char)b);
				}

				AssertMultilineASCIIEquals("SW.ToString()", SerialiseToByteArrayExpectedValue, sW.ToString());
			}
		}
		#region SerialiseToByteArrayExpectedValue
		const string SerialiseToByteArrayExpectedValue = @"<VisualiserDataSet>
  <xs:schema id=""VisualiserDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""VisualiserDataSet"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
          <xs:element name=""MainBizObject"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""FortyTwo"" type=""xs:string"" minOccurs=""0"" />
                <xs:element name=""Another"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <MainBizObject>
    <FortyTwo>42</FortyTwo>
    <Another>yet another field</Another>
  </MainBizObject>
</VisualiserDataSet>";
		#endregion

		public void TestSerialiseOnlyKeepsChangedData()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			using (dS.SuspendChangeTracking)
			{
				dS.MainTable.Columns.Add("FortyTwo");
				dS.MainRow["FortyTwo"] = "42";
			}
			dS.MainTable.Columns.Add("Another");
			dS.MainRow["Another"] = "yet another field";

			string addedTable1Name = VisualiserDataSet.GetTableName("Test1");
			dS.Tables.Add(addedTable1Name);
			dS.Tables[addedTable1Name].Columns.Add("Fld1");
			dS.Tables[addedTable1Name].Rows.Add(new object[] { "" });
			dS.Tables[addedTable1Name].Rows.Add(new object[] { "" });
			dS.Tables[addedTable1Name].Rows[0]["Fld1"] = "Val1";
			dS.Tables[addedTable1Name].Rows[1]["Fld1"] = "Val2";

			using (dS.SuspendChangeTracking)
			{
				string addedTable2Name = VisualiserDataSet.GetTableName("Test2");
				dS.Tables.Add(addedTable2Name);
				dS.Tables[addedTable2Name].Columns.Add("Fld1");
				dS.Tables[addedTable2Name].Rows.Add(new object[] { "" });
				dS.Tables[addedTable2Name].Rows.Add(new object[] { "" });
				dS.Tables[addedTable2Name].Rows[0]["Fld1"] = "Val1";
				dS.Tables[addedTable2Name].Rows[1]["Fld1"] = "Val2";
			}

			using (StringWriter sW = new StringWriter())
			{
				foreach (byte b in dS.SerialiseToByteArray())
				{
					sW.Write((char)b);
				}

				AssertMultilineASCIIEquals("SW.ToString()", SerialiseOnlyKeepsChangedDataExpectedValue, sW.ToString());
			}
		}
		#region SerialiseOnlyKeepsChangedDataExpectedValue
		const string SerialiseOnlyKeepsChangedDataExpectedValue = @"<VisualiserDataSet>
  <xs:schema id=""VisualiserDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""VisualiserDataSet"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
          <xs:element name=""MainBizObject"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Another"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
          <xs:element name=""VisualiserTable_Test1"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Fld1"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <MainBizObject>
    <Another>yet another field</Another>
  </MainBizObject>
  <VisualiserTable_Test1>
    <Fld1>Val1</Fld1>
  </VisualiserTable_Test1>
  <VisualiserTable_Test1>
    <Fld1>Val2</Fld1>
  </VisualiserTable_Test1>
</VisualiserDataSet>";
		#endregion

		public void TestDeSerialiseKeepsChangedDataSettings()
		{
			#region Xml
			string xml = @"<VisualiserDataSet>
  <MainBizObject>
    <Another>yet another field</Another>
  </MainBizObject>
  <VisualiserTable_Test1>
    <Fld1>Val1</Fld1>
  </VisualiserTable_Test1>
  <VisualiserTable_Test1>
    <Fld1>Val2</Fld1>
  </VisualiserTable_Test1>
</VisualiserDataSet>";
			#endregion

			VisualiserDataSet dS = new VisualiserDataSet();
			dS.DeSerialise(xml);

			dS.MainTable.Columns.Add("FortyTwo");
			dS.MainRow["FortyTwo"] = "42";

			string addedTable2Name = VisualiserDataSet.GetTableName("Test2");
			dS.Tables.Add(addedTable2Name);
			dS.Tables[addedTable2Name].Columns.Add("Fld1");
			dS.Tables[addedTable2Name].Rows.Add(new object[] { "" });
			dS.Tables[addedTable2Name].Rows.Add(new object[] { "" });
			dS.Tables[addedTable2Name].Rows[0]["Fld1"] = "Val1";
			dS.Tables[addedTable2Name].Rows[1]["Fld1"] = "Val2";

			using (StringWriter sW = new StringWriter())
			{
				foreach (byte b in dS.SerialiseToByteArray())
				{
					sW.Write((char)b);
				}

				AssertMultilineASCIIEquals("SW.ToString()", DeSerialiseKeepsChangedDataSettingsExpectedValue, sW.ToString());
			}
		}
		#region DeSerialiseKeepsChangedDataSettingsExpectedValueExpectedValue
		const string DeSerialiseKeepsChangedDataSettingsExpectedValue = @"<VisualiserDataSet>
  <xs:schema id=""VisualiserDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""VisualiserDataSet"" msdata:IsDataSet=""true"" msdata:Locale=""en-US"">
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
          <xs:element name=""MainBizObject"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Another"" type=""xs:string"" minOccurs=""0"" />
                <xs:element name=""FortyTwo"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
          <xs:element name=""VisualiserTable_Test1"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Fld1"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
          <xs:element name=""VisualiserTable_Test2"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Fld1"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <MainBizObject>
    <Another>yet another field</Another>
    <FortyTwo>42</FortyTwo>
  </MainBizObject>
  <VisualiserTable_Test1>
    <Fld1>Val1</Fld1>
  </VisualiserTable_Test1>
  <VisualiserTable_Test1>
    <Fld1>Val2</Fld1>
  </VisualiserTable_Test1>
  <VisualiserTable_Test2>
    <Fld1>Val1</Fld1>
  </VisualiserTable_Test2>
  <VisualiserTable_Test2>
    <Fld1>Val2</Fld1>
  </VisualiserTable_Test2>
</VisualiserDataSet>";
		#endregion

		public void TestClone()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			using (dS.SuspendChangeTracking)
			{
				dS.MainTable.Columns.Add("FortyTwo");
				dS.MainRow["FortyTwo"] = "42";
			}
			dS.MainTable.Columns.Add("Another");
			dS.MainRow["Another"] = "yet another field";

			string addedTable1Name = VisualiserDataSet.GetTableName("Test1");
			dS.Tables.Add(addedTable1Name);
			dS.Tables[addedTable1Name].Columns.Add("Fld1");
			dS.Tables[addedTable1Name].Rows.Add(new object[] { "" });
			dS.Tables[addedTable1Name].Rows.Add(new object[] { "" });
			dS.Tables[addedTable1Name].Rows[0]["Fld1"] = "Val1";
			dS.Tables[addedTable1Name].Rows[1]["Fld1"] = "Val2";

			using (dS.SuspendChangeTracking)
			{
				string addedTable2Name = VisualiserDataSet.GetTableName("Test2");
				dS.Tables.Add(addedTable2Name);
				dS.Tables[addedTable2Name].Columns.Add("Fld1");
				dS.Tables[addedTable2Name].Rows.Add(new object[] { "" });
				dS.Tables[addedTable2Name].Rows.Add(new object[] { "" });
				dS.Tables[addedTable2Name].Rows[0]["Fld1"] = "Val1";
				dS.Tables[addedTable2Name].Rows[1]["Fld1"] = "Val2";
			}

			var cloned = dS.Clone();
			AssertEquals(2, cloned.MainTable.Columns.Count);
			Assert(cloned.MainTable.Columns.Contains("FortyTwo"));
			Assert(cloned.MainTable.Columns.Contains("Another"));
			using (StringWriter sW = new StringWriter())
			{
				foreach (byte b in cloned.SerialiseToByteArray())
				{
					sW.Write((char)b);
				}

				AssertMultilineASCIIEquals("SW.ToString()", CloneExpectedValue, sW.ToString());
			}
		}
		#region CloneExpectedValue
		const string CloneExpectedValue = @"<VisualiserDataSet>
  <xs:schema id=""VisualiserDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""VisualiserDataSet"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
          <xs:element name=""MainBizObject"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Another"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
          <xs:element name=""VisualiserTable_Test1"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Fld1"" type=""xs:string"" minOccurs=""0"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <MainBizObject>
    <Another>yet another field</Another>
  </MainBizObject>
  <VisualiserTable_Test1>
    <Fld1>Val1</Fld1>
  </VisualiserTable_Test1>
  <VisualiserTable_Test1>
    <Fld1>Val2</Fld1>
  </VisualiserTable_Test1>
</VisualiserDataSet>";
		#endregion

		public void TestGetTableName()
		{
			AssertEquals("VisualiserTable_Blah", VisualiserDataSet.GetTableName("Blah"));
			AssertEquals("VisualiserTable_Blah_Blah", VisualiserDataSet.GetTableName("Blah.Blah"));
		}

		public void TestGetCollectionName()
		{
			AssertEquals("Blah", VisualiserDataSet.GetCollectionName("VisualiserTable_Blah"));
			AssertEquals("Blah_Blah", VisualiserDataSet.GetCollectionName("VisualiserTable_Blah_Blah"));
		}

		#region Implementation
		class VisualiserDataSetForTesting : VisualiserDataSet
		{
			public new List<string> UnTouchedTableNames
			{
				get { return base.UnTouchedTableNames; }
			}
		}

		DummyBusinessObject GetNewBizObjDataSource()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			List<ZBool> zBools = new List<ZBool>();
			zBools.Add(new ZBool(true));
			zBools.Add(new ZBool(false));
			List<ZDecimal> zDecimals = new List<ZDecimal>();
			zDecimals.Add(new ZDecimal(3.14m));
			zDecimals.Add(new ZDecimal(0.04m));
			zDecimals.Add(new ZDecimal(9.872m));
			List<ZString> zStrings = new List<ZString>();
			zStrings.Add(new ZString("foo"));
			zStrings.Add(new ZString("bar"));
			zStrings.Add(new ZString("hello"));
			zStrings.Add(new ZString("world"));

			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[2], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[1], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[1], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[1], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[1], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[2], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[1], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[2], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[1], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[0], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[2], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			return topLevelDataSource;
		}

		DummyChildBusinessObject AddChildToDummyObject(DummyBusinessObject topLevelDataSource, ZBool valueBool, ZDecimal valueDecimal, ZString valueNText)
		{
			DummyChildBusinessObject result = topLevelDataSource.Collection.AddNew();
			result.Z0_Bool = valueBool;
			result.Z0_Decimal = valueDecimal;
			result.Z0_NVarCharMax = valueNText;
			return result;
		}

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplateForUnitTesting excelTemplate)
		{
			return new Report(new DocumentPack(Factory.New<DocumentCommand>()), excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}
		#endregion
	}
}
