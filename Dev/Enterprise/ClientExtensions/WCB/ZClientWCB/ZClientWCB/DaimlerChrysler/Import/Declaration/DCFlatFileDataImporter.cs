using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	public delegate bool IsOKToImport(JobDeclarationCollection fixedJobDecs, BaseJobDeclarationCollection jobDecs);

	public class DCFlatFileDataImporter : FlatFileDataImporter
	{
		public DCFlatFileDataImporter(bool isMercedes, IsOKToImport okToDoImportDelegate)
		{
			fIsMercedes = isMercedes;
			this.okToDoImportDelegate = okToDoImportDelegate;
		}
		readonly IsOKToImport okToDoImportDelegate;

		public bool IsMercedes
		{
			get { return fIsMercedes; }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new DCImportFlatFileConverter(notifications, FactoryProvider.Current, IsMercedes);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.ConsolAndShipmentCollection();
		}

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			try
			{
				return base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}
			catch (WCBException ex)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}
			additionalTransactionActions = null;
			return false;
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			bool result = false;

			Xsd.ConsolAndShipmentCollection xsdDecs = xSD as Xsd.ConsolAndShipmentCollection;
			if (xsdDecs != null)
			{
				Xsd.ConsolAndShipmentCollection consolidatedXsdDecs = GetConsolidatedDeclarationValueObjects(xsdDecs);
				BaseJobDeclarationCollection jobDecs = new BaseJobDeclarationCollection(FactoryProvider.Current);

				AUDeclarationValueObjectDataAdapter adapter = new AUDeclarationValueObjectDataAdapter();
				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
				foreach (Xsd.ConsolAndShipment xsdDec in consolidatedXsdDecs)
				{
					adapter.ImportFromValueObject(jobDecs, xsdDec.Consol, typeof(JobDeclarationWithFixedInvHeads), importContext);
				}

				if (jobDecs.Count > 0)
				{
					JobDeclarationFixedCollection fixedJobDecs = new JobDeclarationFixedCollection(jobDecs);
					if (okToDoImportDelegate == null || okToDoImportDelegate(fixedJobDecs, jobDecs))
					{
						SetPacksToBondAndPacksForRelease(fixedJobDecs);
						result = true;
					}
				}
			}

			return result;
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new DCImportFlatFileFormat(); }
		}

		#region Implementation

		readonly bool fIsMercedes;

#if DEBUG
		protected
#else
		static 
#endif
 Xsd.ConsolAndShipmentCollection GetConsolidatedDeclarationValueObjects(Xsd.ConsolAndShipmentCollection xsdDecs)
		{
			Xsd.ConsolAndShipmentCollection result = new Xsd.ConsolAndShipmentCollection();

			if (xsdDecs != null)
			{
				foreach (Xsd.ConsolAndShipment importXsdDec in xsdDecs)
				{
					bool exists = false;
					foreach (Xsd.ConsolAndShipment resultXsdDec in result)
					{
						if (resultXsdDec.Consol.Shipments[0].ShipmentDetails.PortofDestination.Port.Value == importXsdDec.Consol.Shipments[0].ShipmentDetails.PortofDestination.Port.Value)
						{
							if (resultXsdDec.Consol.ConsolDetail.TransportMode == importXsdDec.Consol.ConsolDetail.TransportMode)
							{
								if (resultXsdDec.Consol.ConsolDetail.TransportMode == Xsd.ConsolTransportMode.AIR)
								{
									Xsd.FlightWithFlightNumber resultFlight = resultXsdDec.Consol.ConsolDetail.Item as Xsd.FlightWithFlightNumber;
									Xsd.FlightWithFlightNumber importFlight = importXsdDec.Consol.ConsolDetail.Item as Xsd.FlightWithFlightNumber;
									if (resultFlight.FlightNoJourneyNoTruckRegNo == importFlight.FlightNoJourneyNoTruckRegNo)
									{
										CopyInvoices(resultXsdDec.Consol.Shipments[0], importXsdDec.Consol.Shipments[0]);
										exists = true;
										break;
									}
								}
								else
								{
									Xsd.SailingWithVesselVoyage resultSailing = resultXsdDec.Consol.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;
									Xsd.SailingWithVesselVoyage importSailing = importXsdDec.Consol.ConsolDetail.Item as Xsd.SailingWithVesselVoyage;
									if (resultSailing.VesselName == importSailing.VesselName && resultSailing.VoyageNo == importSailing.VoyageNo)
									{
										CopyInvoices(resultXsdDec.Consol.Shipments[0], importXsdDec.Consol.Shipments[0]);
										exists = true;
										break;
									}
								}
							}
						}
					}

					if (!exists)
					{
						result.Add(importXsdDec);
					}
				}
			}
			return result;
		}

		static void CopyInvoices(Xsd.Shipment targetXsdShipment, Xsd.Shipment sourceXsdShipment)
		{
			if (targetXsdShipment != null && sourceXsdShipment != null)
			{
				foreach (Xsd.InvoiceHeader sourceXsdInvHead in sourceXsdShipment.Invoices)
				{
					Xsd.InvoiceHeader existingInvoice = GetExistingInvoice(targetXsdShipment.Invoices, sourceXsdInvHead.InvoiceNumber);
					if (existingInvoice != null)
					{
						CopyInvoiceLines(existingInvoice, sourceXsdInvHead);
					}
					else
					{
						targetXsdShipment.Invoices.Add(sourceXsdInvHead);
					}
				}
			}
		}

		static Xsd.InvoiceHeader GetExistingInvoice(Xsd.InvoiceHeaderCollection collection, string invoiceNumber)
		{
			Xsd.InvoiceHeader result = null;

			foreach (Xsd.InvoiceHeader existingXsdInvHead in collection)
			{
				if (existingXsdInvHead.InvoiceNumber == invoiceNumber)
				{
					result = existingXsdInvHead;
					break;
				}
			}

			return result;
		}

		static void CopyInvoiceLines(Xsd.InvoiceHeader targetXsdInvHead, Xsd.InvoiceHeader sourceXsdInvHead)
		{
			if (targetXsdInvHead != null && sourceXsdInvHead != null)
			{
				foreach (Xsd.InvoiceLine sourceXsdInvLine in sourceXsdInvHead.InvoiceLines)
				{
					targetXsdInvHead.InvoiceLines.Add(sourceXsdInvLine);
				}
			}
		}

		protected virtual void SetPacksToBondAndPacksForRelease(JobDeclarationCollection jobDecs)
		{
			foreach (JobDeclaration jobDec in jobDecs)
			{
				foreach (JobComInvoiceHeader invHead in jobDec.Invoices)
				{
					foreach (JobComInvoiceLine invLine in invHead.JobComInvoiceLines)
					{
						if (invLine.JI_IsPackToBondForLine)
						{
							invHead.JZ_BondPackCount++;
						}
						else
						{
							invHead.JZ_Nature10PackCount++;
						}
					}
				}
			}
		}
		#endregion
	}
}
