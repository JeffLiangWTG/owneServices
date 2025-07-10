using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class GlobalElectronicInvoiceRequestByIdBuilderForTurkey : GlobalElectronicInvoiceBuilderBaseForTurkey
	{
		public GlobalElectronicInvoiceRequestByIdBuilderForTurkey(string batchNumber, string messageType, GlbCompany company, string invoiceId) : base(batchNumber, messageType)
		{
			Argument.NotNull(company, nameof(company));
			Argument.NotNullOrEmpty(invoiceId, nameof(invoiceId));

			this.company = company;
			InvoiceId = invoiceId;
		}

		protected string InvoiceId { get; }

		protected override GlbCompany GEIMessageCompany => company;
		GlbCompany company { get; }

		protected override BusinessObjectFactory Factory => GEIMessageCompany.Factory;

		protected override ZString GetPayloadXML(INotifications notifications) => InvoiceId;
	}
}
