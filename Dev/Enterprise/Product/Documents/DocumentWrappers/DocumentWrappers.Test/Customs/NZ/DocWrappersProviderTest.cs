using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.NewZealand)]
	[TestedType(typeof(DocWrappersProvider))]
	sealed class DocWrappersProviderTest : DocWrappersProviderAbstractTest
	{
		protected override Type ExpectedDocJobDeclarationType => typeof(DocDeclaration);

		protected override BaseJobDeclaration Declaration => Factory.New<JobDeclaration>();

		protected override IDocWrappersProvider DocWrappersProvider => new DocWrappersProvider();
	}
}
