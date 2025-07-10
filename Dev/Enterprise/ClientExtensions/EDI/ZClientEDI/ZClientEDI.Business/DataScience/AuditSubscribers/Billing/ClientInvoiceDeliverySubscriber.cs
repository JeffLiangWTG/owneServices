using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class ClientInvoiceDeliverySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.ClientInvoiceDeliverySubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => ClientInvoiceDeliverySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			ClientInvoiceDeliverySchema.PK,
			ClientInvoiceDeliverySchema.L9_LC,
			ClientInvoiceDeliverySchema.L9_ServerCode,
			ClientInvoiceDeliverySchema.L9_SystemCode,
			ClientInvoiceDeliverySchema.L9_OH_InvoiceTo,
			ClientInvoiceDeliverySchema.L9_IsBilled,
			ClientInvoiceDeliverySchema.L9_Note,
			ClientInvoiceDeliverySchema.L9_GroupBy,
			ClientInvoiceDeliverySchema.L9_GB_InvoicingBranch,
			ClientInvoiceDeliverySchema.L9_AT_TaxId,
			ClientInvoiceDeliverySchema.L9_RX_NKInvoiceCurrency,
			ClientInvoiceDeliverySchema.L9_AC_SalesTaxChargeCode,
			ClientInvoiceDeliverySchema.L9_UseParentPrices,
		};
	}
}
