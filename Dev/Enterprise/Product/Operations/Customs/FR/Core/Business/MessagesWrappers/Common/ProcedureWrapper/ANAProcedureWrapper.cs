using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ANAProcedureWrapper : CusProcedureWrapper
	{
		public ANAProcedureWrapper(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
			: base(entryHeader, actionCode, messageSentDate, errorCollector)
		{
		}
	}
}
