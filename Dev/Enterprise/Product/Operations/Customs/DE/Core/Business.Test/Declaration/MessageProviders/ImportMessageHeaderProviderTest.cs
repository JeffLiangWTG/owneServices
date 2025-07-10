using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(ImportMessageHeaderProvider))]
	public abstract class ImportMessageHeaderProviderTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : ImportMessageHeaderProvider
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		protected CusEntryInstruction entryInstruction;
	}
}
