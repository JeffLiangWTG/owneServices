using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class ApplicationExtenderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertType<DeltaGApplicationExtender>("DG", ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaG));
				AssertType<DeltaIEApplicationExtender>("DI", ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaIE));
				AssertType<DeltaGApplicationExtender>("DG", ApplicationExtender.New(DeclarationApplicationCodeList.Codes.Interface));
				AssertType<DeltaGApplicationExtender>("DG", ApplicationExtender.New(Customs.Business.DeclarationApplicationCodeList.Codes.Builtin));
				AssertType<DeltaGApplicationExtender>("default", ApplicationExtender.New(""));
			});
		}
	}
}
