using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class GlobalElectronicInvoiceBuilderForTaxCore : IGlobalElectronicInvoiceBuilder
	{
		public GlobalElectronicInvoiceBuilderForTaxCore(ZString batchNumber, UniversalTransactionBatch transactionBatch, TransactionHeader transactionHeader, ITaxCoreCountryEInvoicingObjectFactory countryFactory)
		{
			Argument.NotNull(countryFactory, nameof(countryFactory));
			Argument.NotNull(transactionBatch, nameof(transactionBatch));
			Argument.NotNullOrEmpty(batchNumber, nameof(batchNumber));
			Argument.NotNull(transactionHeader, nameof(transactionHeader));

			BatchNumber = batchNumber;
			if (transactionBatch.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException("Each transaction batch can have only one transaction in order to generate a Electronic Invoice.");
			}
			else
			{
				UniversalTransaction = transactionBatch.TransactionCollection[0];
			}
			Transaction = transactionHeader;
			CountryFactory = countryFactory;
		}

		public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
		{
			var errorNotifications = new Logger();
			var warningNotifications = new Logger();

			var certificateCredential = GetCertificateCredential(errorNotifications);

			if (certificateCredential != null)
			{
				var eInvoice = new GlobalElectronicInvoicing()
				{
					Header = new GlobalElectronicInvoicingHeader()
					{
						ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
						{
							MessagingSystem = CountryFactory.InvoicingSystemName,
							BatchNumber = BatchNumber,
							MessageType = "REQ",
							Certificate = certificateCredential.GetCertificateAsBase64String(),
							IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(TransactionBranch.Company),
							IsProductionSystemSpecified = true,
							Password = certificateCredential.GetEncryptedPassphraseForEHub(),
							Credentials = certificateCredential.GetCredentials(),
						}
					},
					Payload = GetPayload(errorNotifications, certificateCredential),
					TransactionBatchSpecified = false,
				};

				return (eInvoice, errorNotifications, warningNotifications);
			}

			return (null, errorNotifications, warningNotifications);
		}

		string GetPayload(INotifications notifications, EInvoicingCertificateCredential certificateCredential)
		{
			var invoiceCreator = CountryFactory.GetInvoiceCreator();
			var payloadInvoice = invoiceCreator.Create(new TaxCoreEInvoiceCreatorParameter
			{
				UniversalTransaction = UniversalTransaction,
				CreateUser = CreateUser,
				CertificateCredential = certificateCredential,
				AdditionalTransactionInfo = CountryFactory.GetAdditionalInfoFromTransactionHeader(Transaction),
				Notifications = notifications,
				TaxFileCode = CountryFactory.TaxFileCode,
				TFNCode = CountryFactory.TFNCode,
				CountryCode = CountryFactory.CountryCode,
				Branch = TransactionBranch
			});

			var jsonPayload = payloadInvoice.GetJSONPayload();
			return Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(jsonPayload));
		}

		EInvoicingCertificateCredential GetCertificateCredential(Logger errorNotifications)
		{
			var certificateCredential = EInvoicingCertificateCredential.LoadBestCertificate(TransactionBranch,
										currentTime: TransactionBranch?.HomePort?.LocationDateTime ?? ZDateTime.MinSmallDateTimeValue,
										passwordType: PasswordTypesList.Codes.FPC,
										orderByColumn: GlbExternalPasswordSchema.GP_ExpiryDate.Name
									);
			if (certificateCredential == null)
			{
				errorNotifications.AddError(Res.GetString("d203a28b-fe11-4e29-99b9-ad44be843ef5", "Credential required for sending invoice is missing for branch: {0}", TransactionBranch.GB_Code));
			}
			return certificateCredential;
		}

		GlbBranch TransactionBranch => transactionBranch ??= new ReadOnlyBusinessObjectFactory().LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, UniversalTransaction.Branch.Code ?? string.Empty);
		GlbBranch transactionBranch;

		GlbStaff CreateUser => createUser ??= Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, UniversalTransaction.CreateUser.Code));
		GlbStaff createUser;

		ITaxCoreCountryEInvoicingObjectFactory CountryFactory { get; }

		TransactionInfo UniversalTransaction { get; }

		TransactionHeader Transaction { get; }

		ZString BatchNumber { get; }

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;
	}
}
