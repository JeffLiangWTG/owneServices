using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PreviousDocument))]
class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
{
	public void TestCSI_CodeMaxLength()
	{
		AssertEquals(6, PreviousDocument.CSI_CodeInfo.MaxLength);
	}

	public void TestCSI_ReferenceNumberMaxLength()
	{
		AssertEquals(35, PreviousDocument.CSI_ReferenceNumberInfo.MaxLength);
	}
	public void TestCSI_DescriptionMaxLength()
	{
		AssertEquals(70, PreviousDocument.CSI_DescriptionInfo.MaxLength);
	}

	public void TestCSI_CodeCaption()
	{
		AssertEquals("Type", PreviousDocument.CSI_CodeInfo.Description);
	}

	public void TestCSI_ReferenceNumberCaption()
	{
		AssertEquals("Reference", PreviousDocument.CSI_ReferenceNumberInfo.Description);
	}
	public void TestCSI_DescriptionCaption()
	{
		AssertEquals("Additional Information", PreviousDocument.CSI_DescriptionInfo.Description);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewPreviousDocument(Factory);

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewPreviousDocument(factory);
	}

	PreviousDocument GetNewPreviousDocument(BusinessObjectFactory factory)
	{
		return factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.AddNew();
	}

	PreviousDocument PreviousDocument => previousDocument ?? (previousDocument = GetNewPreviousDocument(Factory));
	PreviousDocument previousDocument;
}
