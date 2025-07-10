using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(PGARequirementCollection))]
	sealed class PGARequirementCollectionTests : NonPersistentBusinessObjectCollectionTestCase<PGARequirementCollection>
	{
		public void TestHasPGAProgramCodesDeclared()
		{
			var invoiceLine = CreateInvoiceLine();
			var collection = new PGARequirementCollection(new PGARequirementProvider(invoiceLine));
			AssertEquals(0, collection.Count);
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.PHAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.NRCan));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.DFO));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.GAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.ECCC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CNSC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.TC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CFIA));

			collection.Populate();
			AssertEquals(9, collection.Count);
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.PHAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.NRCan));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.DFO));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.GAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.ECCC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CNSC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.TC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CFIA));

			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.PHAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.NRCan));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.DFO));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.GAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.ECCC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CNSC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.TC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CFIA));

			invoiceLine.HCPGAHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.PHACPGAHeader.CA_HAPProgramInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			invoiceLine.DFOPGAHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.GACPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ODSProgramInd = YesNoList.Codes.Yes;
			invoiceLine.CNSCPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.PHAC));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.NRCan));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.DFO));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.GAC));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.ECCC));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CNSC));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.TC));
			AssertEquals(true, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CFIA));

			invoiceLine.HCPGAHeader.CA_APIProgramInd = YesNoList.Codes.No;
			invoiceLine.PHACPGAHeader.CA_HAPProgramInd = YesNoList.Codes.No;
			invoiceLine.NRCanPGAHeader.CA_EEFProgramInd = YesNoList.Codes.No;
			invoiceLine.DFOPGAHeader.CA_ABIProgramInd = YesNoList.Codes.No;
			invoiceLine.GACPGAHeader.CA_AllProgramInd = YesNoList.Codes.No;
			invoiceLine.ECCCPGAHeader.CA_ODSProgramInd = YesNoList.Codes.No;
			invoiceLine.CNSCPGAHeader.CA_AllProgramInd = YesNoList.Codes.No;
			invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.No;
			invoiceLine.CFIAPGAHeader.CA_AllProgramInd = YesNoList.Codes.No;
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.PHAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.NRCan));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.DFO));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.GAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.ECCC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CNSC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.TC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CFIA));

			invoiceLine.HCPGAHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.PHACPGAHeader.CA_HAPProgramInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			invoiceLine.DFOPGAHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.GACPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ODSProgramInd = YesNoList.Codes.Yes;
			invoiceLine.CNSCPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			invoiceLine.Delete();
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.PHAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.NRCan));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.DFO));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.GAC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.ECCC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CNSC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.TC));
			AssertEquals(false, collection.HasPGAProgramCodesDeclared(PGACodes.Codes.CFIA));
		}

		public void TestCopyPersistentValuesFrom()
		{
			CusClassPartPivot pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			JobComInvoiceLine invoiceLine = CreateInvoiceLine();

			PGARequirementCollection collection1 = new PGARequirementCollection(new PGARequirementProvider(pivot));
			PGARequirementCollection collection2 = new PGARequirementCollection(new PGARequirementProvider(invoiceLine));

			collection1.Populate();
			collection2.Populate();

			// we need to check only one field from each type of child object or collection element, the rest is the responsibility of BusinessObject.CopyPersistentValuesFrom

			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			pivot.CFIAPGAHeader.CA_AIRSExtensionCode = "EC1";
			pivot.CFIAPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF1";

			pivot.CCA_CNSCIndicator = YesNoList.Codes.Yes;
			pivot.CNSCPGAHeader.CA_PackMarks = "PKM002";
			pivot.CNSCPGAHeader.Components.AddNew().CA_Name = "COMP2";
			pivot.CNSCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF2";

			pivot.CCA_DFOIndicator = YesNoList.Codes.Yes;
			pivot.DFOPGAHeader.CA_TSN = "TSN003";
			pivot.DFOPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF3";

			pivot.CCA_ECCCIndicator = YesNoList.Codes.Yes;
			pivot.ECCCPGAHeader.CA_IntendedUseCode = "UC4";
			pivot.ECCCPGAHeader.Components.AddNew().CA_Name = "COMP4";
			pivot.ECCCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF4";

			pivot.CCA_GACIndicator = YesNoList.Codes.Yes;
			pivot.GACPGAHeader.CA_CommodityCode = "CC005";
			pivot.GACPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF5";

			pivot.CCA_HCIndicator = YesNoList.Codes.Yes;
			pivot.HCPGAHeader.CA_GTINNumber = "GTN6";
			pivot.HCPGAHeader.Components.AddNew().CA_Name = "COMP6";
			pivot.HCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF6";

			pivot.CCA_NRCanIndicator = YesNoList.Codes.Yes;
			pivot.NRCanPGAHeader.CA_IntendedUseCode = "UC7";
			pivot.NRCanPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF7";

			pivot.CCA_PHACIndicator = YesNoList.Codes.Yes;
			pivot.PHACPGAHeader.CA_IntendedUseCode = "UC8";
			pivot.PHACPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF8";

			pivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			pivot.TCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF9";

			Factory.Save();

			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CFIAInd);
			AssertEquals("EC1", invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode);
			AssertEquals("REF1", invoiceLine.CFIAPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CNSCInd);
			AssertEquals("PKM002", invoiceLine.CNSCPGAHeader.CA_PackMarks);
			AssertEquals("COMP2", invoiceLine.CNSCPGAHeader.Components.OfType<Component>().Single().CA_Name);
			AssertEquals("REF2", invoiceLine.CNSCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_DFOInd);
			AssertEquals("TSN003", invoiceLine.DFOPGAHeader.CA_TSN);
			AssertEquals("REF3", invoiceLine.DFOPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_ECCCInd);
			AssertEquals("UC4", invoiceLine.ECCCPGAHeader.CA_IntendedUseCode);
			AssertEquals("COMP4", invoiceLine.ECCCPGAHeader.Components.OfType<Component>().Single().CA_Name);
			AssertEquals("REF4", invoiceLine.ECCCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_GACInd);
			AssertEquals("CC005", invoiceLine.GACPGAHeader.CA_CommodityCode);
			AssertEquals("REF5", invoiceLine.GACPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_HCInd);
			AssertEquals("GTN6", invoiceLine.HCPGAHeader.CA_GTINNumber);
			AssertEquals("COMP6", invoiceLine.HCPGAHeader.Components.OfType<Component>().Single().CA_Name);
			AssertEquals("REF6", invoiceLine.HCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_NRCanInd);
			AssertEquals("UC7", invoiceLine.NRCanPGAHeader.CA_IntendedUseCode);
			AssertEquals("REF7", invoiceLine.NRCanPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_PHACInd);
			AssertEquals("UC8", invoiceLine.PHACPGAHeader.CA_IntendedUseCode);
			AssertEquals("REF8", invoiceLine.PHACPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_TCInd);
			AssertEquals("REF9", invoiceLine.TCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);
		}

		public void TestCopyPersistentValuesFromWithModifiedValues()
		{
			CusClassPartPivot pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			JobComInvoiceLine invoiceLine = CreateInvoiceLine();

			PGARequirementCollection collection1 = new PGARequirementCollection(new PGARequirementProvider(pivot));
			PGARequirementCollection collection2 = new PGARequirementCollection(new PGARequirementProvider(invoiceLine));

			collection1.Populate();
			collection2.Populate();

			// we need to check only one field from each type of child object or collection element, the rest is the responsibility of BusinessObject.CopyPersistentValuesFrom

			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			pivot.CFIAPGAHeader.CA_AIRSExtensionCode = "EC1";
			pivot.CFIAPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF1";

			pivot.CCA_CNSCIndicator = YesNoList.Codes.Yes;
			pivot.CNSCPGAHeader.CA_PackMarks = "PKM002";
			pivot.CNSCPGAHeader.Components.AddNew().CA_Name = "COMP2";
			pivot.CNSCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF2";

			pivot.CCA_DFOIndicator = YesNoList.Codes.Yes;
			pivot.DFOPGAHeader.CA_TSN = "TSN003";
			pivot.DFOPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF3";

			pivot.CCA_ECCCIndicator = YesNoList.Codes.Yes;
			pivot.ECCCPGAHeader.CA_IntendedUseCode = "UC4";
			pivot.ECCCPGAHeader.Components.AddNew().CA_Name = "COMP4";
			pivot.ECCCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF4";

			pivot.CCA_GACIndicator = YesNoList.Codes.Yes;
			pivot.GACPGAHeader.CA_CommodityCode = "CC005";
			pivot.GACPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF5";

			pivot.CCA_HCIndicator = YesNoList.Codes.Yes;
			pivot.HCPGAHeader.CA_GTINNumber = "GTN6";
			pivot.HCPGAHeader.Components.AddNew().CA_Name = "COMP6";
			pivot.HCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF6";

			pivot.CCA_NRCanIndicator = YesNoList.Codes.Yes;
			pivot.NRCanPGAHeader.CA_IntendedUseCode = "UC7";
			pivot.NRCanPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF7";

			pivot.CCA_PHACIndicator = YesNoList.Codes.Yes;
			pivot.PHACPGAHeader.CA_IntendedUseCode = "UC8";
			pivot.PHACPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF8";

			pivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			pivot.TCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF9";

			Factory.Save();

			collection2.CopyPersistentValuesFrom(collection1);

			// overwriting copied values

			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode = "TTT";
			invoiceLine.CFIAPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine.CNSCPGAHeader.CA_PackMarks = "TTTTTT";
			invoiceLine.CNSCPGAHeader.Components.AddNew().CA_Name = "TTTTT";
			invoiceLine.CNSCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine.DFOPGAHeader.CA_TSN = "TTTTTT";
			invoiceLine.DFOPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_IntendedUseCode = "TTT";
			invoiceLine.ECCCPGAHeader.Components.AddNew().CA_Name = "TTTTT";
			invoiceLine.ECCCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine.GACPGAHeader.CA_CommodityCode = "TTTTT";
			invoiceLine.GACPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_GTINNumber = "TTTT";
			invoiceLine.HCPGAHeader.Components.AddNew().CA_Name = "TTTTT";
			invoiceLine.HCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_IntendedUseCode = "TTT";
			invoiceLine.NRCanPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;
			invoiceLine.PHACPGAHeader.CA_IntendedUseCode = "TTT";
			invoiceLine.PHACPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.LPCOViews.AddNew().CLP_RefNo = "TTTT";

			// copying values again

			collection2.CopyPersistentValuesFrom(collection1);

			// copied values should overwrite modified values

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CFIAInd);
			AssertEquals("EC1", invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode);
			AssertEquals("REF1", invoiceLine.CFIAPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CNSCInd);
			AssertEquals("PKM002", invoiceLine.CNSCPGAHeader.CA_PackMarks);
			AssertEquals("COMP2", invoiceLine.CNSCPGAHeader.Components.OfType<Component>().Single().CA_Name);
			AssertEquals("REF2", invoiceLine.CNSCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_DFOInd);
			AssertEquals("TSN003", invoiceLine.DFOPGAHeader.CA_TSN);
			AssertEquals("REF3", invoiceLine.DFOPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_ECCCInd);
			AssertEquals("UC4", invoiceLine.ECCCPGAHeader.CA_IntendedUseCode);
			AssertEquals("COMP4", invoiceLine.ECCCPGAHeader.Components.OfType<Component>().Single().CA_Name);
			AssertEquals("REF4", invoiceLine.ECCCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_GACInd);
			AssertEquals("CC005", invoiceLine.GACPGAHeader.CA_CommodityCode);
			AssertEquals("REF5", invoiceLine.GACPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_HCInd);
			AssertEquals("GTN6", invoiceLine.HCPGAHeader.CA_GTINNumber);
			AssertEquals("COMP6", invoiceLine.HCPGAHeader.Components.OfType<Component>().Single().CA_Name);
			AssertEquals("REF6", invoiceLine.HCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_NRCanInd);
			AssertEquals("UC7", invoiceLine.NRCanPGAHeader.CA_IntendedUseCode);
			AssertEquals("REF7", invoiceLine.NRCanPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_PHACInd);
			AssertEquals("UC8", invoiceLine.PHACPGAHeader.CA_IntendedUseCode);
			AssertEquals("REF8", invoiceLine.PHACPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_TCInd);
			AssertEquals("REF9", invoiceLine.TCPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);
		}

		JobComInvoiceLine CreateInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			return invoiceLine;
		}

		#region Overrides

		protected override PGARequirementCollection GetCollectionToTest()
		{
			var invoiceLine = CreateInvoiceLine();
			return invoiceLine.PGARequirements;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new InvalidOperationException();
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		#endregion
	}
}
