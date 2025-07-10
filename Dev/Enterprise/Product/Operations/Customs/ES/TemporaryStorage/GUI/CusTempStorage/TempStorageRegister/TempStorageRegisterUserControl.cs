using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using CusTempStorageRegLine = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public partial class TempStorageRegisterUserControl : ZUserControl
{
	public TempStorageRegisterUserControl()
	{
		InitializeComponent();
		ItemsTabPage.RunWhenBindingOrFirstShown((s, args) => InitializeItemsTabPage());
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetHeaderLayout();
		SetGuaranteeLayout();
		SetPremisesLayout();
		SetDetailsLayout();
		SetRegLinesGridMenuItems();
	}

	void SetHeaderLayout()
	{
		DynamicHeaderPanel.UpdateLayout(new TempStorageRegisterHeaderLayout());
	}

	void SetGuaranteeLayout()
	{
		DynamicGuaranteePanel.UpdateLayout(new EU.GUI.GuaranteeGroupBoxLayout());
	}

	void SetPremisesLayout()
	{
		DynamicPremisesPanel.UpdateLayout(new TempStorageRegisterPremisesLayout());
	}

	void SetDetailsLayout()
	{
		DynamicDetailsPanel.UpdateLayout(new TempStorageRegisterDetailsLayout());
	}

	void SetRegLinesGridMenuItems()
	{
		var menuItem = new ZMenuItem(ResString.GetMultilingualString("1AE3094F-CA6B-4C4B-842E-84D2C690E845", "Assign Location + Reference"), AssignLocationAndReferenceClick);
		_ = LinesGrid.ContextMenu.MenuItems.Add(menuItem);
	}

	#region Assign Location and Reference

	void AssignLocationAndReferenceClick(object sender, EventArgs ev)
	{
		if (LinesGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}
		else
		{
			var gridSelectedElementsWithStatusNoCLS = LinesGrid.SelectedElements.Cast<CusTempStorageRegLine>().Where(x => x.SRL_CustomsStatus != "CLS");
			if (gridSelectedElementsWithStatusNoCLS.Any())
			{
				var dataToUpdateLocationAndReference = GetAssignLocationAndReferenceForm();
				if (dataToUpdateLocationAndReference != null)
				{
					var (location, shouldChangeLocation) = ShouldChangeValueForLocationAndReference(dataToUpdateLocationAndReference.EmptyLocation, dataToUpdateLocationAndReference.Location);
					var (reference, shouldChangeReference) = ShouldChangeValueForLocationAndReference(dataToUpdateLocationAndReference.EmptyReference, dataToUpdateLocationAndReference.Reference);

					if (shouldChangeLocation || shouldChangeReference)
					{
						try
						{
							var factory = new BusinessObjectFactory();

							foreach (var regLine in gridSelectedElementsWithStatusNoCLS)
							{
								regLine.SetRegLineLocationAndReference(factory, location, shouldChangeLocation, reference, shouldChangeReference);
							}

							factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
		}
	}

	(ZString, ZBool) ShouldChangeValueForLocationAndReference(ZBool leaveEmpty, ZString textBoxValue) => leaveEmpty || !textBoxValue.IsEmpty
																										? (leaveEmpty ? ZString.Empty : textBoxValue, true)
																										: (ZString.Empty, false);

	protected virtual DataToUpdateLocationAndReference GetAssignLocationAndReferenceForm() => AssignLocationAndReferenceFormHelper.GetDataFromUpdateLocationAndReferenceForm();

	#endregion

	ZString SelectRowFirstMessage => ResString.GetMultilingualString("4DE20B3A-6230-42A2-B669-67B73A3EC1A0", "Please select a row first");

	void InitializeItemsTabPage()
	{
		var itemsUserControl = (ZUserControl)Activator.CreateInstance<CusTempStorageRegLineItemUserControlWithGrid>();
		ItemsDynamicLayoutPanel.Controls.Add(itemsUserControl);
		BindingSource.SetBindingMember(itemsUserControl, ".");
		itemsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
	}
}
