using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class UPEManifestImporter : ManifestImporter
	{
		public UPEManifestImporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region RecordTypeConstants

		protected static class RecordTypeConstants
		{
			public const string FlightDetails = "100000";
			public const string ShipmentDetails = "200000";
			public const string Record202Details = "202000";
			public const string ShipperDetails = "300000";
			public static readonly string[] InvoiceDetails =
			{
				"500000",
				"501000",
				"502000",
				"503000",
				"504000",
				"505000",
				"506000",
				"507000",
				"508000",
				"509000",
				"510000",
				"511000",
			};
		}

		#endregion

		#region ValidationConstants

		public IReadOnlyList<ForwardingConsol> Consols
		{
			get { return (ForwardingConsol[])fConsols.ToArray(typeof(ForwardingConsol)); }
		}
		ArrayList fConsols = new ArrayList();

		protected static class ValidationConstants
		{
			public const int HeaderLength = 50;
		}

		#endregion

		protected override bool ImportDataToFactoryCore(TextReader dataStream, string attachmentFileName, INotifications notify, out ITransactionParticipant[] transactionActions)
		{
			transactionActions = null;
			DoImport(dataStream.ReadToEnd());
			return true;
		}

		protected override void DoImport(string data)
		{
			base.DoImport(data);
			ProcessOutstandingMappers();
		}

		protected override void ProcessLine(string line)
		{
			Type mapperType = GetLineMapper(line);
			if (mapperType != null)
			{
				UPEDataLine mapper = (UPEDataLine)mapperType.GetConstructor(new Type[] { typeof(UPEManifestImporter), typeof(ExportCustomsManifestHeader), typeof(string) }).Invoke(new object[] { this, GeneratedHeader, line });
				if (!mapper.ValidateLine())
				{
					return;
				}

				if (mapper is FlightDetailsLine)
				{
					mapper.Process();

					// New business object factory for each consols to ensure that when the user saves a consol, it won't save the others
					BusinessObjectFactory factory = new BusinessObjectFactory();
					ConsolImporter consolImporter = new ConsolImporter((FlightDetailsLine)mapper, factory);
					ForwardingConsol consol = consolImporter.ImportToConsol();
					if (fConsols == null)
					{
						fConsols = new ArrayList();
					}

					fConsols.Add(consol);
				}
				else
				{
					string shipmentNumber = line.Substring(33, 11);

					if (shipmentNumber != CurrentShipmentNumber)
					{
						ProcessOutstandingMappers();
						CurrentShipmentNumber = shipmentNumber;
					}

					OutstandingMappers.Add(mapper);
				}
			}
		}

		#region Implementation

		protected Type GetLineMapper(string line)
		{
			if (Mappers == null)
			{
				Mappers = new Hashtable();
				Mappers.Add(RecordTypeConstants.FlightDetails, typeof(FlightDetailsLine));
				Mappers.Add(RecordTypeConstants.ShipmentDetails, typeof(ShipmentDetailsLine));
				Mappers.Add(RecordTypeConstants.Record202Details, typeof(Record202DetailsLine));
				Mappers.Add(RecordTypeConstants.ShipperDetails, typeof(ShipperDetailsLine));

				foreach (string recordType in RecordTypeConstants.InvoiceDetails)
				{
					Mappers.Add(recordType, typeof(InvoiceDetailsLine));
				}
			}

			if (line.Length >= ValidationConstants.HeaderLength)
			{
				return (Type)Mappers[line.Substring(44, 6)];
			}

			return null;
		}
		Hashtable Mappers;

		public void ProcessOutstandingMappers()
		{
			if (OutstandingMappers.Count > 0)
			{
				InvoiceDetailsLine mainInvoice = (InvoiceDetailsLine)OutstandingMappers.Find(mapper => { return mapper is InvoiceDetailsLine && ((InvoiceDetailsLine)mapper).IsMainInvoice(); });

				if (mainInvoice != null)
				{
					Record202DetailsLine record202 = (Record202DetailsLine)OutstandingMappers.Find(mapper => { return mapper is Record202DetailsLine; });
					if (record202 != null)
					{
						mainInvoice.PackageTrackingNumber = record202.PackageTrackingNumber;
					}

					mainInvoice.Process();
					OutstandingMappers.FindAll(mapper => { return mapper != mainInvoice; }).ForEach(mapper => mapper.Process());
				}
				else
				{
					ShipmentDetailsLine shipmentDetails = (ShipmentDetailsLine)OutstandingMappers.Find(mapper => { return mapper is ShipmentDetailsLine && ((ShipmentDetailsLine)mapper).IsLineCountryDifferentToCurrentCountry(); });

					if (shipmentDetails != null)
					{
						shipmentDetails.CreateExportManifestLine();
						shipmentDetails.Process();
						OutstandingMappers.FindAll(mapper => { return mapper != shipmentDetails; }).ForEach(mapper => mapper.Process());
					}
				}

				OutstandingMappers.Clear();
			}
		}

		List<UPEDataLine> OutstandingMappers
		{
			get
			{
				return outstandingMappers ?? (outstandingMappers = new List<UPEDataLine>());
			}
		}

		List<UPEDataLine> outstandingMappers;
		ZString CurrentShipmentNumber;

		#endregion

		protected sealed override BillingInterfaceName InterfaceName
		{
			get { return BillingInterfaceName.ClientSpecifiedImport; }
		}
	}
}
