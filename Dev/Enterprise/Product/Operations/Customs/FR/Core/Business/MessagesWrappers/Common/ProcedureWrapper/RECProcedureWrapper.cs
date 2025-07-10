using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class RECProcedureWrapper : CusProcedureWrapper
	{
		public RECProcedureWrapper(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
			: base(entryHeader, actionCode, messageSentDate, errorCollector)
		{
		}

		protected override ZBool ShouldGetDeclEmergencyProcDateFromSpecialMentions => true;
	}
}
