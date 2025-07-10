using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using CertificateInfo = CargoWise.Cryptoki.Common.ClientServerApi.CertificateInfo;

namespace Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning
{
	public class ESigningBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ESigningBusinessObject(IEnumerable<TransactionHeader> selectedTransactions, IInvoicingBaseToXmlConverter invoicingBaseToXmlConverter, IProgressForm progressForm, ICryptoApi cryptoApi = null)
		{
			this.selectedTransactions = selectedTransactions ?? Enumerable.Empty<TransactionHeader>();
			this.invoicingBaseToXmlConverter = invoicingBaseToXmlConverter;
			this.progressForm = progressForm;
			this.cryptoApi = cryptoApi ?? RemoteCryptoApi.Instance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string")]
		public const string WindowsToken = "WINDOWS (TST)";

		readonly IEnumerable<TransactionHeader> selectedTransactions;
		readonly IInvoicingBaseToXmlConverter invoicingBaseToXmlConverter;
		readonly IProgressForm progressForm;
		readonly ICryptoApi cryptoApi;

		public int NumOfTransactions => selectedTransactions.Count();

		[ResourceStringData("d95b1ede-3fc5-4163-a2c7-b2f45382c374", Caption = "Chipset")]
		[List("AvailableChipsetTypes")]
		public ZString ChipsetType
		{
			get
			{
				return chipsetType;
			}
			set
			{
				SetNonPersistentPropertyValue(ChipsetTypeInfo, ref chipsetType, value);
				ChipsetTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateChipsetType();
				}
				CertificateCode = null;
				RefreshAvailableCertificateCodes();
			}
		}
		ZString chipsetType;
		public ZPropertyInfo ChipsetTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ChipsetType)); }
		}
		public ReadOnlyCodeDescriptionPairList AvailableChipsetTypes
		{
			get
			{
				if (availableChipsetTypes == null)
				{
					var availableChipsets = System.Enum.GetNames(typeof(Chipset));
					if (!Environment.Env.Instance.IsProductionSystem)
					{
						availableChipsets = availableChipsets.Prepend(WindowsToken).ToArray();
					}
					availableChipsetTypes = new CodeDescriptionPairList();
					foreach (var chipset in availableChipsets)
					{
						availableChipsetTypes.AddPair(chipset);
					}
				}
				return availableChipsetTypes;
			}
		}
		CodeDescriptionPairList availableChipsetTypes;

		[ResourceStringData("b2a453ab-632f-4111-88e7-f8a5a53840fc", Caption = "Certificate")]
		[List("AvailableCertificateCodesForCurrentChipset")]
		public ZString CertificateCode
		{
			get
			{
				return certificateCode;
			}
			set
			{
				SetNonPersistentPropertyValue(CertificateCodeInfo, ref certificateCode, value);
				CertificateCodeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCertificateCode();
				}
			}
		}
		ZString certificateCode;
		public ZPropertyInfo CertificateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CertificateCode)); }
		}

		public ReadOnlyCodeDescriptionPairList AvailableCertificateCodesForCurrentChipset { get; private set; } = new ReadOnlyCodeDescriptionPairList();
		public string[] SerialNumbersForCurrentChipset = Array.Empty<string>();
		public Exception HardwareTokenErrorForCurrentChipset;

		Dictionary<string, (CodeDescriptionPairList lookup, string[] serialNumbers, Exception ex)> allAvailableCertificatesByChipset;

		[ResourceStringData("f7e5ed09-668f-4c85-8b83-2a112680b861", Caption = "Pin")]
		public ZString EnteredPin
		{
			get
			{
				return enteredPin;
			}
			set
			{
				SetNonPersistentPropertyValue(EnteredPinInfo, ref enteredPin, value);
				EnteredPinInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateEnteredPin();
				}
			}
		}
		ZString enteredPin;
		public ZPropertyInfo EnteredPinInfo
		{
			get { return GetZPropertyInfo(nameof(EnteredPin)); }
		}

		public bool IsPinRequired => !ChipsetType.Equals(WindowsToken);

		public void LazyInitialize()
		{
			CacheAvailableCertificatesByChipset();

			if (allAvailableCertificatesByChipset.Count > 0)
			{
				ChipsetType = allAvailableCertificatesByChipset.FirstOrDefault(c => c.Value.lookup != null).Key;
			}
		}

		void RefreshAvailableCertificateCodes()
		{
			if (allAvailableCertificatesByChipset == null)
			{
				CacheAvailableCertificatesByChipset();
			}
			if (!allAvailableCertificatesByChipset.TryGetValue(ChipsetType, out var certificates))
			{
				certificates = (new CodeDescriptionPairList(), Array.Empty<string>(), null);
			}
			AvailableCertificateCodesForCurrentChipset = certificates.lookup;
			SerialNumbersForCurrentChipset = certificates.serialNumbers;
			HardwareTokenErrorForCurrentChipset = certificates.ex;
			if (AvailableCertificateCodesForCurrentChipset != null && AvailableCertificateCodesForCurrentChipset.Count > 0)
			{
				CertificateCode = AvailableCertificateCodesForCurrentChipset[0].Code;
			}
		}

		void CacheAvailableCertificatesByChipset()
		{
			allAvailableCertificatesByChipset = new Dictionary<string, (CodeDescriptionPairList lookup, string[] serialNumbers, Exception ex)>();
			foreach (var chipset in AvailableChipsetTypes.GetAllCodes())
			{
				var (lookup, serialNumbers, tokenException) = GetHardwareTokenCertificateList(chipset);
				allAvailableCertificatesByChipset.Add(chipset, (lookup, serialNumbers, tokenException));
			}
		}

		(CodeDescriptionPairList lookup, string[] serialNumbers, Exception ex) GetHardwareTokenCertificateList(string chipset)
		{
			try
			{
				CertificateInfo[] certificates = null;
				if (chipset.Equals(WindowsToken))
				{
					certificates = (cryptoApi).GetCertificatesFromWindowsCertificateStore();
				}
				else if (Enum.TryParse(chipset, out Chipset chipsetEnum))
				{
					certificates = (cryptoApi).GetCertificatesFromToken(chipsetEnum);
				}
				else
				{
					throw new InvalidOperationException("Unknown chipset string");
				}

				var lookup = new CodeDescriptionPairList();
				var serialNumbers = new string[certificates.Length];
				for (int i = 0; i < certificates.Length; i++)
				{
					var certInfo = certificates[i];
					using (var cert = new X509Certificate2(certInfo.Content))
					{
						lookup.AddPair((i + 1).ToString(DefaultCulture.Instance), cert.Subject);
						serialNumbers[i] = cert.SerialNumber;
					}
				}
				return (lookup, serialNumbers, null);
			}
			catch (System.IO.IOException ex)
			{
				return (null, null, ex);
			}
			catch (CryptographicException ex)
			{
				return (null, null, ex);
			}
			catch (InvalidOperationException ex) when (ex.InnerException is InvalidOperationException)
			{
				return (null, null, ex);
			}
		}

		#region Signing

		public int SuccessCount { get; private set; }
		public int ErrorCount { get; private set; }
		public Exception Exception { get; private set; }

		public void MapAndSignAndQueueElectronicInvoices()
		{
			SuccessCount = 0;
			ErrorCount = 0;
			try
			{
				progressForm?.ShowModalTo(null);

				var newFactory = new BusinessObjectFactory();
				var transactionPKs = selectedTransactions.OfType<InvoicingBase>().Select(x => x.PK).ToArray();
				var query = new ZQuery(AccTransactionHeaderSchema.PK, transactionPKs);
				var transactions = newFactory.Load<InvoicingBase>(query);

				for (int i = 0; i < transactions.Length; i++)
				{
					var transaction = transactions[i];
					var pivot = transaction.GetMostRecentEInvoicingTransactionPivot();
					if (pivot == null || transaction.EInvoicingStatus != EInvoicingPivotState.Pending)
					{
						continue;
					}

					var percent = ((double)i / transactions.Length) * 100.0;
					progressForm?.SetStatusAndPercentComplete(Res.GetString("32f371db-eeb0-468d-9e77-2f6e17c9f215", "Processing transaction {0:N0} of {1:N0}.", i + 1, transactions.Length), (int)percent);

					var notifications = new NotificationBuffer();
					var (mappingSuccessful, mappedXml) = invoicingBaseToXmlConverter.ConvertToXml(transaction, notifications);

					if (!mappingSuccessful)
					{
						pivot.AIP_Status = EInvoicingPivotState.Failed;
						var errorMessage = notifications.AsString;
						if (errorMessage.Length > 300)
						{
							errorMessage = new ZString(errorMessage).Truncate(300);
						}
						pivot.AIP_ErrorDescription = errorMessage;
						++ErrorCount;
					}
					else
					{
						var certificateIndex = AvailableCertificateCodesForCurrentChipset?.IndexOfCode(CertificateCode);
						if (certificateIndex == null || certificateIndex == -1)
						{
							throw new InvalidOperationException($"Could not find valid Certificate Serial number with code: {CertificateCode}");
						}
						var certificateSerialNumber = SerialNumbersForCurrentChipset[certificateIndex.Value];
						var certificateSerialBytes = AccountingUtils.ParseHex(certificateSerialNumber);

						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(mappedXml);
						ISignatureAlgorithm algorithm;

						if (ChipsetType.Equals(WindowsToken))
						{
							algorithm = X509Certificate2CryptoApiSignatureAlgorithm.Create(cryptoApi, certificateSerialBytes);
						}
						else if (Enum.TryParse(ChipsetType, out Chipset chipsetEnum))
						{
							algorithm = Pkcs11CryptoApiSignatureAlgorithm.Create(cryptoApi, chipsetEnum, EnteredPin, certificateSerialBytes);
						}
						else
						{
							throw new InvalidOperationException($"ChipsetType: {ChipsetType} is not a recognized value");
						}

						var signatureBuilderProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(transaction.Company.Country.Code) as IEInvoicingSignatureBuilderProvider ?? throw new InvalidOperationException($"Cannot find a signature builder provider for {transaction.Company.Country.Code}");

						var signatureBuilder = signatureBuilderProvider.GetXmlDocumentSignatureBuilder(algorithm, utcNowGetter: ZDateTime.UtcNow.ToDateTime);
						signatureBuilder.SignDocument(xmlDoc);
						var authorisationRecordType = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceInfoElectronicInvoicing(transaction.Company.Country.Code).GetAccTransactionHeaderAuthorisationRecordType();
						var authRecord = GetOrCreateAuthorisationRecord(transaction, authorisationRecordType);
						authRecord.AHF_AuthorisationData = ZBlob.FromUTF8(xmlDoc.OuterXml);

						pivot.AIP_ErrorDescription = ZString.Empty;
						pivot.AIP_Status = EInvoicingPivotState.Queued;
						++SuccessCount;
					}
				}

				progressForm?.SetStatusAndPercentComplete(Res.GetString("4c201c30-0097-46ac-98b6-6267ed879f5e", "Saving results..."), 100);
				newFactory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				this.Exception = ex;
			}
		}

		static AccTransactionHeaderAuthorisationRecord GetOrCreateAuthorisationRecord(TransactionHeader transactionHeader, ZString authRecordType)
		{
			var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, transactionHeader.PK);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, authRecordType);
			var authRecord = transactionHeader.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(query);
			if (authRecord == null)
			{
				authRecord = transactionHeader.Factory.New<AccTransactionHeaderAuthorisationRecord>();
				authRecord.AHF_ParentId = transactionHeader.PK;
				authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authRecord.AHF_RecordType = authRecordType;
			}
			return authRecord;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public ESigningValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ESigningValidation GetNewValidation()
		{
			return new ESigningValidation(this);
		}

		#endregion

	}
}
