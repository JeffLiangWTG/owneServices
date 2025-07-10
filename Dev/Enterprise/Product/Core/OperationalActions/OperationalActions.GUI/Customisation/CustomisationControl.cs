using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed partial class CustomisationControl : ZUserControl
	{
		public CustomisationControl()
		{
			InitializeComponent();
		}

		[Browsable(false)]
		[Bindable(BindableSupport.No)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowFieldsTab
		{
			set { fieldsTabPage.TabVisible = value; }
		}

		[Browsable(false)]
		[Bindable(BindableSupport.No)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowDocumentsTab
		{
			set { documentsTabPage.TabVisible = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of bound property")]
		public OperationalAction SelectedAction
		{
			get
			{
				const string ActionsPropertyName = "Actions";
				CurrencyManager manager;

				if ((manager = (CurrencyManager)GetBindingManager(ActionsPropertyName)) == null || manager.Position < 0)
				{
					return null;
				}
				else
				{
					return (OperationalAction)manager.GetCurrent();
				}
			}
		}
	}
}
