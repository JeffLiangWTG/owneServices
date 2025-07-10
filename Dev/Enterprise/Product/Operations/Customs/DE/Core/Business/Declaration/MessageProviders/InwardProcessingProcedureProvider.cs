using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingProcedureProvider : IInwardProcessingProcedure
	{
		public static InwardProcessingProcedureProvider NeworNull(PreviousDocument warehouseProcedure) => warehouseProcedure == null ? null : new InwardProcessingProcedureProvider(warehouseProcedure);
		public InwardProcessingProcedureProvider(PreviousDocument warehouseProcedure)
		{
			this.warehouseProcedure = warehouseProcedure;
		}
		readonly PreviousDocument warehouseProcedure;

		public string AccessViaAtlasFlag => warehouseProcedure.Status.MapBoolTo10();

		public string MRN => warehouseProcedure.Status && warehouseProcedure.CSI_ReferenceNumber.Length == 18 ? warehouseProcedure.CSI_ReferenceNumber.ToString() : string.Empty;

		public string RegistrationNumber => warehouseProcedure.CSI_ReferenceNumber;

		public int ReferencedSequenceNumber => warehouseProcedure.CSI_LineNo;

		public string GoodsRelatedInformation => warehouseProcedure.CSI_Description;
	}
}
