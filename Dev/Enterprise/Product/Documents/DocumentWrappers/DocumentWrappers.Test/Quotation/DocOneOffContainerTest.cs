using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocOneOffContainer))]
	sealed class DocOneOffContainerTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocOneOffContainer.New(OneOffContainer, Factory) };
		}

		RateOneOffContainers OneOffContainer;

		protected override void SetUp()
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_TransportMode = "SEA";
			rate.CurrentOneOffQuote.TT_ContainerMode = "LCL";
			OneOffContainer = rate.CurrentOneOffQuote.Containers.AddNew();
			base.SetUp();
		}

		#endregion
	}
}
