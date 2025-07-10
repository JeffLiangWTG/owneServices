using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NotifyCustomsOffice))]
class NotifyCustomsOfficeTest : Customs.Business.Testing.CusCodeDataTest<NotifyCustomsOffice>
{
	public void TestHumanReadableName()
	{
		AssertEquals("Notify Customs Office", NotifyCustomsOffice.HumanReadableName);
	}

	public void TestSetDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals(CusCodeDataTypeList.Codes.NotifyCustomsOffice, NotifyCustomsOffice.CY_Type);
			AssertEquals(ZArchitecture.Schema.JobComInvoiceLineSchema.Constants.Prefix, NotifyCustomsOffice.CY_ParentTableCode);
		});
	}

	public void TestCY_DataMaxLength() => AssertEquals(8, NotifyCustomsOffice.CY_DataInfo.MaxLength);

	public void TestCY_DataCaption() => AssertEquals("Customs Office", NotifyCustomsOffice.CY_DataInfo.Description);

	public void TestDataDescriptionCaption() => AssertEquals("Description", NotifyCustomsOffice.DataDescriptionInfo.Description);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override IEnumerable<NotifyCustomsOffice> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var notifyCustomsOffice = GetNewBusinessObject(factory);
		notifyCustomsOffice.CY_Data = "CH001801";
		yield return notifyCustomsOffice;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();

	NotifyCustomsOffice GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var inAndOutwardProcessing = invoiceLine.InAndOutwardProcessings.AddNew();
		inAndOutwardProcessing.CSI_SubType = "0";
		return invoiceLine.NotifyCustomsOffices.AddNew();
	}

	NotifyCustomsOffice NotifyCustomsOffice => notifyCustomsOffice ?? (notifyCustomsOffice = GetNewBusinessObject() as NotifyCustomsOffice);
	NotifyCustomsOffice notifyCustomsOffice;
}
