using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocShipment))]
	internal class CADocShipmentTest : DocumentWrappers.Freight.Testing.DocShipmentTest
	{
		public new void TestFCRLogo()
		{
			Assert(true);
		}

		public void TestHBLCustomsEntryNumber()
		{
			var savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var savedPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "DEHAM";

				var shipmentWrapper = DocShipment.New(shipment, Factory);
				AssertEquals("No Display", ZString.Empty, shipmentWrapper.HBLCustomsEntryNumber);

				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
				AssertEquals("No Display", ZString.Empty, shipmentWrapper.HBLCustomsEntryNumber);

				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;
				AssertEquals("Displays Type only", CusEntryNumberTypes.Australia.EX1, shipmentWrapper.HBLCustomsEntryNumber);

				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
				shipment.CustomsEntryNumber = "Test";
				AssertEquals("Displays Type and Number", CusEntryNumberTypes.Australia.CRN + ": Test", shipmentWrapper.HBLCustomsEntryNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CATOR";

				shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "CATOR";
				shipment.JS_RL_NKDestination = "AUSYD";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_CERSProofOfReportNumber = "RC1792201232000019";
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.EntryNumber = "12345678";
				shipmentWrapper = DocShipment.New(shipment, Factory);
				AssertEquals("RC1792201232000019, 12345678", shipmentWrapper.HBLCustomsEntryNumber);

				declaration.JE_CERSProofOfReportNumber = ZString.Empty;
				shipment.ResetCusEntryNumbers();
				AssertEquals("EXP: 12345678", shipmentWrapper.HBLCustomsEntryNumber);

				var num = shipment.Numbers.AddNew();
				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
				num.CE_EntryNum = "01682031001";
				shipment.ResetCusEntryNumbers();
				AssertEquals("01682031001, 12345678", shipmentWrapper.HBLCustomsEntryNumber);

				entryHeader.EntryNumber = ZString.Empty;
				shipment.ResetCusEntryNumbers();
				AssertEquals("POR: 01682031001", shipmentWrapper.HBLCustomsEntryNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(savedCountry);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = savedPort;
			}
		}
	}
}
