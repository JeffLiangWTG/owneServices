using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEDocCusHAWB))]
	public class UPEDocCusHAWBTest : DocumentWrapperTestCase
	{
		public void TestNew()
		{
			UPEDocCusHAWB doc = UPEDocCusHAWB.New(CusHAWB, Factory);
			AssertNotNull("Doc wrapper of correct type when Callout object passed", doc);
		}

		public void TestConsigneeContactNameOrTheWordCustomer()
		{
			CusHAWB.CS_ConsigneeContactName = "ConsigneeName";
			AssertEquals("When ConsigneeName is available", "ConsigneeName", Doc.ConsigneeContactNameOrTheWordCustomer);
			CusHAWB.CS_ConsigneeContactName = "";
			AssertEquals("When blank", "Customer", Doc.ConsigneeContactNameOrTheWordCustomer);
		}

		public void TestConsignorContactNameOrTheWordShipper()
		{
			CusHAWB.CS_ConsignorContactName = "ConsignorName";
			AssertEquals("When ConsignorName is available", "ConsignorName", Doc.ConsignorContactNameOrTheWordShipper);
			CusHAWB.CS_ConsignorContactName = "";
			AssertEquals("When blank", "Shipper", Doc.ConsignorContactNameOrTheWordShipper);
		}

		public void TestTotalSplitShipmentPiecesManifested()
		{
			UPECusHAWB cusHAWB1 = Factory.NewWithValidTestData<UPECusHAWB>();
			UPECusHAWB cusHAWB2 = Factory.NewWithValidTestData<UPECusHAWB>();
			UPECusHAWB cusHAWB3 = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB1.CS_HAWB = "SPLITHBL";
			cusHAWB1.CS_PiecesManifested = 2;
			cusHAWB2.CS_HAWB = "SPLITHBL";
			cusHAWB2.CS_PiecesManifested = 3;
			cusHAWB3.CS_HAWB = "SPLITHBL";
			cusHAWB3.CS_PiecesManifested = 4;
			UPEDocCusHAWB doc1 = UPEDocCusHAWB.New(cusHAWB1, Factory);
			UPEDocCusHAWB doc2 = UPEDocCusHAWB.New(cusHAWB2, Factory);
			UPEDocCusHAWB doc3 = UPEDocCusHAWB.New(cusHAWB3, Factory);
			AssertEquals(9m, doc1.TotalSplitShipmentPiecesManifested);
			AssertEquals(9m, doc2.TotalSplitShipmentPiecesManifested);
			AssertEquals(9m, doc3.TotalSplitShipmentPiecesManifested);
		}

		public void TestDeclarationEntryNumberOrPending()
		{
			AssertEquals("Empty if there is no formal dec", true, Doc.DeclarationEntryNumberOrPending.IsEmpty);
			CusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			CusHAWB.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("(Pending) if the declaration has not been sent", "(Pending)", Doc.DeclarationEntryNumberOrPending);
			CusEntryHeader entryHeader = CusHAWB.Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123";
			AssertEquals("When there is an entry number available", "123", Doc.DeclarationEntryNumberOrPending);
		}

		public void TestUPETaxInvoiceImage()
		{
			AssertNull("Defult should be null", Doc.UPETaxInvoiceImage);
			Image testImage = new Bitmap(1, 1);
			UPEDataRegistry.Instance.TaxInvoiceImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testImage);
			AssertNotNull("Should not be null", Doc.UPETaxInvoiceImage);
		}

		public void TestHasUPETaxInvoiceImage()
		{
			Assert("Defult should be false", !Doc.HasUPETaxInvoiceImage);
			Image testImage = new Bitmap(1, 1);
			UPEDataRegistry.Instance.TaxInvoiceImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testImage);
			Assert("Should be true", Doc.HasUPETaxInvoiceImage);
		}

		#region UPE HAWB
		#region Consignee Address
		public void TestConsigneeAddress1()
		{
			CusHAWB.CS_ConsigneeName = "ConsigneeName";
			AssertEquals("ConsigneeName", Doc.ConsigneeAddress1);
			CusHAWB.CS_OA_ConsigneeAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignee.OH_FullName = "ConsigneeNameOnOrg";
			AssertEquals("ConsigneeNameOnOrg", Doc.ConsigneeAddress1);
		}

		public void TestConsigneeAddress2()
		{
			CusHAWB.CS_ConsigneeStreet = "ConsigneeStreet";
			AssertEquals("ConsigneeStreet", Doc.ConsigneeAddress2);
			CusHAWB.CS_OA_ConsigneeAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignee.MainAddress.OA_Address1 = "ConsigneeStreetOnOrg";
			AssertEquals("ConsigneeStreetOnOrg", Doc.ConsigneeAddress2);
		}

		public void TestConsigneeAddress3()
		{
			CusHAWB.CS_ConsigneeStreet2 = "ConsigneeStreet2";
			AssertEquals("ConsigneeStreet2", Doc.ConsigneeAddress3);
			CusHAWB.CS_OA_ConsigneeAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignee.MainAddress.OA_Address2 = "ConsigneeStreet2OnOrg";
			AssertEquals("ConsigneeStreet2OnOrg", Doc.ConsigneeAddress3);
		}

		public void TestConsigneeAddress4()
		{
			CusHAWB.CS_ConsigneeCity = "Cape Town";
			CusHAWB.CS_ConsigneeState = "Western Cape";
			AssertEquals("Cape Town Western Cape", Doc.ConsigneeAddress4);
			CusHAWB.CS_OA_ConsigneeAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignee.MainAddress.OA_City = "One You";
			CusHAWB.Consignee.MainAddress.OA_State = "State";
			AssertEquals("One You State", Doc.ConsigneeAddress4);
		}

		public void TestConsigneeAddress5()
		{
			CusHAWB.CS_RN_NKConsigneeCountry = "ZA";
			CusHAWB.CS_ConsigneePostcode = "7550";
			AssertEquals("ZA 7550", Doc.ConsigneeAddress5);
			CusHAWB.CS_OA_ConsigneeAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignee.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			CusHAWB.Consignee.MainAddress.OA_PostCode = "2000";
			AssertEquals("United States 2000", Doc.ConsigneeAddress5);
		}

		#endregion
		#region Consignor Address
		public void TestConsignorAddress1()
		{
			CusHAWB.CS_ConsignorName = "ConsignorName";
			AssertEquals("ConsignorName", Doc.ConsignorAddress1);
			CusHAWB.CS_OA_ConsignorAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignor.OH_FullName = "ConsignorNameOnOrg";
			AssertEquals("ConsignorNameOnOrg", Doc.ConsignorAddress1);
		}

		public void TestConsignorAddress2()
		{
			CusHAWB.CS_ConsignorStreet = "ConsignorStreet";
			AssertEquals("ConsignorStreet", Doc.ConsignorAddress2);
			CusHAWB.CS_OA_ConsignorAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignor.MainAddress.OA_Address1 = "ConsignorStreetOnOrg";
			AssertEquals("ConsignorStreetOnOrg", Doc.ConsignorAddress2);
		}

		public void TestConsignorAddress3()
		{
			CusHAWB.CS_ConsignorStreet2 = "ConsignorStreet2";
			AssertEquals("ConsignorStreet2", Doc.ConsignorAddress3);
			CusHAWB.CS_OA_ConsignorAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignor.MainAddress.OA_Address2 = "ConsignorStreet2OnOrg";
			AssertEquals("ConsignorStreet2OnOrg", Doc.ConsignorAddress3);
		}

		public void TestConsignorAddress4()
		{
			CusHAWB.CS_ConsignorCity = "Cape Town";
			CusHAWB.CS_ConsignorState = "Western Cape";
			AssertEquals("Cape Town Western Cape", Doc.ConsignorAddress4);
			CusHAWB.CS_OA_ConsignorAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignor.MainAddress.OA_City = "One You";
			CusHAWB.Consignor.MainAddress.OA_State = "State";
			AssertEquals("One You State", Doc.ConsignorAddress4);
		}

		public void TestConsignorAddress5()
		{
			CusHAWB.CS_RN_NKConsignorCountry = "ZA";
			CusHAWB.CS_ConsignorPostcode = "7550";
			AssertEquals("ZA 7550", Doc.ConsignorAddress5);
			CusHAWB.CS_OA_ConsignorAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.Consignor.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			CusHAWB.Consignor.MainAddress.OA_PostCode = "2000";
			AssertEquals("United States 2000", Doc.ConsignorAddress5);
		}

		#endregion
		#region BillTo Address
		public void TestBillToHAWBAddress1()
		{
			AssertEquals("", Doc.BillToHAWBAddress1);
		}

		public void TestBillToHAWBAddress2()
		{
			AssertEquals("", Doc.BillToHAWBAddress2);
		}

		public void TestBillToHAWBAddress3()
		{
			AssertEquals("", Doc.BillToHAWBAddress3);
		}

		public void TestBillToHAWBAddress4()
		{
			AssertEquals("", Doc.BillToHAWBAddress4);
		}

		public void TestBillToHAWBAddress5()
		{
			AssertEquals("", Doc.BillToHAWBAddress5);
		}

		#endregion
		public void TestHouseBill()
		{
			CusHAWB.CS_HAWB = "Test";
			AssertEquals("Test", Doc.HouseBill);
		}

		public void TestMasterBill()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "08112345678";
			CusHAWB.CS_CM = uPECusMAWB.PK;
			AssertEquals("081-12345678", Doc.MasterBill);
		}

		public void TestAirlinePrefix()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "08112345678";
			CusHAWB.CS_CM = uPECusMAWB.PK;
			AssertEquals("081", Doc.AirlinePrefix);
		}

		public void TestAWBSerialNumber()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "08112345678";
			CusHAWB.CS_CM = uPECusMAWB.PK;
			AssertEquals("12345678", Doc.AWBSerialNumber);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("", Doc.InvoiceNumber);
		}

		public void TestAWBOrigin()
		{
			CusHAWB.CS_RL_NKOrigin = "USLAX";
			AssertEquals("LAX", Doc.AWBOrigin);
		}

		public void TestOrigin()
		{
			CusHAWB.CS_RL_NKOrigin = "USLAX";
			AssertEquals("USLAX", Doc.Origin);
		}

		public void TestDestination()
		{
			CusHAWB.CS_RL_NKDestination = "SGSIN";
			AssertEquals("SGSIN", Doc.Destination);
		}

		public void TestArrivalDate()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_ArrivalDate = new ZDateTime(2006, 01, 01);
			CusHAWB.CS_CM = uPECusMAWB.PK;
			AssertEquals(new ZDateTime(2006, 01, 01), Doc.ArrivalDate);
		}

		public void TestCurrencyCode()
		{
			CusHAWB.CS_RX_NKGoodsCurrency = "AUD";
			AssertEquals("AUD", Doc.CurrencyCode);
		}

		public void TestUQ()
		{
			CusHAWB.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("K", Doc.UQ);
		}

		public void TestDimensionalWeight()
		{
			CusHAWB.CS_ChargableWeight = 12.23m;
			AssertEquals(12.23m, Doc.DimensionalWeight);
		}

		public void TestOtherCharges1()
		{
			AssertEquals("", Doc.OtherCharges1);
		}

		public void TestOtherCharges2()
		{
			AssertEquals("", Doc.OtherCharges2);
		}

		public void TestOtherCharges3()
		{
			AssertEquals("", Doc.OtherCharges3);
		}

		public void TestOtherCharges4()
		{
			AssertEquals("", Doc.OtherCharges4);
		}

		#endregion
		#region Related Doc Wrappers
		public void TestShipmentHeldLetterDetails()
		{
			AssertNotNull(Doc.ShipmentHeldLetterDetails);
		}

		public void TestFirstAndSubsequentSplitCusHAWB()
		{
			UPECusHAWB firstSplitCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			firstSplitCusHAWB.CS_CM = Factory.NewWithValidTestData<Customs.AU.Declaration.Business.CusMAWB>().PK;
			UPECusHAWB subsequentSplitCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			subsequentSplitCusHAWB.CS_CM = Factory.NewWithValidTestData<Customs.AU.Declaration.Business.CusMAWB>().PK;
			firstSplitCusHAWB.MAWB.CM_MAWB = "11111";
			firstSplitCusHAWB.CS_HAWB = "SPLITHBL";
			firstSplitCusHAWB.MAWB.CM_ArrivalDate = ZDateTime.Now;
			subsequentSplitCusHAWB.MAWB.CM_MAWB = "22222";
			subsequentSplitCusHAWB.CS_HAWB = "SPLITHBL";
			subsequentSplitCusHAWB.MAWB.CM_ArrivalDate = ZDateTime.Now.AddDays(1);
			UPEDocCusHAWB doc1 = UPEDocCusHAWB.New(firstSplitCusHAWB, Factory);
			UPEDocCusHAWB doc2 = UPEDocCusHAWB.New(subsequentSplitCusHAWB, Factory);
			AssertEquals("FirstSplitCusHAWB", firstSplitCusHAWB.MAWB.CM_MAWB, doc1.FirstSplitCusHAWB.MAWB.MAWB);
			AssertEquals("FirstSplitCusHAWB", firstSplitCusHAWB.MAWB.CM_MAWB, doc2.FirstSplitCusHAWB.MAWB.MAWB);
			AssertEquals("SubsequentSplitCusHAWB", subsequentSplitCusHAWB.MAWB.CM_MAWB, doc1.SubsequentSplitCusHAWB.MAWB.MAWB);
			AssertEquals("SubsequentSplitCusHAWB", subsequentSplitCusHAWB.MAWB.CM_MAWB, doc2.SubsequentSplitCusHAWB.MAWB.MAWB);
			subsequentSplitCusHAWB.Delete();
			doc2 = UPEDocCusHAWB.New(firstSplitCusHAWB, Factory); // refresh the RelatedCusHAWBs collection
			AssertEquals("SubsequentSplitCusHAWB when no subsequent split", null, doc2.SubsequentSplitCusHAWB);
		}

		#endregion
		#region Implementation
		UPECusHAWB CusHAWB;
		UPEDocCusHAWB Doc;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			CusHAWB = Factory.New<UPECusHAWB>();
			Doc = UPEDocCusHAWB.New(CusHAWB, Factory);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { UPEDocCusHAWB.New(CusHAWB, Factory) };
		}
		#endregion
	}
}
