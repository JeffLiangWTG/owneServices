using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class DeclarationDocManagerInfo : BaseJobDeclaration.DeclarationDocManagerInfo
	{
		public DeclarationDocManagerInfo(JobDeclaration parent) : base(parent)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = base.GetRelatedObjects();
			var resultList = result == null ? new List<BusinessObject>() : result.ToList();

			foreach (CusEntryInstruction cusEntryInstruction in ((JobDeclaration)BusinessEntity).CustomsEntryInstructions)
			{
				foreach (CusInBondMoveHeader cusInBondMoveHeader in cusEntryInstruction.CusInBondPermitsHeaders)
				{
					MasterFactory.RetrieveExistingOrCreateStorageMainForPK(cusInBondMoveHeader.PK, Core.Constants.RefDocTypes.AsycudaCusInBondMoveHeader);
					resultList.Add(cusInBondMoveHeader);
				}
			}

			return resultList.ToArray();
		}

		protected override bool RelatedObjectTypesAlwaysShowCore(BusinessObject relatedObject) => relatedObject is CusInBondMoveHeader;
	}
}
