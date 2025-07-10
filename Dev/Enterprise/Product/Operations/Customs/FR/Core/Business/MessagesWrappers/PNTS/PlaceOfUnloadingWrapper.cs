using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class PlaceOfUnloadingWrapper : IPlaceOfUnloading
	{
		PlaceOfUnloadingWrapper(TemporaryStorageHeader temporaryStorageHeader)
		{
			this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		}
		readonly TemporaryStorageHeader temporaryStorageHeader;

		public string UnLoCode => unLoCode ?? (unLoCode = temporaryStorageHeader.MasterBill?.ABL_RL_NKPortOfDischarge);
		string unLoCode;

		public static PlaceOfUnloadingWrapper New(TemporaryStorageHeader temporaryStorageHeader) => temporaryStorageHeader == null ? null : new PlaceOfUnloadingWrapper(temporaryStorageHeader);
	}
}
