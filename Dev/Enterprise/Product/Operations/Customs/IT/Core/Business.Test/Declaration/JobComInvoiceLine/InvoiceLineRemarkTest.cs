using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceLineRemarkTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new InvoiceLineRemark(null));
	}

	public void TestCheckRemarks()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var invoiceLineRemark = new InvoiceLineRemark(invoiceLine);
		invoiceLineRemark.Remarks = "AAA";

		var cusRemGettingQuery = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK)
			.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, invoiceLine.TablePrefix)
			.AddToFilter(CusSupportingInfoSchema.CSI_Type, "REM");

		var remarksCusSupInfo = Factory.LoadTop1<CusSupportingInfo>(cusRemGettingQuery);
		AssertNotNull("The remarks value should be stored in a CusSupportingInfo [REM]", remarksCusSupInfo);
		AssertEquals("CSI_Description", "AAA", remarksCusSupInfo.CSI_Description);

		invoiceLineRemark.Remarks = "";
		remarksCusSupInfo = Factory.LoadTop1<CusSupportingInfo>(cusRemGettingQuery);
		AssertNull("When remarks is empty, the CusSupportingInfo [REM] should be deleted", remarksCusSupInfo);
	}
}
