using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonDeclarantWrapper : PartyIdWrapper
	{
		public static AESCommonDeclarantWrapper New(JobDeclaration jobDeclaration)
		{
			OrgHeader orgHeader = null;
			if (jobDeclaration != null)
			{
				if (jobDeclaration != null && (jobDeclaration.JE_DeclarantType == (ZString)ESRepresentationTypeList.Codes._2Direct || jobDeclaration.JE_DeclarantType == (ZString)ESRepresentationTypeList.Codes._5IndirectATC))
				{
					orgHeader = jobDeclaration?.Representative?.Header == null ? jobDeclaration?.Supplier : jobDeclaration?.DeclarantOrgAddress?.Header != null ? jobDeclaration.DeclarantOrgAddress.Header : jobDeclaration?.Supplier;
				}
				else
				{
					orgHeader = jobDeclaration?.Representative?.Header != null ? jobDeclaration.Representative.Header : jobDeclaration?.DeclarantOrgAddress?.Header != null ? jobDeclaration.DeclarantOrgAddress.Header : jobDeclaration?.Supplier;
				}
			}
			return orgHeader == null ? null : new AESCommonDeclarantWrapper(orgHeader);
		}

		protected AESCommonDeclarantWrapper(OrgHeader orgH) : base(orgH)
		{
		}

		protected override ZString IdCore
		{
			get
			{
				var (id, isNaturalPersonIndividual) = GetIdForNaturalPerson();
				return isNaturalPersonIndividual ? id : base.IdCore;
			}
		}
	}
}
