using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ActiveBorderTransportMeansWrapper : IActiveBorderTransportMeans
	{
		ActiveBorderTransportMeansWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public string Nationality => nationality ?? (nationality = declaration.ZG_Box18TransportNationality);
		string nationality;

		public static ActiveBorderTransportMeansWrapper New(JobDeclaration declaration) => declaration == null ? null : new ActiveBorderTransportMeansWrapper(declaration);
	}
}
