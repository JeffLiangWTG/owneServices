using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class BulkLookupChangeForm : ZChildForm
	{
		public BulkLookupChangeForm(BulkLookupChanger businessObject)
			: base(businessObject)
		{
		}

		#region Events

		private void ContinueBtn_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors())
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
			}
		}

		#endregion
	}
}
