namespace Enterprise.RemotePrinting.Client
{
	public enum WebPrintEventLogEntryType
	{
		//
		// Summary:
		//     An error event. This indicates a significant problem the user should know about;
		//     usually a loss of functionality or data.
		Error = 1,
		//
		// Summary:
		//     A warning event. This indicates a problem that is not immediately significant,
		//     but that may signify conditions that could cause future problems.
		Warning = 2,
		//
		// Summary:
		//     A SystemInformation. Can be a warning or an error event
		SystemInformation = 3,
		//
		// Summary:
		//     An information event. This indicates a significant, successful operation.
		Information = 4,
		//
		// Summary:
		//     A success audit event. This indicates a security event that occurs when an audited
		//     access attempt is successful; for example, logging on successfully.
		VerboseInformation = 5
	}
}
