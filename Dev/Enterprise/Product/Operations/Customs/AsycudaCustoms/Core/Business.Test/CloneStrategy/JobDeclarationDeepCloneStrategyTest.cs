using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	sealed class JobDeclarationDeepCloneStrategyTest : Customs.Business.Testing.JobDeclarationDeepCloneStrategyAbstractTest<JobDeclaration>
	{
		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(JobDeclaration declarationToClone, Customs.Business.CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			return new JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		[UseSnapshotProtection]
		public void TestNotCopyBGMReferenceCounter()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var declaration = Factory.New<JobDeclaration>();
			var header1 = declaration.ActiveEntryHeaders.AddNew();
			var header2 = declaration.ActiveEntryHeaders.AddNew();
			declaration.AllocateAllBGMReferences();
			AssertEquals("B00001000", declaration.JE_DeclarationReference);
			AssertEquals("B00001000/1", header1.CH_BGMReference);
			AssertEquals("B00001000/2", header2.CH_BGMReference);
			Factory.Save();

			Factory.ReloadAll<GenAddOnColumn>();
			var genAddOnColumnCollection = new GenAddOnColumnCollection(declaration);
			AssertEquals("Precondition", "2", genAddOnColumnCollection.Find(JobDeclaration.BGMReferenceCounterString).XA_Data);

			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			AssertEquals(string.Empty, clonedDec.JE_DeclarationReference);
			AssertEquals(0, clonedDec.ActiveEntryHeaders.Count);
			Factory.ReloadAll<GenAddOnColumn>();
			var cloneGenAddOnColumnCollection = new GenAddOnColumnCollection(clonedDec);
			AssertEquals("not copy BGMReferenceCounter", null, cloneGenAddOnColumnCollection.Find(JobDeclaration.BGMReferenceCounterString));
		}
	}
}
