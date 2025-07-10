using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class PermitLookups : CusSupportingInfoLookups
	{
		public PermitLookups(Permit parent) : base(parent)
		{
		}

		JobComInvoiceLine InvoiceLine => Parent.Parent as JobComInvoiceLine;

		public CodeDescriptionPairList UQList => InvoiceLine?.Lookups.InvoiceUQList ?? new CodeDescriptionPairList();

		public CusLPCOHeaderCollection LPCOHeaders
		{
			get
			{
				var collection = new CusLPCOHeaderCollection(Factory);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusLPCOHeaderCollection.FilterConstants.StartDate, "PropertySearch", ModuleDateFilter.Past));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusLPCOHeaderCollection.FilterConstants.EndDate, "PropertySearch", ModuleDateFilter.Future));
				return collection;
			}
		}
	}
}
