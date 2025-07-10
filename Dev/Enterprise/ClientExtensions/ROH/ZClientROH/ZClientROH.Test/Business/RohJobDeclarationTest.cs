using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.Testing
{
	[TestedType(typeof(RohJobDeclaration))]
	public class RohJobDeclarationTest : BaseJobDeclarationAbstractTest
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.AUJobDeclaration);
		public void TestNew()
		{
			AssertEquals("Type of instance created by static New", typeof(RohJobDeclaration), RohJobDeclaration.New(Factory).GetType());
		}

		public void TestDocumentSupporter()
		{
			AssertEquals("Type of Document Supporter", typeof(RohJobDeclarationDocumentSupporter), RohJobDeclaration.New(Factory).DocumentSupporter.GetType());
		}
	}
}
