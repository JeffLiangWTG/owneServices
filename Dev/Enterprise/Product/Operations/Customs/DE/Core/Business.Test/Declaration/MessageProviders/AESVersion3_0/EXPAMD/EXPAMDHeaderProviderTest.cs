using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPAMDHeaderProvider))]
	class EXPAMDHeaderProviderTest : AESHeaderProviderAbstractTest<EXPAMDHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPAMDHeaderProvider(null));
		}

		public void TestIsContainerized()
		{
			foreach (var containerFlag in new ZString[] { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.ULD, Core.Constants.ContainerModes.Containerised })
			{
				declaration.JE_ContainerMode = containerFlag;
				AssertEquals($"IsContainerised = '{containerFlag}'", true, Provider.IsContainerized);
			}
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Miscellaneous IsContainerised", false, Provider.IsContainerized);
		}

		public void TestTransportEquipments()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(2, Provider.TransportEquipments.Count);
		}

		public void TestTransportEquipments_NotPopulated()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(false, Provider.TransportEquipments.Any());
		}

		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				entryHeader.LocalReferenceNumber = "12345";
				AssertEquals("MRN is empty", "12345", Provider.LocalReferenceNumber);

				entryHeader.MovementReferenceNumberSetter("MRN1", new ZDateTime(2019, 6, 12));
				AssertEquals("MRN isn't empty", null, Provider.LocalReferenceNumber);
			});
		}

		public void TestDeclarant()
		{
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
			var declarant = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR1", Provider.Declarant.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", Provider.Declarant.EoriBranchSuffix);
			});
		}

		public void TestDeclarant_InvalidPartyConstellation()
		{
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
			var declarant = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			AssertNull("Declarant is null", Provider.Declarant);
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 1;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;

			var entryLine3 = entryHeader.MergedLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			entryLine3.CL_LineNumber = 3;

			action.EntryLines.Cast<ExportEntryLine>().First(x => x.LineNumber == 2).ShouldSend = false;
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 3 }, Provider.Lines.Select(x => x.LineNumber));
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		CusEntryInstruction entryInstruction;
		CusEntryLine entryLine;
		ExportEntryMessageSendingAction action;

		protected override EXPAMDHeaderProvider GetProvider() => new EXPAMDHeaderProvider(action);

		new IEXPAMDHeader Provider => base.Provider;
	}
}
