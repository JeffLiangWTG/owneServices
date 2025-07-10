using System;
using System.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PersonalItemsDeclarationUserControl : ZUserControl
	{
		public PersonalItemsDeclarationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CustomsAndItemDetailsPanel.UpdateLayout(new SEDDetails008Layout());
			TransportDetailsPanel.UpdateLayout(new TransportDetailsLayout());
			OrganizationPanel.UpdateLayout(new OrganizationLayout());
			VehiclesPanel.UpdateLayout(new VehicleLayout());

			if (JobDeclaration != null)
			{
				JobDeclaration.JE_ExportGoodsTypeInfo.ValueChanged -= HasItem_ValueChanged;
				JobDeclaration.JE_ExportGoodsTypeInfo.ValueChanged += HasItem_ValueChanged;

				JobDeclaration.OnHasItemsChanging -= new CancelEventHandler(JobDeclaration_OnHasItemsChanging);
				JobDeclaration.OnHasItemsChanging += new CancelEventHandler(JobDeclaration_OnHasItemsChanging);
				UpdateItemDetailsGroupBoxVisiblity();
			}
		}
		void HasItem_ValueChanged(object sender, EventArgs e)
		{
			UpdateItemDetailsGroupBoxVisiblity();
		}

		void UpdateItemDetailsGroupBoxVisiblity()
		{
			ItemDetailsGroupBox.Visible = JobDeclaration.PIDHasItems == YesNoList.Codes.Yes;
		}

		void JobDeclaration_OnHasItemsChanging(object sender, CancelEventArgs e)
		{
			var eventArgs = (JobDeclaration.PIDHasItemsEventArgs)e;
			if (JobDeclaration.HasPIDItemLinesAboutToLose(eventArgs.PIDHasItemsNewValue))
			{
				var messageBoxResult = Globals.Message.Show(
							Res.GetString("73D6D80D-0D2D-4DBE-98F6-5E4326491BF7", "There are lines entered. If you proceed, system will delete all the lines. Are you sure?"),
							Res.GetString("A99207FB-10FB-4FB3-BFBE-2803C4FC18E7", "Warning"),
							ZMessageBoxButtons.YesNo,
							ZMessageBoxIcon.Warning,
							ZDialogResult.Yes
						);

				if (messageBoxResult != ZDialogResult.Yes)
				{
					eventArgs.Cancel = true;
				}
			}
		}

		JobDeclaration JobDeclaration
		{
			get
			{
				JobDeclaration result = null;
				if (BindingSource.DataSource != null)
				{
					result = (JobDeclaration)BindingSource.DataSource;
				}
				return result;
			}
		}
	}
}
