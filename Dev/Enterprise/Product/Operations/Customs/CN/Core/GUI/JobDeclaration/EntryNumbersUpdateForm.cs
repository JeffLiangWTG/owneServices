using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class EntryNumbersUpdateForm : ZChildForm
	{
		public EntryNumbersUpdateForm(EntryNumbersBO header)
			: base(header)
		{
			BusinessEntity = header;
			InitializeComponent();
		}

		public new EntryNumbersBO BusinessEntity { get; }

		protected override void SaveInternal()
		{
			BusinessEntity.SetEntryNumbers();
			base.SaveInternal();
		}

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors() || ValidateAndSave() != ContinueWithSave.Yes)
			{
				Globals.Message.ShowError(Res.GetString("47B0B342-59D1-4CFE-8150-69DBEDD147CB", "Please clear all errors before saving."));
			}
			else
			{
				Close();
			}
		}

		void zCancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		public override string FormHeading => Res.GetString("9F9219A7-AC6F-4CEC-95EB-8D7533E4513C", "Update Entry Numbers");
	}
}
