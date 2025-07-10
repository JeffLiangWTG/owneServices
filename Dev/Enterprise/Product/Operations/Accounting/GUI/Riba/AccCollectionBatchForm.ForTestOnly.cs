#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AccCollectionBatchForm
	{
		public void OnShown_ForTestOnly(System.EventArgs e)
		{
			OnShown(e);
		}

		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}
	}
}

#endif
