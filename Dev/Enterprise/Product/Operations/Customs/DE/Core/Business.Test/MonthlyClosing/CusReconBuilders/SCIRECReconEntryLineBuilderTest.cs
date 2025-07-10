using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(SCIRECReconEntryLineBuilder))]
	sealed class SCIRECReconEntryLineBuilderTest : CusReconEntryLineBuilderAbstractTest<SCIRECReconEntryLineBuilder, ISCIRECLine, ISCIRECHeader, ISCIPEDLine>
	{
		public void TestCreateCurrentEntryLineAndSnapshot_HasChangesNotInSnapshotProviderUsesPersistedEntryLine()
		{
			invoiceLine.JI_Description = "DifferentDescriptionButNotPersisted";
			entryLineBuilder = new SCIRECReconEntryLineBuilder(CusReconBuildersTestHelper.EmptySCIRECLine(), HeaderProvider, cusReconEntry);
			var currentSnapshot = CreateLodgedAndCurrentSnapshots();
			AssertNull("No CUR-snapshot created because is NormalXMLDeclaration and HasChangesNotInSnapshotProvider = False due to not persisted change", currentSnapshot);
		}

		protected override ISCIRECHeader HeaderProvider => CusReconBuildersTestHelper.SCIRECHeader();

		protected override ISCIRECLine LineProvider => CusReconBuildersTestHelper.SCIRECLine().Object;

		protected override SCIRECReconEntryLineBuilder GetEntryLineBuilder(ISCIRECLine lineProvider, ISCIRECHeader headerProvider, CusReconEntry reconEntry)
			=> new SCIRECReconEntryLineBuilder(lineProvider, headerProvider, reconEntry);
	}
}
