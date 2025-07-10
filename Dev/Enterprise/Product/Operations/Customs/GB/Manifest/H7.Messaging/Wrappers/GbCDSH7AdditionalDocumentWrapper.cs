using System;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class GbCDSH7AdditionalDocumentWrapper : IAdditionalDocument
	{
		public GbCDSH7AdditionalDocumentWrapper(SupportingDocument supportingDocument)
		{
			this.document = supportingDocument ?? throw new ArgumentNullException(nameof(supportingDocument));
		}

		protected readonly SupportingDocument document;

		public ZString CategoryCode => document.CSI_Code.SubstringSafe(0, 1);

		public ZString TypeCode => document.CSI_Code.SubstringSafe(1);

		public ZString ID => document.CSI_ReferenceNumber;

		public ZString LPCOExemptionCode => document.CSI_Status;

		public ZString Name => document.CSI_Description.StripNewlineCharacters(CDSDataElementsLengths.AdditionalDocumentNameMaxLength);

		public ZDateTime EffectiveDateTime => document.CSI_DateOfIssue;

		public ZString Submitter => document.CSI_ReferenceNumber2.StripNewlineCharacters(CDSDataElementsLengths.AdditionalDocumentSubmitterNameMaxLength);

		public IWriteOff WriteOff => null;

		public ZDateTime SystemCreateTime => document.CSI_SystemCreateTimeUtc;
	}
}
