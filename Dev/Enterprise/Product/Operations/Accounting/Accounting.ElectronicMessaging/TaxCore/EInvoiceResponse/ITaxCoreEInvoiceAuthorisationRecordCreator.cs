using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.TaxCore;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreEInvoiceAuthorisationRecordCreator
	{
		TaxCoreAccTransactionHeaderAuthorisationRecord Create(BusinessObjectFactory factory, TaxCoreEInvoiceResponse response, ZGuid transactionPK);
	}
	public abstract class TaxCoreEInvoiceAuthorisationRecordCreator : ITaxCoreEInvoiceAuthorisationRecordCreator
	{
		protected TaxCoreEInvoiceAuthorisationRecordCreator()
		{ }

		TaxCoreAccTransactionHeaderAuthorisationRecord ITaxCoreEInvoiceAuthorisationRecordCreator.Create(BusinessObjectFactory factory, TaxCoreEInvoiceResponse response, ZGuid transactionPK) => Create(factory, response, transactionPK);

		protected virtual TaxCoreAccTransactionHeaderAuthorisationRecord Create(BusinessObjectFactory factory, TaxCoreEInvoiceResponse response, ZGuid transactionPK)
		{
			Argument.NotNull(response, "response");
			var authorizationRecord = GetNewAuthorisationRecordInstance(factory);

			//TODO: Need to write unit tests
			if (DateTimeOffset.TryParse(response.DT, out var dateTime))
			{
				authorizationRecord.AHF_DateTime = new ZDateTimeOffset(dateTime);
			}
			else
			{
				throw new InvalidZDateTimeResultException(FormattableString.Invariant($"Failed to set {nameof(authorizationRecord.AHF_DateTime)}, as '{response.DT}' cannot be converted to a DateTimeOffset value."));
			}

			authorizationRecord.AHF_Counter = response.InvoiceCounter;
			authorizationRecord.AHF_Number = response.InvoiceNumber;
			authorizationRecord.AHF_VerificationUrl = response.VerificationUrl;
			if (!string.IsNullOrEmpty(response.InternalData))
			{
				authorizationRecord.AHF_AuthorisationData = ZBlob.FromUTF8(response.InternalData);
			}
			if (!string.IsNullOrEmpty(response.Signature))
			{
				authorizationRecord.AHF_PublicKey = ZBlob.FromUTF8(response.Signature);
			}
			if (!string.IsNullOrEmpty(response.Hash))
			{
				authorizationRecord.AHF_ITransactionHash = ZBlob.FromUTF8(response.Hash);
			}
			if (!string.IsNullOrEmpty(response.TIN))
			{
				authorizationRecord.AHF_IDType = OrgCusCode.CodeTypes.TaxFileCode;
				authorizationRecord.AHF_IDNumber = response.TIN;
			}
			authorizationRecord.BusinessName = response.BusinessName;
			authorizationRecord.LocationName = response.LocationName;
			authorizationRecord.Address = response.Address;
			authorizationRecord.District = response.District;
			authorizationRecord.AHF_ParentId = transactionPK;
			authorizationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			return authorizationRecord;
		}

		protected abstract TaxCoreAccTransactionHeaderAuthorisationRecord GetNewAuthorisationRecordInstance(BusinessObjectFactory factory);
	}
}
