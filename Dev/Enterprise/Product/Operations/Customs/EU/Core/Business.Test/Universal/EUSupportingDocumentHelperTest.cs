using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class EUSupportingDocumentHelperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be Exception when parameter is null", () => new EUSupportingDocumentHelper(null));

			var declaration = Factory.New<JobDeclaration>();
			AssertNoExceptionThrown(() => new EUSupportingDocumentHelper(declaration));

			var invoice = declaration.Invoices.AddNew();
			AssertNoExceptionThrown(() => new EUSupportingDocumentHelper(invoice));
		}

		public virtual void TestGetInvoiceSupportingDocumentTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocumentHelper = new EUSupportingDocumentHelper(declaration);

			declaration.JE_MessageType = "IMP";
			AssertContainsExactElementsInAnyOrder("GetInvoiceSupportingDocumentTypes() for IMP", new ZString[] { "N380", "N325", "N935", "D005", "D008" }, supportingDocumentHelper.GetInvoiceSupportingDocumentTypes());

			declaration.JE_MessageType = "EXP";
			AssertContainsExactElementsInAnyOrder("GetInvoiceSupportingDocumentTypes() for EXP", new ZString[] { "N380", "N325", "N935" }, supportingDocumentHelper.GetInvoiceSupportingDocumentTypes());

			declaration.JE_MessageType = "COM";
			AssertContainsExactElementsInAnyOrder("GetInvoiceSupportingDocumentTypes() for COM", Array.Empty<ZString>(), supportingDocumentHelper.GetInvoiceSupportingDocumentTypes());
		}

		public virtual void TestIsInvoiceType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocumentHelper = new EUSupportingDocumentHelper(declaration);

			declaration.JE_MessageType = "IMP";
			CombineAssertions("IsInvoiceType when parent is IMP", () =>
			{
				AssertEquals("When type is N380", ZBool.True, supportingDocumentHelper.IsInvoiceType("N380"));
				AssertEquals("When type is AAA", ZBool.False, supportingDocumentHelper.IsInvoiceType("AAA"));
				AssertEquals("When type is Empty", ZBool.False, supportingDocumentHelper.IsInvoiceType(""));
			});

			declaration.JE_MessageType = "EXP";
			CombineAssertions("IsInvoiceType when parent is EXP", () =>
			{
				AssertEquals("When type is N380", ZBool.True, supportingDocumentHelper.IsInvoiceType("N380"));
				AssertEquals("When type is D005", ZBool.False, supportingDocumentHelper.IsInvoiceType("D005"));
				AssertEquals("When type is Empty", ZBool.False, supportingDocumentHelper.IsInvoiceType(""));
			});
		}
	}
}
