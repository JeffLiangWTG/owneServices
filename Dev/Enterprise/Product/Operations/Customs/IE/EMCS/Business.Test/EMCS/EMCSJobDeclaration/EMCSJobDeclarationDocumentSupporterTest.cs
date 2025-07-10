using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclarationDocumentSupporter))]
	sealed class EMCSJobDeclarationDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestWrapperClassFullNamespace()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var supporter = new EMCSJobDeclarationDocumentSupporterForTesting(declaration);

			var result = supporter.WrapperClassFullNamespace_Exposed();

			AssertEquals("Enterprise.Customs.IE.EMCS.DocumentWrappers.IEEMCSDeclarationWrapper", result);
		}

		public void TestCreateDeclarationWrapper()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var supporter = new EMCSJobDeclarationDocumentSupporterForTesting(declaration);

			var result = supporter.WrapperClassAssemblyName_Exposed();

			AssertEquals("Enterprise.Customs.IE.EMCS.DocumentWrappers", result);
		}

		public class EMCSJobDeclarationDocumentSupporterForTesting : EMCSJobDeclarationDocumentSupporter
		{
			public EMCSJobDeclarationDocumentSupporterForTesting(EMCSJobDeclaration jobDeclaration) : base(jobDeclaration)
			{
			}

			public ZString WrapperClassFullNamespace_Exposed() => WrapperClassFullNamespace;

			public string WrapperClassAssemblyName_Exposed() => WrapperClassAssemblyName;
		}
	}
}
