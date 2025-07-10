using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestsSubclassesOf(typeof(IEMCSTopMenuProvider))]
	public abstract class EMCSTopMenuProviderAbstractTest<T> : TestCaseWithFactory where T : IEMCSTopMenuProvider, new()
	{
		public void TestTopLevelMenu()
		{
			AssertEquals(ExpectedMenuType, provider.TopLevelMenu(declaration).GetType());
		}

		public void TestEMCSTopeMenuProviderType()
		{
			AssertType<T>(provider);
		}

		protected abstract string CountryOrGroupingCode { get; }

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			provider = EMCSTopMenuProvider.GetTopMenuProvider(CountryOrGroupingCode);
		}

		protected abstract Type ExpectedMenuType { get; }
		EMCSJobDeclaration declaration;
		IEMCSTopMenuProvider provider;
	}
}
