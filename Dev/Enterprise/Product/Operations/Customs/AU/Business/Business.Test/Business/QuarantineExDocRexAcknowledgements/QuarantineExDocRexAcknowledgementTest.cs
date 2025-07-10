using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocRexAcknowledgement))]
	sealed class QuarantineExDocRexAcknowledgementTest : CusCodeDataTest<QuarantineExDocRexAcknowledgement>
	{
		public void TestSetDefaultValues()
		{
			var ack = Factory.New<QuarantineExDocRexAcknowledgement>();
			AssertEquals(QuarantineExDocRexAcknowledgement.AcknowledgementCode, ack.CY_Type);
			AssertEquals(QuarantineExDocRexAcknowledgement.AcknowledgementCode, ack.CY_Code);
		}

		public void TestCY_DataMaxLength()
		{
			var ack = Factory.New<QuarantineExDocRexAcknowledgement>();
			AssertEquals(10, ack.CY_DataInfo.MaxLength);
		}

		protected override IEnumerable<QuarantineExDocRexAcknowledgement> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (QuarantineExDocRexAcknowledgement)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<QuarantineExDocRexAcknowledgement>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invHeader = declaration.Invoices.AddNew();
			var quarantine = invHeader.QuarantineExDocHeader;
			return quarantine.Acknowledgements.AddNew();
		}
	}
}
