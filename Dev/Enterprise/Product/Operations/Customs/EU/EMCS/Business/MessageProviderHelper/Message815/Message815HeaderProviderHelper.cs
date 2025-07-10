using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message815HeaderProviderHelper : HeaderProviderHelper
	{
		public Message815HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration)
		{
		}

		public string JourneyTime => string.Concat(emcsJobDeclaration.JourneyTimeFormatPart, emcsJobDeclaration.JourneyTimeNumericPart.ToString().PadLeft(2, '0'));

		public string TransportModeCode => new TransportModeTranslator().TranslateToWCOCode(emcsJobDeclaration.JE_TransportMode);

		public DateTime? InvoiceDate => emcsJobDeclaration.InvoiceDate.ToNullableDateTime();

		public string DestinationTypeCode => emcsJobDeclaration.JE_MessageSubType;

		public string GuarantorType => emcsJobDeclaration.ZG_GuarantorType;

		public string DispatchImportOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDispatch);

		public string CompetentAuthorityDispatchOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch);

		public string ComplementConsigneeMemberStateCode => emcsJobDeclaration.ZG_CCTMSA;

		public string ComplementConsigneeSerialNumberOfCertificateOfExemption => emcsJobDeclaration.ZG_CertOfExemption;

		public string DeferredSubmissionFlag => emcsJobDeclaration.ZG_DeferredSubmission;

		public string SubmissionMessageType => emcsJobDeclaration.ZG_SubmissionType;

		public string TransportArrangement => emcsJobDeclaration.ZG_TransportArrangement;

		public DateTime? DispatchDateTime => emcsJobDeclaration.JE_DateAtOrigin.ToNullableDateTime();

		public string OriginType => emcsJobDeclaration.ZG_OriginType;

		public string LocalReferenceNumber => emcsJobDeclaration.JE_OwnerRef;

		public IReadOnlyCollection<string> ImportSadNumbers => importSadNumbers ?? (importSadNumbers = emcsJobDeclaration.ImportSADNumbers.Cast<ImportSADNumber>().Select(x => x.CSI_Description.ToString()).ToArray());
		IReadOnlyCollection<string> importSadNumbers;

		public string DeliveryPlaceCustomsOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDelivery);

		public JobDocAddress GetJobDocAddressWithFallback(JobDocAddress jobDocAddress, JobDocAddress fallbackJobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? jobDocAddress : fallbackJobDocAddress;
	}
}
