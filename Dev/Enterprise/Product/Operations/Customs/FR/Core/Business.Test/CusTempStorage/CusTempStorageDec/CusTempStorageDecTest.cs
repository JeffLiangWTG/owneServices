using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageDec))]
	public abstract class CusTempStorageDecTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusTempStorageDecTypeDecider>(CusTempStorageDec.TypeDecider);
		}

		public virtual void TestValidation()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			AssertType<CusTempStorageDecValidation>(storageDec.Validation);
		}

		public void TestSTH_SystemCreateTimeUtc_ReadOnly()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			Assert(storageDec.STH_SystemCreateTimeUtcInfo.ReadOnly);
		}

		public void TestSTH_MessageStatus_ReadOnly()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			Assert(storageDec.STH_MessageStatusInfo.ReadOnly);
		}

		public void TestSTH_DeclarationStatus_ReadOnly()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			Assert(storageDec.STH_DeclarationStatusInfo.ReadOnly);
		}

		public void TestSTH_Calc_CustomsDocumentReferences()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var supportingDocument1 = storageDec.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "Code2";
			supportingDocument1.CSI_ReferenceNumber = "ReferenceNumber2";
			var supportingDocument2 = storageDec.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "Code1";
			supportingDocument2.CSI_ReferenceNumber = "ReferenceNumber1";
			AssertEquals("Code1=ReferenceNumber1; Code2=ReferenceNumber2", storageDec.STH_Calc_CustomsDocumentReferences);
		}

		public void TestSTH_Calc_Containers()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var container1 = storageDec.CusTempStorageContainers.AddNew();
			container1.CY_Code = "Code2";
			container1.CY_Data = "Data2";
			var container2 = storageDec.CusTempStorageContainers.AddNew();
			container2.CY_Code = "Code1";
			container2.CY_Data = "Data1";
			AssertEquals("Data1; Data2", storageDec.STH_Calc_Containers);
		}

		[TestDate(2020, 7, 28)]
		public void TestSTH_Calc_StorageLimitRemaining()
		{
			var header = CusTempStorageJobHeader.New(Factory, GetAppCode());
			var storageDec = header.CusTempStorageDec;

			header.SJH_TempStorageEndDateUtc = new ZDateTime(2020, 7, 27);
			AssertEquals("", storageDec.STH_Calc_StorageLimitRemaining);

			storageDec.STH_SystemCreateTimeUtc = new ZDateTime(2020, 7, 26, 0, 0, 1);
			AssertEquals("", storageDec.STH_Calc_StorageLimitRemaining);

			header.SJH_TempStorageEndDateUtc = new ZDateTime(2020, 7, 28);
			AssertEquals("47h", storageDec.STH_Calc_StorageLimitRemaining);

			storageDec.STH_SystemCreateTimeUtc = new ZDateTime(2020, 7, 26, 0, 0, 0);
			AssertEquals("2j", storageDec.STH_Calc_StorageLimitRemaining);
		}

		public void TestLocationOfGoods()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			AssertEquals("", storageDec.LocationOfGoods);

			var line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_LocationOfGoods = "test";
			AssertEquals("test", storageDec.LocationOfGoods);

			line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_LocationOfGoods = "test2";
			AssertEquals("Multiple", storageDec.LocationOfGoods);
		}

		public void TestUnionStatus()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			AssertEquals("", storageDec.UnionStatus);

			var line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_UnionStatus = "TST";
			AssertEquals("TST", storageDec.UnionStatus);

			line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_UnionStatus = "TS2";
			AssertEquals("Multiple", storageDec.UnionStatus);
		}

		public void TestSupportingDocuments()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			AssertType<SupportingDocumentCollection>(storageDec.SupportingDocuments);
			AssertNotNull("SupportingDocuments", storageDec.SupportingDocuments);
		}

		public void TestCusTempStorageContainers()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory, GetAppCode());
			var storageDec = storageHeader.CusTempStorageDec;

			AssertType<CusTempStorageContainerCollection>(storageDec.CusTempStorageContainers);
			AssertNotNull("CusTempStorageContainers", storageDec.CusTempStorageContainers);

			var cusTempStorageContainers = storageDec.CusTempStorageContainers;
			var container1 = cusTempStorageContainers.AddNew();
			var container2 = cusTempStorageContainers.AddNew();
			var container3 = cusTempStorageContainers.AddNew();
			AssertEquals("StorageHeader.SJH_ContainerCount", 3, storageHeader.SJH_ContainerCount);

			cusTempStorageContainers.RemoveAndDelete(container1);
			AssertEquals("StorageHeader.SJH_ContainerCount", 2, storageHeader.SJH_ContainerCount);

			cusTempStorageContainers.AddNew();
			cusTempStorageContainers.AddNew();
			AssertEquals("StorageHeader.SJH_ContainerCount", 4, storageHeader.SJH_ContainerCount);

			cusTempStorageContainers.RemoveAndDeleteAll();
			AssertEquals("StorageHeader.SJH_ContainerCount", 0, storageHeader.SJH_ContainerCount);
		}

		public void TestICanBeImportOrExport()
		{
			EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport item = GetCusTempStorageDecForTesting();
			AssertEquals("Header", item.Level);
			AssertEquals(ZBool.False, item.IsExport);
			AssertEquals(ZBool.False, item.IsImport);

			// Add tests
		}

		#region IAllowPermitProcessing

		public void TestIAllowPermitProcessing()
		{
			var storageDec = SetupStorageDecForIAllowPermitProcessing();
			var allowPermitProcessing = (IAllowPermitProcessing)storageDec;
			AssertEquals(10, allowPermitProcessing.PackageCount);
			AssertEquals("REFNumb001", allowPermitProcessing.GetPermitComment(null));
			AssertEquals("SJHREF001", allowPermitProcessing.GetPermitReference());
			AssertEquals(ZInt.Zero, allowPermitProcessing.GetPermitReferenceNumberLine());
			AssertEquals(5, allowPermitProcessing.PermitQuantityDecimalPlaceCount);
			AssertEquals(2, allowPermitProcessing.PermitValueDecimalPlaceCount);

			var permitRecords = allowPermitProcessing.GetPermitRecords();
			AssertEquals(1, permitRecords.Count);
			var permitRecord = permitRecords[0];
			AssertEquals("Zero is currently hard coded, but needs no implementation - this is a fiscal guarantee not a licence/permit", 0m, permitRecord.Quantity);
			AssertEquals(7m, permitRecord.Value);
			var guaranteePK = storageDec.StorageHeader.SJH_CPH_Guarantee;
			AssertEquals(guaranteePK, permitRecord.PermitHeader.PK);

			storageDec.StorageHeader.SJH_CPH_Guarantee = ZGuid.Empty;
			AssertEquals(0, allowPermitProcessing.GetPermitRecords().Count);

			storageDec.StorageHeader.SJH_CPH_Guarantee = guaranteePK;
			storageDec.CusTempStorageLines.RemoveAndDeleteAll();
			AssertEquals(0, allowPermitProcessing.GetPermitRecords().Count);
		}

		CusTempStorageDec SetupStorageDecForIAllowPermitProcessing()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "COD";
			guaranteeHeader.CPH_SubType = "ALT";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var storageHeader = CusTempStorageJobHeader.New(Factory, GetAppCode());
			storageHeader.SJH_JobReference = "SJHREF001";
			storageHeader.SJH_ReferenceNumber = "REFNumb001";
			storageHeader.SJH_CPH_Guarantee = guaranteeHeader.PK;
			var storageDec = storageHeader.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "REF000001";
			var line1 = storageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 2;
			line1.CusTempStorageLineItems.AddNew().TSI_GuaranteedValue = 1m;
			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 8;
			line2.CusTempStorageLineItems.AddNew().TSI_GuaranteedValue = 2m;
			line2.CusTempStorageLineItems.AddNew().TSI_GuaranteedValue = 4m;

			return storageDec;
		}
		#endregion

		protected abstract CusTempStorageDec GetCusTempStorageDecForTesting();

		protected abstract ZString GetAppCode();
	}
}
