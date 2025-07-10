namespace Enterprise.DbUpgrader.Transformation.Common
{
	#region ISourceTableTestHelper

	public interface ISourceTableTestHelper
	{
		#if DEBUG
		void CreateOriginalTableAndOrColumnsIfNotExist();
		void DropCreatedOriginalTableAndOrColumnsAfterCopyingData();
		#endif
	}

	#endregion
}
