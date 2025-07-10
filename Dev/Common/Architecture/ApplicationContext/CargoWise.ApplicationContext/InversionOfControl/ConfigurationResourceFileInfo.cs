namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Contains the information for a configuration resource file.
	/// </summary>
	class ConfigurationResourceFileInfo
	{
		/// <summary>
		/// Gets/Sets the assembly name of the configuration resource file.
		/// </summary>
		public string AssemblyName { get; set; }

		/// <summary>
		/// Gets/Sets the namespace of the configuration resource file.
		/// </summary>
		public string Namespace { get; set; }

		/// <summary>
		/// Gets/Sets the name of the configuration resource file.
		/// </summary>
		public string FileName { get; set; }
	}
}
