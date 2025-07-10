using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Manifest.Business;

public class JPManifestTypes
{
	public IManifestType HCH => hch ??= new ManifestType(
			JPManifestTypeCodeList.Codes.HCH,
			JPManifestTypeCodeList.Descriptions.HCH,
			GetAllPossibleTransportModes().GetAllCodes(),
			[ApplicationCodeTypeList.Codes.Consolidator],
			MessageLevel.Manifest,
			JPJobMessageTypeList.ImportOnly()
		);
	IManifestType hch;

	public IManifestType HDF => hdf ??= new ManifestType(
			JPManifestTypeCodeList.Codes.HDF,
			JPManifestTypeCodeList.Descriptions.HDF,
			GetAllPossibleTransportModes().GetAllCodes(),
			[ApplicationCodeTypeList.Codes.Consolidator],
			MessageLevel.Manifest,
			JPJobMessageTypeList.ExportOnly()
		);
	IManifestType hdf;

	public IManifestType NVC => nvc ??= new ManifestType(
			JPManifestTypeCodeList.Codes.NVC,
			JPManifestTypeCodeList.Descriptions.NVC,
			GetAllPossibleTransportModes().GetAllCodes(),
			[ApplicationCodeTypeList.Codes.Consolidator],
			MessageLevel.Manifest,
			JPJobMessageTypeList.ImportOnly()
		);
	IManifestType nvc;

	public IManifestType VAN => van ??= new ManifestType(
		JPManifestTypeCodeList.Codes.VAN,
		JPManifestTypeCodeList.Descriptions.VAN,
		GetAllPossibleTransportModes().GetAllCodes(),
		[ApplicationCodeTypeList.Codes.Consolidator],
		MessageLevel.Manifest,
		JPJobMessageTypeList.ExportOnly()
	);
	IManifestType van;

	static CodeDescriptionPairList GetAllPossibleTransportModes()
	{
		return new CodeDescriptionPairList
		{
			new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Air, Customs.Business.TransportTypeList.Descriptions.Air),
			new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Descriptions.Sea),
			new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Mail, Customs.Business.TransportTypeList.Descriptions.Mail),
			new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Road, Customs.Business.TransportTypeList.Descriptions.Road)
		};
	}
}
