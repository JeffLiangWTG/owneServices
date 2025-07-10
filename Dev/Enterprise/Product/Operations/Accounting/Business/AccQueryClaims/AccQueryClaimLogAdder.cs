using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class AccQueryClaimLogAdder : NonPersistentBusinessObject
	{
		public AccQueryClaimLogAdder(AccQueryClaim accQueryClaim)
		{
			this.Parent = accQueryClaim;
		}

		ZString logComment;
		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString LogComment
		{
			get { return logComment; }
			set
			{
				if (logComment != value)
				{
					SetNonPersistentPropertyValue(LogCommentInfo, ref logComment, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLogComment();
					}
				}
			}
		}

		public ZPropertyInfo LogCommentInfo
		{
			get { return GetZPropertyInfo(nameof(LogComment)); }
		}

		public void AddLogToParent()
		{
			Parent.AddToLog(LogComment);
		}

		public readonly AccQueryClaim Parent;

		#region Validation

		public AccQueryClaimLogAdderValidation Validation
		{
			get { return GetNewValidation(); }
		}

		AccQueryClaimLogAdderValidation GetNewValidation()
		{
			return new AccQueryClaimLogAdderValidation(this);
		}

		#endregion
	}
}

