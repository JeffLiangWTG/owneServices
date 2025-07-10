using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Australia)]
	[TestedType(typeof(DocWrappersProvider))]
	sealed class DocWrappersProviderTest : DocWrappersProviderAbstractTest
	{
		protected override Type ExpectedDocJobDeclarationType => typeof(DocDeclaration);

		protected override BaseJobDeclaration Declaration => Factory.New<JobDeclaration>();

		protected override IDocWrappersProvider DocWrappersProvider => new DocWrappersProvider();
	}
}
