using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EU.H7.Business.Testing.CusGoodsLocationTest
	{
		protected override Type LookupType => typeof(CusGoodsLocationLookups);

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<H7ManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill.CusGoodsLocation;
		}
	}
}
