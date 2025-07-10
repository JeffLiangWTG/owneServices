using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba
{
	public class BackDatePostHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BackDatePostHolder()
			: base(new BusinessObjectFactory())
		{
			backPostDate = ZDate.Today;
			backInvoiceDate = ZDate.Today;
		}

		#region BackPostDate
		public ZDateTime BackPostDate
		{
			get { return backPostDate; }
			set
			{
				SetNonPersistentPropertyValue(BackPostDateInfo, ref backPostDate, value);
				ValidateBackPostDate();
			}
		}
		ZDateTime backPostDate;

		public ZPropertyInfo BackPostDateInfo
		{
			get { return GetZPropertyInfo(nameof(BackPostDate)); }
		}

		public void ValidateBackPostDate()
		{
			BackPostDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BackPostDateInfo);
			TypeValidation.CheckValidSmallDateTime(BackPostDateInfo);
			if (!BackPostDateInfo.HasErrors())
			{
				if (BackPostDate.Date > ZDate.Today)
				{
					BackPostDateInfo.AddError(Res.GetString("1df123d7-8ea4-4d06-8679-75c030042b94", "Post date must be equal or less than today."));
				}
			}
			if (!BackPostDateInfo.HasErrors())
			{
				var periodValidation = new PeriodValidationProvider(Factory);
				periodValidation.CheckDateFallsIntoValidPeriod(BackPostDateInfo);
			}
		}

		#endregion

		#region BackInvoiceDate
		public ZDateTime BackInvoiceDate
		{
			get { return backInvoiceDate; }
			set
			{
				SetNonPersistentPropertyValue(BackInvoiceDateInfo, ref backInvoiceDate, value);
				ValidateBackInvoiceDate();
			}
		}
		ZDateTime backInvoiceDate;

		public ZPropertyInfo BackInvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(BackInvoiceDate)); }
		}

		public void ValidateBackInvoiceDate()
		{
			BackInvoiceDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BackInvoiceDateInfo);
			TypeValidation.CheckValidSmallDateTime(BackInvoiceDateInfo);
			if (!BackInvoiceDateInfo.HasErrors())
			{
				if (BackInvoiceDate.Date > ZDate.Today)
				{
					BackInvoiceDateInfo.AddError(Res.GetString("8d3b597c-7888-458b-8268-d7908185e2e6", "Invoice date must be equal or less than today."));
				}
			}
			if (!BackInvoiceDateInfo.HasErrors())
			{
				var periodValidation = new PeriodValidationProvider(Factory);
				periodValidation.CheckDateFallsIntoValidPeriod(BackInvoiceDateInfo);
			}
		}

		#endregion
	}
}

