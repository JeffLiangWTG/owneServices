using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetEnumerator()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var collection = new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(header);
			collection.AddNew().ABL_SequenceNumber = 1;
			collection.AddNew().ABL_SequenceNumber = 2;
			var enumerator = collection.GetEnumerator();
			enumerator.MoveNext();
			AssertEquals((ZShort)1, enumerator.Current.ABL_SequenceNumber);
			enumerator.MoveNext();
			AssertEquals((ZShort)2, enumerator.Current.ABL_SequenceNumber);
		}

		public void TestAsEnumerable()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var collection = new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(header);
			AssertSame("Add AsEnumerable to remove the confusion between IEnumerable<T> and IEnumerable<BusinessObject> to enable invocation of IEnumerable<T> extension methods", collection, collection.AsEnumerable());
		}

		public void TestCorrectTypeIsUsedInAddNew()
		{
			var businessObject = Factory.New<AsycudaManifestHeader>();
			var headerMock = new Mock<AsycudaManifestHeader>(Factory, ((INeedRow)businessObject).Row);
			headerMock.CallBase = true;

			var bill = Factory.New<AsycudaBill>();
			var billType = bill.GetType();
			headerMock.Protected().Setup<Type>("GetBillTypeCore").Returns(billType);
			var header = headerMock.Object;
			AssertEquals(billType, header.Bills.TypeOfElements);
			AssertType(billType, header.Bills.AddNew());
		}

		public void TestLoadWithClusterKey()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterbill = Factory.New<AsycudaBill>();
			masterbill.ABL_AMA = header.PK;
			masterbill.ABL_BolType = "XYZ";
			masterbill.ABL_ClusterKey = header.AMA_ClusterKey;
			for (var i = 0; i < 999; i++)
			{
				_ = header.Bills.AddNew();
			}
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			_ = newFactory.Load<AsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_ClusterKey, headerReloaded.AMA_ClusterKey));
			var billsWithoutXYZ = new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(headerReloaded, new ZQuery(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, "XYZ"));
			billsWithoutXYZ.Load();
			var billsWithXYZ = new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(headerReloaded);
			billsWithXYZ.Load();
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaBill.Schema.TableName));
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return (BusinessObjectCollection)header.Bills;
		}
	}
}
