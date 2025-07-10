using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ScheduleValidation : ZValidation
	{
		public ScheduleValidation(Schedule parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public sealed override Type AutoValidationType
		{
			get { return null; }
		}

		public override void ValidateAll()
		{
			ValidatePeriodCount();
			ValidatePeriodScope();
		}

		protected Schedule Parent
		{
			get { return parent; }
		}

		readonly Schedule parent;

		#region Period Number

		public void ValidatePeriodCount()
		{
			ValidateCalculatedProperty(Parent.PeriodCountInfo);
		}

		protected void CheckPeriodCount()
		{
			if (!Parent.IsThisPeriodScope && !Parent.PeriodScopeInfo.HasErrors())
			{
				CompareValidation.CheckWithinRange(Parent.PeriodCountInfo, 1, Schedule.MaxPeriodCount);
			}
		}

		#endregion

		#region Period Scope

		public void ValidatePeriodScope()
		{
			ValidateCalculatedProperty(parent.PeriodScopeInfo);
		}

		protected virtual void CheckPeriodScope()
		{
			MandatoryValidation.CheckEntered(parent.PeriodScopeInfo);
			ListValidation.ErrorIfInvalidCode(parent.PeriodScopeInfo, parent.Lookups.PeriodScopes);
		}

		#endregion
	}
}
