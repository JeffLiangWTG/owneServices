using CargoWiseOne.ResourceStrings;

/// <summary>
/// Represents the definition of a username + password pair.
/// </summary>
public interface IEInvoicingPasswordCredentialDefinition
{
	/// <summary>
	/// Defines the ascending order of credentials displayed on screen.
	/// </summary>
	int DisplayOrder { get; }

	/// <summary>
	/// Unique identifier for each username + password pair.
	/// Used to send data in GEI message, so should be kept short due to xT limitation.
	/// </summary>
	string UniqueKey { get; }

	/// <summary>
	/// Name of the username part on GUI.
	/// </summary>
	IMultilingualString UsernameLabel { get; }

	/// <summary>
	/// Name of the password part on GUI.
	/// </summary>
	IMultilingualString PasswordLabel { get; }
}
