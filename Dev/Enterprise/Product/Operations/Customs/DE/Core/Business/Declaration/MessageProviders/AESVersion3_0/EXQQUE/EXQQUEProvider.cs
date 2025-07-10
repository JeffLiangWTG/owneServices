using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXQQUEProvider : StatusRequestHeaderProvider
	{
		public EXQQUEProvider(StatusRequest statusRequest) : base(statusRequest)
		{
		}

		public override string InterchangeRecipientID => CachedValueHelper.GetValue(ref interchangeRecipientID, () => ExportStatusRequestRecipientRegistry.CurrentAESMessageRecipient);
		CachedValue<string> interchangeRecipientID;

		public override PartyType PartyType => CachedValueHelper.GetValue(ref partyTypeCached, () =>
		{
			switch (statusRequest.Role)
			{
				case ExportStatusRequestAESRoleList.Codes.Declarant:
					return PartyType.AESDeclarant;
				case ExportStatusRequestAESRoleList.Codes.Representative:
					return PartyType.AESRepresentative;
				case ExportStatusRequestAESRoleList.Codes.Exporter:
					return PartyType.AESExporter;
				case ExportStatusRequestAESRoleList.Codes.Subcontractor:
					return PartyType.AESContractor;
				default:
					return PartyType.Unknown;
			}
		});
		CachedValue<PartyType> partyTypeCached;
	}
}
