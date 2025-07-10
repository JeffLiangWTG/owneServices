using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class PaymentDocumentTest : ReceiptPaymentBaseDocumentTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			ARPayment = Factory.New<ARPayment>();
			AssertNotNull("ARPayment should not be null", ARPayment);
			DocumentSupporterAR = new Payment.PaymentDocumentSupporter(ARPayment);
			AssertNotNull("Document Supporter AP should not be null", DocumentSupporterAR);

			APPayment = Factory.New<APPayment>();
			AssertNotNull("ARPayment should not be null", APPayment);
			DocumentSupporterAP = new Payment.PaymentDocumentSupporter(APPayment);
			AssertNotNull("Document Supporter AP should not be null", DocumentSupporterAP);
		}

		Payment.PaymentDocumentSupporter DocumentSupporterAP;
		ARPayment ARPayment;
		Payment.PaymentDocumentSupporter DocumentSupporterAR;
		APPayment APPayment;

		public void TestGetAdditionalDeliveryContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			ARPayment.AH_OC_InvoiceContactOverride = contact.PK;
			AssertEquals(DocumentSupporterAR.GetAdditionalDeliveryContact().PK, contact.PK);

			ARPayment.AH_OC_InvoiceContactOverride = ZGuid.Empty;
			AssertEquals(DocumentSupporterAR.GetAdditionalDeliveryContact(), null);

			APPayment.AH_OC_InvoiceContactOverride = contact.PK;
			AssertEquals(DocumentSupporterAP.GetAdditionalDeliveryContact().PK, contact.PK);

			APPayment.AH_OC_InvoiceContactOverride = ZGuid.Empty;
			AssertEquals(DocumentSupporterAP.GetAdditionalDeliveryContact(), null);
		}

		public void TestGetOverriddenDeliveryDetails()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "123 Main Street";
			ARPayment.AH_OA_InvoiceAddressOverride = address.PK;
			AssertNotNull("Overridden delivery address should not be null as invoice address is overridden", DocumentSupporterAR.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY));
			AssertEquals(DocumentSupporterAR.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY).E2_Address1, address.OA_Address1);

			ARPayment.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			AssertNull("Overridden delivery address should be null as invoice address is not overridden", DocumentSupporterAR.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY));

			APPayment.AH_OA_InvoiceAddressOverride = address.PK;
			AssertNotNull("Overridden delivery address should not be null as invoice address is overridden", DocumentSupporterAP.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY));
			AssertEquals(DocumentSupporterAP.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY).E2_Address1, address.OA_Address1);

			APPayment.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			AssertNull("Overridden delivery address should be null as invoice address is not overridden", DocumentSupporterAP.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY));
		}
	}
}
