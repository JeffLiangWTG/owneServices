using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7StandAloneDeclarationUserControl : ZUserControl
	{
		public EUH7StandAloneDeclarationUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			const string IsVisibleForBindingString = nameof(IsVisibleForBinding);

			if (dataSource != null)
			{
				ConvertButton.DataBindings.RemoveBinding(IsVisibleForBindingString);
				EditButton.DataBindings.RemoveBinding(IsVisibleForBindingString);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				ConvertButton.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, DataMember + "." + nameof(AsycudaBill.CanConvertToStandAloneDeclaration)));
				EditButton.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, DataMember + "." + nameof(AsycudaBill.CanEditStandAloneDeclaration)));
			}
		}

		void ConvertButton_Click(object sender, EventArgs e)
		{
			new ConvertToStandAloneDeclarationMenuItem(() => [CurrentBill]).MenuAction();
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			var declaration = CurrentBill?.StandAloneDeclaration;

			if (declaration != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
				controller.ShowEditForm(declaration);
			}
		}

		AsycudaBill CurrentBill => CurrentDataItem as AsycudaBill;
	}
}
