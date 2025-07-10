namespace CargoWise.Loader.Common
{
	public sealed class InstallationResult
	{
		public static InstallationResult OK(string message = null)
		{
			return new InstallationResult(InstallationResultStatus.OK, message ?? string.Empty, false);
		}

		public static InstallationResult Warning(string message)
		{
			return new InstallationResult(InstallationResultStatus.Warning, message, true);
		}

		public static InstallationResult NotAffectCargoWiseFunctionsWarning(string message)
		{
			return new InstallationResult(InstallationResultStatus.Warning, message, false);
		}

		public static InstallationResult Error(string message)
		{
			return new InstallationResult(InstallationResultStatus.Error, message, true);
		}

		InstallationResult(InstallationResultStatus status, string message, bool affectCargoWiseFunctions)
		{
			Status = status;
			Message = message;
			AffectCargoWiseFunctions = affectCargoWiseFunctions;
		}

		public readonly InstallationResultStatus Status;
		public readonly string Message;

		public bool IsOK
		{
			get
			{
				return Status == InstallationResultStatus.OK;
			}
		}

		public bool IsWarning
		{
			get
			{
				return Status == InstallationResultStatus.Warning;
			}
		}

		public bool AffectCargoWiseFunctions { get; }

		public bool IsError
		{
			get
			{
				return Status == InstallationResultStatus.Error;
			}
		}

		public override string ToString()
		{
			return Status.ToString() + ": " + Message;
		}
	}
}
