using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Business
{
	sealed class ApprovalCertificateProvider : IApprovalCertificate
	{
		public ApprovalCertificateProvider(ApprovalCertificateInfo certificate)
		{
			Argument.NotNull(certificate, nameof(certificate));
			this.approvalCertificate = certificate;
		}

		readonly ApprovalCertificateInfo approvalCertificate;

		public string Type => approvalCertificate.CSI_Code;

		public string Number => approvalCertificate.CSI_ReferenceNumber;
	}
}
