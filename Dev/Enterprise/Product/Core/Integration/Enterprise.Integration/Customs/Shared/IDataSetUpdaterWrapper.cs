namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IRefDataSetUpdaterWrapper : ISharedDataSetUpdaterWrapper
			{
				bool DbInitializationCheck();
			}
			public interface ISRDbDataSetUpdaterWrapper : ISharedDataSetUpdaterWrapper
			{
				bool IsSchemaUpgradeSuccessful();
			}
			public interface INudgeUpdaterManagerWrapper : ISharedDataSetUpdaterWrapper
			{
				IRefDataSetUpdaterWrapper RefDataSetUpdaterWrapper { get; }
				ISRDbDataSetUpdaterWrapper SRDbDataSetUpdaterWrapper { get; }
			}
		}
	}
}
