using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTaxCollection<AsycudaTax, AsycudaBillWithIAsycudaTaxTypeSupporter>))]
	class AsycudaTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaTaxCollection<AsycudaTax, AsycudaBillWithIAsycudaTaxTypeSupporter>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBillWithIAsycudaTaxTypeSupporter>();
			header.Bills.Add(bill);
			return new AsycudaTaxCollection<AsycudaTax, AsycudaBillWithIAsycudaTaxTypeSupporter>(bill);
		}
	}

	class AsycudaBillWithIAsycudaTaxTypeSupporter : AsycudaBill
		, IAsycudaTaxTypeSupporter
	{
		public AsycudaBillWithIAsycudaTaxTypeSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Type GetAsycudaTaxType() => typeof(AsycudaTax);
	}
}
