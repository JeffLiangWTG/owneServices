namespace CargoWise.Integration
{
	public interface IClientHook
	{
		ITypeDeciderDictionary ClientTypeDeciders { get; }
		object GetTableSchema(string tableName);
		string UniqueId { get; }

		bool IsInitialised { get; }
		void Initialise();
		void Initialise(bool loggedIn);
		void Uninitialise();
		bool HasCompanySpecificOverrides { get; }
		bool IsUpgrading { get; set; }
	}
}
