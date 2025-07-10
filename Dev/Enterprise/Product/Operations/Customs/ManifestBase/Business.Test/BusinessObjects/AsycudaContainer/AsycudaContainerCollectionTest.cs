using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>))]
	class AsycudaContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCorrectTypeIsUsedInAddNew()
		{
			var businessObject = Factory.New<AsycudaManifestHeader>();
			var headerMock = new Mock<AsycudaManifestHeader>(Factory, ((INeedRow)businessObject).Row);
			headerMock.CallBase = true;

			var container = Factory.New<AsycudaContainer>();
			var containerType = container.GetType();
			headerMock.Protected().Setup<Type>("GetContainerTypeCore").Returns(containerType);
			var header = headerMock.Object;
			AssertEquals(containerType, header.Containers.TypeOfElements);
			AssertType(containerType, header.Containers.AddNew());
		}

		public void TestLoadWithClusterKey()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			for (var i = 0; i < 500; i++)
			{
				_ = header.Containers.AddNew();
			}
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			_ = newFactory.Load<AsycudaContainer>(new ZQuery(AsycudaContainerSchema.ACN_ClusterKey, headerReloaded.AMA_ClusterKey));
			var containers = new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(headerReloaded);
			containers.Load();
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaContainer.Schema.TableName));
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return (BusinessObjectCollection)header.Containers;
		}
	}
}
