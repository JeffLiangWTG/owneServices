using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(IDocWrappersProvider))]
	public abstract class DocWrappersProviderAbstractTest : TestCaseWithFactory
	{
		public void TestDocJobDeclarationType()
		{
			AssertEquals(ExpectedDocJobDeclarationType, DocWrappersProvider.DocJobDeclarationType);
		}

		public void TestNewDocDeclarationWrapper()
		{
			AssertType(ExpectedDocJobDeclarationType, DocWrappersProvider.NewDocDeclarationWrapper(Declaration, Factory));
		}

		protected abstract Type ExpectedDocJobDeclarationType { get; }

		protected abstract BaseJobDeclaration Declaration { get; }

		protected abstract IDocWrappersProvider DocWrappersProvider { get; }
	}
}
