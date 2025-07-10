using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public abstract class GlobalElectronicInvoiceBuilderBaseForTurkey : IGlobalElectronicInvoiceBuilder
	{
		protected GlobalElectronicInvoiceBuilderBaseForTurkey(string batchNumber, string messageType)
		{
			Argument.NotNullOrEmpty(batchNumber, nameof(batchNumber));
			Argument.NotNullOrEmpty(messageType, nameof(messageType));

			BatchNumber = batchNumber;
			MessageType = messageType;
		}

		public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
		{
			var errorsNotifications = new Logger();
			var warningsNotifications = new Logger();

			var signatureCredential = GEIMessageCompany.GetCompanySignatureCredential();
			if (signatureCredential == null)
			{
				errorsNotifications.AddError(Res.GetString("701e303c-cbb2-4361-9651-57ff163b9737", "Credential required for sending invoice is missing for company: {0}", GEIMessageCompany.GC_Code));
				return (null, errorsNotifications, warningsNotifications);
			}

			var payLoad = GetPayloadXML(errorsNotifications);
			var eInvoice = new GlobalElectronicInvoicing()
			{
				Header = new GlobalElectronicInvoicingHeader()
				{
					ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
					{
						MessagingSystem = TURKEY_ELECTRONIC_INVOICING,
						CompanyCode = GEIMessageCompany.GC_Code,
						BatchNumber = BatchNumber.PadLeft(5, '0'),
						MessageType = MessageType,
						Certificate = signatureCredential.GP_UserID,
						IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(GEIMessageCompany),
						IsProductionSystemSpecified = true,
						Password =
#if DEBUG
						ZArchitecture.Environment.Globals.IsTest ? "jZDzzR3QkksEDJ+vWzxp+/qvJ+ZXFUZ+r0BFsxkndGpu+ZCrSxAebcyrVh8UJWUNKrOjH2IGT2W5+5ngy36znD/Xk8Tjv+oYm3ol1efHhUjpa5lnrFnemPoHsWqT70jQN2+2UZp6pYF6o/ubpf5lqN04UYMT7jsNL4s9rNFZJ4I=" :
#endif
						signatureCredential.GetEncryptedPassphraseForEHub()
					},
				},
				TransactionBatchSpecified = false,
				Payload = Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(payLoad))
			};

			return (eInvoice, errorsNotifications, warningsNotifications);
		}

		protected abstract ZString GetPayloadXML(INotifications notifications);

		protected abstract GlbCompany GEIMessageCompany { get; }

		protected string MessageType { get; }

		string BatchNumber { get; }

		protected virtual BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in XML file. Not a user visible text.")]
		const string TURKEY_ELECTRONIC_INVOICING = "Turkey E-Invoicing System";
	}
}
