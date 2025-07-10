using CargoWise.Integration;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal delegate void LookupGuidChanged(ICodeDescription change);

	internal interface IGuidChangeNotifier
	{
		event LookupGuidChanged LookupGuidChanged;
	}

	public interface IColumnHeadingManagerListener
	{
		void SetManager(ColumnConfigurationsManager manager);
	}
}
