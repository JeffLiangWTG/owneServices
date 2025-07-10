namespace Enterprise.Customs.EU.Business.Declaration
{
	public static class IUcc6ValueProviderExtensions
	{
		public static bool IsUCC6AndIsImport(this IUcc6ValueProvider provider) => provider != null && provider.IsUCC6 && provider.IsImport;
		public static bool IsUCC6AndIsExport(this IUcc6ValueProvider provider) => provider != null && provider.IsUCC6 && provider.IsExport;
	}

	public interface IUcc6ValueProvider
	{
		bool IsUCC6 { get; }

		bool IsExport { get; }

		bool IsImport { get; }
	}
}
