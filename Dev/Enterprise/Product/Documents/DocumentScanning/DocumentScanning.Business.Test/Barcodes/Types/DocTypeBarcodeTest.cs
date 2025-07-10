using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocTypeBarcodeTest : TransactionedTestCase
	{
		public void TestDocTypeBarcode()
		{
			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=12345678;CIV|");
			AssertEquals("CIV", Barcode.DocType);
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
			AssertEquals("12345678", Barcode.RefCode);

			Barcode = new DocTypeBarcode(MasterFactory, "^DOC=CIV|");
			AssertEquals("CIV", Barcode.DocType);
			AssertEquals(ZString.Empty, Barcode.RefCode);
			AssertEquals(ZString.Empty, Barcode.DocManagerCode);

			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=123456|");
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
			AssertEquals("123456", Barcode.RefCode);
			AssertEquals(ZString.Empty, Barcode.DocType);

			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=12345678;|");
			AssertEquals("", Barcode.DocType);
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
			AssertEquals("12345678", Barcode.RefCode);

			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=12345678;;|");
			AssertEquals("", Barcode.DocType);
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
			AssertEquals("12345678", Barcode.RefCode);

			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=12345678;;;|");
			AssertEquals("", Barcode.DocType);
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
			AssertEquals("12345678", Barcode.RefCode);
		}

		public void TestDocTypeBarcodeWithDocTypeTooLong()
		{
			Barcode = new DocTypeBarcode(MasterFactory, "^DOC=D:>!|");
			AssertEquals("D:>!", Barcode.DocType);
			AssertEquals(ZString.Empty, Barcode.RefCode);
			AssertEquals(ZString.Empty, Barcode.DocManagerCode);
		}

		public void TestDocumentType()
		{
			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=10001000;CIV|");
			AssertEquals("CIV", Barcode.DocType);

			Barcode = new DocTypeBarcode(MasterFactory, "^DOC=COO|");
			AssertEquals("COO", Barcode.DocType);
		}

		public void TestReferenceType()
		{
			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=10001000;CIV|");
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, Barcode.DocManagerCode);
		}

		public void TestReferenceCode()
		{
			Barcode = new DocTypeBarcode(MasterFactory, "^SHP=10001000;CIV|");
			AssertEquals("10001000", Barcode.RefCode);
		}

		public void TestCompanyCode()
		{
			AccTransactionHeader payment1 = MasterFactory.NewWithValidTestData<AccTransactionHeader>();
			payment1.AH_TransactionType = TransactionTypes.Payment;
			payment1.AH_Ledger = LedgerTypes.AccountsPayable;
			payment1.AH_TransactionNum = "00001000";
			payment1.AH_GC = GlbCompany.CurrentCompany.PK;
			MasterFactory.Save();

			AssertEquals("Should not find any payment (because it's for a different company)", ZGuid.Empty, new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY@NEW|").RefPK);

			GlbCompany newCompany = MasterFactory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "NEW";
			AccTransactionHeader payment2 = MasterFactory.NewWithValidTestData<AccTransactionHeader>();
			payment2.AH_TransactionType = TransactionTypes.Payment;
			payment2.AH_Ledger = LedgerTypes.AccountsPayable;
			payment2.AH_TransactionNum = "00001000";
			payment2.AH_GC = newCompany.PK;
			MasterFactory.Save();

			AssertEquals("Should reference the correct payment", payment2.PK, new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY@NEW|").RefPK);

			AssertEquals("Shouldn't throw", "", new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY@|").CompanyCode);
			AssertEquals("Shouldn't throw", "", new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY@;|").CompanyCode);
			AssertEquals("Shouldn't throw", "", new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY@@|").CompanyCode);
			AssertEquals("Shouldn't throw", "", new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY@@@|").CompanyCode);
		}

		public void TestBackwardCompatibility_WithoutCompanyCode()
		{
			AccTransactionHeader payment1 = MasterFactory.NewWithValidTestData<AccTransactionHeader>();
			payment1.AH_TransactionType = TransactionTypes.Payment;
			payment1.AH_Ledger = LedgerTypes.AccountsPayable;
			payment1.AH_TransactionNum = "00001000";
			payment1.AH_GC = GlbCompany.CurrentCompany.PK;
			MasterFactory.Save();

			AssertEquals("Should find payment", payment1.PK, new DocTypeBarcode(MasterFactory, "^ACP=00001000;PAY|").RefPK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		DocTypeBarcode Barcode;
		DocumentFactory MasterFactory;
	}
}
