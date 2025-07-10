using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPRLCusTempStorageLineLookups : CusTempStorageLineLookups
	{
		public CUSPRLCusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CUSPRLCusTempStorageLine Parent => (CUSPRLCusTempStorageLine)base.Parent;

		public override CodeDescriptionPairList UnionStatusList
		{
			get
			{
				var previousReferenceType = Parent.StorageHeader?.SJH_PreviousReferenceType ?? ZString.Empty;

				return Factory.GetCachedValue(ZString.Format("DE|UnionStatusList|{0}", previousReferenceType), () => // Cache Key
				{
					var result = new CodeDescriptionPairList();

					switch (previousReferenceType)
					{
						case PreviousReferenceType.Codes._OHNE:
							result.AddPair(Business.DEUnionStatusList.Codes.N, Business.DEUnionStatusList.Descriptions.N);
							break;
						case PreviousReferenceType.Codes._199:
						case PreviousReferenceType.Codes._200:
						case PreviousReferenceType.Codes._T:
						case PreviousReferenceType.Codes._T2:
							result.AddPair(Business.DEUnionStatusList.Codes.C, Business.DEUnionStatusList.Descriptions.C);
							result.AddPair(Business.DEUnionStatusList.Codes.D, Business.DEUnionStatusList.Descriptions.D);
							result.AddPair(Business.DEUnionStatusList.Codes.F, Business.DEUnionStatusList.Descriptions.F);
							result.AddPair(Business.DEUnionStatusList.Codes.N, Business.DEUnionStatusList.Descriptions.N);
							result.AddPair(Business.DEUnionStatusList.Codes.X, Business.DEUnionStatusList.Descriptions.X);
							break;
						default:
							result.AddPair(Business.DEUnionStatusList.Codes.C, Business.DEUnionStatusList.Descriptions.C);
							result.AddPair(Business.DEUnionStatusList.Codes.F, Business.DEUnionStatusList.Descriptions.F);
							result.AddPair(Business.DEUnionStatusList.Codes.N, Business.DEUnionStatusList.Descriptions.N);
							break;
					}

					return result;
				});
			}
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				return Factory.GetCachedValue("DE|CUSPRLCusTempStorageLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList()
					{
						{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB) },
						{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.SIN, Business.OwnerReferenceTypeList.Descriptions.SIN) },
						{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD) },
						{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ZZZ, Business.OwnerReferenceTypeList.Descriptions.ZZZ) }
					});
			}
		}

		public ZZRefCusCodeListCombinedCollection TransportNumberTypeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, ZDateTime.Now);

		public OrgHeaderCollection OrgHeaderCollection => new OrgHeaderCollection(Factory);
	}
}
