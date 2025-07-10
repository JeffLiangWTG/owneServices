using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderLookups : EU.TemporaryStorage.Business.CusTempStorageRegHeaderLookups
	{
		public CusTempStorageRegHeaderLookups(CusTempStorageRegHeader parent) : base(parent)
		{
			header = parent;
		}

		readonly CusTempStorageRegHeader header;

		public override CodeDescriptionPairList PreviousReferenceTypeList
		{
			get
			{
				var previousReferenceTypeList = base.PreviousReferenceTypeList;
				if (header.SRH_AppCode == FRConstants.TemporaryStorage.AppCodeIST)
				{
					previousReferenceTypeList = Factory.GetCachedValue<PreviousDocumentCodeList>();
				}
				return previousReferenceTypeList;
			}
		}

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<TempStorageDeclarationStatusList>();
	}
}
