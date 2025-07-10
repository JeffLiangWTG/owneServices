using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocSalesTradeLane))]
	public class DocSalesTradeLaneTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocSalesTradeLane.New(new ViewDocTradeLane(), Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocSalesTradeLane.New(new ViewDocTradeLane(), Factory);
		}
	}
}
