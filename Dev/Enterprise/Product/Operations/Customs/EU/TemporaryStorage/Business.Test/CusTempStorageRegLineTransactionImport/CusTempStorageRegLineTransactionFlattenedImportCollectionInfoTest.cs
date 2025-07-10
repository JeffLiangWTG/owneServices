using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransactionFlattenedImportCollectionInfo))]
	sealed class CusTempStorageRegLineTransactionFlattenedImportCollectionInfoTest : TestCaseWithFactory
	{
		public void TestSRP_CustomsLocation() => AssertProperty("SRP_CustomsLocation", "Customs Location", true, ZCharacterCasing.Upper);
		public void TestSRH_Reference() => AssertProperty("SRH_Reference", "TSD Number", true, ZCharacterCasing.Normal);
		public void TestSRI_GoodsItemNumber() => AssertProperty("SRI_GoodsItemNumber", "TSD Item Number", true, ZCharacterCasing.Normal);
		public void TestSRL_PackageType() => AssertProperty("SRL_PackageType", "Package Type", true, ZCharacterCasing.Upper);
		public void TestSRL_PackageMarks() => AssertProperty("SRL_PackageMarks", "Package Marks", true, ZCharacterCasing.Normal);
		public void TestSRT_PhysicalInOutDate() => AssertProperty("SRT_PhysicalInOutDate", "Physical In/Out Date", true, ZCharacterCasing.Normal);
		public void TestSRT_TransactionDate() => AssertProperty("SRT_TransactionDate", "Transaction Date", true, ZCharacterCasing.Normal);
		public void TestSRT_GrossWeight() => AssertProperty("SRT_GrossWeight", "Gross Weight in KGs", true, ZCharacterCasing.Normal);
		public void TestSRT_PackageQty() => AssertProperty("SRT_PackageQty", "Package Quantity", true, ZCharacterCasing.Normal);
		public void TestSRT_InternalReferenceType() => AssertProperty("SRT_InternalReferenceType", "Internal Ref. Type", true, ZCharacterCasing.Normal);
		public void TestSRT_InternalReferenceNumber() => AssertProperty("SRT_InternalReferenceNumber", "Internal Ref. Number", true, ZCharacterCasing.Normal);
		public void TestSRT_ReferenceType() => AssertProperty("SRT_ReferenceType", "Reference Type", true, ZCharacterCasing.Normal);
		public void TestSRT_Reference() => AssertProperty("SRT_Reference", "Reference Number", true, ZCharacterCasing.Normal);
		public void TestSRT_Comments() => AssertProperty("SRT_Comments", "Comments", false, ZCharacterCasing.Normal);

		void AssertProperty(ZString propertyName, ZString caption, bool isMandatory, ZCharacterCasing characterCasing)
		{
			var property = Properties.Single(p => p.MappingName == propertyName);

			CombineAssertions(() =>
			{
				AssertNotNull($"{propertyName} exists", property);
				AssertEquals($"{propertyName} Caption", caption, property.HeaderText);
				AssertEquals($"{propertyName} IsMandatory", isMandatory, property.IsMandatory);
				AssertEquals($"{propertyName} Character Casing", characterCasing, property.CharacterCasing);
			});
		}

		ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>[] GetProperties()
		{
			var collection = new CusTempStorageRegLineTransactionFlattenedCollection(Factory);
			var impl = new CusTempStorageRegLineTransactionFlattenedImportCollectionInfo(collection);
			return impl.Cast<ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>>().ToArray();
		}

		ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>[] Properties => properties ??= GetProperties();
		ImportPropertyInfoImpl<CusTempStorageRegLineTransactionFlattened>[] properties;

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.SetOrgAllowMixedCase(true);
		}
	}
}
