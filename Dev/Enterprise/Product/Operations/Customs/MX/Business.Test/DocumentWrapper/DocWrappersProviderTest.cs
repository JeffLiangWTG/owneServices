using System;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocWrappersProvider))]
	sealed class DocWrappersProviderTest : DocWrappersProviderAbstractTest
	{
		protected override Type ExpectedDocJobDeclarationType => typeof(DocDeclaration);

		protected override BaseJobDeclaration Declaration => Factory.New<JobDeclaration>();

		protected override IDocWrappersProvider DocWrappersProvider => new DocWrappersProvider();
	}
}
