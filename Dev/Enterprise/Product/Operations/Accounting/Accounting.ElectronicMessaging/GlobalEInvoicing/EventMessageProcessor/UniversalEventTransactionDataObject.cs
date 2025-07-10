using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Newtonsoft.Json;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	/// <summary>
	/// Data transfer object which represents all settable fields for AR/AP transaction e-Invoicing responses.
	/// </summary>
	public class UniversalEventTransactionDataObject
	{
		// 🚩🚩🚩
		// All changes to XUE structure must be compatible with XUE wiki pages.
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
		// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
		// 🚩🚩🚩

		// BEGIN fields to identify a transaction within the batch.

		[JsonProperty(PropertyName = "Tldg")]
		public string TransactionLedger { get; set; }

		[JsonProperty(PropertyName = "Ttyp")]
		public string TransactionType { get; set; }

		[JsonProperty(PropertyName = "Torg")]
		public string TransactionOrgHeaderCode { get; set; }

		[JsonProperty(PropertyName = "Tnum")]
		public string TransactionNumber { get; set; }

		// END fields to identify a transaction within the batch.

		[JsonProperty(PropertyName = "St")]
		public string PivotStatus { get; set; }

		[JsonProperty(PropertyName = "Rsn")]
		public string FailureReason { get; set; }

		// BEGIN setters

		[JsonProperty(PropertyName = "GId")]
		public string GovernmentAllocatedId { get; set; }    // AH_GovernmentAllocatedID

		[JsonProperty(PropertyName = "CNum")]
		public string ComplianceNumber { get; set; }    // AH_TransactionReference

		[JsonProperty(PropertyName = "CDate")]
		public string ComplianceDate { get; set; }    // AH_ComplianceDocumentDate

		[JsonProperty(PropertyName = "CSubType")]
		public string ComplianceSubType { get; set; }    // AH_ComplianceSubType

		[JsonProperty(PropertyName = "CDocumentStatus")]
		public string ComplianceDocumentStatus { get; set; }    // AccTransactionHeaderReference.ECN

		[JsonProperty(PropertyName = "VoidedAndCreditedAmountForCN")]
		public string VoidedAndCreditedAmountForCN { get; set; }    // AccTransactionHeaderReference.ECN

		[JsonProperty(PropertyName = "Num")]
		public string Number { get; set; }      // AHF_Number

		[JsonProperty(PropertyName = "Ctr")]
		public string Counter { get; set; }     // AHF_Counter

		[JsonProperty(PropertyName = "Ityp")]
		public string IDType { get; set; }      // AHF_IDType

		[JsonProperty(PropertyName = "Inum")]
		public string IDNumber { get; set; }    // AHF_IDNumber

		[JsonProperty(PropertyName = "Dt")]
		public string DateTime { get; set; }    // AHF_DateTime

		[JsonProperty(PropertyName = "Vurl")]
		public string VerificationUrl { get; set; } // AHF_VerificationUrl

		[JsonProperty(PropertyName = "Pky")]
		public string PublicKey { get; set; }   // AHF_PublicKey

		[JsonProperty(PropertyName = "Adt")]
		public string AuthorisationData { get; set; }   // AHF_AuthorisationData

		[JsonProperty(PropertyName = "Hsh")]
		public string ITransactionHash { get; set; }    // AHF_ITransactionHash

		[JsonIgnore]
		public string IssuerCertificateIdentifier { get; set; }    // AHF_IssuerCertificateIdentifier

		[JsonIgnore]
		public string IssuerAuthorizationData { get; set; }    // AHF_IssuerAuthorizationData

		[JsonIgnore]
		public string DebtorNumber { get; set; }    // AHF_DebtorNumber

		[JsonIgnore]
		public string PlaceOfIssue { get; set; }    // AHF_PlaceOfIssue

		// END setters

		// 🚩🚩🚩
		// All changes to XUE structure must be compatible with XUE wiki pages.
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
		// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
		// 🚩🚩🚩

		[JsonIgnore]
		public bool HasAnyFieldsForAuthorisationRecord
			=> !string.IsNullOrEmpty(Counter)
			|| !string.IsNullOrEmpty(Number)
			|| !string.IsNullOrEmpty(IDType)
			|| !string.IsNullOrEmpty(IDNumber)
			|| !string.IsNullOrEmpty(DateTime)
			|| !string.IsNullOrEmpty(VerificationUrl)
			|| !string.IsNullOrEmpty(PublicKey)
			|| !string.IsNullOrEmpty(AuthorisationData)
			|| !string.IsNullOrEmpty(ITransactionHash)
			|| !string.IsNullOrEmpty(IssuerCertificateIdentifier)
			|| !string.IsNullOrEmpty(IssuerAuthorizationData)
			|| !string.IsNullOrEmpty(DebtorNumber)
			|| !string.IsNullOrEmpty(PlaceOfIssue);

		internal static UniversalEventTransactionDataObject FromUniversalEvent(UniversalEvent universalEvent)
		{
			Argument.NotNull(universalEvent, nameof(universalEvent));

			var errorMessage = universalEvent.EventParameters?.Reason ?? string.Empty;
			var eventType = universalEvent.EventType ?? string.Empty;
			var contextCollection = GetContextCollectionSafe(universalEvent);

			if (!contextCollection.TryGetValue(EventContextTypeCode.AIP_Status, out var pivotStatus))
			{
				pivotStatus = eventType == AutoEvents.InterchangeAcknowledgedCode ? EInvoicingPivotState.Succeed : EInvoicingPivotState.Failed;
			}

			var result = new UniversalEventTransactionDataObject()
			{
				PivotStatus = pivotStatus,
				FailureReason = errorMessage,
			};

			// 🚩🚩🚩
			// All changes to XUE structure must be compatible with XUE wiki pages.
			// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
			// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
			// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
			// 🚩🚩🚩

			SetPropertyValue(v => result.GovernmentAllocatedId = v, EventContextTypeCode.AH_GovernmentAllocatedID);
			SetPropertyValue(v => result.ComplianceNumber = v, EventContextTypeCode.AH_TransactionReference);
			SetPropertyValue(v => result.ComplianceDate = v, EventContextTypeCode.AH_ComplianceDocumentDate);
			SetPropertyValue(v => result.ComplianceSubType = v, EventContextTypeCode.AH_ComplianceSubType);
			SetPropertyValue(v => result.ComplianceDocumentStatus = v, EventContextTypeCode.AH1_ComplianceDocumentStatus);
			SetPropertyValue(v => result.VoidedAndCreditedAmountForCN = v, EventContextTypeCode.AH1_CN_VoidedAndCreditedAmount);
			SetPropertyValue(v => result.AuthorisationData = v, EventContextTypeCode.AHF_AuthorisationData);
			SetPropertyValue(v => result.Counter = v, EventContextTypeCode.AHF_Counter);
			SetPropertyValue(v => result.DateTime = v, EventContextTypeCode.AHF_DateTime);
			SetPropertyValue(v => result.IDNumber = v, EventContextTypeCode.AHF_IDNumber);
			SetPropertyValue(v => result.IDType = v, EventContextTypeCode.AHF_IDType);
			SetPropertyValue(v => result.ITransactionHash = v, EventContextTypeCode.AHF_ITransactionHash);
			SetPropertyValue(v => result.Number = v, EventContextTypeCode.AHF_Number);
			SetPropertyValue(v => result.PublicKey = v, EventContextTypeCode.AHF_PublicKey);
			SetPropertyValue(v => result.VerificationUrl = v, EventContextTypeCode.AHF_VerificationUrl);
			SetPropertyValue(v => result.IssuerCertificateIdentifier = v, EventContextTypeCode.AHF_IssuerCertificateIdentifier);
			SetPropertyValue(v => result.IssuerAuthorizationData = v, EventContextTypeCode.AHF_IssuerAuthorizationData);
			SetPropertyValue(v => result.DebtorNumber = v, EventContextTypeCode.AHF_DebtorNumber);
			SetPropertyValue(v => result.PlaceOfIssue = v, EventContextTypeCode.AHF_PlaceOfIssue);

			return result;

			#region SetPropertyValue Helper

			void SetPropertyValue(Action<string> setValue, string contextTypeCode)
			{
				if (contextCollection.TryGetValue(contextTypeCode, out var r))
				{
					setValue(r);
				}
			}

			#endregion SetPropertyValue
		}

		internal static IReadOnlyCollection<UniversalEventTransactionDataObject> FromJsonInContextCollection(UniversalEvent universalEvent, out bool jsonDeserialisationFailed)
		{
			Argument.NotNull(universalEvent, nameof(universalEvent));
			jsonDeserialisationFailed = false;

			var batchResponseContexts = (universalEvent.ContextCollection ?? Enumerable.Empty<Context>()).Where(ctx => ctx.Type == EventContextTypeCode.BatchResponseObject);
			var result = new List<UniversalEventTransactionDataObject>(batchResponseContexts.Count());
			foreach (var ctx in batchResponseContexts)
			{
				try
				{
					var dto = JsonConvert.DeserializeObject<UniversalEventTransactionDataObject>(ctx.Value);
					result.Add(dto);
				}
				catch (JsonException)
				{
					jsonDeserialisationFailed = true;
				}
			}
			return result;
		}

		internal static IReadOnlyDictionary<ZString, ZString> GetContextCollectionSafe(UniversalEvent universalEvent)
			=> (universalEvent.ContextCollection ?? Enumerable.Empty<Context>())
					.Where(c => c.Type.Type.HasValue)
					.ToLookup(k => k.Type.Type.Value, v => v.Value ?? ZString.Empty)
					.Where(x => x.Count() == 1)
					.ToDictionary(x => x.Key, x => x.Single());
	}
}
