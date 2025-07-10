using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC013BWrapper : EU.NCTS.Business.CC013BDeclarationWrapper, ICC013BDeclaration
	{
		public CC013BWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		ZString EU.NCTS.Messaging.ICC013BDeclaration.ControlResultCode
		{
			get
			{
				var departureMovementHeader = NctsHeader.MovementHeader;
				return departureMovementHeader != null
						&& departureMovementHeader.IsSimplifiedNctsProcedure
						&& departureMovementHeader.BM_InBondEntryType != EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR
						? EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader
						: EU.NCTS.Business.NctsControlResult.Codes.ConsideredSatisfactory;
			}
		}

		public ZString NatureOfSeals => NctsHeader.FRNctsHeader.CFN_NatureOfSeals;

		public ZString AgreementNumber => CachedValueHelper.GetValue(ref agreementNumber, () => DeclarationWrapperHelper.DepartureAgreementNumber(NctsHeader));
		CachedValue<ZString> agreementNumber;

		public ZBool PrelodgeDeclarationIndicator => NctsHeader.IsPrelodgedMovement;

		public ZString AmendmentDate => EU.NCTS.Business.WrapperHelper.GetLongDate(ZDateTime.Now);

		IReadOnlyCollection<IGuarantee> EU.NCTS.Messaging.ICC013BDeclaration.Guarantees => guarantees ?? (guarantees = NctsHeader.GetEffectiveGuarantees().Cast<FRNctsGuarantee>().Select(x => new FRGuaranteeWrapper(x)).Cast<IGuarantee>().ToArray());
		IReadOnlyCollection<IGuarantee> guarantees;

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;
	}
}
