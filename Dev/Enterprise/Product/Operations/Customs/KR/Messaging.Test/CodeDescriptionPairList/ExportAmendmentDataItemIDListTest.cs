namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ExportAmendmentDataItemIDListTest : NUnit.Framework.TestCase
	{
		public void TestIsDateTimeField()
		{
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.AB01));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.AB02));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A904));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A903));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A905));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A901));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A606));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A607));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A608));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.A601));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.H103));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.H102));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.H104));
			Assert(!ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.H101));
			Assert(ExportAmendmentDataItemIDList.IsDateTimeField(ExportAmendmentDataItemIDList.Codes.G105));
		}
		public void TestIsHeaderDataItem()
		{
			Assert(ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.AB01));
			Assert(!ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.H103));
			Assert(!ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.B102));
			Assert(!ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.C001));
			Assert(!ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.H101));
			Assert(!ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.G105));
			Assert(!ExportAmendmentDataItemIDList.IsHeaderDataItem(ExportAmendmentDataItemIDList.Codes.E102));
		}
	}
}
