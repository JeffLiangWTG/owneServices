using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoContainersExporter : CMRDataExporterCSV
	{
		public SeaCargoContainersExporter(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
		}

		public CusSCAOceanBill OceanBill
		{
			get { return (CusSCAOceanBill)BizObj; }
		}

		public override string PartFileName
		{
			get
			{
				return "ExportOceanBill";
			}
		}

		public override string MailSubject
		{
			get
			{
				return "Sea Cargo Containers Report";
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return OceanBill; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return OceanBill; }
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					fAdditionalData.IsOriginPremiseReadOnly = true;
				}
				return fAdditionalData;
			}
		}
		AdditionalContingencyData fAdditionalData;

		string ConvertBoolToString(bool value)
		{
			return value ? Yes : No;
		}
		const string Yes = "Y";
		const string No = "N";

		public Stream GetStream(ZString fileName)
		{
			IFileMapper fileMapper = ObjectFactory.Get<IFileMapper>();
			string fullPath;
			if (string.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
			{
				fullPath = Path.Combine(fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), fileName);
			}
			else
			{
				fullPath = fileName;
			}
			return fileMapper.OpenWrite(fullPath);
		}

		protected override StringCollectionX[] Values
		{
			get
			{
				List<StringCollectionX> list = new List<StringCollectionX>();
				StringCollectionX header = new StringCollectionX();
				header.Add("Container Number"); //1
				header.Add("Seal Number"); //2
				header.Add("Mode"); //3
				header.Add("Type"); //4
				header.Add("Shipper Owned"); //5
				header.Add("Container Type"); //6
				header.Add("Container Size"); //7
				header.Add("Container Status"); //8
				header.Add("Packages"); //9
				header.Add("Package Type"); //10
				header.Add("Net Weight"); //11
				header.Add("Gross Weight"); //12
				header.Add("Gross Weight Unit"); //13
				header.Add("Volume"); //14
				header.Add("Hazardous Goods"); //15
				header.Add("Fumigation Certificate"); //16
				header.Add("Flammable"); //17
				header.Add("Personal Effects"); //18
				header.Add("Perishable Goods"); //19
				header.Add("Timber"); //20
				header.Add("Goods Description"); //21
				header.Add("Is SAC"); //22
				header.Add("Marks And Numbers"); //23
				header.Add("House Bill"); //24
				header.Add("Origin"); //25
				header.Add("Destination"); //26
				header.Add("Goods Origin"); //27
				header.Add("Payment Type"); //28
				header.Add("Shipment Status Code"); //29
				header.Add("Shipment Status Desc."); //30
				header.Add("Message Status Code"); //31
				header.Add("Message Status Desc."); //32
				header.Add("FF Ind"); //33
				header.Add("Shipment ID"); //34
				header.Add("Parent Bill"); //35
				header.Add("Responsible Party ID"); //36
				header.Add("Consignee Code"); //37
				header.Add("Consignee Name"); //38
				header.Add("Consignee Address 1"); //39
				header.Add("Consignee Address 2"); //40
				header.Add("Consignee Postcode"); //41
				header.Add("Consignee Suburb"); //42
				header.Add("Consignee Country"); //43
				header.Add("Consignor Code"); //44
				header.Add("Consignor Name"); //45
				header.Add("Consignor Address 1"); //46
				header.Add("Consignor Address 2"); //47
				header.Add("Consignor Postcode"); //48
				header.Add("Consignor Suburb"); //49
				header.Add("Consignor Country"); //50
				header.Add("Notify Code"); //51
				header.Add("Notify Name"); //52
				header.Add("Notify Address 1"); //53
				header.Add("Notify Address 2"); //54
				header.Add("Notify Postcode"); //55
				header.Add("Notify Suburb"); //56
				header.Add("Notify Country"); //57

				var containersNeedToExport = OceanBill.Containers.ToList();
				var nullContainer = Factory.GetNull<CusSCAContainer>();
				var nullPivot = Factory.GetNull<CusSCAPivot>();
				var nullHouseBill = Factory.GetNull<CusSCAHouse>();
				var shipmentID = GetShipmentNo();

				foreach (CusSCAHouse house in OceanBill.HouseBills)
				{
					if (house.Pivot.Count > 0)
					{
						foreach (CusSCAPivot pivot in house.Pivot)
						{
							var container = pivot.Container ?? nullContainer;
							list.Add(GetALine(house, container, pivot, shipmentID));
							containersNeedToExport.Remove(container);
						}
					}
					else
					{
						list.Add(GetALine(house, nullContainer, nullPivot, shipmentID));
					}
				}

				foreach (CusSCAContainer container in containersNeedToExport)
				{
					list.Add(GetALine(nullHouseBill, container, nullPivot, shipmentID));
				}
				var sortedList = list.OrderBy(o => o[0]).ThenBy(o => o[8]).ToList();
				sortedList.Insert(0, header);
				return sortedList.ToArray<StringCollectionX>();
			}
		}

		internal ZString GetShipmentNo()
		{
			var consolQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
			consolQuery.AddToFilter(JobConsolSchema.JK_MasterBillNum, OceanBill.CB_OceanBill);

			var consolLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			consolLinkQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, consolQuery, JoinCondition.And);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddToFilter(JobShipmentSchema.JS_HouseBill, OceanBill.CB_MasterHouseBill);
			shipmentQuery.AddSubQuery(consolLinkQuery, JoinCondition.And);
			var shipment = Factory.Load<ForwardingShipment>(shipmentQuery).FirstOrDefault();
			var result = shipment != null ? shipment.JS_UniqueConsignRef : ZString.Empty;
			return result;
		}

		StringCollectionX GetALine(CusSCAHouse house, CusSCAContainer container, CusSCAPivot pivot, ZString shipmentID)
		{
			StringCollectionX result = new StringCollectionX();

			result.Add(container.CN_ContainerNumber.IsEmpty ? " " : container.CN_ContainerNumber.ToString()); //1
			result.Add(container.CN_SealNumber); //2
			result.Add(container.CN_ContainerMode); //3
			result.Add(container.CN_RC_NKContainerType); //4
			result.Add(ConvertBoolToString(container.CN_ShipperOwnedContainer)); //5
			result.Add(container.CN_TypeOfContainer); //6
			result.Add(container.CN_ContainerSizeOrISOCode); //7
			result.Add(container.CN_ContainerStatus); //8	
			result.Add(pivot.CV_PackageCount.ToString()); //9
			result.Add(pivot.CV_PackageType); //10
			result.Add(pivot.CV_NetWeight.ToString()); //11
			result.Add(pivot.CV_Weight.ToString()); //12
			result.Add(pivot.CV_WeightUQ); //13
			result.Add(pivot.CV_Volume.ToString()); //14
			result.Add(ConvertBoolToString(pivot.CV_HazardousGoods)); //15
			result.Add(ConvertBoolToString(pivot.CV_FumigationCert)); //16
			result.Add(ConvertBoolToString(pivot.CV_Flammable)); //17
			result.Add(ConvertBoolToString(pivot.CV_PersonalEffects)); //18
			result.Add(ConvertBoolToString(pivot.CV_PerishableGoods)); //19
			result.Add(ConvertBoolToString(pivot.CV_Timber)); //20
			result.Add(pivot.CV_GoodsDescription); //21
			result.Add(ConvertBoolToString(pivot.CV_IsSAC)); //22
			result.Add(pivot.CV_MarksAndNumbers); //23
			result.Add(house.CA_HouseBill); //24
			result.Add(house.CA_RL_NK_PortOfOrigin); //25
			result.Add(house.CA_RL_NK_PortOfDestination); //26
			result.Add(house.CA_RN_NKGoodsOrigin); //27
			result.Add(house.CA_PrepaidCollectOther); //28
			result.Add(house.CA_ShipmentStatus); //29
			result.Add(house.ShipmentStatus); //30
			result.Add(house.CA_MessageStatus); //31
			result.Add(house.MessageStatus); //32
			result.Add(ConvertBoolToString(house.CA_IsMasterHouse)); //33
			result.Add(house.Shipment != null ? house.Shipment.JS_UniqueConsignRef : shipmentID); //34
			result.Add(house.AggregatedMasterHouseBill); //35
			var responsiblePartyID = house.CA_ResponsiblePartyID;
			result.Add(!responsiblePartyID.IsEmpty ? responsiblePartyID : OceanBill.CB_ResponsiblePartyID); //36
			result.Add(house.Consignee?.OH_Code ?? ZString.Empty); //37					
			result.Add(house.CA_ConsigneeName); //38
			result.Add(house.CA_ConsigneeAddress1); //39
			result.Add(house.CA_ConsigneeAddress2); //40
			result.Add(house.CA_ConsigneePostcode); //41
			result.Add(house.CA_ConsigneeSuburb); //42
			result.Add(house.CA_RN_NKConsigneeCountryCode); //43
			result.Add(house.Consignor?.OH_Code ?? ZString.Empty); //44
			result.Add(house.CA_ConsignorName); //45
			result.Add(house.CA_ConsignorAddress1); //46
			result.Add(house.CA_ConsignorAddress2); //47
			result.Add(house.CA_ConsignorPostcode); //48
			result.Add(house.CA_ConsignorSuburb); //49
			result.Add(house.CA_RN_NKConsignorCountryCode); //50
			result.Add(house.Notify?.OH_Code ?? ZString.Empty); //51
			result.Add(house.CA_NotifyName); //52
			result.Add(house.CA_NotifyAddress1); //53
			result.Add(house.CA_NotifyAddress2); //54
			result.Add(house.CA_NotifyPostcode); //55
			result.Add(house.CA_NotifySuburb); //56
			result.Add(house.CA_RN_NKNotifyCountryCode); //57
			return result;
		}
	}
}
