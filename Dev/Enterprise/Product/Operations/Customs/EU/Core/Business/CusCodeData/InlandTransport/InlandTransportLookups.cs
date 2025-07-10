using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InlandTransportLookups : CusCodeDataLookups
	{
		public InlandTransportLookups(InlandTransport inlandTransport)
			: base(inlandTransport)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				var transportModeInland = Parent.Parent?.JE_TransportModeInland ?? ZString.Empty;

				return Factory.GetCachedValue(string.Join("|", "EUInlandTransportLookups.CY_CodeList", transportModeInland), () =>
				{
					var result = new CodeDescriptionPairList();
					switch (transportModeInland)
					{
						case TransportTypeList.Codes.Sea:
							result.AddPair(ExportInlandTransportTypeList.Codes._10, ExportInlandTransportTypeList.Descriptions._10);
							result.AddPair(ExportInlandTransportTypeList.Codes._11, ExportInlandTransportTypeList.Descriptions._11);
							break;
						case TransportTypeList.Codes.Rail:
							result.AddPair(ExportInlandTransportTypeList.Codes._20, ExportInlandTransportTypeList.Descriptions._20);
							result.AddPair(ExportInlandTransportTypeList.Codes._21, ExportInlandTransportTypeList.Descriptions._21);
							break;
						case TransportTypeList.Codes.Road:
							result.AddPair(ExportInlandTransportTypeList.Codes._30, ExportInlandTransportTypeList.Descriptions._30);
							result.AddPair(ExportInlandTransportTypeList.Codes._31, ExportInlandTransportTypeList.Descriptions._31);
							break;
						case TransportTypeList.Codes.Air:
							result.AddPair(ExportInlandTransportTypeList.Codes._40, ExportInlandTransportTypeList.Descriptions._40);
							result.AddPair(ExportInlandTransportTypeList.Codes._41, ExportInlandTransportTypeList.Descriptions._41);
							break;
						case TransportTypeList.Codes.InlandWaterwayTransport:
							result.AddPair(ExportInlandTransportTypeList.Codes._80, ExportInlandTransportTypeList.Descriptions._80);
							result.AddPair(ExportInlandTransportTypeList.Codes._81, ExportInlandTransportTypeList.Descriptions._81);
							break;
						case TransportTypeList.Codes.FixedTransportInstallations:
						case TransportTypeList.Codes.OwnPropulsion:
						case TransportTypeList.Codes.Mail:
							result = new ExportInlandTransportTypeList();
							break;
					}
					return result;
				});
			}
		}

		public virtual RefCountryCollection TransportCountryList => new RefCountryCollection(Factory);

		protected new InlandTransport Parent => (InlandTransport)base.Parent;
	}
}
