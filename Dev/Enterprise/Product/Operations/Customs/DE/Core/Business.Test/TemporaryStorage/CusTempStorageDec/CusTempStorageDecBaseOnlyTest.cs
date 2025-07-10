using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageDecBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCheckIfIssueDateIsNotEmpty()
		{
			CombineAssertions(() =>
			{
				storageDec.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
				AssertEquals("Empty", false, storageDec.ReferenceNumberIssueDatePopulated);
				storageDec.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
				AssertEquals("Populated", true, storageDec.ReferenceNumberIssueDatePopulated);
			});
		}

		public void TestCanDelete()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
				AssertEquals("Cannot Delete", false, storageDec.CanDelete);
				AssertEquals("storageDec.ReasonForNotAbleToDelete", "Declaration cannot be deleted as customs messaging has occurred", storageDec.ReasonForNotAbleToDelete);
				storageDec.STH_MessageStatus = ZString.Empty;
				AssertEquals("Can Delete", true, storageDec.CanDelete);
			});
		}

		public void TestSTH_SystemCreateTimeUtc()
		{
			AssertEquals(true, storageDec.STH_SystemCreateTimeUtcInfo.ReadOnly);
		}

		public void TestSTH_MessageStatus()
		{
			AssertEquals(true, storageDec.STH_MessageStatusInfo.ReadOnly);
		}

		public void TestReferenceNumberCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Reference No.", DataBoundResourceStrings.GetDataForProperty(storageDec.ReferenceNumberInfo).Caption);
				AssertEquals("Ref. No.", DataBoundResourceStrings.GetDataForProperty(storageDec.ReferenceNumberInfo).ShortCaption);
			});
		}

		public void TestReferenceNumberReadOnly()
		{
			AssertEquals(true, storageDec.ReferenceNumberInfo.ReadOnly);
		}

		public void TestGetReferenceNumber()
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentTable = storageDec.TableName;
			entryNum.CE_ParentID = storageDec.PK;
			entryNum.CE_EntryType = CusEntryNumberTypes.Germany.SumAEntryNumber;
			entryNum.CE_EntryNum = "ATB150000010320006000";
			CombineAssertions(() =>
			{
				AssertEquals("Formatted", "AT/B/15/000001/03/2000/6000", storageDec.ReferenceNumber);
				entryNum.CE_EntryNum = "ATB150000010320006000EXTRA";
				AssertEquals("Longer", "AT/B/15/000001/03/2000/6000EXTRA", storageDec.ReferenceNumber);
				entryNum.CE_EntryNum = "ATB15SHORT";
				AssertEquals("Shoter", "AT/B/15/SHORT", storageDec.ReferenceNumber);
			});
		}

		public void TestSetReferenceNumber()
		{
			CombineAssertions(() =>
			{
				storageDec.ReferenceNumber = "AT/B/15/000001/03/2000/6000";
				var storageDecEntryNum = CusEntryNumber.Load(storageDec, CusEntryNumberTypes.Germany.SumAEntryNumber, Core.Constants.CountryCodes.Germany);
				AssertEquals("CusEntryNum created", "ATB150000010320006000", storageDecEntryNum.CE_EntryNum);
				var storageDecEntryNumPK = storageDecEntryNum.PK;
				storageDecEntryNum.Delete();
				storageDec.ReferenceNumber = "12$%#$LKS DNFJVN*()_)98324ASDAD";
				var storageDecEntryNum2 = CusEntryNumber.Load(storageDec, CusEntryNumberTypes.Germany.SumAEntryNumber, Core.Constants.CountryCodes.Germany);
				AssertNotEquals("Delete creates a new CusEntryNum", storageDecEntryNumPK, storageDecEntryNum2.PK);
				AssertEquals("New one has the correct information.", "12LKSDNFJVN98324ASDAD", storageDecEntryNum2.CE_EntryNum);
			});
		}

		public void TestSTH_OwnerReferenceNumber()
		{
			AssertEquals("Length of Owner Ref matches what is set on the Line", 44, storageDec.STH_OwnerReferenceNumberInfo.MaxLength);
		}

		public void TestFormattedOwnerReferenceNumberForAWB()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = Messaging.TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				AssertEquals("Length of Formattted Owner Ref matches what is set on the Line", 44, storageDec.FormattedOwnerReferenceNumberInfo.MaxLength);
				storageDec.FormattedOwnerReferenceNumber = "AT/B/15/000001/03/2000/6001";
				AssertEquals("Is AWB no formatting", "ATB150000010320006001", storageDec.FormattedOwnerReferenceNumber);
				AssertEquals("Underlying property AWB no formatting", "ATB150000010320006001", storageDec.STH_OwnerReferenceNumber);
			});
		}

		public void TestFormattedOwnerReferenceNumberForREG()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = Messaging.TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("No '/' in default empty", ZString.Empty, storageDec.FormattedOwnerReferenceNumber);
				storageDec.FormattedOwnerReferenceNumber = "ATB150000010320006001";
				AssertEquals("Is REG so format to ATB No.", "AT/B/15/000001/03/2000/6001", storageDec.FormattedOwnerReferenceNumber);
				AssertEquals("Underlying property REG no formatting", "ATB150000010320006001", storageDec.STH_OwnerReferenceNumber);
			});
		}

		public void TestIsAWBDeclaration()
		{
			AssertDeclarationIdentificationIndicator(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.AWB, () => storageDec.IsAWBDeclaration);
		}

		public void TestIsREGDeclaration()
		{
			AssertDeclarationIdentificationIndicator(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.REG, () => storageDec.IsREGDeclaration);
		}

		public void TestIsSINDeclaration()
		{
			AssertDeclarationIdentificationIndicator(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.SIN, () => storageDec.IsSINDeclaration);
		}

		public void TestReferenceNumberColumnFieldType()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = ZString.Empty;
				AssertEquals("STH_IdentificationIndicator is empty", storageDec.ReferenceNumberColumnFieldType, nameof(FieldType.Text));

				storageDec.STH_IdentificationIndicator = Messaging.TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				AssertEquals("STH_IdentificationIndicator isn't REG", storageDec.ReferenceNumberColumnFieldType, nameof(FieldType.Text));

				storageDec.STH_IdentificationIndicator = Messaging.TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("STH_IdentificationIndicator is REG", storageDec.ReferenceNumberColumnFieldType, nameof(FieldType.TextCodeFindBox));
			});
		}

		public void TestFormattedOwnerReferenceNumberCaption()
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(storageDec.FormattedOwnerReferenceNumberInfo);
			AssertEquals($"FormattedOwnerReferenceNumber Caption", "Reference", propertyData.Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<CusTempStorageDecForTest>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader;
		}
		CusTempStorageDecForTest storageDec;

		void AssertDeclarationIdentificationIndicator(ZString identificationIndicator, Func<bool> propertyToTest)
		{
			CombineAssertions(() =>
			{
				AssertEquals("IdentificationIndicator empty", false, propertyToTest.Invoke());

				storageDec.STH_IdentificationIndicator = identificationIndicator;
				AssertEquals($"IdentificationIndicator: {identificationIndicator}", true, propertyToTest.Invoke());
			});
		}
	}

	sealed class CusTempStorageDecForTest : CusTempStorageDec
	{
		public CusTempStorageDecForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
