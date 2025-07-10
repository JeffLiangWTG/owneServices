#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	public partial class BankStatement
	{
		[ChildEditable(true)]
		public StatementCollection Statements_ForTestOnly => Statements;

		public int GetMaxSequenceNo_ForTestOnly(ZDateTime date)
		{
			return GetMaxSequenceNo(date);
		}

		public void SetSequenceNo_ForTestOnly()
		{
			SetSequenceNo();
		}
	}
}

#endif
