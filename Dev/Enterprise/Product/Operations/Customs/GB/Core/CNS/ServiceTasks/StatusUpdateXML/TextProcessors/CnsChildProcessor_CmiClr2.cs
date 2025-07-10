using CargoWise.Types;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// CLEARANCE ADVICE REPORT  - processing same as CMI-CLR-1
	class CnsChildProcessor_CmiClr2 : CnsChildProcessor_CmiClr1
	{
		protected override ZString RegexPatternForEntryNumberAndDate
		{
			get { return @"EPU:? *-? *(?:\d{3}-)*?(\d{3}-.{7}|[^- ]{1,35}) *-? *(\d\d-\d\d-\d\d)"; }
		}

		protected override ZString DateMaskPatternForEntryDate
		{
			get { return "dd-MM-yy"; }
		}
	}
}
