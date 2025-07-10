using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTransferHeaderCollection<AsycudaTransferHeader>))]
	public class AsycudaTransferHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaTransferHeaderCollection<AsycudaTransferHeader>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			return new AsycudaTransferHeaderCollection<AsycudaTransferHeader>(arrivalHeader);
		}
	}
}
