using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract class CashAdvanceModule : ZFilterGridModule
	{
		public CashAdvanceModule()
		{
			DoNotShowRecentItems = true;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CashAdvanceFilterControl(GridCollection, (CashAdvanceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccCashAdvanceRequestHeaderCollection(Factory);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("0EF98235-BE41-408D-9651-A744671042DA", "Cancel"), new EventHandler(HandleCancel)));

			if (IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed)
			{
				menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("204CBC4A-F636-4C45-925B-4C23ED9EEE5F", "Mark as Paid"), new EventHandler(HandleMarkAsPaid)));
				menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("36894005-796A-4292-A084-B2E8E53CC9F5", "Mark as Unpaid"), new EventHandler(HandleMarkAsUnPaid)));
			}
			return menuItems.ToArray();
		}

		protected void HandlePrint(object sender, EventArgs e)
		{
			var printingFactory = new BusinessObjectFactory();
			var requestCollection = new AccCashAdvanceRequestHeaderCollection(printingFactory);

			var errorMessages = new ZStringBuilder(Res.GetString("1A05F6E8-2810-4B11-91F5-661F038190AB", "The following Advance Payment request(s) cannot be printed"));
			errorMessages.AppendLine();
			bool hasError = false;

			BusinessObject[] requestsSelected;
			if (Grid.SelectedElements.Length == 0 && CurrentBusinessObjectInGrid != null)
			{
				requestsSelected = new BusinessObject[] { (AccCashAdvanceRequestHeader)CurrentBusinessObjectInGrid };
			}
			else
			{
				requestsSelected = Grid.SelectedElements;
			}
			foreach (AccCashAdvanceRequestHeader header in requestsSelected)
			{
				if (!header.Organization.OH_IsActive)
				{
					hasError = true;
					errorMessages.AppendLine(Res.GetString("F102891C-BD58-46CF-BA75-FEAEBE3EAF89", "Advance Payment request {0} cannot be printed because it is for an inactive organization", header.CAH_RequestReferenceNumber));
				}
				else if (header.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Cancelled)
				{
					hasError = true;
					errorMessages.AppendLine(Res.GetString("00456BCA-9011-4C70-8FB0-56312DF6654C", "Advance Payment request {0} cannot be printed because it is canceled", header.CAH_RequestReferenceNumber));
				}
				else
				{
					requestCollection.Add(header);
				}
			}
			if (hasError)
			{
				Globals.Message.ShowInformation(errorMessages.ToString(), Res.GetString("D68E3BFC-348B-4F68-8AFA-6EBC99F61F39", "Print Advance Payment"));
			}
			if (requestCollection.Count > 0)
			{
				Print(requestCollection);
			}
		}

		void Print(AccCashAdvanceRequestHeaderCollection requestsToPrint)
		{
			try
			{
				var requests = requestsToPrint.OfType<AccCashAdvanceRequestHeader>().ToList();
				new CashAdvancePrintTask(requests).Run();
			}
			catch (UnableToFindInvoiceDocumentCommandException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void HandleCancel(object sender, EventArgs e)
		{
			CashAdvanceRequestHelper.MarkAsCancelRequest(MarkAsCancelSecurityCheckpoint, SelectedBusinessObjects);
		}

		void HandleMarkAsPaid(object sender, EventArgs e)
		{
			CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(MarkAsPaidSecurityCheckpoint, SelectedBusinessObjects, (ca) => ca.MarkAsPaid(), Res.GetString("A74B0442-118E-462C-A5CA-D66F0674EEF4", "Paid"));
		}

		void HandleMarkAsUnPaid(object sender, EventArgs e)
		{
			CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(MarkAsUnpaidSecurityCheckpoint, SelectedBusinessObjects, (ca) => ca.UndoPaidStatus(), Res.GetString("58DA497C-10AD-40CC-8F62-335B3508E160", "Unpaid"));
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected abstract SecurityCheckpoint MarkAsCancelSecurityCheckpoint { get; }
		protected abstract SecurityCheckpoint MarkAsPaidSecurityCheckpoint { get; }
		protected abstract SecurityCheckpoint MarkAsUnpaidSecurityCheckpoint { get; }
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowView => false;
		public override bool AllowDelete => false;
		protected abstract bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed { get; }
	}
}
