using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		public override ICodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<ValuationCodeList>();

		public override ICodeDescriptionPairList RelatedIndicatorList => Factory.GetCachedValue<RelatedIndicatorList>();

		public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue<BRIncoTermList>();

		public CodeDescriptionPairList ExchangeHedgeList => Parent.ExchangeHedge.Lookups.ExchangeHedgeList;

		public CodeDescriptionPairList FinancialInstitutionList => Parent.ExchangeHedge.Lookups.FinancialInstitutionList;

		public CodeDescriptionPairList ReasonTypeList => Parent.ExchangeHedge.Lookups.ReasonTypeList;

		public CodeDescriptionPairList ExchangeHedgePaymentMethodList => Parent.ExchangeHedge.Lookups.PaymentMethodList;

		public override ConsignorCollection SupplierList
		{
			get
			{
				var isImportOnly = Parent.IsImportOnly;

				return Factory.GetCachedValue("BR|JobComInvoiceHeaderLookups|SupplierList_" + isImportOnly, () =>
				{
					var suppliers = base.SupplierList;
					if (BRCustomsDataRegistry.Instance.EnableForeignOperator.Value && isImportOnly)
					{
						suppliers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.IsForeignOperator.FilterName, "Property", (ZString)OrgConstants.FilterControl.IsForeignOperator.Code.Yes));
					}
					return suppliers;
				});
			}
		}
	}
}
