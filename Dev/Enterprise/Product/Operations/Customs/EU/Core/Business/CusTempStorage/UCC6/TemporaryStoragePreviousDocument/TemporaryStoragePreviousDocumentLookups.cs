using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocumentLookups : CusSupportingInfoLookups
	{
		public TemporaryStoragePreviousDocumentLookups(TemporaryStoragePreviousDocument parent) : base(parent)
		{
		}

		public new TemporaryStoragePreviousDocument Parent => (TemporaryStoragePreviousDocument)base.Parent;

		public override ICollection CodeList
		{
			get
			{
				ICollection result;
				var temporaryStorageHeader = Parent.TemporaryStorageHeader;
				if (temporaryStorageHeader == null)
				{
					result = new ZZRefCusCodeListCombinedCollection(Factory);
				}
				else
				{
					result = temporaryStorageHeader.Configuration.PreviousDocumentConfiguration.GetCodeList(Parent);
				}
				return result;
			}
		}

		public override CodeDescriptionPairList PackTypeList => GetPackTypeList(Factory);

		CodeDescriptionPairList GetPackTypeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDateTime.Today);
		}

		public override CodeDescriptionPairList UnitOfQuantityList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
