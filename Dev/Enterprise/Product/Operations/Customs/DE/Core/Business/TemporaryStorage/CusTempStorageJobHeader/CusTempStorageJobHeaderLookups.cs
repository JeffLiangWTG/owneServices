using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderLookups : EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups
	{
		public CusTempStorageJobHeaderLookups(AutoCusTempStorageJobHeader parent)
			: base(parent)
		{
		}

		protected new CusTempStorageJobHeader Parent => (CusTempStorageJobHeader)base.Parent;

		public override CodeDescriptionPairList TransportMeansList => Factory.GetCachedValue<TemporaryStorageTransportMeansList>();

		public override CodeDescriptionPairList PreviousReferenceTypeList => Factory.GetCachedValue<PreviousReferenceType>();

		public override CodeDescriptionPairList TransportModeList
		{
			get
			{
				return Factory.GetCachedValue(string.Concat("DE|TransportModeList|", Parent.SJH_TransportMeansCode), delegate
					{
						switch (Parent.SJH_TransportMeansCode)
						{
							case TemporaryStorageTransportMeansList.Codes.Truck:
							case TemporaryStorageTransportMeansList.Codes.Car:
								return new CodeDescriptionPairList() { new CodeDescriptionPair(TransportTypeList.Codes.Road, TransportTypeList.Descriptions.Road) };
							case TemporaryStorageTransportMeansList.Codes.Vessel:
								return new CodeDescriptionPairList()
								{
									{ new CodeDescriptionPair(TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Descriptions.InlandWaterwayTransport) },
									{ new CodeDescriptionPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea) }
								};
							case TemporaryStorageTransportMeansList.Codes.Wagon:
								return new CodeDescriptionPairList() { new CodeDescriptionPair(TransportTypeList.Codes.Rail, TransportTypeList.Descriptions.Rail) };
							case TemporaryStorageTransportMeansList.Codes.Aircraft:
								return new CodeDescriptionPairList() { new CodeDescriptionPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air) };
							case TemporaryStorageTransportMeansList.Codes.Without:
							case TemporaryStorageTransportMeansList.Codes.Other:
								return new CodeDescriptionPairList()
								{
									{ new CodeDescriptionPair(TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Descriptions.FixedTransportInstallations) },
									{ new CodeDescriptionPair(TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Descriptions.OwnPropulsion) },
									{ new CodeDescriptionPair(TransportTypeList.Codes.Mail, TransportTypeList.Descriptions.Mail) }
								};
							default:
								return new CodeDescriptionPairList();
						}
					});
			}
		}
	}
}
