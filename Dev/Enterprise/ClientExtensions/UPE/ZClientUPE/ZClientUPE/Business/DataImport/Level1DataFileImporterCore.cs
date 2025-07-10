using System;
using System.Collections.Specialized;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public abstract class Level1DataFileImporterCore
	{
		protected Level1DataFileImporterCore(Level1DataImport level1DataImport, INotifications notify)
		{
			this.level1DataImport = level1DataImport;
			this.notify = notify;
		}

		public void LoadFile()
		{
			int currentRecord = 0;
			PrepareForLoad();
			var progressNotification = new ProgressNotification(0);

			try
			{
				foreach (Level1Record record in Level1RecordList)
				{
					if (!record.IsEmpty)
					{
						currentRecord++;

						LoadRecord(record);

						progressNotification.PercentageComplete = (currentRecord * 100) / totalNoOfRecords;
						UpdateProgress(progressNotification);
					}
					else
					{
						ZString trackingNumber = record._202000 == null ? "" : record._202000.PackageTrackingNumber.ToString();
						if (!trackingNumber.IsEmpty)
						{
							EmptyShipmentsList.Add(trackingNumber);
						}
					}
				}

				if (TotalNoOfShipments != 0)
				{
					fPercentageOfDuplicateHAWBs = GetDuplicatePercentage();
					fPercentageOfIncorrectPorts = TotalNoOfShipments == 0 ? 0 : (IncorrectPortOfDestinationList.Count * 100) / TotalNoOfShipments;
				}
				NotificationLogger.Append(GetSummaryInformation());
			}
			catch (Level1FileReadException e)
			{
				level1DataImport.FileName = "";
				string errorMessage = e.Message + "\n" +
					"Approximate Line Number: " + e.LineNumber + "\n\n" +
					"Line Value: " + e.LineValue + "\n\n" +
					"Previous Line Value: " + e.PreviousLineValue + "\n\n";

				Globals.Message.ShowError(errorMessage);
			}
		}

		public void Save()
		{
			var start = ZDateTime.UtcNow;
			AddToLog("Save started at: " + start.ToLongTimeString());

			ProcessData();

			var end = ZDateTime.UtcNow;
			AddToLog("Save completed at: " + end.ToLongTimeString());
			AddToLog("Duration (minutes): " + new TimeSpan(end.Ticks - start.Ticks).TotalMinutes);

			SaveSummaryTextToEDocs();
		}

		public virtual ZString GetErrorMessage()
		{
			if (TotalPiecesManifested > 32767)
			{
				return "The level 1 file is invalid, total pieces manifested should be < 32767. \r\n";
			}
			if (TotalNoOfShipments <= 0)
			{
				return "There is no valid shipments in the file to be imported. \r\n";
			}
			else
			{
				return ZString.Empty;
			}
		}

		#region Summary Information
		protected string FlightDetailsSummary
		{
			get
			{
				return string.Format(
					CultureInfo.InvariantCulture,
					"Flight Details Summary\r\n" +
					"======================\r\n" +
					"Flight Number     : {0}\r\n" +
					"Port Of Loading   : {1}\r\n" +
					"Port Of Discharge : {2}\r\n" +
					"Arrival Date      : {3}\r\n" +
					"Master Bill       : {4}\r\n" +
					"\r\n",
					level1DataImport.FlightNumber,
					level1DataImport.PortOfLoading,
					level1DataImport.PortOfDischarge,
					level1DataImport.ArrivalDate.ToShortDateString(),
					level1DataImport.MasterBill);
			}
		}

		protected ZStringBuilder IncorrectPortOfDestinationSummary
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				string header = string.Format(
					CultureInfo.InvariantCulture,
					"Port of Destination Not Matched % : {0}\r\n" +
					"\r\n" +
					"Port of Destination <> Port of Discharge: '{1}'\r\n" +
					"=================================================\r\n",
					PercentageOfIncorrectPorts,
					level1DataImport.PortOfDischarge);
				result.Append(header);

				if (IncorrectPortOfDestinationList.Count > 0)
				{
					foreach (string s in IncorrectPortOfDestinationList)
					{
						result.Append(s);
						result.Append("\r\n");
					}
				}

				result.Append("\r\n");

				return result;
			}
		}

		protected ZStringBuilder EmptyShipmentsSummary
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				result.Append(
					"Empty Shipments Not Imported\r\n" +
					"============================\r\n");

				if (EmptyShipmentsList.Count > 0)
				{
					foreach (string s in EmptyShipmentsList)
					{
						result.Append(s);
						result.Append("\r\n");
					}

					result.Append("Number of Empty Shipments = " + EmptyShipmentsList.Count + "\r\n");
				}

				result.Append("\r\n");

				return result;
			}
		}

		public StringCollection IncorrectChildPacks
		{
			get { return fIncorrectChildPacks ?? (fIncorrectChildPacks = new StringCollection()); }
		}
		StringCollection fIncorrectChildPacks;

		void ResetIncorrectChildPacks()
		{
			fIncorrectChildPacks = null;
		}

		public StringCollection IncorrectPortOfDestinationList
		{
			get { return fIncorrectPortOfDestinationList ?? (fIncorrectPortOfDestinationList = new StringCollection()); }
		}
		StringCollection fIncorrectPortOfDestinationList;

		void ResetIncorrectPortOfDestinationList()
		{
			fIncorrectPortOfDestinationList = null;
		}

		public StringCollection EmptyShipmentsList
		{
			get { return fEmptyShipmentsList ?? (fEmptyShipmentsList = new StringCollection()); }
		}
		StringCollection fEmptyShipmentsList;

		void ResetEmptyShipmentsList()
		{
			fEmptyShipmentsList = null;
		}

		public int TotalPiecesManifested
		{
			get { return fTotalPiecesManifested; }
		}
		int fTotalPiecesManifested;

		public int TotalNoOfShipments
		{
			get { return fTotalNoOfShipments; }
		}
		int fTotalNoOfShipments;

		public int TotalNoOfChildPackages
		{
			get { return fTotalNoOfChildPackages; }
		}
		int fTotalNoOfChildPackages;

		public int PercentageOfDuplicateHAWBs
		{
			get { return fPercentageOfDuplicateHAWBs; }
		}
		int fPercentageOfDuplicateHAWBs;

		public int PercentageOfIncorrectPorts
		{
			get { return fPercentageOfIncorrectPorts; }
		}
		int fPercentageOfIncorrectPorts;
		#endregion

		#region Implementation

		protected abstract void ProcessData();

		protected virtual void SaveSummaryTextToEDocs()
		{
		}

		#region Load Tasks

		void PrepareForLoad()
		{
			fTotalPiecesManifested = 0;
			fTotalNoOfShipments = 0;
			fTotalNoOfChildPackages = 0;
			fNotificationLogger = null;

			PrepareForLoadCore();
			ResetIncorrectChildPacks();
			ResetIncorrectPortOfDestinationList();
			ResetEmptyShipmentsList();
		}

		protected virtual void PrepareForLoadCore()
		{
		}

		protected abstract bool CheckForDuplicateShipment(Level1Record record);
		public abstract ZString GetSummaryInformation();

		protected virtual bool RecordShouldNotBeLoaded(Level1Record record)
		{
			return false;
		}

		void LoadRecord(Level1Record record)
		{
			if (RecordShouldNotBeLoaded(record))
			{
				return;
			}

			if (record.IsGCCChild())
			{
				fTotalNoOfChildPackages++;
			}
			else
			{
				if (record.IsGCCLead())
				{
					fTotalPiecesManifested += record._401000 != null ? record._401000.TotalPackageCountForGCCShipment : ZInt.Zero;
				}
				else
				{
					CheckManifestedAgainstChildPacks(record);
				}

				CheckForDuplicateShipment(record);

				fTotalNoOfShipments++;
				CheckDestinationPortIsDifferentToDischargePort(record);
			}
		}

		#region Checks

		protected virtual int GetDuplicatePercentage()
		{
			return 0;
		}

		void CheckManifestedAgainstChildPacks(Level1Record record)
		{
			if (record._202000 != null)
			{
				fTotalPiecesManifested += record._202000.PiecesManifested;
				fTotalNoOfChildPackages += record._600000Lines.Count;
				if ((record._202000.PiecesManifested - 1) != record._600000Lines.Count)
				{
					string incorrectChildPackDescription = string.Format(CultureInfo.InvariantCulture, "{0}: {1} childpack/s, {2} expected", record._202000.PackageTrackingNumber.PadRight(18, ' '), record._600000Lines.Count, record._202000.PiecesManifested - 1);
					IncorrectChildPacks.Add(incorrectChildPackDescription);
				}
			}
		}

		void CheckDestinationPortIsDifferentToDischargePort(Level1Record record)
		{
			if (record._202000 != null)
			{
				var destinationUNLoco = GetUNLOCOFromUPSMapping(record._202000.DestinationPort);
				if (destinationUNLoco == null || destinationUNLoco.Code != level1DataImport.PortOfDischarge)
				{
					string destinationUNLocoCode = destinationUNLoco != null ? destinationUNLoco.Code.ToString() : record._202000.DestinationCountry + record._202000.DestinationPort;
					IncorrectPortOfDestinationList.Add(record._202000.PackageTrackingNumber + " PortCode: " + destinationUNLocoCode);
				}
			}
		}

		#endregion

		#endregion

		protected RefUNLOCO GetUNLOCOFromUPSMapping(ZString code)
		{
			return FactoryProvider.Current.GetCachedValue(code, () =>
			{
				return code.IsEmpty ? null : RefUNLOCO.LoadFromLocalMap(FactoryProvider.Current, code, GetCountryCodeForUPSMapping(), UPEDataLine.Constants.RefLocoSystemUsage);
			});
		}

		protected abstract ZString GetCountryCodeForUPSMapping();

		#region Notification Logs

		protected void AddToLog(ZString message)
		{
			notify.AddWarning(message);
			NotificationLogger.Append(message);
		}

		public void UpdateProgress(ProgressNotification notification)
		{
			notify.Notify(notification);
		}

		protected internal ZStringBuilder NotificationLogger => fNotificationLogger ?? (fNotificationLogger = new ZStringBuilder());
		ZStringBuilder fNotificationLogger;

		#endregion

		#region Factory Provider

		internal BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					var nameForDebugging = "Level1 Data File Importer";
					factoryProvider = new BusinessObjectFactoryProvider();
					factoryProvider.Current.NameForDebugging = nameForDebugging;
					factoryProvider.CurrentFactoryChanged += (sender, e) =>
					{
						if (sender is BusinessObjectFactoryProvider provider)
						{
							provider.Current.NameForDebugging = nameForDebugging;
						}
					};
				}
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		#endregion

		#region Currency Conversion

		protected ZDecimal GetValueInLocalCurrency(ZDecimal value, string fromCurrency)
		{
			return ConvertValueInCurrencies(value, fromCurrency, LocalCurrency.RX_Code);
		}

		protected ZDecimal ConvertValueInCurrencies(ZDecimal value, ZString fromCurrencyCode, ZString toCurrencyCode)
		{
			ZDecimal result = 0;

			if (fromCurrencyCode == toCurrencyCode)
			{
				result = value;
			}
			else
			{
				var fromCurrency = RefCurrency.LoadFromCurrencyCode(FactoryProvider.Current, fromCurrencyCode);
				var toCurrency = RefCurrency.LoadFromCurrencyCode(FactoryProvider.Current, toCurrencyCode);
				if (fromCurrency != null && toCurrency != null)
				{
					var moneyInToCurrency = CurrencyConverter.ConvertRounded(new Money(value, fromCurrency), toCurrency);
					if (moneyInToCurrency != null)
					{
						result = moneyInToCurrency.Amount.Round(2);
					}
				}
			}

			return result;
		}

		protected virtual CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = CurrencyConverter.New(FactoryProvider.Current, ZDateTime.Now, ZArchitecture.Core.ExchangeRateType.Customs, 10);
				}
				return fCurrencyConverter;
			}
		}
		protected CurrencyConverter fCurrencyConverter;

		protected RefCurrency LocalCurrency => localCurrency ?? (localCurrency = RefCurrency.LoadFromCurrencyCode(FactoryProvider.Current, LocalCurrencyCode));
		RefCurrency localCurrency;

		protected abstract string LocalCurrencyCode { get; }

		#endregion

		#region Level1RecordList
		protected Level1RecordList Level1RecordList
		{
			get
			{
				if (fLevel1RecordList == null)
				{
					Level1FileReader level1FileReader = new Level1FileReader(level1DataImport.FileName);
					fLevel1RecordList = level1FileReader.Level1RecordList;
					totalNoOfRecords = fLevel1RecordList.Count;
				}
				return fLevel1RecordList;
			}
		}
		Level1RecordList fLevel1RecordList;
		#endregion

		public Level1DataImport level1DataImport;
		readonly INotifications notify;
		protected const int ClearFactoryAfterNRecords = 50;
		protected int totalNoOfRecords;

		#endregion
	}
}
