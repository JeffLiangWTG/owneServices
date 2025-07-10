using System.Collections.Generic;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePreviousDocument))]
	class TemporaryStoragePreviousDocumentTest : CusSupportingInfoTest<TemporaryStoragePreviousDocument>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Previous Document", Factory.New<TemporaryStoragePreviousDocument>().HumanReadableName);
		}

		protected override IEnumerable<TemporaryStoragePreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var storageHeader = factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = factory.New<TemporaryStorageBill>();
			bill.ABL_AMA = storageHeader.PK;
			var packedItem = bill.PackedItems.AddNew();
			yield return storageHeader.PreviousDocuments.AddNew();
			yield return bill.PreviousDocuments.AddNew();
			yield return packedItem.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			return storageHeader.PreviousDocuments.AddNew();
		}

		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();
			var billPreviousDocument = bill.PreviousDocuments.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var packedItemPreviousDocument = packedItem.PreviousDocuments.AddNew();
			var headerPreviousDocument = header.PreviousDocuments.AddNew();

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = billPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = headerPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}

			foreach (var status in TemporaryStorageHeaderTest.GetAmendableFieldsEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				header.ENSReuse = 1;
				bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
				var propertyInfos = billPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
				propertyInfos = billPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				header.ENSReuse = 0;

				bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
				propertyInfos = billPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
				propertyInfos = billPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemPreviousDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		protected void TestPreviousDocumentInTable()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var document = storageHeader.PreviousDocuments.AddNew();
			AssertEquals("ParentTableCode should be equal to AMA", storageHeader.TablePrefix, document.CSI_ParentTableCode);
			AssertEquals("parentID should be equal to Storage ID PK", storageHeader.PK, document.CSI_ParentID);
			AssertEquals("Type should be equal to PRE", Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, document.CSI_Type);
		}

		public void TestTemporaryStorageHeader()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_AMA = storageHeader.PK;
			var packedItem = bill.PackedItems.AddNew();

			var previousDocumentHeader = storageHeader.PreviousDocuments.AddNew();
			var previousDocumentBill = bill.PreviousDocuments.AddNew();
			var previousDocumentItem = packedItem.PreviousDocuments.AddNew();

			AssertEquals("Previous Document of the header shoud have correct TemporaryStorageHeader", storageHeader, previousDocumentHeader.TemporaryStorageHeader);
			AssertEquals("Previous Document of the bill shoud have correct TemporaryStorageHeader", storageHeader, previousDocumentBill.TemporaryStorageHeader);
			AssertEquals("Previous Document of the item shoud have correct TemporaryStorageHeader", storageHeader, previousDocumentItem.TemporaryStorageHeader);
		}

		public void TestCSI_Code_ReadOnly_Transfer()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var previousDocument = storageHeader.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Enable CSI_Code when TemporaryStorageHeader.AMA_MessageType <> 'TF'", false, previousDocument.CSI_CodeInfo.ReadOnly);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				AssertEquals("Disable CSI_Code when TemporaryStorageHeader.AMA_MessageType = 'TF'", true, previousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnly_Deconsolidation()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var previousDocument = storageHeader.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Enable CSI_Code when TemporaryStorageHeader.AMA_MessageType <> 'DC'", false, previousDocument.CSI_CodeInfo.ReadOnly);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				AssertEquals("Disable CSI_Code when TemporaryStorageHeader.AMA_MessageType = 'DC'", true, previousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnly_TemporaryStorageBill_Transfer()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Enable CSI_Code when TemporaryStorageHeader.AMA_MessageType <> 'TF'", false, previousDocument.CSI_CodeInfo.ReadOnly);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				AssertEquals("Enable CSI_Code when TemporaryStorageHeader.AMA_MessageType = 'TF'", false, previousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnly_TemporaryStorageBill_Deconsolidation()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Enable CSI_Code when TemporaryStorageHeader.AMA_MessageType <> 'DC'", false, previousDocument.CSI_CodeInfo.ReadOnly);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				AssertEquals("Enable CSI_Code when TemporaryStorageHeader.AMA_MessageType = 'DC'", false, previousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_PackType()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();
			var packType = typeof(TemporaryStoragePreviousDocument).GetProperty(nameof(previousDocument.CSI_PackType));

			AssertEquals("List data source", "Lookups.PackTypeList", packType.GetCustomAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestCSI_UnitOfQuantity()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();
			var unitOfQuantity = typeof(TemporaryStoragePreviousDocument).GetProperty(nameof(previousDocument.CSI_UnitOfQuantity));

			AssertEquals("List data source", "Lookups.UnitOfQuantityList", unitOfQuantity.GetCustomAttribute<ListAttribute>().ListDataSourceMember);
		}
	}
}
