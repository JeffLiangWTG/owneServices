using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class LicenceModuleFeeBasisForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public LicenceModuleFeeBasisForm()
		{
			InitializeComponent();
		}

		public LicenceModuleFeeBasisForm(LicenceModuleFeeBasis bo)
			: base(bo)
		{
			InitializeComponent();
		}

		void okButton_Click(object sender, EventArgs e)
		{
			LicenceModuleFeeBasis bizo = (LicenceModuleFeeBasis)BusinessEntity;
			if (bizo != null)
			{
				bizo.RunPreSaveValidation();

				if (bizo.HasErrors)
				{
					return;
				}
			}

			DialogResult = DialogResult.OK;
		}
	}
}

