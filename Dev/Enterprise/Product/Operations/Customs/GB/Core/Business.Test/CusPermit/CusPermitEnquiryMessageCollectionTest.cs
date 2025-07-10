using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(CusPermitEnquiryMessageCollection))]
	public class CusPermitEnquiryMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusPermitEnquiryMessageCollection(PermitHeader);

		CusPermitHeader PermitHeader => Factory.New<CusPermitHeader>();
	}
}
