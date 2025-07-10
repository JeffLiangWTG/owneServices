using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class HouseWrapper : IHouse
	{
		internal HouseWrapper(AsycudaBill bill, CusTransactionNumber transactionNumber)
		{
			this.bill = Argument.NotNull(bill, "AsycudaBill cannot be null");
			this.documentID = transactionNumber;
		}
		readonly AsycudaBill bill;
		readonly CusTransactionNumber documentID;

		double IHouse.DocumentID => COWrappersHelper.GetDocumentID(documentID);

		string IHouse.LoadDisposition => bill.CargoDisposition;

		string IHouse.StateCode => COWrappersHelper.GetCustomsStateCode(bill.FinalDestination, bill.Factory, ZDateTime.Today);

		string IHouse.CityCode => COWrappersHelper.GetCustomsCityCode(bill.FinalDestination, bill.Factory, ZDateTime.Today);

		string IHouse.Deposit => bill.GoodsLocation?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Colombia)?.OK_CustomsRegNo ?? ZString.Empty;

		string IHouse.VoyageDocumentType => bill.TravelDocumentType;

		string IHouse.TransportDocumentNumber => bill.ABL_BillNumber;

		DateTime IHouse.TransportDocumentDate => !bill.ABL_BillIssueDate.IsEmpty ? bill.ABL_BillIssueDate.ToDateTime() : DateTime.MinValue;

		IParty IHouse.Shipper => shipper ?? (shipper = new PartyWrapper(bill, COWrappersConstants.Parties.HouseShipperCode));
		IParty shipper;

		IParty IHouse.Consignee => consignee ?? (consignee = new PartyWrapper(bill, COWrappersConstants.Parties.HouseConsigneeCode));
		IParty consignee;

		IOperationCharacteristics IHouse.OpCharacteristics => opCharacteristics ?? (opCharacteristics = new OpCharacteristicsWrapper(bill, true));
		IOperationCharacteristics opCharacteristics;

		IItem IHouse.Item => item ?? (item = new BillItemWrapper(bill));
		IItem item;
	}
}
