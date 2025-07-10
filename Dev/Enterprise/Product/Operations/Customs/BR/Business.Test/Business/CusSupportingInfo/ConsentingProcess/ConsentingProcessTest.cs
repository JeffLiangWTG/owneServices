using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ConsentingProcess))]
	public class ConsentingProcessTest : Customs.Business.Testing.CusSupportingInfoTest<ConsentingProcess>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ConsentingProcessCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ConsentingProcess>();
			AssertEquals(CusSupportingInfoTypeList.Codes.ConsentingProcess, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<ConsentingProcess> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var consentingProcess = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ConsentingProcessCollection.AddNew();
			consentingProcess.CSI_ReferenceNumber = "1";
			consentingProcess.CSI_CustomsOffice = "TEST_DATA";
			yield return consentingProcess;
		}
	}
}
