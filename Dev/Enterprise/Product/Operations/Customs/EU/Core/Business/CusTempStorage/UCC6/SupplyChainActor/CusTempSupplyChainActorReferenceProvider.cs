using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempSupplyChainActorReferenceProvider : CusSupplyChainActorReferenceProvider
	{
		public CusTempSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override CusSupplyChainActorReferenceValidation GetNewValidationCore(CusSupplyChainActorReference reference) => new CusTempSupplyChainActorReferenceValidation(reference);

		protected override ZString GetReferenceFromOwnerCore(OrgHeader ownerOrg) => ownerOrg?.GetEoriDetailsForSpecificCountry() ?? ZString.Empty;
	}
}
