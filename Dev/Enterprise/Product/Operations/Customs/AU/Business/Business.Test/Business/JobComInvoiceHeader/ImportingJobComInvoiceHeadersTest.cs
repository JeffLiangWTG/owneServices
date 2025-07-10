using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ImportingJobComInvoiceHeadersTest : TestCaseWithFactory
	{
		public void TestJZ_IncoTermNotSetDuringImporting()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostInsuranceAndFreight;
			((ISupportDataImporting)header).IsImportingData = true;
			header.JZ_OH_Supplier = TestSupplierWithIncoTermDefault(Enterprise.Core.Constants.IncoTerms.FreeOnBoard);
			AssertEquals("Inco should not have changed", Enterprise.Core.Constants.IncoTerms.CostInsuranceAndFreight, header.JZ_IncoTerm);
			((ISupportDataImporting)header).IsImportingData = false;
			header.JZ_OH_Supplier = TestSupplierWithIncoTermDefault(Enterprise.Core.Constants.IncoTerms.DeliveredDutyPaid);
			AssertEquals("Inco should have updated with Supplier Being Set", Enterprise.Core.Constants.IncoTerms.DeliveredDutyPaid, header.JZ_IncoTerm);
		}

		#region Implementation

		ZGuid TestSupplierWithIncoTermDefault(ZString incoTerm)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "INCO Org " + incoTerm;
			result.MiscServ.OM_EXDefaultIncoTerm = incoTerm;
			return result.PK;
		}

		#endregion
	}
}
