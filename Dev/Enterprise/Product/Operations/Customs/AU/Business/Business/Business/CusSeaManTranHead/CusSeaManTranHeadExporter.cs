using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadExporter : CMRDataExporterCSV
	{
		public CusSeaManTranHeadExporter(CusSeaManTranHead seaMan, ZString messageType)
			: base(seaMan)
		{
			this.messageType = messageType;
		}

		public CusSeaManTranHead SeaMan
		{
			get { return (CusSeaManTranHead)BizObj; }
		}

		readonly ZString messageType;

		public override string PartFileName
		{
			get
			{
				switch (messageType)
				{
					case "IA":
						return "IARSEA";
					case "AA":
						return "AARSEA";
					default:
						return "Cargo";
				}
			}
		}

		public override string MailSubject
		{
			get
			{
				switch (messageType)
				{
					case "IA":
						return "Contingency Impending Arrival Report - Sea";
					case "AA":
						return "Contingency Actual Arrival Report - Sea";
					default:
						return "Contingency Cargo Report";
				}
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return SeaMan; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return SeaMan; }
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					switch (messageType)
					{
						case "IA":
						case "AA":
							fAdditionalData.IsOriginPremiseReadOnly = true;
							break;
						default:
							if (SeaMan.Arrivals.Count > 0)
							{
								fAdditionalData.OriginPremise = SeaMan.Arrivals[0].CTOEstablishmentID;
							}
							break;
					}
				}
				return fAdditionalData;
			}
		}
		AdditionalContingencyData fAdditionalData;

		protected override StringCollectionX[] Values
		{
			get
			{
				ArrayList list = new ArrayList();

				switch (messageType)
				{
					case "IA":
						ValuesForImpendingArrival(list);
						break;
					case "AA":
						ValuesForActualArrival(list);
						break;
					default:
						ValuesForCargoReport(list);
						break;
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		void ValuesForCargoReport(ArrayList list)
		{
			foreach (CusSeaManOBLHeader oBL in SeaMan.OceanBills)
			{
				foreach (CusSeaManOBLDetail detail in oBL.Details)
				{
					StringCollectionX result = new StringCollectionX();

					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
					result.Add(SeaMan.BT_LloydsIMO);
					result.Add(SeaMan.BT_VoyageNum);
					result.Add(oBL.BO_OceanBill);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(GetContainerDetails(detail));
					result.Add(StripLineBreakInString(detail.BD_GoodsDescription));
					result.Add(oBL.BO_ConsignorName);
					result.Add(oBL.BO_ConsigneeName);
					var consigneeAddress = oBL.Consignee != null ? oBL.Consignee.MainAddress : null;
					result.Add(consigneeAddress != null ? consigneeAddress.AddressAsASingleLine : ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(oBL.BO_RL_NKLoadPort);
					result.Add(oBL.BO_RL_NKDischargePort);
					result.Add(oBL.BO_RL_NKDestinationPort);
					result.Add(ZString.Empty);
					result.Add(detail.BD_HazardousIndicator ? "Y" : "N");
					result.Add(ZString.Empty);
					result.Add("N");
					result.Add(AdditionalData.OriginPremise);
					result.Add(detail.BD_NoOfPacks.ToString());
					result.Add(oBL.BO_SendersMessageReference);

					list.Add(result);
				}
			}
		}

		void ValuesForImpendingArrival(ArrayList list)
		{
			foreach (CusSeaManArrivalPort arrivalPort in SeaMan.Arrivals)
			{
				StringCollectionX result = new StringCollectionX();
				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				result.Add(SeaMan.BT_LloydsIMO);
				result.Add(SeaMan.BT_VoyageNum);
				result.Add(SeaMan.BT_RL_NKPortOfLastForeignPort);
				result.Add(CMRDataExporterCSV.CMRDateString(SeaMan.DateTimeOfDepartureUTC));
				result.Add(CMRDataExporterCSV.CMRTimeString(SeaMan.DateTimeOfDepartureUTC));
				result.Add(SeaMan.FirstPortOfArrival);
				result.Add(arrivalPort.BA_RL_NKArrivalPort);
				result.Add(CMRDataExporterCSV.CMRDateString(arrivalPort.EstimatedDateTimeOfArrivalUTC));
				result.Add(CMRDataExporterCSV.CMRTimeString(arrivalPort.EstimatedDateTimeOfArrivalUTC));
				result.Add(arrivalPort.CTOEstablishmentID);
				result.Add(arrivalPort.BA_DischargeIndicator ? "YES" : "");
				result.Add(arrivalPort.BA_SendersMessageReference);
				list.Add(result);
			}
		}

		void ValuesForActualArrival(ArrayList list)
		{
			foreach (CusSeaManArrivalPort arrivalPort in SeaMan.Arrivals)
			{
				if (arrivalPort.BA_ArrivalPortATA.IsValid && arrivalPort.ActualArrivalResponseStatus.Code == CMRBaseStatuses.Codes.NotSent)
				{
					StringCollectionX result = new StringCollectionX();
					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
					result.Add(SeaMan.BT_LloydsIMO);
					result.Add(SeaMan.BT_VoyageNum);
					result.Add(arrivalPort.BA_RL_NKArrivalPort);
					result.Add(CMRDataExporterCSV.CMRDateString(arrivalPort.ActualDateTimeOfArrivalUTC));
					result.Add(CMRDataExporterCSV.CMRTimeString(arrivalPort.ActualDateTimeOfArrivalUTC));
					result.Add(arrivalPort.BA_StevadoreID);
					result.Add(arrivalPort.CTOEstablishmentID);
					result.Add(arrivalPort.BA_BerthCode);
					result.Add(arrivalPort.BA_SendersMessageReference);
					list.Add(result);
				}
			}
		}

		string GetContainerDetails(CusSeaManOBLDetail detail)
		{
			string result = "";

			if (detail.IsBreakBulk || detail.IsBulk)
			{
				if (detail.IsBreakBulk)
				{
					result += "B/B ";
				}
				else
				{
					result += "BULK ";
				}
				result += detail.BD_GrossWeight.ToString() + " ";
				result += detail.BD_GrossWeightUM + " ";
				result += detail.BD_CargoVolume + " ";
				result += detail.BD_CargoVolumeUM;
			}
			else
			{
				result = detail.BD_ContainerNumber;
			}

			return result;
		}
	}
}
