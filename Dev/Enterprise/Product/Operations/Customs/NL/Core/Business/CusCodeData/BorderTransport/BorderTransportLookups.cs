using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class BorderTransportLookups : CusCodeDataLookups
{
	public BorderTransportLookups(BorderTransport borderTransport)
		: base(borderTransport)
	{
	}

	public new BorderTransport Parent => (BorderTransport)base.Parent;

	public JobDeclaration JobDeclaration => Parent.Parent;

	public override CodeDescriptionPairList CY_CodeList
	{
		get
		{
			var jobDeclaration = JobDeclaration;
			var transportModeBorder = jobDeclaration != null ? jobDeclaration.JE_TransportModeInland : ZString.Empty;

			return Factory.GetCachedValue(string.Join("|", "BorderTransportTypeList.CY_CodeList", transportModeBorder), () =>
			{
				var result = new CodeDescriptionPairList();

				switch (transportModeBorder)
				{
					case TransportTypeList.Codes.Air:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._40, TransportTypeIdList.Descriptions._40);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._41, TransportTypeIdList.Descriptions._41);
						break;
					case TransportTypeList.Codes.Sea:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._10, TransportTypeIdList.Descriptions._10);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._11, TransportTypeIdList.Descriptions._11);
						break;
					case TransportTypeList.Codes.Rail:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._20, TransportTypeIdList.Descriptions._20);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._21, TransportTypeIdList.Descriptions._21);
						break;
					case TransportTypeList.Codes.Road:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._30, TransportTypeIdList.Descriptions._30);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._31, TransportTypeIdList.Descriptions._31);
						break;
					case TransportTypeList.Codes.InlandWaterwayTransport:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._80, TransportTypeIdList.Descriptions._80);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._81, TransportTypeIdList.Descriptions._81);
						break;
					default:
						result = new TransportTypeIdList();
						break;
				}

				return result;
			});
		}
	}

	public RefCountryCollection TransportCountryList
	{
		get
		{
			return Factory.GetCachedValue("ExportInlandTransportTypeList.TransportCountryList", () =>
			{
				return new RefCountryCollection(Factory);
			});
		}
	}
}
