using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class MasterBillWrapper : IMaster
	{
		internal MasterBillWrapper(AsycudaManifestHeader header, List<CusTransactionNumber> documentIDs)
		{
			this.header = Argument.NotNull(header, "AsycudaManifestHeader cannot be null");
			this.documentIDs = documentIDs;
			houseBill = header.Bills[0];
		}
		readonly AsycudaManifestHeader header;
		readonly List<CusTransactionNumber> documentIDs;
		readonly AsycudaBill houseBill;

		double IMaster.DocumentNumber
		{
			get
			{
				double result = 0;
				var headerLevelID = documentIDs.FirstOrDefault();
				result = COWrappersHelper.GetDocumentID(headerLevelID);
				documentIDs.Remove(headerLevelID);
				return result;
			}
		}

		string IMaster.VoyageDocumentType => header.TravelDocumentType;

		string IMaster.AdministrationCode => header.AMA_CustomsOffice;

		string IMaster.LoadingArrangement => header.CargoDisposition;

		string IMaster.StateCode => COWrappersHelper.GetCustomsStateCode(header.PortOfDischarge, header.Factory, ZDateTime.Today);

		string IMaster.CityCode => COWrappersHelper.GetCustomsCityCode(header.PortOfDischarge, header.Factory, ZDateTime.Today);

		string IMaster.TransportDocumentNumber => header.AMA_MasterBill;

		DateTime IMaster.TransportDocumentDate => !header.AMA_MasterBillIssueDate.IsEmpty ? header.AMA_MasterBillIssueDate.ToDateTime() : DateTime.MinValue;

		IParty IMaster.Carrier => carrier ?? (carrier = new PartyWrapper(houseBill, COWrappersConstants.Parties.CarrierCode));
		IParty carrier;

		IParty IMaster.Shipper => shipper ?? (shipper = new PartyWrapper(houseBill, COWrappersConstants.Parties.MasterShipperCode));
		IParty shipper;

		IParty IMaster.Consignee => consignee ?? (consignee = new PartyWrapper(houseBill, COWrappersConstants.Parties.MasterConsigneeCode));
		IParty consignee;

		string IMaster.DangerousGoodsContact
		{
			get
			{
				var companyName = ZString.Empty;
				var dgContact = houseBill.Packs[0]?.UNDGs.FirstOrDefault()?.DGContact;
				var countryCode = dgContact?.Header?.MainAddress?.Country?.Code ?? ZString.Empty;
				if (!countryCode.IsEmpty && countryCode != Core.Constants.CountryCodes.Colombia)
				{
					companyName = string.Concat(dgContact.OC_ContactName, COWrappersConstants.Separator, dgContact.OC_Phone);
				}
				return companyName;
			}
		}

		IOperationCharacteristics IMaster.OpCharacteristics => opCharacteristics ?? (opCharacteristics = new OpCharacteristicsWrapper(header?.MasterBill));
		IOperationCharacteristics opCharacteristics;

		IReadOnlyCollection<IHouse> IMaster.Houses
		{
			get
			{
				var result = new List<IHouse>();
				if (header.Bills.Count == documentIDs.Count)
				{
					int index = 0;
					foreach (AsycudaBill bill in header.Bills)
					{
						result.Add(new HouseWrapper(bill, documentIDs[index]));
						index++;
					}
				}

				return result.ToArray();
			}
		}

		IReadOnlyCollection<IItem> IMaster.Items
		{
			get
			{
				var result = new List<IItem>();
				foreach (AsycudaContainer container in header.Containers)
				{
					result.Add(new ContainerItemWrapper(container));
				}
				return result.ToArray();
			}
		}
	}
}
