using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class DeclarationDmExtWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationDmExtensions>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationDmExtWrapper.NewOrNull(null));
			AssertNotNull(Provider);
		}

		public void TestAgentFileReferenceID()
		{
			AssertEquals("B00100001", Provider.AgentFileReferenceID.Value);
		}

		public void TestExternalDeclarationID()
		{
			AssertEquals("10000001", Provider.ExternalDeclarationID.Value);
		}

		public void TestPreviousDocument()
		{
			AssertNull(Provider.PreviousDocument);
		}

		public void TestDefaultTaxationDateTime()
		{
			AssertEquals(ZString.Empty, Provider.TaxationDateTime);
		}

		public void TestTaxationDateTime()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 05, 14, 16, 34, 00);
			var provider = DeclarationDmExtWrapper.NewOrNull(entryHeader);
			AssertEquals("2024-05-14T16:34:00", provider.TaxationDateTime);
		}

		public void TestAdditionalDocument()
		{
			AssertNull(Provider.AdditionalDocument);
		}

		public void TestDefaultAutonomyRegionType()
		{
			AssertNull(Provider.AutonomyRegionType);
		}

		public void TestAutonomyRegionType()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_AutonomyRegionType = "90";
			var provider = DeclarationDmExtWrapper.NewOrNull(entryHeader);
			AssertEquals("90", provider.AutonomyRegionType.Value);
		}

		public void TestCustomsValueComponent()
		{
			AssertNull(Provider.CustomsValueComponent);
		}

		public void TestExpenseLoadingFactor()
		{
			AssertNull(Provider.ExpenseLoadingFactor);
		}

		public void TestReleaseDateTime()
		{
			AssertNull(Provider.ReleaseDateTime);
		}

		public void TestTehilaDeclarationID()
		{
			AssertNull(Provider.TehilaDeclarationID);
		}

		public void TestVersionID()
		{
			AssertNull(Provider.VersionID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00100001";
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "10000001";
		}

		protected override IDeclarationDmExtensions GetProvider()
		{
			return DeclarationDmExtWrapper.NewOrNull(entryHeader);
		}
		CusEntryHeader entryHeader;
	}
}
