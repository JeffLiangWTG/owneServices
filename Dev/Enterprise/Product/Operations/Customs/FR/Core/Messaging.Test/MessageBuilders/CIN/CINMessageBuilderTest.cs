using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessagesWrappers.CIN;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN.Testing
{
	class CINMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestWarehouseMovementIn_NOTCIN()
		{
			var jobHeader = CreateTestJobHeader();
			var messageId = "UT-MOVE-IN-001";

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var builder = CINImportMessageBuilder.Create(headerWrapper, null, messageId, CINImportMessageBuilder.MessageBuilderType.MovementIn);

			var msg = builder.GetMessage();
			var expectedMsg = @"<Message>
  <EnvelopeMessage>
    <schemaID>750</schemaID>
    <schemaVersion>XML</schemaVersion>
    <transactionID>0000000001</transactionID>
  </EnvelopeMessage>
  <CinMessage type=""WarehouseMovement-In"">
    <Header from=""PUT-CIN-ID-HERE"" to=""CIN"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""UT-MOVE-IN-001"" />
    <WarehouseMovementIn>
      <movementTime>2011-12-13T14:15:16.017Z</movementTime>
      <declaredIn code=""ORIGINAL"" label=""ORIGINAL"" />
      <from code=""NOTCIN"" label=""NOTCIN"" />
      <goods>
        <ref type=""AWB"" code=""UNITTEST"" />
        <amount quantity=""25"" weight=""50.000"" />
        <description>Original Goods</description>
        <totalRefAmount quantity=""25"" weight=""50.000"" />
      </goods>
      <customsStatus></customsStatus>
    </WarehouseMovementIn>
  </CinMessage>
</Message>";

			AssertContains(expectedMsg, msg);
		}

		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestWarehouseMovementIn_CIN()
		{
			var jobHeader = CreateTestJobHeader();
			var messageId = "UT-MOVE-IN-001";

			jobHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_DestinationPlace = "ABC";

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var builder = CINImportMessageBuilder.Create(headerWrapper, null, messageId, CINImportMessageBuilder.MessageBuilderType.MovementIn);

			var msg = builder.GetMessage();

			var expectedMsg = @"<Message>
  <EnvelopeMessage>
    <schemaID>750</schemaID>
    <schemaVersion>XML</schemaVersion>
    <transactionID>0000000001</transactionID>
  </EnvelopeMessage>
  <CinMessage type=""WarehouseMovement-In"">
    <Header from=""PUT-CIN-ID-HERE"" to=""CIN"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""UT-MOVE-IN-001"" />
    <WarehouseMovementIn>
      <movementTime>2011-12-13T14:15:16.017Z</movementTime>
      <declaredIn code=""ORIGINAL"" label=""ORIGINAL"" />
      <from code=""ABC"" label=""ABC"" />
      <goods>
        <ref type=""AWB"" code=""UNITTEST"" />
        <amount quantity=""25"" weight=""50.000"" />
        <description>Original Goods</description>
        <totalRefAmount quantity=""25"" weight=""50.000"" />
      </goods>
      <customsStatus></customsStatus>
    </WarehouseMovementIn>
  </CinMessage>
</Message>";
			AssertContains(expectedMsg, msg);
		}

		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestWarehouseMovementOut_NOTCIN()
		{
			var jobHeader = CreateTestJobHeader();
			var messageId = "UT-MOVE-OUT-001";

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var builder = CINImportMessageBuilder.Create(headerWrapper, null, messageId, CINImportMessageBuilder.MessageBuilderType.MovementOut);

			var msg = builder.GetMessage();

			var expectedMsg = @"<Message>
  <EnvelopeMessage>
    <schemaID>750</schemaID>
    <schemaVersion>XML</schemaVersion>
    <transactionID>0000000001</transactionID>
  </EnvelopeMessage>
  <CinMessage type=""WarehouseMovement-Out"">
    <Header from=""PUT-CIN-ID-HERE"" to=""CIN"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""UT-MOVE-OUT-001"" />
    <WarehouseMovementOut>
      <movementTime>2011-12-13T14:15:16.017Z</movementTime>
      <declaredIn code=""ORIGINAL"" label=""ORIGINAL"" />
      <to code=""NOTCIN"" label=""NOTCIN"" />
      <goods>
        <ref type=""AWB"" code=""UNITTEST"" />
        <amount quantity=""25"" weight=""50.000"" />
        <description>Original Goods</description>
        <totalRefAmount quantity=""25"" weight=""50.000"" />
      </goods>
      <customsStatus></customsStatus>
    </WarehouseMovementOut>
  </CinMessage>
</Message>";
			AssertContains(expectedMsg, msg);
		}

		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestWarehouseMovementOut_CIN()
		{
			var jobHeader = CreateTestJobHeader();
			var messageId = "UT-MOVE-OUT-001";

			jobHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_DestinationPlace = "XYZ";

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var builder = CINImportMessageBuilder.Create(headerWrapper, null, messageId, CINImportMessageBuilder.MessageBuilderType.MovementOut);

			var msg = builder.GetMessage();

			var expectedMsg = @"<Message>
  <EnvelopeMessage>
    <schemaID>750</schemaID>
    <schemaVersion>XML</schemaVersion>
    <transactionID>0000000001</transactionID>
  </EnvelopeMessage>
  <CinMessage type=""WarehouseMovement-Out"">
    <Header from=""PUT-CIN-ID-HERE"" to=""CIN"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""UT-MOVE-OUT-001"" />
    <WarehouseMovementOut>
      <movementTime>2011-12-13T14:15:16.017Z</movementTime>
      <declaredIn code=""ORIGINAL"" label=""ORIGINAL"" />
      <to code=""XYZ"" label=""XYZ"" />
      <goods>
        <ref type=""AWB"" code=""UNITTEST"" />
        <amount quantity=""25"" weight=""50.000"" />
        <description>Original Goods</description>
        <totalRefAmount quantity=""25"" weight=""50.000"" />
      </goods>
      <customsStatus></customsStatus>
    </WarehouseMovementOut>
  </CinMessage>
</Message>";
			AssertContains(expectedMsg, msg);
		}

		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestWarehouseMovementCor()
		{
			var jobHeader = CreateTestJobHeader();

			var line = (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB1-UnitTest";
			line.TSL_LocationOfGoods = "CHG-UP";
			line.TSL_GoodsDescription = "Value Increase";
			line.TSL_PackageQty = 15;
			line.TSL_GrossWeight = 30.0m;

			line = (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB2-UnitTest";
			line.TSL_LocationOfGoods = "CHG-DWN";
			line.TSL_GoodsDescription = "Value Decrease";
			line.TSL_PackageQty = 4;
			line.TSL_GrossWeight = 16.876m;

			line = (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB5-UnitTest";
			line.TSL_LocationOfGoods = "CHG-UQ";
			line.TSL_GoodsDescription = "Weight UQ Change";
			line.TSL_PackageQty = 8;
			line.TSL_GrossWeight = 1266.7m;
			line.TSL_GrossWeightUQ = Core.Constants.Weight.Grams;

			Factory.Save();

			var prevFac = Factory.CreateNewFactory();
			var prevJobHeader = prevFac.Load<CusTempStorageJobHeader>(jobHeader.PK);

			line = (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB3-UnitTest";
			line.TSL_LocationOfGoods = "NEW-LINE";
			line.TSL_GoodsDescription = "New Line Added";
			line.TSL_PackageQty = 4;
			line.TSL_GrossWeight = 16.876m;

			prevJobHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageQty -= 3;
			prevJobHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeight -= 7.5m;

			prevJobHeader.CusTempStorageDec.CusTempStorageLines[2].TSL_PackageQty += 6;
			prevJobHeader.CusTempStorageDec.CusTempStorageLines[2].TSL_GrossWeight += 16.377m;

			prevJobHeader.CusTempStorageDec.CusTempStorageLines[3].TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			line = (CusTempStorageLine)prevJobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB4-UnitTest";
			line.TSL_LocationOfGoods = "DEL-LINE";
			line.TSL_GoodsDescription = "Line Deleted";
			line.TSL_PackageQty = 10;
			line.TSL_GrossWeight = 20.0m;

			var messageId = "UT-MOVE-COR-001";

			AssertNotEquals("Qty should be different - Line 1", jobHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageQty, prevJobHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageQty);
			AssertNotEquals("Weight should be different - Line 1", jobHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeight, prevJobHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeight);

			AssertNotEquals("Qty should be different - Line 2", jobHeader.CusTempStorageDec.CusTempStorageLines[2].TSL_PackageQty, prevJobHeader.CusTempStorageDec.CusTempStorageLines[2].TSL_PackageQty);
			AssertNotEquals("Weight should be different - Line 2", jobHeader.CusTempStorageDec.CusTempStorageLines[2].TSL_GrossWeight, prevJobHeader.CusTempStorageDec.CusTempStorageLines[2].TSL_GrossWeight);

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var prevHeaderWrapper = new CINHeaderWrapper(prevJobHeader);
			var builder = CINImportMessageBuilder.Create(headerWrapper, prevHeaderWrapper, messageId, CINImportMessageBuilder.MessageBuilderType.Correction);

			var msg = builder.GetMessage();

			var expectedMsg = @"<Message>
  <EnvelopeMessage>
    <schemaID>750</schemaID>
    <schemaVersion>XML</schemaVersion>
    <transactionID>0000000001</transactionID>
  </EnvelopeMessage>
  <CinMessage type=""WarehouseMovement-Cor"">
    <Header from=""PUT-CIN-ID-HERE"" to=""CIN"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""UT-MOVE-COR-001"" />
    <WarehouseMovementCor>
      <movementTime>2011-12-13T14:15:16.017Z</movementTime>
      <declaredIn code=""ORIGINAL"" label=""ORIGINAL"" />
      <goods>
        <ref type=""HWB"" code=""HWB1-UnitTest"" />
        <amount quantity=""3"" weight=""7.500"" />
        <description>Value Increase</description>
        <totalRefAmount quantity=""15"" weight=""30.000"" />
      </goods>
      <goods>
        <ref type=""HWB"" code=""HWB2-UnitTest"" />
        <amount quantity=""-6"" weight=""-16.377"" />
        <description>Value Decrease</description>
        <totalRefAmount quantity=""4"" weight=""16.876"" />
      </goods>
      <goods>
        <ref type=""HWB"" code=""HWB5-UnitTest"" />
        <amount quantity=""0"" weight=""-1265.433"" />
        <description>Weight UQ Change</description>
        <totalRefAmount quantity=""8"" weight=""1.267"" />
      </goods>
      <goods>
        <ref type=""HWB"" code=""HWB3-UnitTest"" />
        <amount quantity=""4"" weight=""16.876"" />
        <description>New Line Added</description>
        <totalRefAmount quantity=""4"" weight=""16.876"" />
      </goods>
      <goods>
        <ref type=""HWB"" code=""HWB4-UnitTest"" />
        <amount quantity=""-10"" weight=""-20.000"" />
        <description>Line Deleted</description>
        <totalRefAmount quantity=""0"" weight=""0.000"" />
      </goods>
    </WarehouseMovementCor>
  </CinMessage>
</Message>";
			AssertContains(expectedMsg, msg);
		}

		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestWarehouseMovementDeconsolidation()
		{
			var jobHeader = CreateTestJobHeader();
			var messageId = "UT-MOVE-DECON-001";

			var line = (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB1-UnitTest";
			line.TSL_LocationOfGoods = "There";
			line.TSL_GoodsDescription = "Decon Line 1";
			line.TSL_PackageQty = 15;
			line.TSL_GrossWeight = 30.0m;

			line = (CusTempStorageLine)jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "HWB";
			line.TSL_OwnerReferenceNumber = "HWB2-UnitTest";
			line.TSL_LocationOfGoods = "Everywhere";
			line.TSL_GoodsDescription = "Decon Line 2";
			line.TSL_PackageQty = 10;
			line.TSL_GrossWeight = 20.0m;

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var builder = CINImportMessageBuilder.Create(headerWrapper, null, messageId, CINImportMessageBuilder.MessageBuilderType.Deconsolidation);

			var msg = builder.GetMessage();

			var expectedMsg = @"<Message>
  <EnvelopeMessage>
    <schemaID>750</schemaID>
    <schemaVersion>XML</schemaVersion>
    <transactionID>0000000001</transactionID>
  </EnvelopeMessage>
  <CinMessage type=""WarehouseMovement-Decons"">
    <Header from=""PUT-CIN-ID-HERE"" to=""CIN"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""UT-MOVE-DECON-001"" />
    <WarehouseMovementDecons>
      <movementTime>2011-12-13T14:15:16.017Z</movementTime>
      <declaredIn code=""ORIGINAL"" label=""ORIGINAL"" />
      <fromGoods>
        <ref type=""AWB"" code=""UNITTEST"" />
        <amount quantity=""25"" weight=""50.000"" />
      </fromGoods>
      <toGoods>
        <goods>
          <ref type=""HWB"" code=""HWB1-UnitTest"" />
          <amount quantity=""15"" weight=""30.000"" />
        </goods>
        <goods>
          <ref type=""HWB"" code=""HWB2-UnitTest"" />
          <amount quantity=""10"" weight=""20.000"" />
        </goods>
      </toGoods>
    </WarehouseMovementDecons>
  </CinMessage>
</Message>";
			AssertContains(expectedMsg, msg);
		}

		public void TestCINLineDifferences()
		{
			var newLine = Factory.New<ISTCusTempStorageLine>();
			var prevLine = Factory.New<ISTCusTempStorageLine>();

			newLine.TSL_OwnerReferenceType = "AWB";
			newLine.TSL_OwnerReferenceNumber = "UNITTEST";
			newLine.TSL_LocationOfGoods = "ORIGINAL";
			newLine.TSL_GoodsDescription = "Original Goods";
			newLine.TSL_PackageQty = 25;
			newLine.TSL_GrossWeight = 50.0m;

			prevLine.TSL_PackageQty = 20;
			prevLine.TSL_GrossWeight = 40.0m;

			var newLineWrapper = new CINLineWrapper(newLine);
			var prevLineWrapper = new CINLineWrapper(prevLine);
			var cinLineDiff = new CINWarehouseMovementCorMessageBuilder.CINLineDifference(newLineWrapper, prevLineWrapper, CINWarehouseMovementCorMessageBuilder.LineMode.Edit);

			AssertEquals(newLine.PK, cinLineDiff.PK);
			AssertEquals("AWB", cinLineDiff.Type);
			AssertEquals("UNITTEST", cinLineDiff.ReferenceNumber);
			AssertEquals(5, cinLineDiff.NoPieces);
			AssertEquals(25, cinLineDiff.TotalNoPieces);
			AssertEquals(10.0m, cinLineDiff.Mass);
			AssertEquals(50.0m, cinLineDiff.TotalMass);
			AssertEquals("Original Goods", cinLineDiff.DescriptionOfGoods);
			AssertEquals(true, cinLineDiff.IsAirwayBill);

			cinLineDiff = new CINWarehouseMovementCorMessageBuilder.CINLineDifference(newLineWrapper, prevLineWrapper, CINWarehouseMovementCorMessageBuilder.LineMode.Deleted);
			AssertEquals(-20, cinLineDiff.NoPieces);
			AssertEquals(0, cinLineDiff.TotalNoPieces);
			AssertEquals(-40.0m, cinLineDiff.Mass);
			AssertEquals(0.0m, cinLineDiff.TotalMass);

			cinLineDiff = new CINWarehouseMovementCorMessageBuilder.CINLineDifference(newLineWrapper, prevLineWrapper, CINWarehouseMovementCorMessageBuilder.LineMode.New);
			AssertEquals(25, cinLineDiff.NoPieces);
			AssertEquals(25, cinLineDiff.TotalNoPieces);
			AssertEquals(50.0m, cinLineDiff.Mass);
			AssertEquals(50.0m, cinLineDiff.TotalMass);
		}

		CusTempStorageJobHeader CreateTestJobHeader()
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var dec = jobHeader.CusTempStorageDec;

			var line = dec.CusTempStorageLines[0] as CusTempStorageLine;

			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "ORIGINAL";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;
			Factory.Save();

			return jobHeader;
		}
	}
}
