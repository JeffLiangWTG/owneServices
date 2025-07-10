using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class VARProcedureWrapper : CusProcedureWrapper
	{
		public VARProcedureWrapper(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
			: base(entryHeader, actionCode, messageSentDate, errorCollector)
		{
		}
	}
}
