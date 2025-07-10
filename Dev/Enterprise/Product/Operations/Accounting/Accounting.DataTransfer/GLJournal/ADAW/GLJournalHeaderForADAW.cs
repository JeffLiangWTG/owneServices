using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	[GlowDataDefinition("IGLJournalHeader")]
	public class GLJournalHeaderForADAW : GLJournalEnvironmentForADAW , IGLJournalHeader
	{
		public GLJournalHeaderForADAW(BusinessObjectFactory factory, string headerType) : base(factory)
		{
			HeaderType = headerType;
		}

		public readonly string HeaderType;

		[BusinessObjectTestExclude]
		public ZString JournalType { get; set; }
		public ZPropertyInfo JournalTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JournalType)); }
		}

		[BusinessObjectTestExclude]
		public ZString PostPeriod { get; set; }
		public ZPropertyInfo PostPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(PostPeriod)); }
		}

		[BusinessObjectTestExclude]
		public ZString ReverseOrEndPeriod { get; set; }
		public ZPropertyInfo ReverseOrEndPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(ReverseOrEndPeriod)); }
		}

		[BusinessObjectTestExclude]
		public ZString JournalDescription { get; set; }
		public ZPropertyInfo JournalDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(JournalDescription)); }
		}

		[BusinessObjectTestExclude]
		public ZString PresentationCategory { get; set; }
		public ZPropertyInfo PresentationCategoryInfo
		{
			get { return GetZPropertyInfo(nameof(PresentationCategory)); }
		}

		[BusinessObjectTestExclude]
		public ZString PostDate { get; set; }
		public ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(PostDate)); }
		}

		[BusinessObjectTestExclude]
		public ZString ReverseOrEndDate { get; set; }
		public ZPropertyInfo ReverseOrEndDateInfo
		{
			get { return GetZPropertyInfo(nameof(ReverseOrEndDate)); }
		}

		public GLJournalLineForADAWCollection GLJournalLines => glJournalLines ?? (glJournalLines = new GLJournalLineForADAWCollection(Factory, this));
		GLJournalLineForADAWCollection glJournalLines;
	}

	interface IGLJournalHeader : IJournalCompanyAndBranchForImport
	{
		public ZString JournalType { get; set; }
		public ZString PostPeriod { get; set; }
		public ZString ReverseOrEndPeriod { get; set; }
		public ZString JournalDescription { get; set; }
		public ZString PresentationCategory { get; set; }
		public ZString PostDate { get; set; }
		public ZString ReverseOrEndDate { get; set; }
	}
}
