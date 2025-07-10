using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(RefundInvoiceLine))]
	sealed class RefundInvoiceLineTest : CusSupportingInfoTest<RefundInvoiceLine>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cusReconEntryLine = Factory.New<CusReconEntryLine>();
			return cusReconEntryLine.RefundInvoiceLines.AddNew();
		}

		protected override IEnumerable<RefundInvoiceLine> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "KR1", "TestCompany");
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			factory.Save();

			var cusReconDeclaration = factory.NewWithValidTestData<CusReconDeclaration>();
			cusReconDeclaration.CusReconEntryLines.AddNew();

			var reconEntry = cusReconDeclaration.CusReconEntries[0];
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			var cusReconEntryLine = cusReconDeclaration.CusReconEntryLines[0];
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 1;
			factory.Save();

			yield return cusReconEntryLine.RefundInvoiceLines.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (RefundInvoiceLine)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.RefundInvoiceLine;
			return businessObj;
		}

		public void TestValidationAndLookups()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			AssertEquals(typeof(RefundInvoiceLineValidation), refundInvoiceLine.Validation.GetType());
			AssertEquals(typeof(CusSupportingInfoLookups), refundInvoiceLine.Lookups.GetType());
		}

		public void TestParent()
		{
			var cusReconEntryLine = Factory.New<CusReconEntryLine>();
			var refundInvoiceLine = cusReconEntryLine.RefundInvoiceLines.AddNew();
			AssertEquals(cusReconEntryLine, refundInvoiceLine.Parent);
		}

		public void TestReadOnly()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			AssertEquals(false, refundInvoiceLine.CSI_LineNoInfo.ReadOnly);
			AssertEquals(true, refundInvoiceLine.CSI_AdditionalDescriptionInfo.ReadOnly);
			AssertEquals(true, refundInvoiceLine.CSI_DescriptionInfo.ReadOnly);
			AssertEquals(false, refundInvoiceLine.CSI_QuantityInfo.ReadOnly);
			AssertEquals(true, refundInvoiceLine.CSI_Quantity2Info.ReadOnly);
			AssertEquals(true, refundInvoiceLine.CSI_ValueInfo.ReadOnly);
		}

		public void TestDecimalPlaces()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(refundInvoiceLine.GetType(), "CSI_Quantity", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(refundInvoiceLine.GetType(), "CSI_Quantity2", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(refundInvoiceLine.GetType(), "CSI_Value", true, attrib => attrib.DecimalPlaces == 6);
		}

		public void TestCaption()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(refundInvoiceLine.GetType(), "CSI_LineNo", true, attrib => attrib.Caption == "IMP Invoice Line No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(refundInvoiceLine.GetType(), "CSI_AdditionalDescription", true, attrib => attrib.Caption == "Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(refundInvoiceLine.GetType(), "CSI_Description", true, attrib => attrib.Caption == "Goods Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(refundInvoiceLine.GetType(), "CSI_Quantity", true, attrib => attrib.Caption == "Refund Quantity");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(refundInvoiceLine.GetType(), "CSI_Quantity2", true, attrib => attrib.Caption == "Invoice Quantity");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(refundInvoiceLine.GetType(), "CSI_Value", true, attrib => attrib.Caption == "Unit Price");
		}

		public void TestFormattedCSI_LineNo()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			refundInvoiceLine.CSI_LineNo = 1;
			AssertEquals("01", refundInvoiceLine.FormattedCSI_LineNo);
			refundInvoiceLine.CSI_LineNo = 10;
			AssertEquals("10", refundInvoiceLine.FormattedCSI_LineNo);
			refundInvoiceLine.CSI_LineNo = 101;
			AssertEquals("101", refundInvoiceLine.FormattedCSI_LineNo);
		}
	}
}
