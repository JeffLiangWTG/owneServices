using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class ModifyAffinitiesForm : ZChildForm
	{
		public ModifyAffinitiesForm()
		{
			InitializeComponent();
		}

		public ModifyAffinitiesForm(BMNCNShape diagramShape)
			: base(diagramShape.ShapeAffinities)
		{
			InitializeComponent();
		}

		public new ShapeAffinityCollection DataSource
		{
			get { return (ShapeAffinityCollection)base.DataSource; }
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			ValidateAll(ValidationType.Full);

			if (DataSource.HasErrors)
			{
				Globals.Message.Show(Res.GetString("f1e08794-77bc-466f-a808-09b21516c09e", "There are Errors on this Page"), Res.GetString("f3691bd9-bf94-4ea0-bfa3-77e9f976e6da", "Fix Errors"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.Cancel = true;
			}
		}
	}
}
