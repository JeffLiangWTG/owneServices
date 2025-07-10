namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Notifies the user about errors and warnings during posting.
	/// </summary>
	public interface IPostManagerUserNotifier
	{
		/// <summary>
		/// Display a validation error message to the user via GUI or other means.
		/// </summary>
		void NotifyPostValidationError(string error);

		/// <summary>
		/// Display a warning message to the user via GUI or other means.
		/// </summary>
		void NotifyPostingWarning(string warning);
	}
}
