//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDataImportHistoryValidation
//
//    This class should be used for overriding validation in AutoStmDataImportHistoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DataTransfer.Business
{
	using Res = Res;

	public class StmDataImportHistoryValidation : AutoStmDataImportHistoryValidation
	{
		public StmDataImportHistoryValidation(AutoStmDataImportHistory parent) : base(parent)
		{
		}

		protected new StmDataImportHistory Parent
		{
			get { return (StmDataImportHistory)base.Parent; }
		}

		protected override void CheckDIH_DataHash()
		{
			base.CheckDIH_DataHash();
			if (Parent.DIH_DataHash.Length != 32)
			{
				Parent.DIH_DataHashInfo.AddError(Res.GetString("5bc91259-d67e-4011-adec-ef8cc76f9b0d", "Data Hash must be 32 bytes; please use SHA256."));
			}
		}
	}
}
