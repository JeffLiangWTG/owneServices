using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC015BWrapper : EU.NCTS.Business.CC015BDeclarationWrapper, ICC015BDeclaration
	{
		public CC015BWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		ZString EU.NCTS.Messaging.ICC015BDeclaration.ControlResultCode
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

		public ZBool IsPrelodge => NctsHeader.IsPrelodgedMovement;

		IReadOnlyCollection<IGuarantee> EU.NCTS.Messaging.ICC015BDeclaration.Guarantees => guarantees ?? (guarantees = NctsHeader.GetEffectiveGuarantees().Cast<FRNctsGuarantee>().Select(x => new FRGuaranteeWrapper(x)).Cast<IGuarantee>().ToArray());
		IReadOnlyCollection<IGuarantee> guarantees;

		NctsHeader NctsHeader => (NctsHeader)nctsHeader;
	}
}
