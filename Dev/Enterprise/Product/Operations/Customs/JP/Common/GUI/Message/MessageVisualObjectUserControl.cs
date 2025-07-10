using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.Shared.GUI;

public partial class MessageVisualObjectUserControl : ZUserControl
{
	public MessageVisualObjectUserControl()
	{
		InitializeComponent();

		if (!DesignModeFinder.IsDesigning)
		{
			SetupButtons();
		}
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		ItemsNextButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
		ItemsPreviousButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));

		NewItemButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
		DeleteItemButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));

		ObjectsNextButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
		ObjectsPreviousButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));

		ItemsCurrentNumberCalcEdit.MaxValue = 0;
		ObjectsCurrentNumberCalcEdit.MaxValue = 0;

		if (VisualObjectParent != null)
		{
			VisualObjectParent.CurrentVisualObjectChanged -= OnCurrentItemChanged;
		}

		base.SetDataBinding(dataSource, dataMember);

		if (VisualObjectParent != null)
		{
			UpdatePreAndNextControls(VisualObjectParent);

			VisualObjectParent.CurrentVisualObjectChanged -= OnCurrentItemChanged;
			VisualObjectParent.CurrentVisualObjectChanged += OnCurrentItemChanged;
			OnCurrentItemChanged(VisualObjectParent, EventArgs.Empty);
		}
	}

	void UpdatePreAndNextControls(MessageVisualObjectParent visualObjectParent)
	{
		ObjectsNextButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), visualObjectParent, "CanMoveNext"));
		ObjectsPreviousButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), visualObjectParent, "CanMovePrevious"));

		ObjectsCurrentNumberCalcEdit.MaxValue = visualObjectParent.NumberOfResults;
		ObjectsPreNextPanel.Visible = visualObjectParent.NumberOfResults > 1;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:Don't Use Currency Manager Current Rule")]
	void OnCurrentItemChanged(object sender, EventArgs e)
	{
		var currencyManager = GetCurrencyManager();
		var numberOfResults = currencyManager != null && currencyManager.Count > 0
			? (currencyManager.Current as MessageVisualObject).NumberOfResults
			: ZInt.Zero;

		ItemsCurrentNumberCalcEdit.MaxValue = numberOfResults;
		MainSplitContainer.Panel2Collapsed = numberOfResults == 0;

		ItemsNextButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
		ItemsPreviousButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
		NewItemButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
		DeleteItemButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));

		if (sender is MessageVisualObjectParent visualObjectParent && visualObjectParent.CurrentVisualObject != null)
		{
			currencyManager.Position = Math.Max(0, visualObjectParent.CurrentNumber - 1);

			ItemsNextButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), visualObjectParent.CurrentVisualObject, "CanMoveNext"));
			ItemsPreviousButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), visualObjectParent.CurrentVisualObject, "CanMovePrevious"));
			NewItemButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), visualObjectParent.CurrentVisualObject, "CanCreateNewItem"));
			DeleteItemButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), visualObjectParent.CurrentVisualObject, "CanDeleteItem"));
		}
	}

	void SetupButtons()
	{
		ItemsPreviousButton.Click -= ItemsPreviousButton_Click;
		ItemsPreviousButton.Click += ItemsPreviousButton_Click;

		ItemsNextButton.Click -= ItemsNextButton_Click;
		ItemsNextButton.Click += ItemsNextButton_Click;

		ObjectsPreviousButton.Click -= ObjectsPreviousButton_Click;
		ObjectsPreviousButton.Click += ObjectsPreviousButton_Click;

		ObjectsNextButton.Click -= ObjectsNextButton_Click;
		ObjectsNextButton.Click += ObjectsNextButton_Click;

		NewItemButton.Click -= NewItemButton_Click;
		NewItemButton.Click += NewItemButton_Click;

		DeleteItemButton.Click -= DeleteItemButton_Click;
		DeleteItemButton.Click += DeleteItemButton_Click;
		DeleteItemButton.Enabled = false;

		SetupNavigationButton(ItemsNextButton, IconTypes.BlackWhite_Forward);
		SetupNavigationButton(ObjectsNextButton, IconTypes.BlackWhite_Forward);

		SetupNavigationButton(ItemsPreviousButton, IconTypes.BlackWhite_Back);
		SetupNavigationButton(ObjectsPreviousButton, IconTypes.BlackWhite_Back);

		SetupNavigationButton(NewItemButton, IconTypes.NewButtonActive);
		SetupNavigationButton(DeleteItemButton, IconTypes.DeleteButtonActive);
	}

	void SetupNavigationButton(ZButton button, IconTypes imageType)
	{
		button.DoNoOverrideMyEditableMode = true;
		button.Image = new Bitmap(Icons.GetImage(imageType), ControlDpiScalingHelper.NewScaledSize(15, 15));
	}

	void ItemsNextButton_Click(object sender, EventArgs e)
	{
		VisualObject?.MoveNext();
	}

	void ItemsPreviousButton_Click(object sender, EventArgs e)
	{
		VisualObject?.MovePrevious();
	}

	void ObjectsNextButton_Click(object sender, EventArgs e)
	{
		VisualObjectParent.MoveNext();
	}

	void ObjectsPreviousButton_Click(object sender, EventArgs e)
	{
		VisualObjectParent.MovePrevious();
	}

	void NewItemButton_Click(object sender, EventArgs e)
	{
		VisualObject.NewEmptyChildItem();
		OnCurrentItemChanged(VisualObjectParent, EventArgs.Empty);
	}

	void DeleteItemButton_Click(object sender, EventArgs e)
	{
		VisualObject.DeleteCurrentChildItem();
		OnCurrentItemChanged(VisualObjectParent, EventArgs.Empty);
	}

	CurrencyManager GetCurrencyManager() => BindingContext[DataSource, "VisualObjectParent.VisualObjects"] as CurrencyManager;
	MessageVisualObject VisualObject => GetCurrencyManager()?.Current as MessageVisualObject;
	MessageVisualObjectParent VisualObjectParent => (DataSource as IMessageVisualObjectParentProvider)?.VisualObjectParent;
}
