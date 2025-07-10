using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderSEACRManager : CMRMessageManager
	{
		public CusSeaManOBLHeaderSEACRManager(CusSeaManOBLHeader oceanBill)
		{
			this.oceanBill = oceanBill;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { oceanBill.Calculator, oceanBill.ShipmentCalculator };

		internal override string GetStatus() => oceanBill.CargoReportStatus.Code;

		public override BusinessObject BusinessObject => oceanBill;

		public override string MessageFriendlyName => "Sea Cargo Report for: " + oceanBill.BO_OceanBill;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new SEACRMessageBuilder(bizo as CusSeaManOBLHeader) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusSeaManOBLHeader).Messages;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusSeaManOBLHeaderSEACRAmendmentGenerator(bizo as CusSeaManOBLHeader);

		protected override Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			Customs.Business.MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();
			if (oceanBill.Details.Count < 1)
			{
				result.AddError("Ocean bill must have container or bulk/breakbulk details entered.");
			}
			return result;
		}

		protected override bool RequiresAmendmentCore()
		{
			var result = false;

			if (oceanBill.HasChanges || ((IBusinessObjectState)oceanBill.TransportHeader).HasChangesNotIncludingChildren)
			{
				result = base.RequiresAmendmentCore();
			}

			return result;
		}

		readonly CusSeaManOBLHeader oceanBill;
	}
}
