using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CCF15AWrapper : EU.NCTS.Business.CCF15ADeclarationWrapper, ICCF15ADeclaration
	{
		public CCF15AWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZString AgreementNumber => CachedValueHelper.GetValue(ref agreementNumber, () => DeclarationWrapperHelper.DepartureAgreementNumber(NctsHeader));
		CachedValue<ZString> agreementNumber;

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;
	}
}
