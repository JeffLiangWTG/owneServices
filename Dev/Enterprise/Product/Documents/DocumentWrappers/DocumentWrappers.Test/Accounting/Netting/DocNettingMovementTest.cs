using Enterprise.Accounting.Netting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingMovement))]
	sealed class DocNettingMovementTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocNettingMovement[] { DocNettingMovement.New(new NettingMovement(Factory), Factory) };
		}
	}
}
