namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ITemporaryStorageSettings
			{
				bool IsUsingUCC5 { get; }

				bool IsUsingUCC6 { get; }

				bool IsUsingTSRegister { get; }
			}
		}
	}
}
