using System;

namespace CargoWise.NGenInstallerProgram.Exceptions
{
	[Serializable]
	class ParameterParseException : Exception
	{
		public ParameterParseException(Exception ex)
			: base($"Failed to parse command line parameters. Should be: {Program.ProgName}.exe RootFilePath ({GetUseableActions()}) [ExecutionTimeoutSeconds]", ex)
		{
		}

#if NETFRAMEWORK
		protected ParameterParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		static string GetUseableActions()
		{
			return string.Join(
				" | ",
				Enum.GetNames(typeof(NGenAction)));
		}
	}
}
