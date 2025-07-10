using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentJobTypeOptionLookups : ZLookups
	{
		public NonPersistentJobTypeOptionLookups(BusinessObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList JobTypeList => Factory.GetCachedValue("NonPersistentJobTypeOptionLookups.JobTypeList", delegate
		{
			var result = new CodeDescriptionPairList();
			result.AddPairIfNotExist(CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Descriptions.NCTS4);
			result.AddPairIfNotExist(CusInBondApplicationCodeList.Codes.NCTS5, CusInBondApplicationCodeList.Descriptions.NCTS5);
			return result;
		});
	}
}
