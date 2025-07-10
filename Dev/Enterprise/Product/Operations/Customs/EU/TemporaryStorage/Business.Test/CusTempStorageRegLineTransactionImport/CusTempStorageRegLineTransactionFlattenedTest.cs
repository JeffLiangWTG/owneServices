using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransactionFlattened))]
	sealed class CusTempStorageRegLineTransactionFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSRP_CustomsLocation() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRP_CustomsLocationInfo, "SRP_CustomsLocation", 35);
		public void TestSRH_Reference() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRH_ReferenceInfo, "SRH_Reference", 35);
		public void TestSRI_GoodsItemNumber() => AssertZIntProperty(CusTempStorageRegHeaderFlattened.SRI_GoodsItemNumberInfo, "SRI_GoodsItemNumber");
		public void TestSRL_PackageType() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRL_PackageTypeInfo, "SRL_PackageType", 3);
		public void TestSRL_PackageMarks() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRL_PackageMarksInfo, "SRL_PackageMarks", 512);
		public void TestSRT_PhysicalInOutDate() => AssertZDateTimeProperty(CusTempStorageRegHeaderFlattened.SRT_PhysicalInOutDateInfo, "SRT_PhysicalInOutDate");
		public void TestSRT_TransactionDate() => AssertZDateTimeProperty(CusTempStorageRegHeaderFlattened.SRT_TransactionDateInfo, "SRT_TransactionDate");
		public void TestSRT_GrossWeight() => AssertZDecimalProperty(CusTempStorageRegHeaderFlattened.SRT_GrossWeight, CusTempStorageRegHeaderFlattened.SRT_GrossWeightInfo, "SRT_GrossWeight", 19, 5);
		public void TestSRT_PackageQty() => AssertZIntProperty(CusTempStorageRegHeaderFlattened.SRT_PackageQtyInfo, "SRT_PackageQty");
		public void TestSRT_InternalReferenceType() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRT_InternalReferenceTypeInfo, "SRT_InternalReferenceType", 3);
		public void TestSRT_InternalReferenceNumber() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRT_InternalReferenceNumberInfo, "SRT_InternalReferenceNumber", 35);
		public void TestSRT_ReferenceType() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRT_ReferenceTypeInfo, "SRT_ReferenceType", 4);
		public void TestSRT_Reference() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRT_ReferenceInfo, "SRT_Reference", 60);
		public void TestSRT_Comments() => AssertZStringProperty(CusTempStorageRegHeaderFlattened.SRT_CommentsInfo, "SRT_Comments", 200);

		void AssertZStringProperty(ZPropertyInfo propertyInfo, ZString name, int maxLength)
		{
			CombineAssertions(() =>
			{
				AssertEquals($"Property: {name}", name, propertyInfo.Name);
				AssertEquals($"{name} Type", "ZString", propertyInfo.PropertyType.Name);
				AssertEquals($"{name} MaxLength", maxLength, propertyInfo.MaxLength);
			});
		}

		void AssertZIntProperty(ZPropertyInfo propertyInfo, ZString name)
		{
			CombineAssertions(() =>
			{
				AssertEquals($"Property: {name}", name, propertyInfo.Name);
				AssertEquals($"{name} Type", "ZInt", propertyInfo.PropertyType.Name);
			});
		}

		void AssertZDateTimeProperty(ZPropertyInfo propertyInfo, ZString name)
		{
			CombineAssertions(() =>
			{
				AssertEquals($"Property: {name}", name, propertyInfo.Name);
				AssertEquals($"{name} Type", "ZDateTime", propertyInfo.PropertyType.Name);
			});
		}

		void AssertZDecimalProperty(ZDecimal property, ZPropertyInfo propertyInfo, ZString name, int precision, int scale)
		{
			CombineAssertions(() =>
			{
				AssertEquals($"Property: {name}", name, propertyInfo.Name);
				AssertEquals($"{name} Type", "ZDecimal", propertyInfo.PropertyType.Name);
				Assert($"{name} Precision / Scale", property.IsWithinSqlPrecisionAndScale(precision, scale));
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusTempStorageRegLineTransactionFlattened();
		}

		#endregion

		CusTempStorageRegLineTransactionFlattened CusTempStorageRegHeaderFlattened => cusTempStorageRegHeaderFlattened ?? (cusTempStorageRegHeaderFlattened = (CusTempStorageRegLineTransactionFlattened)GetNewBusinessObject());
		CusTempStorageRegLineTransactionFlattened cusTempStorageRegHeaderFlattened;
	}
}
