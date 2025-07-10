using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageDec))]
	public class CusTempStorageDecTest : EnterpriseBusinessObjectTestCase
	{
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
			var storageHeader = CusTempStorageJobHeader.New(Factory);
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

		public virtual void TestSetStorageHeaderDefaultValues()
		{
			AssertNotNull(GetCusTempStorageDecForTesting().StorageHeader);
		}

		public void TestICanBeImportOrExport()
		{
			EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport item = GetCusTempStorageDecForTesting();
			AssertEquals("Header", item.Level);
			AssertEquals(ZBool.False, item.IsExport);
			AssertEquals(ZBool.False, item.IsImport);
			AssertEquals("CountryCode", "ES", item.TrueCountryCode);
			AssertEquals("Data Grouping", "ES", item.DataGroupingCode);
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
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "COD";
			guaranteeHeader.CPH_SubType = "ALT";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var storageDec = CusTempStorageDec.New(storageHeader);
			storageHeader.SJH_JobReference = "SJHREF001";
			storageHeader.SJH_ReferenceNumber = "REFNumb001";
			storageHeader.SJH_CPH_Guarantee = guaranteeHeader.PK;
			storageDec.STH_OwnerReferenceNumber = "REF000001";
			var line1 = storageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 2;
			var item1 = line1.CusTempStorageLineItems.AddNew();
			item1.TSI_GuaranteedValue = 1m;
			item1.TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 8;
			var item2 = line2.CusTempStorageLineItems.AddNew();
			item2.TSI_GuaranteedValue = 2m;
			item2.TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
			var item3 = line2.CusTempStorageLineItems.AddNew();
			item3.TSI_GuaranteedValue = 4m;
			item3.TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;

			return storageDec;
		}
		#endregion

		CusTempStorageDec GetCusTempStorageDecForTesting()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTEST";
			var presenter = Factory.NewWithValidTestData<OrgAddress>();
			var representative = Factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = CusTempStorageJobHeader.New(Factory);
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "TSJHREF001";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDec = storageJobHeader.CusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = "IST";

			return storageDec;
		}

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var storageDec = CusTempStorageDec.New(storageHeader);
			storageDec.Delete();
		}
	}
}
