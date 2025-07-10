using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DEPDATSummaryDeclarationReferenceProvider : IDEPDATSummaryDeclarationReference
	{
		public static DEPDATSummaryDeclarationReferenceProvider NewOrNull(NctsPreviousDocument previousProcedure) => previousProcedure != null ? new DEPDATSummaryDeclarationReferenceProvider(previousProcedure) : null;

		DEPDATSummaryDeclarationReferenceProvider(NctsPreviousDocument previousProcedure)
		{
			this.previousProcedure = Argument.NotNull(previousProcedure, nameof(previousProcedure));
		}

		public bool IsIdentificationByKey => previousProcedure.CSI_SubType == PreviousDocSubTypeList.Codes.AWB || previousProcedure.CSI_SubType == PreviousDocSubTypeList.Codes.ULD;

		public bool IsIdentificationByRegistrationNumber => previousProcedure.CSI_SubType == PreviousDocSubTypeList.Codes.REG;

		public string Type => previousProcedure.CSI_SubType;

		public string ReferenceNumber => previousProcedure.CSI_ReferenceNumber;

		public string IdentificationNumber => previousProcedure.CSI_ReferenceNumber2;

		public string MRN => NCTSProviderHelpers.IsDEMRN(previousProcedure.CSI_ReferenceNumber) ? (string)previousProcedure.CSI_ReferenceNumber : null;

		public string RegistrationNumber => !NCTSProviderHelpers.IsDEMRN(previousProcedure.CSI_ReferenceNumber) ? (string)previousProcedure.CSI_ReferenceNumber : null;

		public int GoodsItemNumber => previousProcedure.CSI_LineNo;

		public int NumberOfPackages => (int)previousProcedure.CSI_Quantity;

		readonly NctsPreviousDocument previousProcedure;
	}
}
