using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public const string DefaultVatCode = UniversalReferenceConstants.TaxOrFeeType.Codes.Standard;

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override ICollection CountryOfOriginsCore() => GetNewCountriesList();

		protected override ICollection GetNewCountriesList() => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, ZDateTime.Today);

		public override RefCusProcedureCollection CPCList
		{
			get
			{
				var baseCollection = base.CPCList;
				if (Parent.IsImport)
				{
					baseCollection.AdditionalFilter.AddToFilter(new ZQuery(RefCusProcedureSchema.ZZ6_Concession, SQLComparisonOperator.NotEqual, ZString.Empty));
				}
				return baseCollection;
			}
		}
	}
}
