using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclaration>
	{
		public void TestNewOrNull()
		{
			var factory = Factory;
			AssertNull("When EntryHeader is null", DeclarationWrapper.NewOrNull(null));
			AssertNull("When EntryHeader without declaration", DeclarationWrapper.NewOrNull(factory.New<CusEntryHeader>()));
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";

			AssertNull("When entryHeader without EntryInstruction", DeclarationWrapper.NewOrNull(entryHeader));

			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			AssertNotNull(Provider);
		}

		public void TestAcceptanceDateTime()
		{
			AssertEquals(ZString.Empty, Provider.AcceptanceDateTime);
		}

		public void TestAgent()
		{
			AssertEquals(1, Provider.Agent.Count);
			var agent = Provider.Agent.FirstOrDefault();
			AssertType<DeclarationAgentWrapper>(agent);
		}

		public void TestDeclarationOfficeID()
		{
			AssertEquals("IL123", Provider.DeclarationOfficeID.Value);
		}

		public void TestDmExtensions()
		{
			AssertType<DeclarationDmExtWrapper>(Provider.DmExtensions);
		}

		public void TestDefaultGovernmentProcedure()
		{
			AssertNotNull(Provider.GovernmentProcedure);
			AssertType<DeclarationGovernmentProcedureWrapper>(Provider.GovernmentProcedure);
		}

		public void TestGovernmentProcedure()
		{
			var (entryHeader, declaration) = GetEntryHeader();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.CustomsEntryInstructions.Add(entryInstruction);
			var wrapper = DeclarationWrapper.NewOrNull(entryHeader);

			AssertType<DeclarationGovernmentProcedureWrapper>(wrapper.GovernmentProcedure);
		}

		public void TestID()
		{
			AssertEquals("CE123", Provider.ID.Value);
		}

		public void TestImporter()
		{
			AssertEquals(1, Provider.Importer.Count);
			var importer = Provider.Importer.FirstOrDefault();
			AssertType<DeclarationImporterWrapper>(importer);
		}

		public void TestIssueDateTime()
		{
			AssertEquals(ZString.Empty, Provider.IssueDateTime);
		}

		public void TestTypeCode()
		{
			AssertEquals("1", Provider.TypeCode.Value);

			var (entryHeader, declaration) = GetEntryHeader();
			declaration.JE_MessageType = "EXP";
			var provider = DeclarationWrapper.NewOrNull(entryHeader);
			AssertNull("When not IMP it should be null", provider.TypeCode);
		}

		public void TestDutyTaxFee()
		{
			AssertNull(Provider.DutyTaxFee);
		}

		public void TestGoodsShipment()
		{
			var (entryHeader, declaration) = GetEntryHeader();

			var wrapper = DeclarationWrapper.NewOrNull(entryHeader);
			CombineAssertions("Without merge line", () =>
			{
				AssertNotNull(wrapper.GoodsShipment);
				AssertEquals(0, wrapper.GoodsShipment.Count);
			});

			(entryHeader, declaration) = GetEntryHeader();
			var entryLine = entryHeader.MergedLines.AddNew();

			var primaryInvoiceHeader = declaration.Invoices.AddNew();
			var primaryInvoiceLine = primaryInvoiceHeader.InvoiceLines.AddNew();
			primaryInvoiceLine.JI_CL = entryLine.PK;

			var additionalEntryLine = entryHeader.MergedLines.AddNew();
			var additionalInvoiceHeader = declaration.Invoices.AddNew();
			var additionalInvoiceLine = additionalInvoiceHeader.InvoiceLines.AddNew();
			additionalInvoiceLine.JI_CL = additionalEntryLine.PK;
			wrapper = DeclarationWrapper.NewOrNull(entryHeader);

			CombineAssertions("With merge lines", () =>
			{
				AssertEquals(2, wrapper.GoodsShipment.Count);
				var primaryGoodsShipment = wrapper.GoodsShipment.First();
				AssertType<DeclarationGoodsShipmentWrapper>(primaryGoodsShipment);
				AssertNotNull("Primary Goods Shipment should have Consignment", primaryGoodsShipment.Consignment);
				var secondaryGoodsShipment = wrapper.GoodsShipment.Last();
				AssertNull("Secondary Goods Shipment should not have Consignment", secondaryGoodsShipment.Consignment);
			});
		}

		protected override IDeclaration GetProvider()
		{
			var (entryHeader, _) = GetEntryHeader();
			return DeclarationWrapper.NewOrNull(entryHeader);
		}

		(CusEntryHeader header, JobDeclaration declaration) GetEntryHeader()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			return (entryHeader, declaration);
		}
	}
}
