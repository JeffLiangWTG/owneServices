using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CA.Module.OperationalActions;

public class CAB3OperationalActionMethodApplicatorValidation : ZValidation
{
	public CAB3OperationalActionMethodApplicatorValidation(CAB3OperationalActionMethodApplicator parent)
		: base(parent)
	{
		this.parent = parent;
		this.zValidationInternals = this;
		this.parentListInternals = parent;
	}

	public void Add(CAB3OperationalActionMethodApplicatorValidation validation)
	{
		zValidationInternals.Add(validation);
	}

	public void Remove(CAB3OperationalActionMethodApplicatorValidation validation)
	{
		zValidationInternals.Remove(validation);
	}

	#region ValidateAll

	public override void ValidateAll()
	{
		using (parentListInternals.SuspendListChanged())
		{
			ValidateAllCore();
		}
	}

	protected void ValidateAllCore()
	{
		ValidateDefaultScheduleActionCode();
	}

	#endregion

	#region DefaultScheduleActionCode

	public void ValidateDefaultScheduleActionCode()
	{
		zValidationInternals.Validate(Parent.DefaultScheduleActionCodeInfo, new RunValidationInvoker(this.DefaultScheduleActionCodeValidationInvoker));
	}

	void DefaultScheduleActionCodeValidationInvoker()
	{
		CheckDefaultScheduleActionCodeIsWesternEuropean();
		CheckDefaultScheduleActionCode();
	}

	protected void CheckDefaultScheduleActionCodeIsWesternEuropean()
	{
		EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.DefaultScheduleActionCodeInfo);
	}

	protected void CheckDefaultScheduleActionCode()
	{
		MandatoryValidation.CheckEntered(Parent.DefaultScheduleActionCodeInfo, DefaultScheduleActionDescription);
		ListValidation.ErrorIfInvalidCode(Parent.DefaultScheduleActionCodeInfo, DefaultScheduleActionDescription);
	}

	#endregion

	public override System.Type AutoValidationType
	{
		get
		{
			return typeof(CAB3OperationalActionMethodApplicatorValidation);
		}
	}

	public CAB3OperationalActionMethodApplicator Parent
	{
		[System.Diagnostics.DebuggerStepThrough]
		get
		{
			return parent;
		}
	}

	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	readonly CAB3OperationalActionMethodApplicator parent;
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	readonly IValidationInternals zValidationInternals;
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	readonly ISingleElementListInternal parentListInternals;

	IMultilingualString DefaultScheduleActionDescription
	{
		get { return ResString.GetMultilingualString("FEA41287-2B18-4D56-8FA5-9E0E4B648576", "default action"); }
	}
}
