namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Describes a notification type.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.Immutable]
	public interface INotificationType
	{
		/// <summary>
		/// Get whether this notification type prevents action such as saving an entity to
		/// the data store.
		/// </summary>
		bool IsFatal { get; }

		/// <summary>
		/// Get the severity of the notification type. For example, an error should be shown as
		/// more severe as a warning.
		/// </summary>
		int Severity { get; }

		/// <summary>
		/// In general comes from System.Forms.MessageBoxIcon enum. Put just a name here.
		/// </summary>
		string EnumValueName { get; }
	}
}
