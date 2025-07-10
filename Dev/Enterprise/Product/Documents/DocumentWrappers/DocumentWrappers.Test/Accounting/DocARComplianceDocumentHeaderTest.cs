using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARComplianceDocument))]
	class DocARComplianceDocumentHeaderTest : DocumentWrapperTestCase
	{
		public void TestDebtorCategory()
		{
			AssertEquals("BUS", Wrapper.DebtorCategory);
		}

		public void TestPrintCount()
		{
			AssertEquals(1, Wrapper.PrintCount);
		}

		public void TestCompanyAddressCode()
		{
			AssertEquals("Test Debtor Address Code", Wrapper.DebtorAddressCode);
		}

		public void TestCompanyName()
		{
			AssertEquals("Test Company", Wrapper.CurrentCompanyName);
		}

		public void TestCompanyVATRegNum()
		{
			AssertEquals("55667788", Wrapper.CompanyVATRegNum);
		}

		public void TestCompanyPhoneNum()
		{
			AssertEquals("123456789", Wrapper.CompanyPhoneNum);
		}

		public void TestCompanyFaxNum()
		{
			AssertEquals("+886 987 654 321", Wrapper.CompanyFaxNum);
		}

		public void TestSystemCompanyVATRegAddress()
		{
			AssertEquals("State Taipei Proxy Address1 Addtional Proxy Address2", Wrapper.CompanyVATRegAddress);
		}

		public virtual void TestTaxInvoiceDate()
		{
			AssertEquals("2018-07-08", Wrapper.InvoiceDateString);
		}

		public void TestDebtorVATRegNum()
		{
			AssertEquals("11223344", Wrapper.DebtorVATRegNum);
		}

		public void TestDebtorName()
		{
			AssertEquals("Test Debtor", Wrapper.DebtorName);
		}

		public virtual void TestChargeLines()
		{
			AssertEquals(9, Wrapper.ChargeLines.Count);
			AssertEquals(100m, Wrapper.ChargeLines[0].LocalAmount);
			AssertEquals("Test Line1", Wrapper.ChargeLines[0].Description);
			AssertEquals(200m, Wrapper.ChargeLines[1].LocalAmount);
			AssertEquals("Test Line2", Wrapper.ChargeLines[1].Description);
			AssertEquals(300m, Wrapper.ChargeLines[2].LocalAmount);
			AssertEquals("Test Line3", Wrapper.ChargeLines[2].Description);
			AssertEquals(400m, Wrapper.ChargeLines[3].LocalAmount);
			AssertEquals("Test Line4", Wrapper.ChargeLines[3].Description);
			AssertEquals(500m, Wrapper.ChargeLines[4].LocalAmount);
			AssertEquals("Test Line5", Wrapper.ChargeLines[4].Description);
			AssertEquals(600m, Wrapper.ChargeLines[5].LocalAmount);
			AssertEquals("Test Line6", Wrapper.ChargeLines[5].Description);
			AssertEquals(700m, Wrapper.ChargeLines[6].LocalAmount);
			AssertEquals("Test Line7", Wrapper.ChargeLines[6].Description);
			AssertEquals(800m, Wrapper.ChargeLines[7].LocalAmount);
			AssertEquals("Test Line8", Wrapper.ChargeLines[7].Description);
			AssertEquals(1000m, Wrapper.ChargeLines[8].LocalAmount);
			AssertEquals("Test Line9", Wrapper.ChargeLines[8].Description);
		}

		public void TestDocumentDescription()
		{
			AssertEquals("备注", Wrapper.DocumentDescription);
		}

		public void TestSumLineAmount()
		{
			AssertEquals(4600m, Wrapper.SumLineAmount);
		}

		public void TestSumTaxAmount()
		{
			AssertEquals(460m, Wrapper.SumTaxAmount);
		}

		public void TestSumTotalAmount()
		{
			AssertEquals(5060m, Wrapper.SumTotalAmount);
		}

		public void TestTransactionNumber()
		{
			AssertEquals("00001000", Wrapper.TransactionNumber);
		}

		public void TestComplianceDocumentNumber()
		{
			AssertEquals("D0000001", Wrapper.DocumentNumber);
		}

		public void TestHouseBill()
		{
			AssertEquals("HBL_S00001", Wrapper.HouseBill);
		}

		public virtual void TestInternalReference()
		{
			AssertEquals("00001000", Wrapper.InternalReference);
		}

		public void TestRemarkAndStamp()
		{
			var image = new Bitmap(10, 10);
			var image1 = new Bitmap(20, 20);

			var list = new ComplianceDocumentImageCollection();
			list.Add(new ComplianceDocumentImage() { Country = Core.Constants.CountryCodes.Taiwan, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC, Remark = "Remark", Image = image });
			list.Add(new ComplianceDocumentImage() { Country = Core.Constants.CountryCodes.Taiwan, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, Remark = "Remark1", Image = image1 });
			AccountingMasterFilesRegistry.Instance.DocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, list);

			AssertEquals("Remark", Wrapper.Remark);
			Assert(Utilities.IsImageEqual(image, Wrapper.Stamp));
		}

		public void TestDocumentDate()
		{
			AssertEquals(new ZDateTime(2018, 7, 8, 15, 05, 13), Wrapper.DocumentDate);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARComplianceDocument.New(Header, Factory) };
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.Taiwan;

		DocARComplianceDocument Wrapper;
		protected ARComplianceDocumentHeader Header;

		protected ZDateTime DocumentDate => new ZDateTime(2018, 7, 8, 15, 05, 13);

		protected override void SetUp()
		{
			/**
			 *Note:
			 * base.SetUp() does change the GlbCompany.CurrentCompany's country to Taiwan (TestingCountry),
			 * And sets it back to original country before the setUp change, in base.TearDown() method.
			 * However the base.SetUp() also expects the following set up to complete before calling base.
			 * And the block needs to be run with GlbCompany.CurrentCompany's country set to Taiwan.
			 * A using block with TemporarilySetCountry method is used to make sure,
			 * This temporary country change is reverted before executing base.SetUp() method.
			 */
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				OrgAddress proxyAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
				proxyAddress.OA_CompanyNameOverride = "Test Company";
				proxyAddress.OA_Address1 = "Proxy Address1";
				proxyAddress.OA_Address2 = "Addtional Proxy Address2";
				proxyAddress.OA_City = "Taipei";
				proxyAddress.State = "State";
				proxyAddress.OA_Phone = "123456789";
				proxyAddress.OA_Fax = "987654321";
				var customsCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "55667788", GlbCompany.CurrentCompany.Country);
				customsCode.OK_OA_PremisesAddress = proxyAddress.PK;

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Language = Core.SharedConstants.Languages.ChineseTraditional;
				orgHeader.OH_RL_NKClosestPort = "TWTPE";
				orgHeader.OH_Category = "BUS";

				OrgAddress address = orgHeader.Addresses.AddNew();
				address.OA_CompanyNameOverride = "Test Debtor";
				address.OA_Address1 = "Test Address1";
				address.OA_Address2 = "Addtional Address2";
				address.OA_Code = "Test Debtor Address Code";
				address.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
				address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
				address.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11223344", GlbCompany.CurrentCompany.Country);

				var shipment = TestObjectCreator.CreateShipment("S00001");
				shipment.JS_HouseBill = "HBL_S00001";
				var job = TestObjectCreator.CreateJob(shipment, false);

				var aRInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.TWD, 1m, orgHeader, DocumentDate);
				aRInvoice.AH_JH = job.PK;
				aRInvoice.AH_TransactionNum = "00001000";
				aRInvoice.AH_TransactionReference = "11234567892";
				aRInvoice.AH_Desc = "备注";

				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				complianceSequence.XD_IsActive = true;
				complianceSequence.XD_StartDate = new ZDate(2018, 6, 1);
				complianceSequence.XD_ExpiryDate = new ZDateTime(2018, 7, 10);

				var line1 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line1", 100m);
				var line2 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line2", 200m);
				var line3 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line3", 300m);
				var line4 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line4", 400m);
				var line5 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line5", 500m);
				var line6 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line6", 600m);
				var line7 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line7", 700m);
				var line8 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "Test Line8", 800m);
				var line9 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, null, TestObjectCreator.CC2, TestObjectCreator.USD, 0.1m, "Test Line9", 100m);
				line1.AL_AT = TestObjectCreator.GST1.PK;
				line2.AL_AT = TestObjectCreator.GST1.PK;
				line3.AL_AT = TestObjectCreator.GST1.PK;
				line4.AL_AT = TestObjectCreator.GST1.PK;
				line5.AL_AT = TestObjectCreator.GST1.PK;
				line6.AL_AT = TestObjectCreator.GST1.PK;
				line7.AL_AT = TestObjectCreator.GST1.PK;
				line8.AL_AT = TestObjectCreator.GST1.PK;
				line9.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				new ComplianceDocumentCreator(new[] { aRInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
				Header = Factory.LoadTop1<ARComplianceDocumentHeader>(new ZQuery());
				Header.ADH_OA_AddressOverride = address.PK;
				Header.ADH_DocumentDate = DocumentDate;
				Header.ADH_ReportingPeriod = 201807;
				Header.ADH_DocumentNumber = "D0000001";
				Header.ADH_ComplianceSubType = "NTC";
				Header.ADH_XD_ComplianceBook = complianceSequence.PK;
				Header.ADH_PrintCount = 1;
				Wrapper = (DocARComplianceDocument)GetDocumentWrappers()[0];
				Factory.Save();
			}

			base.SetUp();
		}
	}
}
