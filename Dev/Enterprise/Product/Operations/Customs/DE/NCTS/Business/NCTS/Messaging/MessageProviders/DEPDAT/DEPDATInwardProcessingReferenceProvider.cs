using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DEPDATInwardProcessingReferenceProvider : IDEPDATInwardProcessingReference
	{
		public static DEPDATInwardProcessingReferenceProvider NewOrNull(NctsPreviousDocument previousProcedure) => previousProcedure != null ? new DEPDATInwardProcessingReferenceProvider(previousProcedure) : null;

		DEPDATInwardProcessingReferenceProvider(NctsPreviousDocument previousProcedure)
		{
			this.previousProcedure = Argument.NotNull(previousProcedure, nameof(previousProcedure));
		}

		public bool AccessViaATLAS => previousProcedure.Status;

		public string MRN => NCTSProviderHelpers.IsDEMRN(previousProcedure.CSI_ReferenceNumber) ? (string)previousProcedure.CSI_ReferenceNumber : null;

		public string RegistrationNumber => !NCTSProviderHelpers.IsDEMRN(previousProcedure.CSI_ReferenceNumber) ? (string)previousProcedure.CSI_ReferenceNumber : null;

		public int GoodsItemNumber => previousProcedure.CSI_LineNo;

		public string GoodsRelatedData => previousProcedure.CSI_Description;

		readonly NctsPreviousDocument previousProcedure;
	}
}
