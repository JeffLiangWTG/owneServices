using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC007ADeclarationWrapper : EU.NCTS.Business.CC007ADeclarationWrapper, ICC007ADeclaration
	{
		public CC007ADeclarationWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZString AgreementNumber => CachedValueHelper.GetValue(ref agreementNumber, () => DeclarationWrapperHelper.ArrivalAgreementNumber(NctsHeader));
		CachedValue<ZString> agreementNumber;

		protected override ZString ArrivalNotificationPlaceCore => nctsHeader.ArrivalMovementHeader?.BM_PlaceOfUnloading ?? ZString.Empty;

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;
	}
}
