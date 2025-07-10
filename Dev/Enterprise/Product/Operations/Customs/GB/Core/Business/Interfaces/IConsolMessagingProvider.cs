using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Interfaces
{
	public interface IConsolMessagingProvider
	{
		BusinessObjectFactory Factory { get; }

		ZString CustomsProfile { get; }

		ZString MessageType { get; }

		ZString MasterBill { get; }

		ZString HouseSplitReference { get; }

		ZString HouseBill { get; }

		bool IsInventoryControlledAirImport { get; }

		ZString LocationOfGoods { get; }

		ZString SubLocation { get; }

		ZString SubLocationOfGoods { get; }

		ForwardingConsol RelevantConsol { get; }

		GlbBranch Branch { get; }

		ZBool IsSea { get; }

		OrgAddress Declarant { get; }

		ZString JobReference { get; }

		ZBool FindGen51Statement { get; }

		InvoiceLineCompleteCollection GetInvoiceLines();

		ZString CourierSiteId { get; }
		ZString CourierCarrierCode { get; }
		ZString CourierConsignmentReference { get; }
	}
}
