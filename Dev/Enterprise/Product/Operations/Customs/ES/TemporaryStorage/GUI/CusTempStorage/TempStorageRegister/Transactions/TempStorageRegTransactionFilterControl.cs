using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusTempStorageRegLine = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransaction;
using CusTempStorageRegLineTransactionCollection = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionCollection;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public partial class TempStorageRegTransactionFilterControl : ZFilterStripControl, IAllowTabBackwardBetweenSomeOfMyChildren
{
	public TempStorageRegTransactionFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
		: base(gridCollection, filterStripBusinessObject)
	{
		InitializeComponent();

		if (!DesignModeFinder.IsDesigning)
		{
			PerformSearch += new EventHandler<PerformSearchEventArgs>(FilterControl_PerformSearch);
			this.gridCollection = gridCollection as CusTempStorageRegLineTransactionCollection;
		}

		ToolStripPermissionsLabel.AllowOverlap(NewTransactionButton);
	}

	public override IBusinessObjectCollection GridCollection => gridCollection;
	CusTempStorageRegLineTransactionCollection gridCollection;

	internal void SetGridCollection(CusTempStorageRegLineTransactionCollection gridCollection)
	{
		if (CurrentDataItem != null)
		{
			var regLine = (CusTempStorageRegLine)CurrentDataItem;
			NewTransactionButton.Visible = !regLine.IsClosed && regLine.RegHeader.Premises.SRP_IsActive;
		}
		this.gridCollection = gridCollection;
	}

	bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		=> control.Name == "NewTransactionButton" && previousControl.Name == "FilteredGrid";

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetTransactionsGridMenuItems();
	}

	protected override void AddAlwaysVisibleFilterStrips()
	{
		base.AddAlwaysVisibleFilterStrips();
		var strip = FilterBusinessObject.FilterStrips.AddNew();
		strip.FilterDescription = TempStorageRegTransactionFilterStripBusinessObject.FilterConstants.Status;
		_ = AddFilterStrip(strip);
	}

	protected void FilterControl_PerformSearch(object sender, EventArgs e)
	{
		gridCollection.AdditionalFilter = FilterBusinessObject.Filter;
	}

	void SetTransactionsGridMenuItems()
	{
		var menuItem = new ZMenuItem(ResString.GetMultilingualString("2F40FD74-B5B4-41E0-A210-A0280FC39760", "Update In/Out Date"), UpdateTransactionDateClick);
		_ = Grid.ContextMenu.MenuItems.Add(menuItem);
	}

	#region UpdateTransactionDate

	void UpdateTransactionDateClick(object sender, EventArgs ev)
	{
		if (Grid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}
		else
		{
			var gridSelectedElementsWithTransactionStatusCON = Grid.SelectedElements
				.Cast<CusTempStorageRegLineTransaction>()
				.Where(x => x.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed && !x.SRT_TransactionDate.IsEmpty)
				.ToArray();

			if (gridSelectedElementsWithTransactionStatusCON.Length > 0)
			{
				var physicalDate = GetRequestDateForm().ToOffset();
				if (!physicalDate.IsEmpty)
				{
					foreach (var transaction in gridSelectedElementsWithTransactionStatusCON)
					{
						if (physicalDate >= transaction.SRT_TransactionDate)
						{
							SaveTransaction(transaction.PK, physicalDate);
						}
						else
						{
							Globals.Message.ShowError(GetUpdateTransactionDateErrorMessage(transaction.SRT_InternalReferenceType, transaction.SRT_InternalReferenceNumber));
						}
					}
				}
			}
		}
	}

	void SaveTransaction(ZGuid transactionPK, ZDateTimeOffset physicalDate)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var newFactoryTransaction = factory.Load<CusTempStorageRegLineTransaction>(transactionPK);
			newFactoryTransaction.SRT_PhysicalInOutDate = physicalDate;
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	protected virtual ZDateTime GetRequestDateForm() => ES.GUI.RequestDateFormHelper.GetDateFromRequestDateForm(formTitle, message, dateCaption);

	readonly ResourceString formTitle = ResString.GetMultilingualString("82307521-8820-4D1A-A084-61D9F2453C01", "Update In/Out Date");
	readonly ZString message = ResString.GetMultilingualString("5E437183-9D0A-43C9-AB96-963B96664090", "Enter date and time when goods physically entered/exited the Temporary Storage");
	readonly ResourceStringData dateCaption = Res.GetData("AFA55808-6C93-4BB8-9F8C-32A86C5BAEA7", englishCaption: "Physical In/Out Date", englishShortCaption: "In/Out Date", englishFullDescription: "Entered Physical In/Out Date");

	ZString GetUpdateTransactionDateErrorMessage(ZString internalReferenceType, ZString internalReferenceNumber) => ResString.GetMultilingualString("6FB27AF3-AFE0-40C0-9E19-6A64EA442BA0",
														"{0}/{1}: Physical In/Out Date cannot be older than Transaction Date",
														internalReferenceType, internalReferenceNumber);

	#endregion

	ZString SelectRowFirstMessage => ResString.GetMultilingualString("20FBE131-6C93-442E-B0BF-CE75F3276683", "Please select a row first");

	protected void NewTransactionButton_Click(object sender, EventArgs e)
	{
		if (CurrentDataItem != null)
		{
			var regLine = (CusTempStorageRegLine)CurrentDataItem;
			var newEditableTransaction = new CusTempStorageRegLineTransactionFormEditable(regLine);
			var transaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			bool continueWithSave;
			do
			{
				ShowNewTransactionForm(regLine, transaction, newEditableTransaction);
				continueWithSave = ParentForm.FireSaveButton() == ContinueWithSave.Yes;
			} while (!continueWithSave);
			NewTransactionButton.Visible = !regLine.IsClosed && regLine.RegHeader.Premises.SRP_IsActive;
		}
	}

	protected virtual void ShowNewTransactionForm(CusTempStorageRegLine regLine, CusTempStorageRegLineTransaction transaction, CusTempStorageRegLineTransactionFormEditable newEditableTransaction)
		=> _ = ZFormModaliser.ShowDialogAndDispose(new TempStorageRegTransactionNewForm(regLine, transaction, newEditableTransaction), ParentForm);
}
