using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ImportDeclarationWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageImportDeclaration>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationWrapper.NewOrNull(null));
			AssertNotNull(Provider);
		}

		public void TestDeclaration()
		{
			AssertNotNull(Provider.Declaration);
			var declaration = Provider.Declaration;
			AssertType<DeclarationWrapper>(declaration);
		}

		public void TestRequestContentHeader()
		{
			AssertType<RequestContentHeaderWrapper>(Provider.RequestContentHeader);
		}

		protected override IMessageImportDeclaration GetProvider()
		{
			var (entryHeader, _) = GetEntryHeader();
			return ImportDeclarationWrapper.NewOrNull(entryHeader);
		}

		(CusEntryHeader header, JobDeclaration declaration) GetEntryHeader()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			var entryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.CustomsEntryInstructions.Add(entryInstruction);
			return (entryHeader, declaration);
		}
	}
}
