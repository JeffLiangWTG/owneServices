using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC014AWrapper : EU.NCTS.Business.CC014ADeclarationWrapper, ICC014ADeclaration
	{
		public CC014AWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZString AgreementNumber => CachedValueHelper.GetValue(ref agreementNumber, () => DeclarationWrapperHelper.DepartureAgreementNumber(NctsHeader));
		CachedValue<ZString> agreementNumber;

		public JustificationReglementaireInvalidation CancellationRegularJustification => CachedValueHelper.GetValue(ref cancellationRegularJustification, () => DeclarationWrapperHelper.CancellationJustification(NctsHeader));
		CachedValue<JustificationReglementaireInvalidation> cancellationRegularJustification;

		public ZString CancellationDate => EU.NCTS.Business.WrapperHelper.GetLongDate(ZDateTime.Now);

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;
	}
}
