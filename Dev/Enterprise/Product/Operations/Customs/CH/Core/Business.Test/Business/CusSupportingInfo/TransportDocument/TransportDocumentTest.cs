using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TransportDocument))]
public sealed class TransportDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<TransportDocument>
{
	public void TestReferenceNumber()
	{
		AssertEquals("MaxLength", 70, TransportDocument.CSI_ReferenceNumberInfo.MaxLength);
		AssertEquals("Caption", "Reference", TransportDocument.CSI_ReferenceNumberInfo.Description);
	}

	public void TestCSI_Code()
	{
		AssertEquals("Caption", "Type", TransportDocument.CSI_CodeInfo.Description);
	}

	protected override IEnumerable<TransportDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.Invoices.AddNew().TransportDocuments.AddNew();
		yield return declaration.CustomsEntryInstructions.AddNew().TransportDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return TransportDocument;
	}

	public TransportDocument TransportDocument => transportDocument ?? (transportDocument = GetTransportDocument());
	TransportDocument transportDocument;

	TransportDocument GetTransportDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		return invoiceHeader.TransportDocuments.AddNew();
	}
}
