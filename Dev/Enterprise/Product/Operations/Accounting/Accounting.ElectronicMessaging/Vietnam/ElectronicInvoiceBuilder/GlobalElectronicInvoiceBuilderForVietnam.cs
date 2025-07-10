using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Integration;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public abstract class GlobalElectronicInvoiceBuilderForVietnam : IGlobalElectronicInvoiceBuilder
	{
		protected GlobalElectronicInvoiceBuilderForVietnam(string batchNumber, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			Argument.NotNullOrEmpty(batchNumber, nameof(batchNumber));

			AdditionalTransactionInfo = additionalTransactionInfo;
			BatchNumber = batchNumber;
		}

		readonly string BatchNumber;
		protected readonly AdditionalTransactionInfoForVietnamEInvoice AdditionalTransactionInfo;

		protected abstract ZString GetPayload(INotifications notifications);
		protected abstract string SchemaResourceName { get; }
		protected abstract ZString MessageType { get; }

		public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
		{
			var errorNotifications = new Logger();
			var warningsNotifications = new Logger();

			var eInvoice = new GlobalElectronicInvoicing()
			{
				Header = new GlobalElectronicInvoicingHeader()
				{
					ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
					{
						MessagingSystem = VIETNAM_ELECTRONIC_INVOICING,
						BatchNumber = BatchNumber,
						MessageType = MessageType,
						BranchCode = BranchCode,
						CompanyCode = CompanyCode,
						IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(GEIMessageBranch.Company),
						IsProductionSystemSpecified = true,
						Credentials = GetCredential()
					}
				},
				Payload = Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(GetPayloadJSON(errorNotifications))),
				TransactionBatchSpecified = false,
			};

			return (eInvoice, errorNotifications, warningsNotifications);
		}

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialCollection GetCredential()
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialCollection();

			if (GEIMessageBranch != null)
			{
				var user = VietnamEInvoiceHelper.GetUser(GEIMessageBranch.Company.PK.ToGuid(), GEIMessageBranch.PK.ToGuid());
				AddCredential(nameof(User.Username), user.Username);
				AddCredential(nameof(User.Password), CredentialSender.EncryptPasswordAsString(user.Password), true);
			}

			return result;

			void AddCredential(ZString key, ZString value, bool valueIsEncrypted = false)
			{
				var credentialValue = valueIsEncrypted
					? new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue { Value = value, Encrypted = true, EncryptedSpecified = true }
					: new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue { Value = value };
				var credential = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential { Key = key, Value = credentialValue };

				result.Add(credential);
			}
		}

		ZString GetPayloadJSON(INotifications notifications)
		{
			var payload = GetPayload(notifications);

			if (!string.IsNullOrEmpty(payload))
			{
				ValidatePayloadJSON(payload, notifications);
			}
			return payload;
		}

		void ValidatePayloadJSON(string payload, INotifications notifications)
		{
			try
			{
				var schema = JsonSchemaLoader.Load(SchemaResourceName);
				schema.ValidateJSON(payload, notifications);
			}
			catch (JsonReaderException)
			{
				notifications.AddError(Res.GetString("D7BA080C-8DCA-43A1-B7D1-CEA48BF89ED2", "Error occurred while reading following text:\r\n{0}", payload));
				throw;
			}
		}

		protected virtual string BranchCode => GEIMessageBranch?.GB_Code;

		protected virtual string CompanyCode => GEIMessageBranch?.Company.GC_Code;

		GlbBranch GEIMessageBranch => geiMessageBranch ?? (geiMessageBranch = new ReadOnlyBusinessObjectFactory().Load<GlbBranch>(AdditionalTransactionInfo.BranchPK));
		GlbBranch geiMessageBranch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in XML file. Not a user visible text.")]
		const string VIETNAM_ELECTRONIC_INVOICING = "Vietnam electronic invoicing system";
	}
}
