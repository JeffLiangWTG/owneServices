using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfoLookups : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoLookups
	{
		public TemporaryStorageAdditionalInfoLookups(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo parent) : base(parent)
		{
		}

		public new TemporaryStorageAdditionalInfo Parent => (TemporaryStorageAdditionalInfo)base.Parent;

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				if (Parent.Parent is TemporaryStoragePackedItem)
				{
					return Factory.GetCachedValue<AdditionalInfoSubTypeList>();
				}
				else
				{
					return base.SubTypeList;
				}
			}
		}
	}
}
