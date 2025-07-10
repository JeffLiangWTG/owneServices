using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDetentionInvoice))]
	sealed class DocDetentionInvoiceTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			ContainerDetention detention = Factory.New<ContainerDetention>();

			return new DocumentWrapper[]
			{
				DocDetentionInvoice.New(detention, Factory)
			};
		}

		#endregion
	}
}
