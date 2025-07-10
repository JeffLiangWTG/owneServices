using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ParcelCollection))]
	sealed class ParcelCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<Parcel>
	{
		protected override CusSupportingInfoCollection<Parcel> GetCusSupportingInfoCollection()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			return new ParcelCollection(invoice);
		}
	}
}
