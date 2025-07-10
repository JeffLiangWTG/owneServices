using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ConsolEDNExporter : CMRDataExporterCSV
	{
		public ConsolEDNExporter(ForwardingConsol consol)
			: base(consol)
		{
		}

		public ForwardingConsol Consol
		{
			get { return (ForwardingConsol)BizObj; }
		}

		public override string PartFileName
		{
			get
			{
				return "EDN";
			}
		}

		public override string MailSubject
		{
			get
			{
				return "Contingency Export Declaration";
			}
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
					if (shipment.Declarations.Length > 0 && !ShipmentHasExistingEDN(shipment))
					{
						var auExportDeclaration = GetAUExportDeclaration(shipment);

						if (auExportDeclaration != null)
						{
							foreach (JobComInvoiceLine aHECCLine in auExportDeclaration.InvoiceLines)
							{
								StringCollectionX result = new StringCollectionX();

								result.Add(GlbCompany.CurrentCompany.GC_Name);
								result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
								result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
								result.Add(shipment.JS_UniqueConsignRef);
								result.Add(auExportDeclaration.JE_MasterBill);
								result.Add(auExportDeclaration.JE_HouseBill);
								result.Add(auExportDeclaration.Supplier != null ? auExportDeclaration.Supplier.OH_FullNameTruncated : ZString.Empty);
								if (auExportDeclaration.Supplier == null)
								{
									result.Add(ZString.Empty);
								}
								else if (!auExportDeclaration.Supplier.PrimaryRegistrationNumber.Number.IsEmpty)
								{
									result.Add(auExportDeclaration.Supplier.PrimaryRegistrationNumber.Number);
								}
								else
								{
									result.Add(auExportDeclaration.Supplier.GetCustomsClientID());
								}
								result.Add(auExportDeclaration.Importer != null ? auExportDeclaration.Importer.OH_FullNameTruncated : ZString.Empty);
								result.Add(auExportDeclaration.JE_RL_NKFinalDestination.SubstringSafe(0, 2));
								result.Add(CMRDataExporterCSV.CMRDateString(auExportDeclaration.JE_ExportDate));
								result.Add(auExportDeclaration.JE_TransportMode);
								if (auExportDeclaration.JE_TransportMode == Core.Constants.TransportModes.Air)
								{
									result.Add(auExportDeclaration.JE_VoyageFlightNo);
								}
								else if (auExportDeclaration.JE_TransportMode == Core.Constants.TransportModes.Sea && Consol.Vessel != null)
								{
									result.Add(Consol.Vessel.RV_LloydsNumber);
								}
								else
								{
									result.Add(ZString.Empty);
								}
								result.Add(aHECCLine.JI_Tariff.Replace(".", ""));
								result.Add(aHECCLine.JI_Description);
								result.Add(aHECCLine.AddInfo.ZA_PermitNumbers_Hidden);
								result.Add(aHECCLine.JI_Calc_FOB_InLocalCurrency.ToString("f"));
								result.Add(auExportDeclaration.JE_RL_NKPortOfLoading);
								if (auExportDeclaration.JE_TransportMode == Core.Constants.TransportModes.Sea)
								{
									result.Add(Consol.Transports.MostInterestingTransport.JW_VoyageFlight);
								}
								else
								{
									result.Add(ZString.Empty);
								}
								list.Add(result);
							}
						}
					}
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		JobDeclaration GetAUExportDeclaration(CommonShipment shipment)
		{
			JobDeclaration result = null;
			foreach (var declaration in shipment.GetAUDeclarations())
			{
				if (declaration.IsExport)
				{
					result = declaration;
					break;
				}
			}
			return result;
		}

		bool ShipmentHasExistingEDN(CommonShipment ship)
		{
			return !ship.CustomsEntryNumberType.StartsWith("EX") &&
				!ship.CustomsEntryNumber.IsEmpty;
		}
	}
}
