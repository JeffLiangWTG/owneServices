using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OutstandingAmountRetrieverTest : TestCase
	{
		public void TestOutstandingAmount()
		{
			var group5Section = new SegmentGroup5MessageSection(3);

			var group5Segment1 = group5Section.InstantiateAChildAndAddItToChildrenCollection();
			var moa1 = group5Segment1.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa1.MonetaryAmount.MonetaryAmountTypeCodeQualifier = null;
			moa1.MonetaryAmount.MonetaryAmountValue = null;

			var group5Segment2 = group5Section.InstantiateAChildAndAddItToChildrenCollection();
			var moa2 = group5Segment1.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa1.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.DutyAmount;
			moa1.MonetaryAmount.MonetaryAmountValue = "999";

			var group5Segment3 = group5Section.InstantiateAChildAndAddItToChildrenCollection();
			var moa3 = group5Segment1.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa1.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.TotalAmount;
			moa1.MonetaryAmount.MonetaryAmountValue = "1024";

			var paymentProvider = new OutstandingPaymentInfoProviderForTest();
			paymentProvider.Group5Section = group5Section;
			var retriever = new OutstandingAmountRetriever(paymentProvider);
			AssertEquals(1024.0m, retriever.OutstandingAmount);
		}

		public void TestHasPaymentPendingProcessingIndicator()
		{
			var gisSection = new GISSegmentMessageSection(3);

			var gisSegment1 = gisSection.InstantiateAChildAndAddItToChildrenCollection();
			gisSegment1.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = null;
			gisSegment1.ProcessingIndicator_X.CodeListIdentificationCode = null;
			gisSegment1.ProcessingIndicator_X.CodeListResponsibleAgencyCode = null;

			var gisSegment2 = gisSection.InstantiateAChildAndAddItToChildrenCollection();
			gisSegment2.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("N");
			gisSegment2.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsProcedure;
			gisSegment2.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;

			var paymentProvider = new OutstandingPaymentInfoProviderForTest();
			paymentProvider.GISSection = gisSection;
			var retriever = new OutstandingAmountRetriever(paymentProvider);
			AssertEquals(false, retriever.HasPaymentPendingProcessingIndicator);

			var gisSegment3 = gisSection.InstantiateAChildAndAddItToChildrenCollection();
			gisSegment3.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("Y");
			gisSegment3.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsProcedure;
			gisSegment3.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;

			AssertEquals(true, retriever.HasPaymentPendingProcessingIndicator);
		}

		sealed class OutstandingPaymentInfoProviderForTest : IOutstandingPaymentInfoProvider
		{
			public SegmentGroup5MessageSection Group5Section { get; set; }

			public GISSegmentMessageSection GISSection { get; set; }
		}
	}
}
