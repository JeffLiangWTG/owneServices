using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChangeTransactionDatesBusinessObjectBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string InvoiceDate = "InvoiceDate";
			public const string PostDate = "PostDate";
		}

		public ChangeTransactionDatesBusinessObjectBase(BusinessObjectFactory factory) : base(factory.GetCachedReadOnlyFactory())
		{
		}

		#region InvoiceDate

		public ZDateTime InvoiceDate
		{
			get { return InvoiceDate_innerValue; }
			set
			{
				if (SetNonPersistentPropertyValue(InvoiceDateInfo, ref InvoiceDate_innerValue, value))
				{
					ValidateInvoiceDate();
					if (!InvoiceDateInfo.HasErrors() && AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate)
					{
						PostDate = InvoiceDate;
					}
				}
			}
		}

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDate); }
		}

		public void ValidateInvoiceDate()
		{
			InvoiceDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(InvoiceDateInfo);
			MandatoryValidation.CheckEntered(InvoiceDateInfo);
			TypeValidation.CheckValidZDateTimeRange(InvoiceDateInfo);
			if (InvoiceDate.Date > ZDateTime.Now.Date && !CountrySpecificValidationHelper.AddWarningIfDateIsInTheFuture(InvoiceDateInfo))
			{
				InvoiceDateInfo.AddWarning(Res.GetString("4A73D020-0CCF-4ca1-AEA9-B20DB103CCBA", "Please note: This date is in the future. Issuing a transaction with an Invoice Date set in the future may cause confusion for your Debtor."));
			}

			if (InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(GlbCompany.CurrentCompany.PK.ToGuid(), InvoiceDateInfo, typeof(ChangeTransactionDatesBusinessObjectBase)))
			{
				InvoiceDateInfo.AddError(AccountingConstants.InvoiceDateIsInTheFutureErrorMessage);
			}

			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IInvoiceDateValidation>;
			var error = provider?.Get()?.ValidateInvoiceDate(InvoiceDate);
			if (error != null)
			{
				InvoiceDateInfo.AddError(error);
			}
		}

		ZDateTime InvoiceDate_innerValue;

		#endregion

		#region PostDate

		public ZDateTime PostDate
		{
			get { return PostDate_innerValue; }
			set
			{
				if (SetNonPersistentPropertyValue(PostDateInfo, ref PostDate_innerValue, value))
				{
					ValidatePostDate();
				}
			}
		}

		public ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(Schema.PostDate); }
		}

		ZDateTime PostDate_innerValue;

		void ValidatePostDate()
		{
			PostDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(PostDateInfo);
			TypeValidation.CheckValidZDateTimeRange(PostDateInfo);
			MandatoryValidation.CheckEntered(PostDateInfo);
			PeriodValidation.CheckDateFallsIntoValidPeriod(PostDateInfo);

			if (PostDate.Date > ZDateTime.Now.Date)
			{
				PostDateInfo.AddError(Res.GetString("8E4C5B32-63FE-4899-8F1E-255A0BCF4DFD", "The Post Date cannot be a future date. Please nominate another date."));
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateInvoiceDate();
			ValidatePostDate();
		}

		#region Implementation

		PeriodValidationProvider PeriodValidation
		{
			get { return PeriodValidation_innerValue ?? (PeriodValidation_innerValue = new PeriodValidationProvider(Factory)); }
		}
		PeriodValidationProvider PeriodValidation_innerValue;

		#endregion
	}
}
