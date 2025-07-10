#if DEBUG

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class PeriodicInvoiceMiscInvoicesFilterBusinessObject
	{
		public CodeDescriptionPairList JobTypeList_ForTestOnly => JobTypeList;

		public CodeDescriptionPairList InvoiceTypeList_ForTestOnly => InvoiceTypeList;

		public CodeDescriptionPairList ShipmentType_List_ForTestOnly => ShipmentType_List;
	}
}

#endif
