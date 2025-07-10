
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEAlternateBrokerSplitNotificationAutoDelivery : UPEDocumentAutoDelivery
	{
		public UPEAlternateBrokerSplitNotificationAutoDelivery(UPECusHAWB cusHAWB)
			: base(cusHAWB)
		{
		}

		protected override DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerSplitNotification(); }
		}

		protected override ZString PrintBatchType
		{
			get { return UPEPrintBatchTypes.Codes.AlternateBrokerSplitNotification; }
		}

		protected override string DeliveryFailureEmailSubject
		{
			get { return "Delivery instructions incomplete for Alternate Broker Split Shipment Notification - " + CusHAWB.Declaration.AlternateBroker.OH_Code; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override string DeliveryFailureDocumentDetails
		{
			get { return "CargoWise One Code : " + AlternateBroker.OH_Code; }
		}

		UPECusHAWB CusHAWB
		{
			get { return (UPECusHAWB)base.DocumentSupportable; }
		}

		OrgHeader AlternateBroker
		{
			get { return CusHAWB.Declaration.AlternateBroker; }
		}
	}
}
