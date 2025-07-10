using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC141AWrapper : EU.NCTS.Business.CC141ADeclarationWrapper, ICC141ADeclaration
	{
		public CC141AWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZString AgreementNumber => CachedValueHelper.GetValue(ref agreementNumber, () => DeclarationWrapperHelper.DepartureAgreementNumber(NctsHeader));
		CachedValue<ZString> agreementNumber;

		public ZBool IsTC11DeliveredByCustoms => NctsHeader.IsTC11DeliveredByCustoms;

		public ZString TC11Date => EU.NCTS.Business.WrapperHelper.GetLongDate(NctsHeader.TC11Date);

		public ZString QueryInformation => NctsHeader.QueryInformation;

		public ZBool IsQueryAvailableOnPaper => NctsHeader.IsQueryAvailableOnPaper;

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;
	}
}
