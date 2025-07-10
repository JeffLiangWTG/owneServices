using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TS313DeclarationTypeProvider : TS313AndTS315DeclarationTypeProvider, ITS313DeclarationType
	{
		public TS313DeclarationTypeProvider(TemporaryStorageMessageSendingObject sendingObject) : base(sendingObject.Header)
		{
			this.sendingObject = sendingObject;
		}
		protected readonly TemporaryStorageMessageSendingObject sendingObject;

		public string MRN => header.MRN;

		public string Remarks => sendingObject.CustomsJustification;
	}
}
