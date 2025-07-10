using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTransferBillCollection<AsycudaTransferBill>))]
	public class AsycudaTransferBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaTransferBillCollection<AsycudaTransferBill>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			return new AsycudaTransferBillCollection<AsycudaTransferBill>(transferHeader);
		}
	}
}
