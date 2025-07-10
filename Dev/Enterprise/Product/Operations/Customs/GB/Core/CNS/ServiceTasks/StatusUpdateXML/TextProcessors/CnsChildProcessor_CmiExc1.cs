using CargoWise.Types;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	class CnsChildProcessor_CmiExc1 : CnsChildProcessor
	{
		// Exam ... Charge Notification 
		protected override ZString RegexPatternForEntryNumberAndDate
		{
			get
			{
				// EPU: 290 Entry No: 023364N Route: 1  Status: 03  Date: 23-09-13
				return @"EPU:? *(\d\d\d) *Entry No:? *(.{7}) *Route: *.*? *Status:? *.*? *Date:? *(\d\d-\d\d-\d\d)";
			}
		}

		protected override ZDateTime EntryDateCore()
		{
			var dateString = RegexMatchForEntryNumberAndDate.Groups[3].Value;
			var result = ZDateTime.Empty;
			ZDateTime.TryParseExact(dateString, out result, "dd-MM-yy");
			return result;
		}

		protected override ZString EntryNumberCore()
		{
			return string.Format("{0}-{1}", RegexMatchForEntryNumberAndDate.Groups[1].Value, RegexMatchForEntryNumberAndDate.Groups[2].Value);
		}
	}
}
