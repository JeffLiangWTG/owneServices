using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BulkClientFaxPriceUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BulkClientFaxPriceUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MonthAndYearPeriod = new ZDateTime((ZShort)ZDateTime.Now.Year, (ZByte)ZDateTime.Now.Month, 1);
		}

		public ClientFaxPriceCollection ClientFaxPriceList
		{
			get
			{
				if (clientFaxPriceList == null)
				{
					clientFaxPriceList = new ClientFaxPriceCollection(Factory, this);
					RegisterEditableChildObject(clientFaxPriceList);
				}
				return clientFaxPriceList;
			}
		}
		ClientFaxPriceCollection clientFaxPriceList;

		public bool HasChangesInChildren
		{
			get { return ClientFaxPriceList.HasChanges; }
		}

		[BusinessObjectTestExclude] // Since allowing invalid dates will cause an exception when validation is suspended
		public ZDateTime MonthAndYearPeriod
		{
			get { return monthAndYearPeriod; }
			set
			{
				monthAndYearPeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateMonthAndYearPeriod();
				}

				if (!MonthAndYearPeriodInfo.HasErrors())
				{
					UpdateClientFaxPriceListToSelectedMonthAndYearPeriod();
				}

				MonthAndYearPeriodInfo.RefreshBinding();
			}
		}
		ZDateTime monthAndYearPeriod;

		public ZPropertyInfo MonthAndYearPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(MonthAndYearPeriod)); }
		}

		public void ValidateMonthAndYearPeriod()
		{
			MonthAndYearPeriodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MonthAndYearPeriodInfo);

			if (!MonthAndYearPeriodInfo.HasErrors() && !monthAndYearPeriod.IsValid)
			{
				MonthAndYearPeriodInfo.AddError(Res.GetString("80ac3197-c376-4b29-8273-ce5113607882", "Please enter a valid date."));
			}

			if (!MonthAndYearPeriodInfo.HasErrors() && monthAndYearPeriod.Day != 1)
			{
				MonthAndYearPeriodInfo.AddError(Res.GetString("a5eb37b0-80e3-4957-93fe-b1ab8ec164d3", "Date must be first day of the month."));
			}
		}

		void UpdateClientFaxPriceListToSelectedMonthAndYearPeriod()
		{
			ZQuery query = new ZQuery(ClientFaxPriceSchema.CFP_Month, (ZByte)MonthAndYearPeriod.Month);
			query.AddToFilter(ClientFaxPriceSchema.CFP_Year, (ZShort)MonthAndYearPeriod.Year);
			ClientFaxPriceList.AdditionalFilter = query;
		}
	}
}
