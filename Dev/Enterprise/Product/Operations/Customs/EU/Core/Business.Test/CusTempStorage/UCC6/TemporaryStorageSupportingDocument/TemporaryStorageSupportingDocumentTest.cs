using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageSupportingDocument))]
	sealed class TemporaryStorageSupportingDocumentTest : CusSupportingInfoTest<TemporaryStorageSupportingDocument>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Supporting Document", Factory.New<TemporaryStorageSupportingDocument>().HumanReadableName);
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			var supportingDocument = (TemporaryStorageSupportingDocument)GetNewBusinessObject();
			AssertEquals(70, supportingDocument.CSI_ReferenceNumberInfo.MaxLength);
		}

		protected override IEnumerable<TemporaryStorageSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var storageHeader = factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var billPackedItem = bill.PackedItems.AddNew();
			yield return bill.SupportingDocuments.AddNew();
			yield return billPackedItem.SupportingDocuments.AddNew();
		}

		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();
			var billSupportingDocument = bill.SupportingDocuments.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var packedItemSupportingDocument = packedItem.SupportingDocuments.AddNew();

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = billSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}

			foreach (var status in TemporaryStorageHeaderTest.GetAmendableFieldsEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				header.ENSReuse = 0;
				bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
				var propertyInfos = billSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
				propertyInfos = billSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				header.ENSReuse = 1;
				bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
				propertyInfos = billSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
				propertyInfos = billSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemSupportingDocument.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		public void TestTemporaryStorageHeader()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var supportingDocument = bill.SupportingDocuments.AddNew();
			AssertEquals(header, supportingDocument.TemporaryStorageHeader);

			var packedItem = bill.PackedItems.AddNew();
			var supportingDocument2 = packedItem.SupportingDocuments.AddNew();
			AssertEquals(header, supportingDocument2.TemporaryStorageHeader);
		}

		public void TestLookups()
		{
			var supportingDocument = Factory.New<TemporaryStorageSupportingDocument>();
			var lookups = supportingDocument.Lookups;
			AssertType<TemporaryStorageSupportingDocumentLookups>("Lookups Type", lookups);
		}

		public void TestValidation()
		{
			var supportingDocument = Factory.New<TemporaryStorageSupportingDocument>();
			var validation = supportingDocument.Validation;
			AssertType<TemporaryStorageSupportingDocumentValidation>("Validation Type", validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			return bill.SupportingDocuments.AddNew();
		}
	}
}
