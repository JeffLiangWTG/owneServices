using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportBorderTransportInfoWrapper : ITransportMediumInfoCommon
	{
		public DUAExportBorderTransportInfoWrapper(JobDeclaration jobDeclaration)
		{
			declaration = Argument.NotNull(jobDeclaration, "JobDeclaration cannot be null");
		}

		readonly JobDeclaration declaration;

		public ZString TransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, false);

		public ZString TransportId
		{
			get
			{
				if (transportId == null)
				{
					transportId = new CachedProperty<ZString>(declaration.Factory, () => declaration.IsAir ? declaration.JE_VoyageFlightNo : declaration.JE_VesselName);
				}
				return transportId.Value;
			}
		}
		CachedProperty<ZString> transportId;

		public ZString TransportNationality => declaration.JE_RN_NKTransportNationality;
	}
}
