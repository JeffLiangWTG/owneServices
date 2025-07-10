using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ANTProcedureWrapper : CusProcedureWrapper
	{
		public ANTProcedureWrapper(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
			: base(entryHeader, actionCode, messageSentDate, errorCollector)
		{
		}

		protected override ZString GetEstimatedAssessmentDate()
		{
			return entryHeader.EntryInstruction?.CEI_DateForDuty.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
		}

		protected override ZString GetEstimatedAssessmentHour()
		{
			return entryHeader.EntryInstruction?.CEI_DateForDuty.ToShortTimeString();
		}

		protected override ZBool ShouldGetDeclEmergencyProcDateFromSpecialMentions => true;
	}
}
