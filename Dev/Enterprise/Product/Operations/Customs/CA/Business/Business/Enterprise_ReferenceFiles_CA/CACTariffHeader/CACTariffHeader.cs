using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	class CACTariffHeader : AutoCACTariffHeader
	{
		public CACTariffHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public static CACTariffHeader Load(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString tariffCode)
		{
			var query = new ZQuery(CACTariffHeaderSchema.ZF_TariffCode, tariffCode);
			query.AddToFilter(CACTariffHeaderSchema.ZF_AuthEffectiveDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CACTariffHeaderSchema.ZF_AuthExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CACTariffHeaderSchema.ZF_Inactive, SQLComparisonOperator.NotEqual, "Y");
			query.OrderBy = CACTariffHeaderSchema.ZF_AuthEffectiveDate.Name + " DESC";
			return factory.LoadTop1<CACTariffHeader>(query);
		}

		#endregion

		#region Rates

		[ChildEditable(true)]
		public CACRateCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new CACRateCollection(this);
					rates.Load();
					RegisterEditableChildObject(rates);
				}
				return rates;
			}
		}

		CACRateCollection rates;

		#endregion

		#region Delete

		public override void Delete()
		{
			Rates.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion
	}
}
