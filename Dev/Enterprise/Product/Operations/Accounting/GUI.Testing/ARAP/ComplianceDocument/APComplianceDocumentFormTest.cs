using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(ComplianceDocumentForm))]
	public class APComplianceDocumentFormTest : ComplianceDocumentFormTestCase
	{
		protected override ComplianceDocumentForm GetFormByComplianceDocument(AccComplianceDocumentHeader complianceDocumentHeader)
		{
			return new TestComplianceDocumentForm(complianceDocumentHeader)
			{
				ControllerID = ControllerIDs.APComplianceDocument
			};
		}

		protected override ComplianceDocumentForm GetFormByVoidComplianceDocument(AccComplianceDocumentHeader complianceDocumentHeader)
		{
			return new ComplianceDocumentForm(complianceDocumentHeader)
			{
				ControllerID = ControllerIDs.APComplianceDocument,
				VoidInsteadOfDelete = true
			};
		}

		protected override AccComplianceDocumentHeader GetComplianceDocumentWithValidTestData(bool fillTestData = true)
		{
			var header = Factory.New<APComplianceDocumentHeader>();
			if (fillTestData)
			{
				header.FillWithValidTestData();
			}

			return header;
		}
	}
}
