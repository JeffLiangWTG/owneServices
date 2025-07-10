using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ConsolCRNExporter : CMRDataExporterCSV
	{
		public ConsolCRNExporter(ForwardingConsol consol)
			: base(consol)
		{
		}

		public ForwardingConsol Consol
		{
			get { return (ForwardingConsol)BizObj; }
		}

		public override string PartFileName
		{
			get { return "CRN"; }
		}

		public override string MailSubject
		{
			get { return "Contingency Export Sub Manifest"; }
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return new ForwardingConsolDataExportCSVFileNameProvider(Consol); }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return Consol; }
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
				ArrayList list = new ArrayList();

				foreach (CommonShipment shipment in Consol.Shipments)
				{
					StringCollectionX result = new StringCollectionX();

					result.Add(GlbCompany.CurrentCompany.GC_Name);
					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Replace(" ", "").SubstringSafe(0, 11));
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
					result.Add(Consol.JK_UniqueConsignRef);
					result.Add(Consol.JK_MasterBillNum);
					result.Add(shipment.JS_HouseBill);
					result.Add(ConsolVoyageFlight);
					result.Add(ConsolTransportMode);
					result.Add(ConsolVoyageDate);
					result.Add(GetExemptionValue(shipment));
					result.Add(shipment.ConsignorDocumentaryAddress != null ? shipment.ConsignorDocumentaryAddress.E2_CompanyNameTruncated : ZString.Empty);
					result.Add(shipment.JS_GoodsDescription);
					result.Add(shipment.JS_RL_NKDestination.SubstringSafe(0, 2));
					result.Add(ConsolLoadPort);
					result.Add(ConsolVessel);
					result.Add(ZString.Empty);
					list.Add(result);
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		#region Consol Transport Leg

		ZString ConsolVoyageFlight
		{
			get { return ExportInternationalTransportLeg != null ? ExportInternationalTransportLeg.JW_VoyageFlight : Consol.Voyage != null ? Consol.Voyage.JV_VoyageFlight : ZString.Empty; }
		}

		ZString ConsolTransportMode
		{
			get { return ExportInternationalTransportLeg != null ? ExportInternationalTransportLeg.JW_TransportMode : Consol.JK_TransportMode; }
		}

		ZString ConsolVoyageDate
		{
			get
			{
				var result = ZString.Empty;
				if (ExportInternationalTransportLeg != null)
				{
					ZDateTime departureDate = ExportInternationalTransportLeg.JW_ATD.IsEmpty ? ExportInternationalTransportLeg.JW_ETD : ExportInternationalTransportLeg.JW_ATD;
					result = CMRDataExporterCSV.CMRDateString(departureDate);
				}
				else
				{
					result = !Consol.JK_JX_JA_E_DEP.IsEmpty ? CMRDataExporterCSV.CMRDateString(Consol.JK_JX_JA_E_DEP) : ZString.Empty;
				}

				return result;
			}
		}

		ZString ConsolLoadPort
		{
			get { return ExportInternationalTransportLeg != null ? ExportInternationalTransportLeg.JW_RL_NKLoadPort : Consol.LoadPort != null ? Consol.LoadPort.RL_Code : ZString.Empty; }
		}

		ZString ConsolVessel
		{
			get { return ExportInternationalTransportLeg != null && ExportInternationalTransportLeg.Vessel != null ? ExportInternationalTransportLeg.Vessel.RV_LloydsNumber : Consol.Vessel != null ? Consol.Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		Transport ExportInternationalTransportLeg
		{
			get
			{
				if (exportInternationalTransportLeg == null)
				{
					foreach (Transport transportLeg in Consol.Transports)
					{
						if (IsCrossingBorderExportLeg(transportLeg))
						{
							exportInternationalTransportLeg = transportLeg;
							break;
						}
					}
				}
				return exportInternationalTransportLeg;
			}
		}
		Transport exportInternationalTransportLeg;

		bool IsCrossingBorderExportLeg(Transport transportLeg)
		{
			bool result = false;

			if (!transportLeg.IsDomestic &&
				(transportLeg.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.Australia) &&
				!(transportLeg.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.Australia))))
			{
				result = true;
			}

			return result;
		}

		#endregion

		string GetExemptionValue(CommonShipment ship)
		{
			string result = "";
			if (!ship.CustomsEntryNumberType.IsEmpty)
			{
				if (ship.CustomsEntryNumberType.StartsWith("EX"))
				{
					result = ship.CustomsEntryNumberType;
				}
				else
				{
					result = ship.CustomsEntryNumber;
				}
			}
			return result;
		}
	}
}
