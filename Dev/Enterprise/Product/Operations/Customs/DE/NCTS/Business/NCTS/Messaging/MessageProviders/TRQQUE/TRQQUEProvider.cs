using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class TRQQUEProvider : StatusRequestHeaderProvider
	{
		public TRQQUEProvider(StatusRequest statusRequest) : base(statusRequest)
		{
		}

		public override string InterchangeRecipientID => CachedValueHelper.GetValue(ref interchangeRecipientID, () => ExportStatusRequestRecipientRegistry.CurrentAtlasMessageRecipient);
		CachedValue<ZString> interchangeRecipientID;

		public override PartyType PartyType => CachedValueHelper.GetValue(ref partyTypeCached, () =>
		{
			switch (statusRequest.Role)
			{
				case ExportStatusRequestNCTSRoleList.Codes.Consignor:
					return PartyType.NCTSConsignor;
				case ExportStatusRequestNCTSRoleList.Codes.Consignee:
					return PartyType.NCTSConsignee;
				case ExportStatusRequestNCTSRoleList.Codes.Principal:
					return PartyType.NCTSProcedureOwner;
				case ExportStatusRequestNCTSRoleList.Codes.AuthorizedConsignee:
					return PartyType.NCTSAuthorisedConsignee;
				case ExportStatusRequestNCTSRoleList.Codes.Representative:
					return PartyType.NCTSRepresentative;
				default:
					return PartyType.Unknown;
			}
		});
		CachedValue<PartyType> partyTypeCached;
	}
}
