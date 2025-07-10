using Enterprise.LogShipping.Setup;

namespace Enterprise.LogShipping.Testing
{
	class MainDatabaseInfoForTesting : MainDatabaseInfo
	{
		public MainDatabaseInfoForTesting(LogShippingInfo setupInfo, string databaseName)
			: base(setupInfo, databaseName)
		{
		}

		public bool? SecondaryDatabaseExistValue { get; set; }
		public override bool SecondaryDatabaseExists()
		{
			return SecondaryDatabaseExistValue ?? base.SecondaryDatabaseExists();
		}

		public string SecondaryDatabaseNameValue { get; set; }
		public override string SecondaryDatabaseName
		{
			get { return SecondaryDatabaseNameValue ?? base.SecondaryDatabaseName; }
		}
	}
}
