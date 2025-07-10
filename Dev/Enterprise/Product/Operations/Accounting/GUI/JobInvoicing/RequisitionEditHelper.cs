using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class RequisitionEditHelper
	{
		public void HandleEditRequisition(ZGrid grid, BusinessObjectFactory factory1, SecurityCheckpoint securityCheckpoint)
		{
			if (securityCheckpoint.IsAllowed)
			{
				string errorMessage = Res.GetString("0511acce-d24a-4c51-8afa-5b48a6ee693c", "You must select at least one AP Invoice.");

				if (grid.SelectedElements.Length > 0)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					APTransactionHeaderCollection collection = new APTransactionHeaderCollection(factory);

					foreach (BusinessObject bo in grid.SelectedElements)
					{
						if (bo is APInvoice)
						{
							collection.AddFromDatabase(bo.PK);
						}
					}

					if (collection.Count >= 0)
					{
						APTransactionHeaderCollectionHolder holder = new APTransactionHeaderCollectionHolder(factory1, collection);
						ZFormModaliser.ShowDialogAndDispose(new APInvoiceRequisitionForm(holder));
					}
					else
					{
						Globals.Message.ShowError(errorMessage);
					}
				}
				else
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
			else
			{
				Globals.Message.ShowError(securityCheckpoint.ErrorMessageForNotAllowed);
			}
		}
	}
}
