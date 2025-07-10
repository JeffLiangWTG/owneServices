using CargoWise.EntityFramework;
using CargoWise.Types;

#if DEBUG

namespace Enterprise.Accounting.Business.ARAP
{
	public partial class Transfer
	{
		public void InitialiseNew_ForTestOnly()
		{
			InitialiseNew();
		}

		public BusinessObject LogsAndNotesTarget_ForTestOnly => LogsAndNotesTarget;

		public ZGuid GetPK_ForTestOnly => GetPK();

		public ZGuid Identifier_ForTestOnly => ((IIdentified)this).Identifier;
	}
}

#endif
