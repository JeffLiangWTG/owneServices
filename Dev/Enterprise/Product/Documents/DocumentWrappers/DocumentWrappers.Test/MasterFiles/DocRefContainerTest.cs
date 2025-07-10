using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocRefContainer))]
	public class DocRefContainerTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocRefContainer.New(Container, Factory)
			};
		}

		public void TestSize()
		{
			AssertEquals("Should be First 2 letters of code", Container.RC_Code.Substring(0, 2), RefContainerWrapper.Size);

			Container.RC_Code = "";
			AssertEquals("Should be empty", "", RefContainerWrapper.Size);
		}

		public void TestType()
		{
			AssertEquals("Should be the Last letters of code excluding first 2", Container.RC_Code.Substring(2), RefContainerWrapper.Type);

			Container.RC_Code = "";
			AssertEquals("Should be empty", "", RefContainerWrapper.Size);
		}

		RefContainer Container;
		DocRefContainer RefContainerWrapper;
		protected override void SetUp()
		{
			Container = Factory.LoadTop1<RefContainer>(new ZQuery());
			RefContainerWrapper = DocRefContainer.New(Container, Factory);
			base.SetUp();
		}
	}
}
