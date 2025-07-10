using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoEstablishmentQueryManager : CMRMessageManager
	{
		public SeaCargoEstablishmentQueryManager(DepotCusOutturn outturn)
		{
			if (outturn == null)
			{
				throw new NullReferenceException("Cus Outturn must not be null when creating the SEQ Manager");
			}
			this.outturn = outturn;
		}

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => false;

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { outturn.StatusCalculator };

		internal override string GetStatus() => CMRBaseStatuses.Codes.NotSent;

		public override BusinessObject BusinessObject => outturn;

		public override string MessageFriendlyName
		{
			get
			{
				string requestText = "";
				if (!outturn.C5_ContainerNumber.IsEmpty)
				{
					requestText += "Cont: " + outturn.C5_ContainerNumber + " ";
				}

				if (!outturn.C5_MasterBill.IsEmpty)
				{
					requestText += "OBL: " + outturn.C5_MasterBill + " ";
				}

				if (!outturn.C5_HouseBill.IsEmpty)
				{
					requestText += "HBL: " + outturn.C5_HouseBill + " ";
				}

				return "Sea Cargo Establishment Query for: " + requestText;
			}
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => throw new NotImplementedException("Amendment Manager not required for Depot Query Request");

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo1) => new CMRMessageBuilder[] { new SEQMessageBuilder(bizo1 as DepotCusOutturn) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => outturn.Header == null ? outturn.Messages : outturn.Header.Messages;

		protected readonly DepotCusOutturn outturn;
	}
}
