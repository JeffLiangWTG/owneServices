using CargoWise.Types;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	//AMALGAMATION CLEARANCE ADVICE REPORT

	class CnsChildProcessor_CerClr2 : CnsChildProcessor_CmiClr1
	{
		protected override ZString RegexPatternForEntryNumberAndDate
		{
			get { return @"ENTRY NO:? *-? *(?:\d{3}-)*?(\d{3}-.{7}|[^- ]{1,35}) *-? *(\d\d\d\d-\d\d-\d\d)"; }
		}

		protected override ZString DateMaskPatternForEntryDate
		{
			get { return "yyyy-MM-dd"; }
		}
	}
}
