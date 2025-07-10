using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportLicense
{
	public class ConsentingProcessProvider : IConsentingProcess
	{
		public ConsentingProcessProvider(ConsentingProcess consentingProcess)
		{
			this.consentingProcess = Argument.NotNull(consentingProcess, nameof(consentingProcess));
		}

		public static ConsentingProcessProvider New(ConsentingProcess consentingProcess) => consentingProcess == null ? null : new ConsentingProcessProvider(consentingProcess);

		readonly ConsentingProcess consentingProcess;

		public string ReferenceNumber => consentingProcess.CSI_ReferenceNumber;

		public string ReferenceType => consentingProcess.CSI_CustomsOffice;
	}
}
