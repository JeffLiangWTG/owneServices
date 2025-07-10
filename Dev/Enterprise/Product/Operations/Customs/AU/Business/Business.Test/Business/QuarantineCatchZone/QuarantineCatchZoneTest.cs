using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineCatchZone))]
	public class QuarantineCatchZoneTest : CusCodeDataTest<QuarantineCatchZone>
	{
		public void TestSetDefaultValues()
		{
			var quarantineCatchZone = Factory.New<QuarantineCatchZone>();
			AssertEquals(CusCodeDataTypeList.Codes.NEXDOCSCatchZone, quarantineCatchZone.CY_Type);
		}

		public void TestCY_DataMaxLength()
		{
			var quarantineCatchZone = Factory.New<QuarantineCatchZone>();
			AssertEquals(35, quarantineCatchZone.CY_DataInfo.MaxLength);
		}

		protected override IEnumerable<QuarantineCatchZone> GetBizObjsForCorrectlyTypeDecideTest(
			BusinessObjectFactory factory)
		{
			yield return (QuarantineCatchZone)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var exDocHeader = invoiceHeader.QuarantineExDocHeader;
			return exDocHeader.NexDocCatchZones.AddNew();
		}
	}
}
