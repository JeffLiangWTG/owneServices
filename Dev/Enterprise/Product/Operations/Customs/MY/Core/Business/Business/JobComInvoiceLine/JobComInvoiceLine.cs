using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.MY.Business
{
	public class JobComInvoiceLine : TypeSafeJobComInvoiceLine, Integration.Customs.MY.IJobComInvoiceLine
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString CustomsCountryCodeCore
		{
			get { return Core.Constants.CountryCodes.Malaysia; }
		}
		protected override System.Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		#region GetTariffDescription - to be overridden once the Tariff is setup for a new country
		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return ZString.Empty;
		}
		#endregion

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			return new JobComInvoiceLineValidation(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(this);
		}

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return true;
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return JI_CustomsUnitQty.IsEmpty;
		}

		#endregion

		#endregion
	}
}
