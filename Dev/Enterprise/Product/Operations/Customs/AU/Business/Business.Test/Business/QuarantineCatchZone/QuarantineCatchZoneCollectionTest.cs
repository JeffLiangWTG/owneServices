using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineCatchZoneCollection))]
	sealed class QuarantineCatchZoneCollectionTest : CusCodeDataCollectionTest<QuarantineCatchZone>
	{
		protected override CusCodeDataCollection<QuarantineCatchZone> GetCusCodeDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var exDocHeader = invoiceHeader.QuarantineExDocHeader;
			return new QuarantineCatchZoneCollection(exDocHeader);
		}
	}
}
