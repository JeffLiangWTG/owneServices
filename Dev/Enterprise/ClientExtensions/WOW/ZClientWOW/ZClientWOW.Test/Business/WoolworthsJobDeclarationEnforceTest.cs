using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobDeclaration))]
	public class WoolworthsJobDeclarationEnforceTest : BaseJobDeclarationAbstractTest
	{
		protected override Type ExpectedMetadataType => typeof(Enterprise.Metadata.Business.AUJobDeclaration);
	}
}
