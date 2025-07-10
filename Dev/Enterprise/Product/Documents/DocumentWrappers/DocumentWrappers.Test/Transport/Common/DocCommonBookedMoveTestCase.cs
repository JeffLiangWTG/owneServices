using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonBookedMove))]
	sealed class DocCommonBookedMoveTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var cartage = Factory.New<CommonCartage>();
			var bookedMove = cartage.LooseBookedMoves.AddNew();
			return new DocumentWrapper[] { DocCommonBookedMove.New(bookedMove, Factory) };
		}
	}
}
