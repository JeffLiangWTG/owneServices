using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ESSupportingDocumentHelperTest : EU.Business.Testing.EUSupportingDocumentHelperTest
	{
		public override void TestGetInvoiceSupportingDocumentTypes()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var supportingDocumentHelper = new ESSupportingDocumentHelper(declaration);

				declaration.JE_MessageType = "IMP";
				AssertContainsExactElementsInAnyOrder("GetInvoiceSupportingDocumentTypes() for IMP", new ZString[] { "N380", "N325", "N935", "D005", "D008", "1001", "1003" }, supportingDocumentHelper.GetInvoiceSupportingDocumentTypes());

				declaration.JE_MessageType = "EXP";
				AssertContainsExactElementsInAnyOrder("GetInvoiceSupportingDocumentTypes() for EXP", new ZString[] { "N380", "N325", "N935" }, supportingDocumentHelper.GetInvoiceSupportingDocumentTypes());

				declaration.JE_MessageType = "COM";
				AssertContainsExactElementsInAnyOrder("GetInvoiceSupportingDocumentTypes() for COM", Array.Empty<ZString>(), supportingDocumentHelper.GetInvoiceSupportingDocumentTypes());
			});
		}
		public void TestGetInvoiceTransportSupportingDocumentTypes()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var supportingDocumentHelper = new ESSupportingDocumentHelper(declaration);
				AssertContainsExactElementsInAnyOrder("GetInvoiceTransportSupportingDocumentTypes()", new ZString[] { "N705", "N710", "N720", "N730", "N740", "N750", "N760", "N785", "N271", "1002", "1833", "C618", "C619", "C625" }, supportingDocumentHelper.GetInvoiceTransportSupportingDocumentTypes());
			});
		}
		public void TestIsTransportType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocumentHelper = new ESSupportingDocumentHelper(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("When type is N705", ZBool.True, supportingDocumentHelper.IsTransportType("N705"));
				AssertEquals("When type is AAA", ZBool.False, supportingDocumentHelper.IsTransportType("AAA"));
				AssertEquals("When type is Empty", ZBool.False, supportingDocumentHelper.IsTransportType(""));
			});
		}
	}
}
