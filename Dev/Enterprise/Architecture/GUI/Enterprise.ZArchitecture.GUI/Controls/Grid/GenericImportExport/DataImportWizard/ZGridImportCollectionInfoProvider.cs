using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	class ZGridImportCollectionInfoProvider : IImportCollectionInfoProvider
	{
		public ZGridImportCollectionInfoProvider(ZGrid grid)
		{
			this.grid = grid;
		}

		public string ContextKey
		{
			get { return null; }
		}

		public IImportCollectionInfo ImportCollectionInfo
		{
			get { return new ZGridImportCollectionInfo(grid); }
		}

		readonly ZGrid grid;
	}
}
