using CargoWise.Customs.DE.MessageContracts.Import;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(SCWRECReconEntryLineBuilder))]
	sealed class SCWRECReconEntryLineBuilderTest : CusReconEntryLineBuilderAbstractTest<SCWRECReconEntryLineBuilder, ISCWRECLine, ISCWRECHeader, ISCWPEDLine>
	{
		public void TestCreateCurrentEntryLineAndSnapshot_HasChangesNotInSnapshotProviderUsesPersistedEntryLine()
		{
			invoiceLine.JI_Description = "DifferentDescriptionButNotPersisted";
			entryLineBuilder = new SCWRECReconEntryLineBuilder(CusReconBuildersTestHelper.EmptySCWRECLine(), HeaderProvider, cusReconEntry);
			var currentSnapshot = CreateLodgedAndCurrentSnapshots();
			AssertNull("No CUR-snapshot created because is NormalXMLDeclaration and HasChangesNotInSnapshotProvider = False due to not persisted change", currentSnapshot);
		}

		protected override ISCWRECHeader HeaderProvider => CusReconBuildersTestHelper.SCWRECHeader();

		protected override ISCWRECLine LineProvider => CusReconBuildersTestHelper.SCWRECLine().Object;

		protected override SCWRECReconEntryLineBuilder GetEntryLineBuilder(ISCWRECLine lineProvider, ISCWRECHeader headerProvider, CusReconEntry reconEntry	)
			=> new SCWRECReconEntryLineBuilder(lineProvider, headerProvider, reconEntry);
	}
}
