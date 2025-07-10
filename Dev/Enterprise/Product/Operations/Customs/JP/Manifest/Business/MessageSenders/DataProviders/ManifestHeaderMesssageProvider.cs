using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	sealed class ManifestHeaderMesssageProvider : IHCH01, IHDF01, INVC01, IHDE, ICHA
	{
		public ManifestHeaderMesssageProvider(IEnumerable<ManifestMessageSendingObject> sendingObjects)
		{
			this.messageSendingObjects = Argument.NotNull(sendingObjects, nameof(sendingObjects));
			header = Argument.NotNull(sendingObjects.FirstOrDefault().Bill.Header, nameof(header));
		}

		readonly IEnumerable<ManifestMessageSendingObject> messageSendingObjects;
		readonly AsycudaManifestHeader header;

		public string ConsolidatorCode
		{
			get
			{
				var mailBoxID = header.CustomsAgentCredential?.GP_MailBoxID;
				var consolidatorCode = header.Consolidator?.CustomsCodes.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.NUC, Core.Constants.CountryCodes.Japan);

				if (!ZString.Equals(mailBoxID, consolidatorCode))
				{
					return consolidatorCode;
				}

				return null;
			}
		}
		public string CustomsOffice => header.AMA_CustomsOffice;

		public string MAWB => header.AMA_MasterBill;

		public string IsSubConsolidation => header.IsSubConsolidation ? YesNoList.Codes.Yes : string.Empty;

		public string ArrivalFlightNumber => header.AMA_Voyage;

		public string ArrivalFlightDate => header.MasterBill.ABL_E_DEP.ToString("ddMMM").ToUpper();

		public string PortOfDischarge => header.PortOfDischargeIATACode;

		public string PortOfLoading => header.PortOfLoadingIATACode;

		public string IsCoLoaded => header.IsCoLoaded ? YesNoList.Codes.Yes : string.Empty;

		public IEnumerable<IHCH01Bills> Bills => messageSendingObjects.Select(TryGetBillProvider);

		IEnumerable<IHDF01Bills> IHDF01.Bills => messageSendingObjects.Select(TryGetBillProvider);

		public string ActionTypeCode => messageSendingObjects.FirstOrDefault().Action;

		public string MasterBillNumber => header.AMA_MasterBill;

		public string BondedLocationCode => header.MasterBill?.ABL_GoodsLocation;

		public IEnumerable<INVC01Bills> HouseBills => messageSendingObjects.Select(TryGetBillProvider);

		IEnumerable<string> IHDE.MAWB => [header.AMA_MasterBill];

		IEnumerable<ICHA.ICHABills> ICHA.Bills => messageSendingObjects.Select(TryGetBillProvider);

		BillProvider TryGetBillProvider(ManifestMessageSendingObject messageSendingObject) => messageSendingObject.Bill != null && (messageSendingObject.Bill.IsInDatabase || messageSendingObject.Bill.ABL_BillNumber == AsycudaBill.HCH01EndHAWB) ? new BillProvider(messageSendingObject) : null;
	}
}
