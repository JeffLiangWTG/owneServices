using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageDec))]
	class REXDISCusTempStorageDecTest : CusTempStorageDecAbstractTest<REXDISCusTempStorageDec>
	{
		public void TestLookupsOverriddenType()
		{
			AssertType<REXDISCusTempStorageDecLookups>(GetCusTempStorageDecForTesting().Lookups);
		}
		public void TestValidationOverriddenType()
		{
			AssertType<REXDISCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch, GetCusTempStorageDecForTesting().STH_DeclarationType);
		}

		public void TestSTH_DeclarationSubTypeMaxLength()
		{
			AssertEquals(1, GetCusTempStorageDecForTesting().STH_DeclarationSubTypeInfo.MaxLength);
		}

		public void TestSTH_IdentificationIndicator()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			storageDec.STH_IdentificationIndicator = "AWB";
			var reExportLine = storageDec.CusTempStorageLines.AddNew();
			var reExportLine2 = storageDec.CusTempStorageLines.AddNew();
			reExportLine.TSL_OwnerReferenceType = "AWB";
			reExportLine.SumALine.TSL_OwnerReferenceType = "AWB";
			reExportLine2.TSL_OwnerReferenceType = "AWB";
			reExportLine2.SumALine.TSL_OwnerReferenceType = "AWB";
			reExportLine.SumALine.TSL_LineNo = 1;
			reExportLine2.SumALine.TSL_LineNo = 2;
			storageDec.STH_IdentificationIndicator = "REG";
			AssertEquals("REG", reExportLine.SumALine.TSL_OwnerReferenceType);
			AssertEquals("AWB", reExportLine.TSL_OwnerReferenceType);
			AssertEquals("REG", reExportLine2.SumALine.TSL_OwnerReferenceType);
			AssertEquals("AWB", reExportLine2.TSL_OwnerReferenceType);
			AssertNotEquals(0, reExportLine.SumALine.TSL_LineNo);
			AssertNotEquals(0, reExportLine2.SumALine.TSL_LineNo);
			var reExportLine3 = storageDec.CusTempStorageLines.AddNew();
			AssertEquals("REG", reExportLine3.SumALine.TSL_OwnerReferenceType);
			AssertEquals(ZString.Empty, reExportLine3.TSL_OwnerReferenceType);
			reExportLine3.SumALine.TSL_LineNo = 3;
			storageDec.STH_IdentificationIndicator = "AWB";
			AssertEquals(0, reExportLine.SumALine.TSL_LineNo);
			AssertEquals(0, reExportLine2.SumALine.TSL_LineNo);
			AssertEquals(0, reExportLine3.SumALine.TSL_LineNo);
		}

		public void TestLoadOrCreate()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = REXDISCusTempStorageDec.New(storageHeader);
			var storageDecLoaded = REXDISCusTempStorageDec.LoadOrCreate(storageHeader);
			var storageDecCreated = REXDISCusTempStorageDec.LoadOrCreate(Factory.New<CusTempStorageJobHeader>());

			AssertNotNull(storageDec);
			AssertNotNull(storageDecLoaded);
			AssertNotNull(storageDecCreated);
			AssertEquals(storageDec, storageDecLoaded);
			AssertNotEquals(storageDec, storageDecCreated);
		}

		public void TestGetATNo()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentTable = storageDec.TableName;
			entryNum.CE_ParentID = storageDec.PK;
			entryNum.CE_EntryType = CusEntryNumberTypes.Germany.ReExportEntryNumber;
			entryNum.CE_EntryNum = "ATB150000010320006000";

			CombineAssertions(() =>
			{
				AssertEquals("Correctly formatted", "AT/B/15/000001/03/2000/6000", storageDec.ReferenceNumber);
				entryNum.CE_EntryNum = "ATB150000010320006000EXTRA";
				AssertEquals("Trimmed", "AT/B/15/000001/03/2000/6000EXTRA", storageDec.ReferenceNumber);
				entryNum.CE_EntryNum = "ATB15SHORT";
				AssertEquals("Short", "AT/B/15/SHORT", storageDec.ReferenceNumber);
			});
		}

		public void TestSetATNo()
		{
			CombineAssertions(() =>
			{
				var storageDec = GetCusTempStorageDecForTesting();
				storageDec.ReferenceNumber = "AT/B/15/000001/03/2000/6000";
				var storageDecEntryNum = CusEntryNumber.Load(storageDec, CusEntryNumberTypes.Germany.ReExportEntryNumber, Core.Constants.CountryCodes.Germany);
				AssertEquals("Set CusEntryNum", "ATB150000010320006000", storageDecEntryNum.CE_EntryNum);
				var storageDecEntryNumPK = storageDecEntryNum.PK;
				storageDecEntryNum.Delete();
				storageDec.ReferenceNumber = "12$%#$LKS DNFJVN*()_)98324ASDAD";
				var storageDecEntryNum2 = CusEntryNumber.Load(storageDec, CusEntryNumberTypes.Germany.ReExportEntryNumber, Core.Constants.CountryCodes.Germany);
				AssertNotEquals("Delete creates a new CusEntryNum", storageDecEntryNumPK, storageDecEntryNum2.PK);
				AssertEquals("New one has the correct information.", "12LKSDNFJVN98324ASDAD", storageDecEntryNum2.CE_EntryNum);
			});
		}

		public void TestReferenceNumberCaption()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(storageDec.ReferenceNumberInfo);
			AssertEquals("ReferenceNumber Caption", "Re-Export Reference", propertyData.Caption);
		}

		protected override REXDISCusTempStorageDec GetCusTempStorageDecForTesting() => Factory.New<REXDISCusTempStorageDec>();
	}
}
