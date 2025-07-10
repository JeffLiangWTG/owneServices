using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRUnderbondExporter : CMRDataExporterCSV
	{
		public CMRUnderbondExporter(CusUnderbond underbond)
			: base(underbond)
		{
		}

		public CusUnderbond Underbond
		{
			get { return (CusUnderbond)BizObj; }
		}

		public override string PartFileName
		{
			get
			{
				return "UBond";
			}
		}

		public override string MailSubject
		{
			get
			{
				return "Contingency Underbond Movement";
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return Underbond; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return Underbond; }
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

		protected override StringCollectionX[] Values
		{
			get
			{
				list = new ArrayList();

				if (Underbond != null)
				{
					switch (Underbond.C4_ParentTableCode)
					{
						case CusHAWBSchema.Constants.Prefix:
							GenericHAWB();
							break;
						case CusSCAPivotSchema.Constants.Prefix:
							LinkedCusSCAPivot();
							break;
						case CusSCAContainerSchema.Constants.Prefix:
							LinkedCusSCAContainer();
							break;
						case CusMAWBSchema.Constants.Prefix:
							LinkedCusMAWB();
							break;
						case JobShipmentSchema.Constants.Prefix:
							LinkedCFSShipment();
							break;
						case JobContainerSchema.Constants.Prefix:
							LinkedCFSContainer();
							break;
						case CusSeaManOBLDetailSchema.Constants.Prefix:
							LinkedCusSeaManOBLDetail();
							break;
					}
				}
				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}
		ArrayList list;

		void GenericHAWB()
		{
			if (Underbond.HAWBLinked != null)
			{
				LinkedCusHAWB();
			}
			else if (Underbond.LinkedObject != null && Underbond.LinkedObject.GetType() == typeof(CTOCusHAWB))
			{
				LinkedCTOCusHAWB();
			}
		}

		void LinkedCusHAWB()
		{
			StringCollectionX result = new StringCollectionX();

			result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
			result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
			result.Add(ZString.Empty);
			if (Underbond.MAWB != null)
			{
				result.Add(Underbond.MAWB.CM_FlightNo);
				result.Add(Underbond.MAWB.CM_MAWB);
			}
			else
			{
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
			}
			result.Add(Underbond.HAWBLinked.CS_HAWB);
			result.Add(ZString.Empty);
			result.Add(ZString.Empty);
			result.Add(Underbond.HAWBLinked.CS_GoodsDescription);
			result.Add(Underbond.HAWBLinked.CS_ConsignorName);
			result.Add(Underbond.HAWBLinked.CS_ConsigneeName);
			result.Add(Underbond.HAWBLinked.ConsigneeAddressAsASingleLine);
			result.Add(Underbond.MAWB != null ? Underbond.MAWB.CM_RL_NKDischargePort : ZString.Empty);
			result.Add(ZString.Empty);
			result.Add(Underbond.HAWBLinked.CS_IsSelfAssessedClearance ? "YES" : "");
			result.Add(Underbond.C4_ModeOfMovement);
			result.Add(ZString.Empty);
			result.Add(Underbond.C4_OriginPremiseID);
			result.Add(Underbond.C4_DestinationPremiseID);
			result.Add(Underbond.C4_PiecesManifested.ToString());
			result.Add(Underbond.C4_SendersMessageReference);
			list.Add(result);
		}

		void LinkedCTOCusHAWB()
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)Underbond.LinkedObject;

			if (hAWB != null)
			{
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				result.Add(ZString.Empty);
				if (hAWB.MAWB != null)
				{
					result.Add(hAWB.MAWB.CM_FlightNo);
					result.Add(hAWB.CS_HAWB);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(hAWB.CS_GoodsDescription);
				result.Add(hAWB.CS_ConsignorName);
				result.Add(hAWB.CS_ConsigneeName);
				result.Add(hAWB.ConsigneeAddressAsASingleLine);
				result.Add(hAWB.MAWB != null ? hAWB.MAWB.CM_RL_NKDischargePort : ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(hAWB.CS_IsSelfAssessedClearance ? "YES" : "");
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}

		void LinkedCusSCAPivot()
		{
			if (Underbond.PivotLinked != null)
			{
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				if (Underbond.OceanBill != null)
				{
					result.Add(Underbond.OceanBill.CB_LloydsIMO);
					result.Add(Underbond.OceanBill.CB_Voyage);
					result.Add(Underbond.OceanBill.CB_OceanBill);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(Underbond.PivotLinked.HouseBill == null ? ZString.Empty : Underbond.PivotLinked.HouseBill.CA_HouseBill);
				result.Add(Underbond.PivotLinked.CN_ContainerMode);
				result.Add(Underbond.PivotLinked.CN_ContainerNumber);
				result.Add(Underbond.PivotLinked.CV_GoodsDescription);
				if (Underbond.PivotLinked.HouseBill != null)
				{
					result.Add(Underbond.PivotLinked.HouseBill.CA_ConsignorName);
					result.Add(Underbond.PivotLinked.HouseBill.CA_ConsigneeName);
					result.Add(Underbond.PivotLinked.HouseBill.ConsigneeAddressAsASingleLine);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(Underbond.OceanBill != null ? Underbond.OceanBill.CB_RL_NKPortOfDischarge : ZString.Empty);
				result.Add(Underbond.PivotLinked.CV_HazardousGoods ? "YES" : "");
				result.Add(Underbond.PivotLinked.CV_IsSAC ? "YES" : "");
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}

		void LinkedCusSCAContainer()
		{
			if (Underbond.ContainerLinked != null)
			{
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				if (Underbond.OceanBill != null)
				{
					result.Add(Underbond.OceanBill.CB_LloydsIMO);
					result.Add(Underbond.OceanBill.CB_Voyage);
					result.Add(Underbond.OceanBill.CB_OceanBill);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(ZString.Empty);
				result.Add(Underbond.ContainerLinked.CN_ContainerMode);
				result.Add(Underbond.ContainerLinked.CN_ContainerNumber);
				CusSCAPivot pivot = Underbond.ContainerLinked.Pivots.Count > 0 ? Underbond.ContainerLinked.Pivots[0] : null;
				result.Add(pivot != null ? pivot.CV_GoodsDescription : ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(Underbond.OceanBill != null ? Underbond.OceanBill.CB_RL_NKPortOfDischarge : ZString.Empty);
				if (pivot != null)
				{
					result.Add(pivot.CV_HazardousGoods ? "YES" : "");
					result.Add(pivot.CV_IsSAC ? "YES" : "");
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}

		void LinkedCusMAWB()
		{
			if (Underbond.MAWB != null)
			{
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				result.Add(ZString.Empty);
				result.Add(Underbond.MAWB.CM_FlightNo);
				result.Add(Underbond.MAWB.CM_MAWB);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);

				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(Underbond.MAWB.CM_RL_NKDischargePort);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}

		void LinkedCFSShipment()
		{
			CFSShipment shipment = null;
			if (Underbond.LinkedObject != null)
			{
				shipment = ((CFSShipmentWrapper)Underbond.LinkedObject).Shipment;
			}
			if (shipment != null)
			{
				CFSLoadListConsol consol = shipment.Consols.Count > 0 ? shipment.Consols[0] : null;
				CFSContainer container = null;
				if (consol != null && consol.Containers.Count > 0)
				{
					container = consol.Containers[0];
				}
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				if (consol != null)
				{
					result.Add(consol.Vessel != null ? consol.Vessel.RV_LloydsNumber : ZString.Empty);
				}
				else
				{
					result.Add(ZString.Empty);
				}
				result.Add(shipment.JS_JK_VoyageFlight);
				result.Add(shipment.JS_JK_MasterBillNum);
				result.Add(shipment.JS_HouseBill);
				if (container != null)
				{
					result.Add(container.JC_ContainerMode);
					result.Add(container.JC_ContainerNum);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(shipment.JS_GoodsDescription);
				result.Add(shipment.JS_Calc_ConsignorCompanyName);
				result.Add(shipment.JS_Calc_ConsigneeCompanyName);
				OrgAddress consigneeAddress = shipment.Consignee != null ? shipment.Consignee.MainAddress : null;
				result.Add(consigneeAddress == null ? "" :
					CMRDataExporterCSV.AddressAsASingleLine(consigneeAddress.OA_Address1, consigneeAddress.OA_Address2, consigneeAddress.OA_City, consigneeAddress.OA_State, consigneeAddress.OA_PostCode));
				result.Add(consol != null ? consol.JK_RL_NKDischargePort : ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}

		void LinkedCFSContainer()
		{
			CFSContainer container = null;
			if (Underbond.LinkedObject != null)
			{
				container = ((CFSContainerWrapper)Underbond.LinkedObject).Container;
			}
			if (container != null)
			{
				CFSLoadListConsol consol = container.Consol;

				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				if (consol != null)
				{
					result.Add(consol.Vessel != null ? consol.Vessel.RV_LloydsNumber : ZString.Empty);
					result.Add(consol.JK_JX_JV_VoyageFlight);
					result.Add(consol.JK_MasterBillNum);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(ZString.Empty);
				result.Add(container.JC_ContainerMode);
				result.Add(container.JC_ContainerNum);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				if (consol != null && consol.InvoicingSupporter.Consignee != null)
				{
					result.Add(consol.InvoicingSupporter.Consignee.OH_FullNameTruncated);
					OrgAddress consigneeAddress = consol.InvoicingSupporter.Consignee.MainAddress;
					result.Add(consigneeAddress == null ? "" :
						CMRDataExporterCSV.AddressAsASingleLine(consigneeAddress.OA_Address1, consigneeAddress.OA_Address2, consigneeAddress.OA_City, consigneeAddress.OA_State, consigneeAddress.OA_PostCode));
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(consol != null ? consol.JK_RL_NKDischargePort : ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}

		void LinkedCusSeaManOBLDetail()
		{
			CusSeaManOBLDetail oBL = (CusSeaManOBLDetail)Underbond.LinkedObject;
			CusSeaManOBLHeader header = (CusSeaManOBLHeader)oBL.Header;

			if (oBL != null)
			{
				StringCollectionX result = new StringCollectionX();

				result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
				if (header != null && header.TransportHeader != null)
				{
					result.Add(header.TransportHeader.BT_LloydsIMO);
					result.Add(header.TransportHeader.BT_VoyageNum);
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(header != null ? header.BO_OceanBill : ZString.Empty);
				result.Add(ZString.Empty);
				result.Add(oBL.BD_LineCargoType);
				result.Add(oBL.BD_ContainerNumber);
				result.Add(oBL.BD_GoodsDescription);
				if (header != null)
				{
					result.Add(header.BO_ConsignorName);
					result.Add(header.BO_ConsigneeName);
					result.Add(CMRDataExporterCSV.AddressAsASingleLine(header.BO_ConsigneeAddress1, header.BO_ConsigneeAddress2, header.BO_ConsigneeCity, header.BO_ConsigneeState, header.BO_ConsigneePostCode));
				}
				else
				{
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
					result.Add(ZString.Empty);
				}
				result.Add(header != null ? header.BO_RL_NKDischargePort : ZString.Empty);
				result.Add(oBL.BD_HazardousIndicator ? "YES" : "");
				result.Add(oBL.BD_SACIndicator ? "YES" : "");
				result.Add(Underbond.C4_ModeOfMovement);
				result.Add(ZString.Empty);
				result.Add(Underbond.C4_OriginPremiseID);
				result.Add(Underbond.C4_DestinationPremiseID);
				result.Add(Underbond.C4_PiecesManifested.ToString());
				result.Add(Underbond.C4_SendersMessageReference);
				list.Add(result);
			}
		}
	}
}
