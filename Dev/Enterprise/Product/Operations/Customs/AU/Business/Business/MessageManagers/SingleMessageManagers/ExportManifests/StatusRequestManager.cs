using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class StatusRequestManager : CMRMessageManager
	{
		public StatusRequestManager(ExportCustomsManifestLines exportLine)
		{
			if (exportLine == null)
			{
				throw new ArgumentNullException(nameof(exportLine));
			}

			this.exportLine = exportLine;
		}

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => false;

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => throw new NotImplementedException();

		internal override string GetStatus() => CMRBaseStatuses.Codes.NotSent;

		public override BusinessObject BusinessObject => exportLine;

		public override string MessageFriendlyName
		{
			get
			{
				string requestText = "CAN: " + exportLine.EL_CAN + " ";
				if (!exportLine.EL_LineNo.IsEmpty)
				{
					requestText += "Line: " + exportLine.EL_LineNo + " ";
				}

				if (!exportLine.EL_AirWayBill.IsEmpty)
				{
					requestText += "Master: " + exportLine.EL_AirWayBill + " ";
				}

				if (!exportLine.EL_UserReferenceNum.IsEmpty)
				{
					requestText += "Ref: " + exportLine.EL_UserReferenceNum + " ";
				}

				return "Status Request for: " + requestText.Trim();
			}
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => throw new NotImplementedException("Amendment Manager not required for Status Request");

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new STREQMessageBuilder(exportLine) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => exportLine.Messages;

		protected readonly ExportCustomsManifestLines exportLine;
	}
}
