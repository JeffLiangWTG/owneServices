using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CodeDescriptionSelectionUserControl : ZUserControl
	{
		public CodeDescriptionSelectionUserControl()
		{
			InitializeComponent();
			EditButton.Click += EditButton_Click;
		}

		public void SetBindingMember(string bindingMember, Func<BusinessObject, ICodeDescriptionOptionStorage> getStorageFunc = null)
		{
			BindingSource.SetBindingMember(TextBox, bindingMember);
			GetCodeDescriptionOptionStorage = getStorageFunc;
		}

		Func<BusinessObject, ICodeDescriptionOptionStorage> GetCodeDescriptionOptionStorage { get; set; }

		[DefaultValue(false)]
		public bool HideCodeOnSelectionForm { get; set; }

		void EditButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is BusinessObject bizObj && GetCodeDescriptionOptionStorage?.Invoke(bizObj) is ICodeDescriptionOptionStorage storage)
			{
				var parent = new CodeDescriptionOptionCollectionParent(bizObj.Factory, storage);
				ZFormModaliser.ShowDialogAndDispose(new CodeDescriptionOptionForm(parent, HideCodeOnSelectionForm));
			}
		}
	}
}
