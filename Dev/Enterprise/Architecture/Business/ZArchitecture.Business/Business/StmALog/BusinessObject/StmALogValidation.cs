//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmALogValidation
//
//    This class should be used for overriding validation in AutoStmALogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmALogValidation : AutoStmALogValidation
	{
		public StmALogValidation(AutoStmALog parent)
			: base(parent)
		{
		}

		protected override void CheckSL_SE_NKEvent()
		{
			base.CheckSL_SE_NKEvent();

			if (!Events.All.Contains(Parent.SL_SE_NKEvent) && (!Parent.IsInDatabase || Parent.SL_SE_NKEventInfo.HasChanges))
			{
				ErrorReporter.ReportOnce(
					"InvalidEventCode" + Parent.SL_SE_NKEvent + Parent.SL_Table,
					string.Format("The event code '{0}' is not valid! (SL_Table: '{1}', SL_Reference: '{2}', IsInDatabase: '{3}')", Parent.SL_SE_NKEvent, Parent.SL_Table, Parent.SL_Reference, Parent.IsInDatabase));
			}
		}

		protected override void CheckSL_PostedTimeUtc()
		{
			// don't validate the posted time as it will be set during OnSaving()
		}

		protected override void CheckSL_PostedTimeUtcIsNotEmpty()
		{
			// don't validate the posted time as it will be set during OnSaving()
		}

		protected override void CheckSL_PostedTimeUtcIsValidZDateTime()
		{
			// don't validate the posted time as it will be set during OnSaving()
		}

		protected override void CheckSL_PostedTimeUtcIsValidZDateTimeRange()
		{
			// don't validate the posted time as it will be set during OnSaving()
		}

		protected override void CheckSL_EventTimeIsValidZDateTimeRange()
		{
			// don't validate the event time as it can legitimatly be more than 5 years into the future or past and
			// there is nothing the user can do to fix it anyway.
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == StmALogSchema.Constants.SL_SE_NKEvent)
			{
				return ShouldValidateInactiveEvents;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		protected virtual bool ShouldValidateInactiveEvents
		{
			get { return false; }
		}
	}
}
