using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using GlbCompany = Enterprise.MasterFiles.Business.GlbCompany;

namespace Enterprise.DocumentWrappers.Freight.NZ.Testing
{
	[TestedType(typeof(DocShipment))]
	sealed class DocShipmentTest : DocumentWrapperTestCase
	{
		public void TestClearanceNo()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber.CE_ParentID = Shipment.PK;
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "234567";
			AssertEquals("Entry number", entryNumber.CE_EntryNum, docShipment.ClearanceNo);
		}

		public void TestClearanceNumberFromShipmentEntryNumber()
		{
			AssertEquals("clearance no", "", docShipment.ClearanceNo);
			Shipment.CustomsEntryNumberType = "TTT";
			Shipment.CustomsEntryNumber = "123456";
			AssertEquals("clearance no", Shipment.CustomsEntryNumber, docShipment.ClearanceNo);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new DocumentWrapper[] { DocShipment.New(shipment, Factory) };
		}

		ForwardingShipment Shipment;
		DocShipment docShipment;
		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<ForwardingShipment>();
			docShipment = DocShipment.New(Shipment, Factory);
		}
	}
}
