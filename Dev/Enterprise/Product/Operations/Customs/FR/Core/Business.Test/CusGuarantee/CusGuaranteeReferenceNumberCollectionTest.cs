using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	[TestedType(typeof(CusGuaranteeReferenceNumberCollection))]
	public class CusGuaranteeReferenceNumberCollectionTest : CusCodeDataCollectionTest<Customs.Business.CusGuaranteeReferenceNumber>
	{
		protected override Customs.Business.CusCodeDataCollection<Customs.Business.CusGuaranteeReferenceNumber> GetCusCodeDataCollection()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			return new CusGuaranteeReferenceNumberCollection(guaranteeHeader);
		}
	}
}
