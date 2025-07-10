using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceHeaderAttachDefaultsTest : BaseJobComInvoiceHeaderAttachDefaultsTest
	{
		public new void TestDefaultsRefreshedWhenUnamendedInvoiceAttached()
		{
			AssertEquals("precondition description", "", line.JI_Description);
			AssertEquals("precondition calssification", ZGuid.Empty, line.JI_CC);
			AssertEquals("precondition value basis", "TV", ((JobComInvoiceHeader)invoice).AddInfo.ZA_VALB_Hidden);
			AssertEquals("precondition related transaction", "N", ((JobComInvoiceHeader)invoice).AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("precondition origin", "AU", ((JobComInvoiceHeader)invoice).AddInfo.ZA_ORG);
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoice);
			AssertEquals("description refreshed", "TESTPARTDESCRIPTION", line.JI_Description);
			AssertEquals("classification refreshed", classification.PK, line.JI_CC);
			AssertEquals("value basis refreshed", "TV", ((JobComInvoiceHeader)invoice).AddInfo.ZA_VALB_Hidden);
			AssertEquals("related transaction refreshed", "N", ((JobComInvoiceHeader)invoice).AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("origin refreshed", "AU", ((JobComInvoiceHeader)invoice).AddInfo.ZA_ORG);

			declaration.Invoices.RemoveFromRelationship(invoice);
			((JobComInvoiceHeader)invoice).AddInfo.ZA_VALB_Hidden = string.Empty;
			((JobComInvoiceHeader)invoice).AddInfo.ZA_HeaderREL_Hidden = string.Empty;
			((JobComInvoiceHeader)invoice).AddInfo.ZA_ORG = string.Empty;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoice);
			AssertEquals("value basis refreshed", "IG", ((JobComInvoiceHeader)invoice).AddInfo.ZA_VALB_Hidden);
			AssertEquals("related transaction refreshed", "Y", ((JobComInvoiceHeader)invoice).AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("origin refreshed", "US", ((JobComInvoiceHeader)invoice).AddInfo.ZA_ORG);
		}

		public new void TestDefaultsNotRefreshedWhenAmendedInvoiceAttached()
		{
			AssertEquals("precondition description", "", line.JI_Description);
			AssertEquals("precondition calssification", ZGuid.Empty, line.JI_CC);
			AssertEquals("precondition value basis", "TV", ((JobComInvoiceHeader)invoice).AddInfo.ZA_VALB_Hidden);
			AssertEquals("precondition related transaction", "N", ((JobComInvoiceHeader)invoice).AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("precondition origin", "AU", ((JobComInvoiceHeader)invoice).AddInfo.ZA_ORG);
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = false;
			declaration.Invoices.Add(invoice);
			AssertEquals("description", "", line.JI_Description);
			AssertEquals("calssification", ZGuid.Empty, line.JI_CC);
			AssertEquals("value basis", "TV", ((JobComInvoiceHeader)invoice).AddInfo.ZA_VALB_Hidden);
			AssertEquals("related transaction", "N", ((JobComInvoiceHeader)invoice).AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("origin", "AU", ((JobComInvoiceHeader)invoice).AddInfo.ZA_ORG);
		}

		#region Implementation
		BaseCusClassification classification;
		OrgHeader importer;

		protected override void SetUp()
		{
			base.SetUp();

			classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_Description = "ClassificationDescription";
			classification.CC_LookupCode = "LOOKUP";
			classification.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			var partPivot = part.PivotsForBinding.AddNew();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			partPivot.CI_CC = classification.PK;
			partPivot.CI_OP = part.PK;

			importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(importer);
			link.OL_ValuationBasis = "IG";
			link.OL_RelatedParty = "Y";
			declaration.JE_OH_Importer = importer.PK;

			supplier.OH_RL_NKClosestPort = "USLAX";
		}
		#endregion
	}
}
