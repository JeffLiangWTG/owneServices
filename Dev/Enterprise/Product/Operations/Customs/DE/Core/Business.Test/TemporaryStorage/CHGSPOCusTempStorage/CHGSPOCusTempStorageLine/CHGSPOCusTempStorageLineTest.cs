using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGSPOCusTempStorageLine))]
	class CHGSPOCusTempStorageLineTest : CusTempStorageLineTest<CHGSPOCusTempStorageLine>
	{
		public void TestSequenceNumberEnabled()
		{
			AssertEquals(false, GetNewCusTempStorageLine().SequenceNumberEnabled);
		}

		public void TestReadOnlyTSL_LineNo()
		{
			AssertEquals(false, GetNewCusTempStorageLine().TSL_LineNoInfo.ReadOnly);
		}

		public void TestDefaultValue()
		{
			AssertEquals(TemporaryStorageIdentificationIndicatorList.Codes.AWB, GetNewCusTempStorageLine().TSL_OwnerReferenceType);
		}

		public void TestTSL_OwnerReferenceNumber()
		{
			var storageLine = GetNewCusTempStorageLine();
			storageLine.TSL_OwnerReferenceType = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			storageLine.TSL_OwnerReferenceNumber = "ATB150000010320006000";
			CombineAssertions(() =>
			{
				AssertEquals("Max Length is the same as the Owner Reference", 44, storageLine.TSL_OwnerReferenceNumberInfo.MaxLength);
				AssertEquals("No formatting for AWB type", "ATB150000010320006000", storageLine.TSL_OwnerReferenceNumber);
			});
		}

		public void TestTSL_OwnerReferenceNumber_AcceptUnicodeCharacters()
		{
			var storageLine = GetNewCusTempStorageLine();
			storageLine.TSL_OwnerReferenceType = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			storageLine.TSL_OwnerReferenceNumber = "中国1234ABC";
			AssertEquals("Accept Unicode characters", "中国1234ABC", storageLine.TSL_OwnerReferenceNumber);
		}

		public void TestTSL_LineNoCaption()
		{
			var storageLine = GetNewCusTempStorageLine();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(storageLine.TSL_LineNoInfo);
			AssertEquals("TSL_LineNo Caption", "Reference Line Number", propertyData.Caption);
			AssertEquals("TSL_LineNo Short Caption", "Reference Line No.", propertyData.ShortCaption);
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (CusTempStorageLineCollection<CHGSPOCusTempStorageLine, CHGSPOCusTempStorageDec>)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(CHGSPOCusTempStorageLine) });
			AssertNotNull(bizO);
			AssertType<CHGSPOCusTempStorageLine>(bizO);
		}

		protected BusinessObjectCollection GetCollectionToTest()
		{
			var storageDec = Factory.New<CHGSPOCusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			return storageDec.CusTempStorageLines;
		}

		#region Implementation

		protected override CHGSPOCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTEST";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECHGSPO001";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDec = storageJobHeader.CHGSPOCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			storageDec.STH_SJH = storageJobHeader.PK;

			return storageDec.CusTempStorageLines.AddNew();
		}

		protected override Type GetDecType() => typeof(CHGSPOCusTempStorageDec);

		protected override Type GetLookupType() => typeof(CHGSPOCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CHGSPOCusTempStorageLineValidation);

		#endregion
	}
}
