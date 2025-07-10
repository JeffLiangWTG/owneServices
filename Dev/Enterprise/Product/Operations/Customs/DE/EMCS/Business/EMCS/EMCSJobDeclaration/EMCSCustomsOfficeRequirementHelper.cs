using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSCustomsOfficeRequirementHelper : EU.EMCS.Business.EMCSCustomsOfficeRequirementHelper
	{
		public EMCSCustomsOfficeRequirementHelper(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		protected new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			var isConsolidatedDocument = Declaration.IsConsolidatedDocument();
			return Factory.GetCachedValue("EMCSCustomsOfficeRequirementHelper_OtherRequirements_" + isConsolidatedDocument, () =>
			{
				var requirements = base.GetOtherRequirements().Where(x => x.OfficeRole != EuOfficeCodesTypes.Codes.OfficeOfDispatch).ToList();
				requirements.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfDispatch, true, isConsolidatedDocument)
				{
					OfficeRolesForLookup = new ZString[] { EU.Business.UniversalReferenceConstants.CustomsOfficeAttributes.Excise }
				});
				return requirements;
			});
		}
	}
}
