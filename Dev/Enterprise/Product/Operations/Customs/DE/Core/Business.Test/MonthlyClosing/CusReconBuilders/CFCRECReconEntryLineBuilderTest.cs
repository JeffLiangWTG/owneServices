using CargoWise.Customs.DE.MessageContracts.Import;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(CFCRECReconEntryLineBuilder))]
	sealed class CFCRECReconEntryLineBuilderTest : CusReconEntryLineBuilderAbstractTest<CFCRECReconEntryLineBuilder, ICFCRECLine, ICFCRECHeader, ICFCPEDLine>
	{
		public void TestCreateCurrentEntryLineAndSnapshot_HasChangesNotInSnapshotProviderUsesPersistedEntryLine()
		{
			invoiceLine.JI_Description = "DifferentDescriptionButNotPersisted";
			entryLineBuilder = new CFCRECReconEntryLineBuilder(CusReconBuildersTestHelper.EmptyCFCRECLine(), HeaderProvider, cusReconEntry);
			var currentSnapshot = CreateLodgedAndCurrentSnapshots();
			AssertNull("No CUR-snapshot created because is NormalXMLDeclaration and HasChangesNotInSnapshotProvider = False due to not persisted change", currentSnapshot);
		}

		protected override ICFCRECHeader HeaderProvider => CusReconBuildersTestHelper.CFCRECHeader();

		protected override ICFCRECLine LineProvider => CusReconBuildersTestHelper.CFCRECLine().Object;

		protected override CFCRECReconEntryLineBuilder GetEntryLineBuilder(ICFCRECLine lineProvider, ICFCRECHeader headerProvider, CusReconEntry reconEntry)
			=> new CFCRECReconEntryLineBuilder(lineProvider, headerProvider, reconEntry);
	}
}
