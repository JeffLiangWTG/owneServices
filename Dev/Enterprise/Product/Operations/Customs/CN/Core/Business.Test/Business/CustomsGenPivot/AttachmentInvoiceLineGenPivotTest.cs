using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AttachmentInvoiceLineGenPivot))]
	class AttachmentInvoiceLineGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(GenPivotTypeDecider.Types.AttachmentInvoiceLineLink, AttachmentInvoiceLineGenPivot.XX_RelationType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AttachmentInvoiceLineGenPivot;
		}

		AttachmentInvoiceLineGenPivot AttachmentInvoiceLineGenPivot => attachmentInvoiceLineGenPivot ??= Factory.New<AttachmentInvoiceLineGenPivot>();
		AttachmentInvoiceLineGenPivot attachmentInvoiceLineGenPivot;
	}
}
