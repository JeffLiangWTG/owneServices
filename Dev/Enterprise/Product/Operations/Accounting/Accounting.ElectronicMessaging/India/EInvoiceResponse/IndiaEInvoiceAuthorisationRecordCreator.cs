using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.India;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	// Code will be removed in WI00829724 once we are confident all customers support new style XUE messages

	public class IndiaEInvoiceAuthorisationRecordCreator
	{
		public IndiaEInvoiceAuthorisationRecordCreator(IndiaEInvoiceResponse response, ZGuid transactionPK)
		{
			Response = Argument.NotNull(response, nameof(response));
			Argument.NotNull(response.AckNo, "response.AckNo");
			Argument.NotNullOrEmpty(response.AckDateTime, "response.AckDateTime");
			Argument.NotNullOrEmpty(response.Irn, "response.Irn");
			Argument.NotNullOrEmpty(response.SignedInvoiceAsJwt, "response.SignedInvoiceAsJwt");
			Argument.NotNullOrEmpty(response.SignedQRCodeAsJwt, "response.SignedQRCodeAsJwt");

			TransactionPK = transactionPK;
		}

		IndiaEInvoiceResponse Response { get; }
		ZGuid TransactionPK { get; }
		readonly TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById((NoResString)"India Standard Time");     // IST timezone, as defined by Windows.

		public IndiaAccTransactionHeaderAuthorisationRecord Create(BusinessObjectFactory factory)
		{
			var authorizationRecord = factory.New<IndiaAccTransactionHeaderAuthorisationRecord>();

			authorizationRecord.AHF_Counter = Response.AckNo.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

			if (!ZDateTime.TryParseExact(Response.AckDateTime, out var localDatestampInIST, (NoResString)"yyyy-MM-dd HH:mm:ss")) // Date parsing format string based on external schema
			{
				throw new FormatException($"Unable to parse 'AckDateTime' field: '{Response.AckDateTime}'");        // Developer only error
			}
			localDatestampInIST = new ZDateTime(localDatestampInIST, DateTimeKind.Local);
			var datestampWithOffset = new ZDateTimeOffset(localDatestampInIST, DateTimeKind.Local, IST.GetUtcOffset(localDatestampInIST.ToDateTime()));
			authorizationRecord.AHF_DateTime = datestampWithOffset;

			authorizationRecord.AHF_Number = Response.Irn;
			if (!string.IsNullOrEmpty(Response.SignedInvoiceAsJwt))
			{
				authorizationRecord.AHF_AuthorisationData = ZBlob.FromAscii(Response.SignedInvoiceAsJwt);
			}
			if (!string.IsNullOrEmpty(Response.SignedQRCodeAsJwt))
			{
				authorizationRecord.AHF_VerificationUrl = Response.SignedQRCodeAsJwt;
			}

			authorizationRecord.AHF_ParentId = TransactionPK;
			authorizationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authorizationRecord.AHF_PublicKey = GetIndiaPublicKey();

			return authorizationRecord;
		}

		// Read this from actual messages, OR put it in registry.
		ZBlob GetIndiaPublicKey() => new ZBlob(Convert.FromBase64String("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArxd93uLDs8HTPqcSPpxZrf0Dc29r3iPp0a8filjAyeX4RAH6lWm9qFt26CcE8ESYtmo1sVtswvs7VH4Bjg/FDlRpd+MnAlXuxChij8/vjyAwE71ucMrmZhxM8rOSfPML8fniZ8trr3I4R2o4xWh6no/xTUtZ02/yUEXbphw3DEuefzHEQnEF+quGji9pvGnPO6Krmnri9H4WPY0ysPQQQd82bUZCk9XdhSZcW/am8wBulYokITRMVHlbRXqu1pOFmQMO5oSpyZU3pXbsx+OxIOc4EDX0WMa9aH4+snt18WAXVGwF2B4fmBk7AtmkFzrTmbpmyVqA3KO2IjzMZPw0hQIDAQAB")); // Hard coded external public key
	}
}
