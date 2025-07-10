namespace Enterprise.Services.OperationalActions.Support
{
	public enum OperationalActionLogErrorLevel
	{
		/// <summary>
		/// Use this to provide information only useful for debugging on a clients system.
		/// (these messages will only be shown if "Include Debug" is ticked at the time the message is added).
		/// </summary>
		Debug = -1,

		/// <summary>
		/// Use this to indicate what stage you are upto without affecting the behaviour.
		/// </summary>
		Informational = 0,

		/// <summary>
		/// Use this to indicate a successful operation or step completed to the user without affecting the behaviour.
		/// </summary>
		Success = 1,

		/// <summary>
		/// Use this to indicate something the user would be interested in knowing without stopping the action.
		/// (this behaves just like 'Informational' but prevents the runner form from being closed at the end.)
		/// </summary>
		Warning = 2,

		/// <summary>
		/// Use this to indicate an error that should abort the action.
		/// </summary>
		Error = 3,
	}
}
