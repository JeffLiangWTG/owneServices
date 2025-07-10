using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.AsycudaUniversalReference;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
{
	public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
		: base(parent)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	public CodeDescriptionPairList InvoiceTypeList
	{
		get
		{
			if (Parent.JobDeclaration == null)
			{
				return new CodeDescriptionPairList();
			}
			return RefCusCodeListTypes.GetCachedList(Factory, Parent.JobDeclaration.GetDefaultDataGroupingCode(), AEConstants.RefCusCodeList.CodeTypes.InvoiceType);
		}
	}

	public override ICodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<ValuationCodeList>();

	protected override CodeDescriptionPairList PaymentMethodListCore
		=> Factory.GetCachedValue("Enterprise.Customs.AE.Business.JobComInvoiceHeaderLookups|PaymentMethodList",
			() =>
			{
				var list = new PaymentMethodList();
				list.SortNumerically();
				return list;
			});
}
