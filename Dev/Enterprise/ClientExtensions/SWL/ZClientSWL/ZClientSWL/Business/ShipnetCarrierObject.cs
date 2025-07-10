using System;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Client.SWL.Business
{
	public class ShipnetCarrierObject
	{
		protected ShipnetCarrierObject(OrgHeader carrier, INotifications notify)
		{
			if (carrier == null)
			{
				throw new ArgumentNullException(nameof(carrier));
			}

			if (notify == null)
			{
				throw new ArgumentNullException(nameof(notify));
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			this.Carrier = factory.Load<OrgHeader>(carrier.PK);

			if (this.Carrier == null)
			{
				throw new ArgumentException(string.Format("Carrier {0} does not exist in database", carrier.OH_FullNameTruncated));
			}

			if (!IsShipnetCarrier(carrier.PK))
			{
				throw new ArgumentException(string.Format("Carrier {0} is not a Shipnet Carrier", carrier.OH_FullNameTruncated));
			}

			Buffer = new NotificationBuffer(notify);
		}

		public static ShipnetCarrierObject New(OrgHeader carrier, INotifications notify)
		{
			ShipnetCarrierObject result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				result = new ShipnetCarrierObject(carrier, notify);
			}
			else
			{
				result = overridden(carrier, notify);
			}
			return result;
		}

		public void Add(ShipnetARInvoice shipnetInvoice)
		{
			if (shipnetInvoice != null)
			{
				if (shipnetInvoice.IsValidShipnetType)
				{
					Buffer.Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "Generating records for Invoice {0}", shipnetInvoice.AH_TransactionNum)));

					switch (shipnetInvoice.AH_TransactionType)
					{
						case TransactionTypes.Invoice:
							AddInvoice(shipnetInvoice);
							break;
						case TransactionTypes.CreditNote:
							AddCreditNote(shipnetInvoice);
							break;
					}
				}
				else
				{
					Buffer.Notify(
						new ErrorNotification(
							ErrorType.Error, Res.GetString("4a7ad21e-b93d-4653-b6fe-7b7eaee8819b", "Cannot add an invalid invoice type")));
				}
			}
		}

		public void Export()
		{
			if (Records.Count == 0)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("fa27c0f8-9ad8-4b14-8b0a-2e3d7c8a7943", "Shipnet Carrier {0} has no data to export.", Carrier.OH_FullNameTruncated)));
			}
			else
			{
				SetRecordStatus();
				ExportCore();
				NotifyIfUnknownChargeCodesWereFound();
			}
			SendEmailIfHasErrors();
		}

		#region Implementation

		protected delegate ShipnetCarrierObject NewDelegate(OrgHeader carrier, INotifications notify);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected virtual void AddCreditNote(ShipnetARInvoice shipnetInvoice)
		{
			ShipnetRecord header = CreateHeader(shipnetInvoice, false);
			Records.Add(header);
			Records.Add(CreatLines(shipnetInvoice, header, true));
		}

		protected virtual void AddInvoice(ShipnetARInvoice shipnetInvoice)
		{
			ShipnetRecord header = CreateHeader(shipnetInvoice, false);
			Records.Add(header);
			Records.Add(CreatLines(shipnetInvoice, header, true));
		}

		protected FlatFileDataRowCollection CreatLines(ShipnetARInvoice shipnetInvoice, ShipnetRecord header, bool isNegatedAmount)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();
			foreach (InvoiceLine line in shipnetInvoice.Lines)
			{
				ZString groupCode;
				ZString groupDescription;
				GetGroupInfo(line.ChargeCode, out groupCode, out groupDescription);
				result.Add(CreateLine(line, header, isNegatedAmount, groupCode, groupDescription));
			}
			return result;
		}

		protected virtual ShipnetRecord CreateLine(InvoiceLine line, ShipnetRecord header, bool isNegatedAmount, ZString groupCode, ZString groupDescription)
		{
			ShipnetRecord result = new ShipnetRecord();
			CopyHeaderData(header, result);
			result.AccountNo = groupCode;
			result.Text = line.Invoice.AH_TransactionType + " " + groupDescription;
			ZDecimal amount = line.AL_OSAmount;
			ZDecimal currAmount = 0;
			if (line.AL_JH.IsValid && line.Job != null)
			{
				GenericJob jobDetails = line.Job.LoadGenericJob<GenericJob>();
				if (jobDetails != null)
				{
					if (jobDetails.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.AgencyBooking
						|| jobDetails.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.AgencyBillOfLading)
					{
						if (!jobDetails.InvoicingSupporter.MasterBillNumber.IsEmpty)
						{
							result.Text = jobDetails.InvoicingSupporter.MasterBillNumber + " " + result.Text;
						}
					}
					else
					{
						if (!jobDetails.InvoicingSupporter.HouseBillNumber.IsEmpty)
						{
							result.Text = jobDetails.InvoicingSupporter.HouseBillNumber + " " + result.Text;
						}
					}
				}
			}

			RefCurrency uSDCurrency = RefCurrency.LoadFromCurrencyCode(line.Factory, Constants.CurrencyCodes.UnitedStates);
			if (uSDCurrency != null && !line.AL_RX_NKTransactionCurrency.IsEmpty && line.AL_RX_NKTransactionCurrency != Constants.CurrencyCodes.UnitedStates)
			{
				amount = Env.CurrentCompany.ExchangeRate.LocalToForeign(line.AL_LocalTotalAmount, result.ExchangeRate, uSDCurrency.RX_Code);
				currAmount = line.AL_OSAmount;
			}
			result.Amount = FixAmount(amount, isNegatedAmount);
			result.CurrAmount = FixAmount(currAmount, isNegatedAmount);
			result.VATCode = GetVatCode(line.AL_TaxRateCalc);
			return result;
		}

		protected virtual ZString GetVatCode(ZDecimal taxRate)
		{
			var result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			result += taxRate > 0 ? taxRate.ToStringTrimZeros() + "S" : "0";
			return result;
		}

		protected virtual void CopyHeaderData(ShipnetRecord header, ShipnetRecord line)
		{
			line.CompanyCode = header.CompanyCode;
			line.EntryDate = header.EntryDate;
			line.InvoiceDate = header.InvoiceDate;
			line.ShipCode = header.ShipCode;
			line.Voyage = header.Voyage;
			line.CurrencyCode = header.CurrencyCode;
			line.ExchangeRate = header.ExchangeRate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected void GetGroupInfo(AccChargeCode chargeCode, out ZString groupCode, out ZString groupDescription)
		{
			groupCode = UnknownCodeString;
			groupDescription = string.Format("({0}) {1}", chargeCode.AC_Code, chargeCode.AC_Desc);
			if (chargeCode != null)
			{
				ShipnetChargeGroup shipnetGroup = GetShipnetChargeGroup(chargeCode.PK);
				if (shipnetGroup == null)
				{
					AddToUnknowCodes(chargeCode.AC_Code);
				}
				else
				{
					groupCode = shipnetGroup.ChargeGroupCode;
					groupDescription = shipnetGroup.ChargeGroupDescription;
				}
			}
		}

		protected void AddToUnknowCodes(ZString code)
		{
			if (!UnknowChargeCodes.Contains(code))
			{
				UnknowChargeCodes.Add(code);
			}
		}

		protected ShipnetChargeGroup GetShipnetChargeGroup(ZGuid chargePK)
		{
			ShipnetChargeGroup result = null;
			foreach (ShipnetChargeGroup chargeGroup in Settings.ChargeGroups)
			{
				if (chargeGroup.Charges.ContainsChargePK(chargePK))
				{
					result = chargeGroup;
					break;
				}
			}
			return result;
		}

		protected virtual ShipnetRecord CreateHeader(ShipnetARInvoice shipnetInvoice, bool isNegatedAmount)
		{
			ShipnetRecord record = new ShipnetRecord();
			record.CompanyCode = GetOrgCode(Carrier);
			record.AccountNo = Settings.DebtorControlCode;
			record.EntryDate = shipnetInvoice.AH_InvoiceDate;
			if (!shipnetInvoice.OceanBill.IsEmpty)
			{
				record.Text = shipnetInvoice.OceanBill + " " + shipnetInvoice.AH_TransactionType;
			}
			else
			{
				record.Text = shipnetInvoice.AH_TransactionType;
			}
			ZDecimal amount;
			ZString currencyCode;
			ZDecimal exchangeRate;
			ZDecimal currAmount;
			ConvertToUSDData(shipnetInvoice, out amount, out currencyCode, out exchangeRate, out currAmount);
			record.Amount = FixAmount(amount, isNegatedAmount);
			record.CurrencyCode = currencyCode;
			record.ExchangeRate = exchangeRate;
			record.CurrAmount = FixAmount(currAmount, isNegatedAmount);
			RefVessel vessel = shipnetInvoice.Vessel;
			if (vessel != null)
			{
				record.ShipCode = (vessel.CustomAttribute1Used && !vessel.RV_CustomAttrib1.IsEmpty) ? vessel.RV_CustomAttrib1 : vessel.RV_LloydsNumber;
			}
			record.Voyage = shipnetInvoice.Voyage;
			record.InvoiceNo = GetPaddedInvoiceNumber(shipnetInvoice.InvoiceNumber);
			record.InvoiceDate = shipnetInvoice.AH_PostDate;
			record.DueDate = shipnetInvoice.AH_DueDate;
			record.LedgerType1 = "1";
			record.LedgerCode1 = GetOrgCode(shipnetInvoice.Header);
			if (!record.LedgerCode1.IsEmpty && shipnetInvoice.AH_RX_NKTransactionCurrency == Core.Constants.CurrencyCodes.UnitedStates)
			{
				record.LedgerCode1 += "U";
			}
			return record;
		}

		protected virtual ZString GetPaddedInvoiceNumber(ZString invoiceNumber)
		{
			ZString numericOnly = invoiceNumber.KeepNumericCharacters();
			return !invoiceNumber.IsEmpty && numericOnly == invoiceNumber && numericOnly.Length < 8 ? numericOnly.PadLeft(8, '0') : invoiceNumber;
		}

		protected virtual ZString GetOrgCode(OrgHeader org)
		{
			ZString result = ZString.Empty;
			if (org != null)
			{
				result = GetMapCode(Constants.OrgPatternMatchOverrideRelationships.Organisation, org.PK);
				if (result.IsEmpty)
				{
					result = org.OH_Code;
				}
			}
			return result;
		}

		#region Records

		protected FlatFileDataRowCollection Records
		{
			get
			{
				if (fRecords == null)
				{
					fRecords = new FlatFileDataRowCollection();
				}
				return fRecords;
			}
		}

		FlatFileDataRowCollection fRecords;

		#endregion

		#region UnknowChargeCodes

		protected StringCollectionX UnknowChargeCodes
		{
			get
			{
				if (fUnknowChargeCodes == null)
				{
					fUnknowChargeCodes = new StringCollectionX();
				}
				return fUnknowChargeCodes;
			}
		}

		StringCollectionX fUnknowChargeCodes;

		#endregion

		protected ZDecimal FixAmount(ZDecimal amount, bool isNegatedAmount)
		{
			return amount * (isNegatedAmount ? -1 : 1);
		}

		#region ConvertToUSDData

		protected void ConvertToUSDData(ShipnetARInvoice shipnetInvoice, out ZDecimal amount, out ZString currencyCode, out ZDecimal exchangeRate, out ZDecimal currAmount)
		{
			amount = shipnetInvoice.AH_OSTotal;
			currencyCode = "";
			exchangeRate = 0;
			currAmount = 0;

			if (shipnetInvoice.AH_RX_NKTransactionCurrency != Constants.CurrencyCodes.UnitedStates)
			{
				currencyCode = shipnetInvoice.AH_RX_NKTransactionCurrency;
				currAmount = shipnetInvoice.AH_OSTotal;
				exchangeRate = shipnetInvoice.AH_ExchangeRate;
				RefCurrency uSDCurrency = RefCurrency.LoadFromCurrencyCode(shipnetInvoice.Factory, Constants.CurrencyCodes.UnitedStates);
				if (uSDCurrency != null)
				{
					ZDecimal totalLocalAmount = (shipnetInvoice.AH_InvoiceAmount + shipnetInvoice.AH_GSTAmount);
					CurrencyConverter converter = CurrencyConverter.New(shipnetInvoice.Factory);
					converter.DateForRate = shipnetInvoice.AH_InvoiceDate;
					Money amountInUSD = converter.ConvertExact(new Money(totalLocalAmount, Env.CurrentCompany.LocalCurrency), uSDCurrency);
					amount = amountInUSD.Amount;
					exchangeRate = converter.GetExchangeRate(uSDCurrency);

					if (shipnetInvoice.ShipmentData.IsExport())
					{
						exchangeRate = GetExchangeRate(shipnetInvoice, exchangeRate, uSDCurrency);
					}
					else if (shipnetInvoice.ShipmentData.IsImport())
					{
						exchangeRate = GetExchangeRate(shipnetInvoice, exchangeRate, uSDCurrency);
					}
					else
					{
						Buffer.Notify(new ErrorNotification(ErrorType.Error,
							Res.GetString("5e7b331c-2b90-46fc-8952-346d0cb768fd", "Invoice {0} cannot be converted exactly to {1} Currency as {2} is not Import or Export",
								shipnetInvoice.AH_TransactionNum,
								Constants.CurrencyCodes.UnitedStates,
								shipnetInvoice.ShipmentData.HumanReadableName)));
					}
					amount = Env.CurrentCompany.ExchangeRate.LocalToForeign(totalLocalAmount, exchangeRate, uSDCurrency.RX_Code);
				}
				else
				{
					Buffer.Notify(new ErrorNotification(ErrorType.Error,
						Res.GetString("a666d2c5-7622-4401-8616-f733dd5dc9b9", "Cannot find {0} Currency in system",
							Constants.CurrencyCodes.UnitedStates)));
				}
			}
		}

		ZDecimal GetExchangeRate(ShipnetARInvoice shipnetInvoice, ZDecimal exchangeRate, RefCurrency usdCurrency)
		{
			Job job = shipnetInvoice.Factory.Load<Job>(shipnetInvoice.AH_JH);

			if (job != null)
			{
				ExchangeRate[] rate = (ExchangeRate[])job.ExchangeRates.Find(new ZQuery(JobExRateSchema.JF_RX_NKRateCurrency, usdCurrency.RX_Code));

				if (rate.Length > 0)
				{
					exchangeRate = rate[0].JF_BaseRate;
				}
				else
				{
					Buffer.Notify(new ErrorNotification(ErrorType.Error,
						Res.GetString("bc541c45-1b35-4934-9822-3917c3af6758", "Invoice {0} cannot be converted exactly to {1} Currency as {2} does not have {1} exchange rate",
							shipnetInvoice.AH_TransactionNum,
							Constants.CurrencyCodes.UnitedStates,
							shipnetInvoice.ShipmentData.HumanReadableName)));
				}
			}
			return exchangeRate;
		}

		#endregion

		protected ZString GetMapCode(ZString relationship, ZGuid localGuid)
		{
			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, localGuid);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, Carrier.PK);
			return Carrier.Factory.LoadTop1<OrgPatternMatchOverride>(filter)?.OO_ForeignCode ?? ZString.Empty;
		}

		#region Settings

		protected ShipnetSetupBusinessObject Settings
		{
			get
			{
				if (fSettings == null)
				{
					ShipnetSetupBusinessObject registryBizObj = (ShipnetSetupBusinessObject)SWLDataRegistry.Instance.GetShipnetSetupBusinessObject(Carrier.CompanyData.PK);
					fSettings = (ShipnetSetupBusinessObject)registryBizObj.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Carrier.CompanyData.PK.ToGuid()), Carrier.Factory);
				}
				return fSettings;
			}
		}

		ShipnetSetupBusinessObject fSettings;

		#endregion

		protected virtual void SetRecordStatus()
		{
			ShipnetRecord header = (ShipnetRecord)Records[0];
			header.Status = "S";
			ShipnetRecord tail = (ShipnetRecord)Records[Records.Count - 1];
			tail.Status = "E";
		}

		#region FileFormat

		protected virtual FlatFileFormat FileFormat
		{
			get
			{
				if (fFileFormat == null)
				{
					fFileFormat = new CsvFlatFileFormat();
				}
				return fFileFormat;
			}
		}
		FlatFileFormat fFileFormat;

		#endregion

		void ExportCore()
		{
			try
			{
				CurrentDateTimePortion = ZDateTime.Now.ToString(DateTimeExtension);
				ExportToTempFile();
				BackupTempFile();
				DeliverTempFile();
			}
			finally
			{
				CleanUpTempFile();
			}
		}

		void DeliverTempFile()
		{
			ExportMethod method = GetExportMethod();
			if (method != null)
			{
				if (method.CanDeliver)
				{
					method.Deliver(TempFile);
					Buffer.Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "{0} record(s) generated for {1} to {2}", Records.Count, Carrier.OH_FullName, Settings.CommunicationMode.EK_Destination)));
				}
				else
				{
					Buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("23ad0966-13c0-45c3-bd9d-46f9e2eb8bb8", "There was an error encountered while delivering file {0}", TempFile)));
				}
			}
		}

		void BackupTempFile()
		{
			try
			{
				FileInfo tempFileInfo = new FileInfo(TempFile);
				if (tempFileInfo.Exists && tempFileInfo.Length > 0)
				{
					ZString carrierCode = GetMapCode(Constants.OrgPatternMatchOverrideRelationships.Organisation, Carrier.PK);
					carrierCode = (carrierCode.IsEmpty) ? Carrier.OH_Code : carrierCode;
					ZString backupDirectory = Path.Combine(SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.Value, carrierCode);
					if (!backupDirectory.IsEmpty && !Directory.Exists(backupDirectory))
					{
						Directory.CreateDirectory(backupDirectory);
					}
					ZString backupFile = Path.Combine(backupDirectory, Settings.CommunicationMode.EK_Filename + CurrentDateTimePortion + "." + Settings.CommunicationMode.EK_FileFormat);
					File.Copy(TempFile, backupFile);
				}
			}
			catch (IOException e)
			{
				Buffer.Notify(
					new ErrorNotification(
						ErrorType.IOError,
						Res.GetString("19185ce3-5bf9-47d8-a9be-f65fe9785bab", "Error encounter during backing up file: {0}", e.Message)));
			}
			catch (UnauthorizedAccessException e)
			{
				Buffer.Notify(
					new ErrorNotification(
						ErrorType.IOError,
						Res.GetString("19185ce3-5bf9-47d8-a9be-f65fe9785bab", "Error encounter during backing up file: {0}", e.Message)));
			}
		}

		ExportMethod GetExportMethod()
		{
			ExportMethod export = null;
			switch (Settings.CommunicationMode.EK_CommunicationsTransport)
			{
				case ShipnetExportCommunicationsTransportMappingList.Codes.Email:
					export = new EmailExport(GetEmailInstructions(), Buffer);
					break;
				case ShipnetExportCommunicationsTransportMappingList.Codes.File:
					export = new FileExport(GetFileInstructions(), Buffer);
					break;
				case ShipnetExportCommunicationsTransportMappingList.Codes.FTP:
					export = new FtpExport(GetFtpInstructions(), Buffer);
					break;
				default:
					Buffer.Notify(
						new ErrorNotification(
							ErrorType.UnknownCode,
							Res.GetString("b0f274d5-86ed-4b24-b6c5-1b93f27097b6", "Unknown Delivery Method for Carrier {0}.", Carrier.OH_FullNameTruncated)));
					break;
			}
			return export;
		}

		protected virtual ExportInstructions GetFtpInstructions()
		{
			ExportInstructions result = CreateNewExportInstructions();
			result.FtpProperties.ServerAddress = Settings.CommunicationMode.EK_ServerAddressSubject;
			result.FtpProperties.PortNumber = Settings.CommunicationMode.EK_PortNumber;
			result.FtpProperties.Username = Settings.CommunicationMode.EK_LoginName;
			result.FtpProperties.Password = Settings.CommunicationMode.EK_Password;
			result.FtpProperties.DestinationPath = Settings.CommunicationMode.EK_Destination;
			return result;
		}

		protected virtual ExportInstructions GetFileInstructions()
		{
			ExportInstructions result = CreateNewExportInstructions();
			result.BasePath = Settings.CommunicationMode.EK_Destination;
			return result;
		}

		protected virtual ExportInstructions GetEmailInstructions()
		{
			ExportInstructions result = CreateNewExportInstructions();
			if (!Settings.CommunicationMode.EK_Destination.IsEmpty)
			{
				result.EmailProperties.AddRecipient(Settings.CommunicationMode.EK_Destination);
			}
			result.EmailProperties.Subject = Settings.CommunicationMode.EK_ServerAddressSubject;
			return result;
		}

		protected ExportInstructions CreateNewExportInstructions()
		{
			ExportInstructions result = new ExportInstructions();
			result.SpecifiedFilename = Settings.CommunicationMode.EK_Filename + CurrentDateTimePortion;
			result.FileExtension = FileExtensionType.ClientSpecific;
			FileExtensionFilterBuilder.ClientSpecificFileExtension.Value = Settings.CommunicationMode.EK_FileFormat;
			return result;
		}

		void ExportToTempFile()
		{
			using (StreamWriter writer = File.CreateText(TempFile))
			{
				foreach (ShipnetRecord record in Records)
				{
					writer.WriteLine(FileFormat.ConvertToLine(record));
				}
				writer.Flush();
			}
		}

		void CleanUpTempFile()
		{
			if (File.Exists(TempFile))
			{
				File.Delete(TempFile);
			}
		}

		#region TempFile

		ZString TempFile
		{
			get
			{
				if (fTempFile == ZString.Empty)
				{
					fTempFile = EnvProxy.Instance.GetTempFileName();
				}
				return fTempFile;
			}
		}
		ZString fTempFile;

		#endregion

		protected virtual void SendEmailIfHasErrors()
		{
			if (Buffer.HasErrors)
			{
				Buffer.SendEmail(SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.Value, SWLDataRegistry.Instance.ShipnetNotificationEmailGroup, Res.GetString("fc72d7cb-da51-4067-b13b-8eb5c635dbf8", "Error Encountered During Exporting Shipnet Data for Carrier {0}", Carrier.OH_FullNameTruncated), "", "", Buffer.Inner);
			}
		}

		void NotifyIfUnknownChargeCodesWereFound()
		{
			if (UnknowChargeCodes.Count > 0)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("2fa8f936-509d-4a13-a97a-eedbf06e5374", "The following charge code(s) were not setup:")));
				foreach (ZString chargeCode in UnknowChargeCodes)
				{
					Buffer.Notify(new InfoNotification(chargeCode));
				}
			}
		}

		bool IsShipnetCarrier(ZGuid carrierPK)
		{
			bool result = false;
			DynamicBusinessObjectCollection shipnetCarrierPKCollection = new ShipnetCarrierFilter().GetShipnetCarrierPKCollection();
			foreach (DynamicBusinessObject shipnetCarrier in shipnetCarrierPKCollection)
			{
				if ((ZGuid)shipnetCarrier[OrgCompanyDataSchema.OB_OH] == carrierPK)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public readonly OrgHeader Carrier;
		public const string DateTimeExtension = "yyyyMMddHHmm";
		protected const string UnknownCodeString = "UNKNOWN";
		protected readonly NotificationBuffer Buffer;
		ZString CurrentDateTimePortion;

		#endregion
	}
}
