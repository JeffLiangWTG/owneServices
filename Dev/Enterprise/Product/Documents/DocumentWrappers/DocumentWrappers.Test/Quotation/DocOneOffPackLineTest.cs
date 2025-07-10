using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocOneOffPackLine))]
	sealed class DocOneOffPackLineTest : DocumentWrapperTestCase
	{
		public void TestDimensions()
		{
			var doc = DocOneOffPackLine.New(OneOffPackLine, Factory);
			OneOffPackLine.TPL_Height = 3.251m;
			OneOffPackLine.TPL_Width = 2.1m;
			OneOffPackLine.TPL_Length = 4;
			OneOffPackLine.TPL_PackLineCount = 3;
			OneOffPackLine.TPL_DimensionUQ = Core.Constants.Length.Metres;
			AssertEquals("3.251 M", doc.Height);
			AssertEquals("2.1 M", doc.Width);
			AssertEquals("4 M", doc.Length);

			OneOffPackLine.TPL_DimensionUQ = Core.Constants.Length.Inches;
			AssertEquals("3.251 IN", doc.Height);
			AssertEquals("2.1 IN", doc.Width);
			AssertEquals("4 IN", doc.Length);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocOneOffPackLine.New(OneOffPackLine, Factory) };
		}

		RateOneOffPackLine OneOffPackLine;

		protected override void SetUp()
		{
			var rate = Factory.New<Quote>();
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_TransportMode = "SEA";
			rate.CurrentOneOffQuote.TT_ContainerMode = "LCL";
			OneOffPackLine = rate.CurrentOneOffQuote.LooseCargo.AddNew();
			base.SetUp();
		}

		#endregion
	}
}
