using System;
using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	public partial class FieldFilterControl : ZUserControl, IFieldFindBox
	{
		public FieldFilterControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			OperationalActionFieldDescriptor descriptor = CurrentDataItem as OperationalActionFieldDescriptor;

			if (descriptor == null)
			{
				rootType = null;
				insertFieldButton.Enabled = false;
			}
			else
			{
				rootType = descriptor.Action.Context.Supporter.RootType;
				insertFieldButton.Enabled = true;
			}

			base.OnCurrentDataItemChanged(e);
		}

		#region IFieldFindBox Members

		string IFieldFindBox.Value
		{
			get { return ""; }
			set
			{
				OperationalActionFieldDescriptor descriptor = CurrentDataItem as OperationalActionFieldDescriptor;

				if (descriptor != null)
				{
					string existing = descriptor.Filter;

					if (existing.Length > 0 && char.IsLetterOrDigit(existing, existing.Length - 1))
					{
						descriptor.Filter += ' ' + value;
					}
					else
					{
						descriptor.Filter += value;
					}
				}
			}
		}

		bool IFieldFindBox.AllowReadOnly
		{
			get { return true; }
		}

		Type IFieldFindBox.RootType
		{
			get { return rootType; }
		}

		#endregion

		#region Implementation

		void Popup()
		{
			if (rootType != null)
			{
				FieldFindBoxPopup.Show(this, FindForm());
			}
		}

		void insertFieldButton_Click(object sender, EventArgs e)
		{
			Popup();
		}

		void filterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.F4:
					Popup();
					break;
			}
		}

		Type rootType;

		#endregion
	}
}
