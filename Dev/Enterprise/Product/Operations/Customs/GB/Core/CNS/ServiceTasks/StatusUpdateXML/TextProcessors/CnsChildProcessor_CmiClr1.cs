using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// CLEARANCE ADVICE REPORT                

	class CnsChildProcessor_CmiClr1 : CnsChildProcessor, IPortAuthorityHoldApplicationProvider
	{
		protected override ZString RegexPatternForEntryNumberAndDate
		{
			get { return @"EPU:? *-? *(?:\d{3}-)*?(\d{3}-.{7}|[^- ]{1,35}) *-? *(\d\d-\d\d-\d\d)"; }
		}

		protected override ZString DateMaskPatternForEntryDate
		{
			get { return "dd-MM-yy"; }
		}

		protected override void DoFurtherProcessingForSuccessfullyFoundEntry(Business.Declaration.CusEntryHeader entryHeader)
		{
			base.DoFurtherProcessingForSuccessfullyFoundEntry(entryHeader);
			entryHeader.ApplyOrRemoveHoldOrClear(this);
		}

		public AddOrRemove DirectionOfApplication
		{
			get { return AddOrRemove.Cleared; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Is a Regex time pattern")]
		public ZDateTime Date
		{
			get
			{
				var clearanceDate = ZDateTime.Empty;
				var dateString = GetFirstGroupMatchFromMessageText(@"Issued\s(\d\d-\d\d-\d\d\s\d\d:\d\d)");
				ZDateTime.TryParseExact(dateString, out clearanceDate, "dd-MM-yy HH:mm");
				return clearanceDate;
			}
		}

		public string HoldType
		{
			get { return ""; }
		}

		public string HoldAuthority
		{
			get { return ""; }
		}
	}
}
