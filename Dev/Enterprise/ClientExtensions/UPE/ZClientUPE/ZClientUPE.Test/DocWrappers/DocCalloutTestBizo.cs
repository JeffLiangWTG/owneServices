using System;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(DocCallout))]
	sealed class DocCalloutTestBizo : DocumentWrapperTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCommercialInvoiceAndCommercialInvoiceImageCollection()
		{
			ZString sampleFilePath = BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\ZClientUPE.Test\DocWrappers\Testing\SampleImage.TIF";
			DocumentFactory documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			Callout callout = Factory.NewWithValidTestData<Callout>();
			StorageMain storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_ParentFK = callout.PK;
			StorageDocs aDocument = storageMain.Documents.AddNew();
			aDocument.SC_Date = ZDateTime.Now;
			aDocument.SC_ImageData = File.ReadAllBytes(sampleFilePath);
			aDocument.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			callout.DocManagerInfo.Documents.Add(aDocument);
			callout.DocManagerInfo.Save();
			AssertNotNull("Should find the Commercial Invoice document from eDocs", callout.CommercialInvoiceImage);
			using (Image testImage = Image.FromFile(sampleFilePath))
			{
				using (StandardImagePageSelector imagePageSelector = new StandardImagePageSelector(testImage))
				{
					AssertEquals("Current index", 0, imagePageSelector.CurrentPageIndex);
					AssertEquals("Total pages", 2, imagePageSelector.TotalPages);
					DocCallout calloutWrapper = DocCallout.New(callout, Factory);
					AssertEquals("The collection should have the same number of images as the total pages", imagePageSelector.TotalPages, calloutWrapper.CommercialInvoiceAsMultiPageImageCollection.Count);
					AssertNotNull("Image", imagePageSelector.CurrentImage);
					AssertNotNull("Doc Image", calloutWrapper.CommercialInvoiceAsMultiPageImageCollection[imagePageSelector.CurrentPageIndex]);
					ImageWrapper currentImageWrapper = calloutWrapper.CommercialInvoiceAsMultiPageImageCollection[imagePageSelector.CurrentPageIndex];
					AssertEquals("The images should be the same", imagePageSelector.CurrentImage.Size.Height, currentImageWrapper.Image.Size.Height);
					AssertEquals("The images should be the same", imagePageSelector.CurrentImage.Size.Width, currentImageWrapper.Image.Size.Width);
					imagePageSelector.CurrentPageIndex = 1;
					AssertNotNull("Image", imagePageSelector.CurrentImage);
					AssertNotNull("Doc Image", calloutWrapper.CommercialInvoiceAsMultiPageImageCollection[imagePageSelector.CurrentPageIndex]);
					currentImageWrapper = calloutWrapper.CommercialInvoiceAsMultiPageImageCollection[imagePageSelector.CurrentPageIndex];
					AssertEquals("The images should be the same", imagePageSelector.CurrentImage.Size.Height, currentImageWrapper.Image.Size.Height);
					AssertEquals("The images should be the same", imagePageSelector.CurrentImage.Size.Width, currentImageWrapper.Image.Size.Width);
				}
			}
		}

		public void TestNew()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout docCallout = DocCallout.New(callout, Factory);
			AssertNotNull("Doc wrapper of correct type when Callout object passed", docCallout);
			CusHAWB cusHAWB = Factory.New<CusHAWB>();
			DocCusHAWB docCusHAWB = DocCallout.New(cusHAWB, Factory);
			AssertNotNull("Doc wrapper of correct type when CusHAWB object passed", docCusHAWB);
		}

		#region World Ease Heading Overrides
		public void TestShipmentDetailsHeaderText()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			AssertEquals("When duty type is NOT GCC", "Import Shipment Detail", doc.ShipmentDetailsHeaderText);
			callout.DutyType = DutyTypeCodeDescriptionPairList.Codes.GCC;
			AssertEquals("When duty type is GCC", "World Ease Import Shipment Detail", doc.ShipmentDetailsHeaderText);
		}

		public void TestShipmentNoHeaderText()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			AssertEquals("When duty type is NOT GCC", "Shipment No.", doc.ShipmentNoHeaderText);
			callout.DutyType = DutyTypeCodeDescriptionPairList.Codes.GCC;
			AssertEquals("When duty type is GCC", "World Ease No.", doc.ShipmentNoHeaderText);
		}

		#endregion
		#region Properties
		public void TestBPay()
		{
			AssertEquals("default", false, DocCallout.ShowBPayPaymentOption);
			AccountingConfigurationRegistry.Instance.EnableElectronicPayments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("modified", true, DocCallout.ShowBPayPaymentOption);
			AssertEquals("default", ZString.Empty, DocCallout.BPayBillerCode);
			AccountingConfigurationRegistry.Instance.ElectronicPaymentBillerCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			AssertEquals("modified", "12345", DocCallout.BPayBillerCode);
			ZString bpayDefaultTerms = "Contact your bank or financial institution to make this payment from your cheque, savings, debit, credit card or transaction account.";
			AssertEquals("default", bpayDefaultTerms, DocCallout.BPayTerms);
			AccountingConfigurationRegistry.Instance.ElectronicPaymentTerms.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "blah blah");
			AssertEquals("modified", "blah blah", DocCallout.BPayTerms);
			Callout.InvoiceNumber = "200001659281";
			AssertEquals("Invoice Number and BPay Reference Number should be different.", false, DocCallout.BPayReferenceNumber == DocCallout.InvoiceNumber);
			AssertEquals("Should be Invoice Number with Check Digit", "2000016592814", DocCallout.BPayReferenceNumber);
			Callout.InvoiceNumber = "200001509010";
			AssertEquals("Should be Invoice Number with Check Digit", "2000015090100", DocCallout.BPayReferenceNumber);
		}

		public void TestWayBillShort()
		{
			Callout.WayBillShort = "WayBillShrt";
			AssertEquals("WayBillShrt", DocCallout.WayBillShort);
		}

		public void TestInvoiceNumber()
		{
			Callout.InvoiceNumber = "InvoiceNumber";
			AssertEquals("InvoiceNumber", DocCallout.InvoiceNumber);
		}

		public void TestFreightPrepaidCollectDescription()
		{
			Callout.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertEquals("Freight Collect", DocCallout.FreightPrepaidCollectDescription);
			Callout.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("Prepaid", DocCallout.FreightPrepaidCollectDescription);
			Callout.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.Collect;
			AssertEquals("Freight Collect", DocCallout.FreightPrepaidCollectDescription);
			Callout.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.PrepaidOnly;
			AssertEquals("Prepaid", DocCallout.FreightPrepaidCollectDescription);
			Callout.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.ReturnContainerFreightFree;
			AssertEquals("Return container freight free", DocCallout.FreightPrepaidCollectDescription);
		}

		public void TestTotalChargeDiscounts()
		{
			Callout.EnsureJobHeaderExists();
			CalloutCharge charge1 = Callout.JobHeader.Charges.AddNew();
			charge1.Discount = 2;
			CalloutCharge charge2 = Callout.JobHeader.Charges.AddNew();
			charge2.Discount = 3;
			AssertEquals("Total discount is 2+3=5", 5m, DocCallout.TotalChargeDiscounts);
		}

		public void TestDeliveryInstructionsNote()
		{
			Callout.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "TestNotes");
			AssertEquals("TestNotes", DocCallout.DeliveryInstructionsNote);
		}

		public void TestDeliverToContact()
		{
			Callout.CS_ConsigneeContactName = "CONTACT CONEE NAME";
			Callout.CS_ConsignorContactName = "CONTACT CONOR NAME";
			AssertEquals("CONTACT CONEE NAME", DocCallout.DeliverToContact);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONTACT CONOR NAME", DocCallout.DeliverToContact);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_ContactName = "DELIVER TO CONTACT";
			AssertEquals("DELIVER TO CONTACT", DocCallout.DeliverToContact);
		}

		public void TestDeliverToName()
		{
			Callout.CS_ConsigneeName = "CONEE NAME";
			Callout.CS_ConsignorName = "CONOR NAME";
			AssertEquals("CONEE NAME", DocCallout.DeliverToName);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONOR NAME", DocCallout.DeliverToName);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_CompanyName = "DELIVER TO NAME";
			AssertEquals("DELIVER TO NAME", DocCallout.DeliverToName);
		}

		public void TestDeliverToStreet1()
		{
			Callout.CS_ConsigneeStreet = "CONEE STREET1";
			Callout.CS_ConsignorStreet = "CONOR STREET1";
			AssertEquals("CONEE STREET1", DocCallout.DeliverToStreet1);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONOR STREET1", DocCallout.DeliverToStreet1);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_Address1 = "DELIVER TO STREET1";
			AssertEquals("DELIVER TO STREET1", DocCallout.DeliverToStreet1);
		}

		public void TestDeliverToStreet2()
		{
			Callout.CS_ConsigneeStreet2 = "CONEE STREET2";
			Callout.CS_ConsignorStreet2 = "CONOR STREET2";
			AssertEquals("CONEE STREET2", DocCallout.DeliverToStreet2);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONOR STREET2", DocCallout.DeliverToStreet2);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_Address2 = "DELIVER TO STREET2";
			AssertEquals("DELIVER TO STREET2", DocCallout.DeliverToStreet2);
		}

		public void TestDeliverToCity()
		{
			Callout.CS_ConsigneeCity = "CONEE CITY";
			Callout.CS_ConsignorCity = "CONOR CITY";
			AssertEquals("CONEE CITY", DocCallout.DeliverToCity);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONOR CITY", DocCallout.DeliverToCity);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_City = "DELIVER TO CITY";
			AssertEquals("DELIVER TO CITY", DocCallout.DeliverToCity);
		}

		public void TestDeliverToPostCode()
		{
			Callout.CS_ConsigneePostcode = "CONEE CODE";
			Callout.CS_ConsignorPostcode = "CONOR CODE";
			AssertEquals("CONEE CODE", DocCallout.DeliverToPostCode);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONOR CODE", DocCallout.DeliverToPostCode);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_PostCode = "PCODE";
			AssertEquals("PCODE", DocCallout.DeliverToPostCode);
		}

		public void TestDeliverToPhone()
		{
			Callout.CS_ConsigneePhone = "CONEE PHONE";
			Callout.CS_ConsignorPhone = "CONOR PHONE";
			AssertEquals("CONEE PHONE", DocCallout.DeliverToPhone);
			Callout.IsRTS = true;
			Callout.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Callout.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals("CONOR PHONE", DocCallout.DeliverToPhone);
			Callout.IsRedirected = true;
			Callout.DeliveryAddressOverride.P3_Phone = "DELIVER TO PHONE";
			AssertEquals("DELIVER TO PHONE", DocCallout.DeliverToPhone);
		}

		public void TestHFCContactName()
		{
			Callout.HFCContactName = "Greg Smith";
			AssertEquals("HFCContact should be blank", ZString.Empty, DocCallout.HFCContactName);
			Callout.IsHoldForCollection = true;
			AssertEquals("HFCContact should be 'Greg Smith'", "Greg Smith", DocCallout.HFCContactName);
		}

		public void TestHFCContactPhoneNumber()
		{
			Callout.HFCContactPhoneNumber = "9313 1641";
			AssertEquals("HFCContact should be blank", ZString.Empty, DocCallout.HFCContactPhoneNumber);
			Callout.IsHoldForCollection = true;
			AssertEquals("HFCContactPhoneNumber should be '9313 1641'", "9313 1641", DocCallout.HFCContactPhoneNumber);
		}

		public void TestLabelType1Text()
		{
			AssertEquals("", DocCallout.LabelType1Text);
			Callout.IsRedirected = true;
			AssertEquals("REDIRECT", DocCallout.LabelType1Text);
			Callout.IsRTS = true;
			AssertEquals("RTS", DocCallout.LabelType1Text);
			Callout.IsAbandoned = true;
			AssertEquals("ABANDON", DocCallout.LabelType1Text);
			Callout.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			AssertEquals("TRANSHIP", DocCallout.LabelType1Text);
			Callout.IsHoldForCollection = true;
			Callout.HoldForCollectDepot = "";
			AssertEquals("should be 'HFC', HoldForCollection is empty", "HFC", DocCallout.LabelType1Text);
			Callout.HoldForCollectDepot = HFCDepotCodeDescriptionPairList.Codes.Tasmania;
			AssertEquals("should be 'HFC at TAS Depot', HoldForCollection is specified", "HFC at TAS Depot", DocCallout.LabelType1Text);
		}

		public void TestLabelType2Text()
		{
			Callout.Payment.IsCreditCard = true;
			AssertEquals("", DocCallout.LabelType2Text);
			Callout.Payment.IsCheque = true;
			AssertEquals("COD", DocCallout.LabelType2Text);
		}

		public void TestCODAmount()
		{
			TestDocCallout testDocCallout = new TestDocCallout(Callout, Factory);
			Callout.Payment.IsCreditCard = true;
			AssertEquals("", testDocCallout.CODAmount);
			Callout.Payment.IsCheque = true;
			AssertEquals("$0.00", testDocCallout.CODAmount);
			Callout.EnsureJobHeaderExists();
			CalloutCharge charge = Callout.JobHeader.Charges.AddNew();
			charge.NettAmount = 30m;
			Callout.CalculateTotalAmountDue();
			AssertEquals("$30.00", testDocCallout.CODAmount);
			charge.NettAmount = 30.126m;
			Callout.CalculateTotalAmountDue();
			AssertEquals("$30.13", testDocCallout.CODAmount);
			UPEOrgHeader billTo = Factory.New<UPEOrgHeader>();
			billTo.OH_IsDebtor = true;
			OrgDebtorGroup orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "1";
			billTo.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;
			billTo.AccountNumber = "UPSACCOUNTNO";
			Callout.BillToAccountNumber = "UPSACCOUNTNO";
			testDocCallout.fLocalCharges = 50.158m;
			AssertEquals("$50.16", testDocCallout.CODAmount);
			orgDebtorGroup.OJ_Code = "3";
			AssertEquals("$50.16", testDocCallout.CODAmount);
			orgDebtorGroup.OJ_Code = "4";
			AssertEquals("$50.16", testDocCallout.CODAmount);
			orgDebtorGroup.OJ_Code = "2";
			AssertEquals("$30.13", testDocCallout.CODAmount);
			Callout.PartPaymentAmountToCollect = 10.23m;
			Callout.PartPaymentUsed = true;
			AssertEquals("$10.23", testDocCallout.CODAmount);
			Callout.PartPaymentAmountToCollect = 0;
			Callout.PartPaymentUsed = false;
			AssertEquals("$30.13", testDocCallout.CODAmount);
			Callout.PartPaymentUsed = true;
			AssertEquals("$0.00", testDocCallout.CODAmount);
		}

		public void TestTranshipmentNumber()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			AssertEquals("", doc.TranshipmentNumber);
			callout.CS_TranshipmentEntryNum = "TEST";
			AssertEquals("CAN: TEST", doc.TranshipmentNumber);
		}

		public void TestInvoiceDate()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			callout.BisiDownloadDate = new ZDateTime(2005, 1, 2);
			AssertEquals("InvoiceDate", new ZDateTime(2005, 1, 2), doc.InvoiceDate);
		}

		public void TestTaxInvoiceCommentsText1()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			UPEDataRegistry.Instance.TaxInvoiceCommentsText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Text");
			AssertEquals("Text", doc.TaxInvoiceCommentsText1);
		}

		public void TestTaxInvoiceCommentsText2()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			UPEDataRegistry.Instance.TaxInvoiceCommentsText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Text");
			AssertEquals("Text", doc.TaxInvoiceCommentsText2);
		}

		public void TestTaxInvoiceFooterText()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			UPEDataRegistry.Instance.TaxInvoiceFooterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Footer");
			AssertEquals("Footer", doc.TaxInvoiceFooterText);
		}

		public void TestIsCusEntryHeaderEntryPrintLinesAvailable()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			AssertEquals("Callout without a declaration doesnt have CusEntryHeaderEntryPrintLines", false, doc.IsCusEntryHeaderEntryPrintLinesAvailable);
			JobDeclaration declaration = CreateMergedDeclaration();
			declaration.JE_DeclarationReference = "B00148999";
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals("Callout with a merged declaration has CusEntryHeaderEntryPrintLines", true, doc.IsCusEntryHeaderEntryPrintLinesAvailable);
		}

		public void TestConsigneeAccountNum()
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			DocCallout doc = DocCallout.New(callout, Factory);
			Assert(doc.ConsigneeAccountNum.IsEmpty);
			callout.CS_OA_ConsigneeAddress = Factory.New<UPEOrgHeader>().MainAddress.PK;
			callout.Consignee.AccountNumber = "AccountNumber";
			AssertEquals("AccountNumber", doc.ConsigneeAccountNum);
		}

		JobDeclaration CreateMergedDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		#endregion
		#region Bill To
		public void TestBillToDetails_FromBillToOrgHeader()
		{
			Callout.BillToAccountNumber = "BillToAccountNum";
			AssertEquals("BillTo should be linked for the test", BillTo.PK, Callout.BillTo.PK);
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("BillToAccountNumber", Callout.BillToAccountNumber, doc.BillToAccountNumber);
			BillTo.OH_FullName = "BillToName";
			AssertEquals("BillToName", doc.BillToName);
			BillTo.MainAddress.OA_Address1 = "BillToAddress1";
			OrgAddress postalAddress = Factory.New<OrgAddress>();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			postalAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Postal);
			postalAddress.OA_Address1 = "PostalAddress1";
			postalAddress.OA_Address2 = "PostalAddress2";
			postalAddress.OA_City = "PostalCity";
			postalAddress.OA_State = "PostalState";
			postalAddress.OA_PostCode = "PostalCode";
			BillTo.Addresses.Add(postalAddress);
			AssertEquals("Should be the BillTo Postal Address1", "PostalAddress1", doc.BillToAddress1);
			BillTo.MainAddress.OA_Address2 = "BillToAddress2";
			BillTo.MainAddress.OA_City = "BillToCity";
			BillTo.MainAddress.OA_State = "State";
			BillTo.MainAddress.OA_PostCode = "Postcode";
			AssertEquals("Should be the BillTo Postal Address2", "PostalAddress2 PostalCity PostalState PostalCode", doc.BillToAddress2);
			BillTo.Addresses.Remove(postalAddress.PK);
			doc = DocCallout.New(Callout, Factory);
			AssertEquals("Should fallback to the office Address", "BillToAddress1", doc.BillToAddress1);
			AssertEquals("Should fall back to the office address", "BillToAddress2 BillToCity State Postcode", doc.BillToAddress2);
		}

		public void TestBillToDetails_FromConsignee()
		{
			Callout.BillToAccountNumber = "BillToAccountNum";
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("BillToAccountNumber", "BillToAccountNum", doc.BillToAccountNumber);
			Callout.CS_ConsigneeName = "BillToName";
			AssertEquals("BillToName", doc.BillToName);
			Callout.CS_ConsigneeStreet = "BillToAddress1";
			AssertEquals("BillToAddress1", doc.BillToAddress1);
			Callout.CS_ConsigneeStreet2 = "BillToStreet2";
			Callout.CS_ConsigneeCity = "BillToCity";
			Callout.CS_ConsigneeState = "State";
			Callout.CS_ConsigneePostcode = "Postcode";
			AssertEquals("BillToStreet2 BillToCity State Postcode", doc.BillToAddress2);
		}

		public void TestBillToHasNoAddressAndNoAccountNumber()
		{
			Callout.BillToAccountNumber = ZString.Empty;
			BillTo.Addresses.RemoveAndDeleteAll();
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("BillToAddress1 should be empty", ZString.Empty, doc.BillToAddress1.Trim());
			AssertEquals("BIllToAddress2 should be empty", ZString.Empty, doc.BillToAddress2.Trim());
		}

		#endregion
		#region BillTo HAWB Address
		public void TestBillToHAWBAddress1()
		{
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("", doc.BillToHAWBAddress1);
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.OH_FullName = "FullName";
			uPEOrgHeader.AccountNumber = "AccountNumber";
			Callout.BillToAccountNumber = "AccountNumber";
			AssertEquals("FullName", doc.BillToHAWBAddress1);
		}

		public void TestBillToHAWBAddress2()
		{
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("", doc.BillToHAWBAddress2);
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.MainAddress.OA_Address1 = "Address1";
			uPEOrgHeader.AccountNumber = "AccountNumber";
			Callout.BillToAccountNumber = "AccountNumber";
			AssertEquals("Address1", doc.BillToHAWBAddress2);
		}

		public void TestBillToHAWBAddress3()
		{
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("", doc.BillToHAWBAddress3);
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.MainAddress.OA_Address2 = "Address2";
			uPEOrgHeader.AccountNumber = "AccountNumber";
			Callout.BillToAccountNumber = "AccountNumber";
			AssertEquals("Address2", doc.BillToHAWBAddress3);
		}

		public void TestBillToHAWBAddress4()
		{
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("", doc.BillToHAWBAddress4);
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.MainAddress.OA_City = "City";
			uPEOrgHeader.MainAddress.OA_State = "State";
			uPEOrgHeader.AccountNumber = "AccountNumber";
			Callout.BillToAccountNumber = "AccountNumber";
			AssertEquals("City State", doc.BillToHAWBAddress4);
		}

		public void TestBillToHAWBAddress5()
		{
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("", doc.BillToHAWBAddress5);
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			uPEOrgHeader.MainAddress.OA_PostCode = "7550";
			uPEOrgHeader.AccountNumber = "AccountNumber";
			Callout.BillToAccountNumber = "AccountNumber";
			AssertEquals("AU 7550", doc.BillToHAWBAddress5);
		}

		#endregion
		#region ReferenceNumber1 / ReferenceNumber2
		public void TestReferenceNumber1()
		{
			Callout.Level1Record = new Level1Record();
			Callout.Level1Record._300000 = new _300000Line("US4196AU9639000626              DA15V04FXKC730000007201004A15V04    AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                 REFERENCE1REFERENCE1REFERENCE1REF11NALENI MCGA");
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("REFERENCE1REFERENCE1REFERENCE1REF11", doc.ReferenceNumber1);
		}

		public void TestReferenceNumber2()
		{
			Callout.Level1Record = new Level1Record();
			Callout.Level1Record._400000 = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    999                       REFERENCE2REFERENCE2REFERENCE2REF22                   000           ");
			DocCallout doc = DocCallout.New(Callout, Factory);
			AssertEquals("REFERENCE2REFERENCE2REFERENCE2REF22", doc.ReferenceNumber2);
		}

		#endregion
		#region Test Classes
		class TestDocCallout : DocCallout
		{
			public TestDocCallout(Callout callout, BusinessObjectFactory factoryToWrap) : base(callout, factoryToWrap)
			{
			}

			public ZDecimal fLocalCharges;
			protected override ZDecimal LocalCharges
			{
				get
				{
					return fLocalCharges;
				}
			}
		}

		#endregion
		#region Implementation
		OrgHeader BillTo
		{
			get
			{
				if (fBillTo == null)
				{
					fBillTo = Factory.NewWithValidTestData<OrgHeader>();
					OrgCusCode accountNumber = fBillTo.CustomsCodes.AddNew();
					accountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
					accountNumber.OK_CustomsRegNo = "BillToAccountNum";
				}

				return fBillTo;
			}
		}

		OrgHeader fBillTo;
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			Callout callout = Factory.New<Callout>();
			DocCallout result = DocCallout.New(callout, Factory);
			return new DocumentWrapper[] { result };
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Callout = Factory.New<Callout>();
			DocCallout = DocCallout.New(Callout, Factory);
		}

		Callout Callout;
		DocCallout DocCallout;
		#endregion
	}
}
