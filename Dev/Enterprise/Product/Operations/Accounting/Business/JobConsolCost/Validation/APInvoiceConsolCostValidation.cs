using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	class APInvoiceConsolCostValidation : CommonConsolCostValidation
	{
		public APInvoiceConsolCostValidation(JobConsolCost parent)
			: base(parent)
		{
		}

		protected override void CheckE6_ParentID()
		{
			base.CheckE6_ParentID();
			MandatoryValidation.CheckEntered(Parent.E6_ParentIDInfo);
			ListValidation.ErrorIfInvalidPK(Parent.E6_ParentIDInfo);
		}

		void ValidateIsAlreadyImportedToAnIncompleteInvoice()
		{
			var collection = ((IBusinessObjectInternals)Parent).ParentCollections.OfType<APInvoiceConsolCostCollection>().FirstOrDefault();
			var currentInvoice = collection?.ParentAPInvoice;

			var linkedIncompleteInvoice = Parent.RelatedConsolCostFromDatabase?.GetTheImportingIncompleteInvoiceIfAny();
			if (linkedIncompleteInvoice != null && currentInvoice != null)
			{
				var errorMessage = Res.GetString("4fced7c3-d269-4ef9-9c25-1ba63ac3e5c5", "The associated apportion charges are already used in an Incomplete Invoice {0} dated {1}. Please delete this row. If required, you can manually enter a new cost without importing the existing one.", linkedIncompleteInvoice.AH_TransactionNum, linkedIncompleteInvoice.AH_InvoiceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
				if (currentInvoice.IsIncompleteInvoice || currentInvoice.IsCompletingInvoice)
				{
					if (linkedIncompleteInvoice.PK != currentInvoice.PK)
					{
						Parent.AddRowError(errorMessage);
					}
				}
				else
				{
					Parent.AddRowError(errorMessage);
				}
			}
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateIsAlreadyImportedToAnIncompleteInvoice();
		}
	}
}
