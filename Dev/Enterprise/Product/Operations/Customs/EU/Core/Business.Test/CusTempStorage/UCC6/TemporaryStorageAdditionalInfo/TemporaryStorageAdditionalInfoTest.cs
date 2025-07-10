using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public abstract class TemporaryStorageAdditionalInfoAbstractTest<T> : ImportExportAwareSupportingInfoTest<T>
		where T : TemporaryStorageAdditionalInfo
	{
		public void TestLookups()
		{
			var additionalInfo = Factory.New<T>();
			AssertEquals(GetLookupsType, additionalInfo.Lookups.GetType());
		}

		protected virtual Type GetLookupsType => typeof(TemporaryStorageAdditionalInfoLookups);
	}

	[TestedType(typeof(TemporaryStorageAdditionalInfo))]
	sealed class TemporaryStorageAdditionalInfoTest : TemporaryStorageAdditionalInfoAbstractTest<TemporaryStorageAdditionalInfo>
	{
		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();
			var billAddInfo = bill.AdditionalInfos.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var packedItemAddInfo = packedItem.AdditionalInfos.AddNew();

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = billAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemAddInfo.ZPropertyInfoHash;
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
				var propertyInfos = billAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
				propertyInfos = billAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					if (propertyInfo.Name == nameof(billAddInfo.CSI_SubType))
					{
						Assert(propertyInfo.ReadOnly);
						continue;
					}
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				header.ENSReuse = 1;
				bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
				propertyInfos = billAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}

				bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
				propertyInfos = billAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					if (propertyInfo.Name == nameof(billAddInfo.CSI_SubType))
					{
						Assert(propertyInfo.ReadOnly);
						continue;
					}
					Assert(!propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemAddInfo.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(!propertyInfo.ReadOnly);
				}
			}
		}

		public void TestTemporaryStorageHeader()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			AssertEquals(header, addInfo.TemporaryStorageHeader);

			var packedItem = bill.PackedItems.AddNew();
			var addInfo2 = packedItem.AdditionalInfos.AddNew();
			AssertEquals(header, addInfo2.TemporaryStorageHeader);
		}

		public void TestValidation()
		{
			var additionalInfo = Factory.New<TemporaryStorageAdditionalInfo>();
			var validation = additionalInfo.Validation;
			AssertType<TemporaryStorageAdditionalInfoValidation>("Validation Type", validation);
		}

		public void TestCSI_SubTypeReadonly()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billMaster = header.Bills.AddNew();
			billMaster.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			var addInfoMaster = billMaster.AdditionalInfos.AddNew();

			Assert("CSI_SubType must be readonly for Additional Info of the Master Bill", addInfoMaster.CSI_SubTypeReadOnly);

			var packedItemMaster = billMaster.PackedItems.AddNew();
			var addInfoPackedItemMaster = packedItemMaster.AdditionalInfos.AddNew();
			Assert("CSI_SubType must not be readonly for Additional Info of the Packed Item", !addInfoPackedItemMaster.CSI_SubTypeReadOnly);

			var billHouse = header.Bills.AddNew();
			billHouse.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			var addInfoHouse = billHouse.AdditionalInfos.AddNew();
			Assert("CSI_SubType must not be readonly for Additional Info of the House Bill", !addInfoHouse.CSI_SubTypeReadOnly);

			var packedItemHouse = billHouse.PackedItems.AddNew();
			var addInfoPackedItemHouse = packedItemHouse.AdditionalInfos.AddNew();
			Assert("CSI_SubType must not be readonly for Additional Info of the Packed Item", !addInfoPackedItemHouse.CSI_SubTypeReadOnly);
		}

		public void TestCSI_ReferenceNumberReadonly()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billMaster = header.Bills.AddNew();
			billMaster.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			var addInfoMaster = billMaster.AdditionalInfos.AddNew();

			Assert("CSI_ReferenceNumber must be readonly for Additional Info of the Master Bill", addInfoMaster.CSI_ReferenceNumberReadOnly);

			var packedItemMaster = billMaster.PackedItems.AddNew();
			var addInfoPackedItemMaster = packedItemMaster.AdditionalInfos.AddNew();
			addInfoPackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_ReferenceNumber must be readonly for Additional Info of the Packed Item", addInfoPackedItemMaster.CSI_ReferenceNumberReadOnly);

			var addReferencePackedItemMaster = packedItemMaster.AdditionalInfos.AddNew();
			addReferencePackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_ReferenceNumber must not be readonly for Additional Reference of the Packed Item", !addReferencePackedItemMaster.CSI_ReferenceNumberReadOnly);

			var billHouse = header.Bills.AddNew();
			billHouse.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			var addInfoHouse = billHouse.AdditionalInfos.AddNew();
			addInfoHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_ReferenceNumber must be readonly for Additional Info of the House Bill", addInfoHouse.CSI_ReferenceNumberReadOnly);

			var addReferenceHouse = billHouse.AdditionalInfos.AddNew();
			addReferenceHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_ReferenceNumber must not be readonly for Additional Reference of the House Bill", !addReferenceHouse.CSI_ReferenceNumberReadOnly);

			var packedItemHouse = billHouse.PackedItems.AddNew();
			var addInfoPackedItemHouse = packedItemHouse.AdditionalInfos.AddNew();
			addInfoPackedItemHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_ReferenceNumber must be readonly for Additional Info of the Packed Item", addInfoPackedItemHouse.CSI_ReferenceNumberReadOnly);

			var addReferencePackedItemHouse = packedItemHouse.AdditionalInfos.AddNew();
			addReferencePackedItemHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_ReferenceNumber must not be readonly for Additional Info of the Packed Item", !addReferencePackedItemHouse.CSI_ReferenceNumberReadOnly);
		}

		public void TestCSI_ReferenceNumberAndDescription_Cleared()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billMaster = header.Bills.AddNew();
			billMaster.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			var addInfoMaster = billMaster.AdditionalInfos.AddNew();

			var packedItemMaster = billMaster.PackedItems.AddNew();
			TestClearingAdditionalInfoAndReference(packedItemMaster.AdditionalInfos);

			var billHouse = header.Bills.AddNew();
			billHouse.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			TestClearingAdditionalInfoAndReference(billHouse.AdditionalInfos);

			var packedItemHouse = billHouse.PackedItems.AddNew();
			TestClearingAdditionalInfoAndReference(packedItemHouse.AdditionalInfos);
		}

		static void TestClearingAdditionalInfoAndReference(ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> additionalInfos)
		{
			var addInfoPackedItemMaster = additionalInfos.AddNew();
			addInfoPackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfoPackedItemMaster.CSI_Description = "Description";
			AssertEquals("CSI_Description received value", addInfoPackedItemMaster.CSI_Description, "Description");

			addInfoPackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals("CSI_Description cleared out when CSI_SubType becomes AdditionalReference", addInfoPackedItemMaster.CSI_Description, ZString.Empty);
			addInfoPackedItemMaster.CSI_ReferenceNumber = "ReferenceNumber";
			AssertEquals("CSI_ReferenceNumber received value", addInfoPackedItemMaster.CSI_ReferenceNumber, "ReferenceNumber");

			addInfoPackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("CSI_ReferenceNumber cleared out when CSI_SubType becomes AdditionalInformation", addInfoPackedItemMaster.CSI_ReferenceNumber, ZString.Empty);
		}

		public void TestCSI_DescriptionReadonly()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billMaster = header.Bills.AddNew();
			billMaster.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			var addInfoMaster = billMaster.AdditionalInfos.AddNew();

			Assert("CSI_Description must not be readonly for Additional Info of the Master Bill", !addInfoMaster.CSI_DescriptionReadOnly);

			var packedItemMaster = billMaster.PackedItems.AddNew();
			var addInfoPackedItemMaster = packedItemMaster.AdditionalInfos.AddNew();
			addInfoPackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_Description must not be readonly for Additional Info of the Packed Item", !addInfoPackedItemMaster.CSI_DescriptionReadOnly);

			var addReferencePackedItemMaster = packedItemMaster.AdditionalInfos.AddNew();
			addReferencePackedItemMaster.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_Description must be readonly for Additional Reference of the Packed Item", addReferencePackedItemMaster.CSI_DescriptionReadOnly);

			var billHouse = header.Bills.AddNew();
			billHouse.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			var addInfoHouse = billHouse.AdditionalInfos.AddNew();
			addInfoHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_Description must not be readonly for Additional Info of the House Bill", !addInfoHouse.CSI_DescriptionReadOnly);

			var addReferenceHouse = billHouse.AdditionalInfos.AddNew();
			addReferenceHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_Description must be readonly for Additional Reference of the House Bill", addReferenceHouse.CSI_DescriptionReadOnly);

			var packedItemHouse = billHouse.PackedItems.AddNew();
			var addInfoPackedItemHouse = packedItemHouse.AdditionalInfos.AddNew();
			addInfoPackedItemHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_Description must not be readonly for Additional Info of the Packed Item", !addInfoPackedItemHouse.CSI_DescriptionReadOnly);

			var addReferencePackedItemHouse = packedItemHouse.AdditionalInfos.AddNew();
			addReferencePackedItemHouse.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_Description must be readonly for Additional Info of the Packed Item", addReferencePackedItemHouse.CSI_DescriptionReadOnly);
		}

		protected override IEnumerable<TemporaryStorageAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var tempHeader = factory.New<TemporaryStorageHeader>();
			var bill = factory.New<TemporaryStorageBill>();
			bill.ABL_AMA = tempHeader.PK;
			yield return bill.AdditionalInfos.AddNew();
		}
	}
}
