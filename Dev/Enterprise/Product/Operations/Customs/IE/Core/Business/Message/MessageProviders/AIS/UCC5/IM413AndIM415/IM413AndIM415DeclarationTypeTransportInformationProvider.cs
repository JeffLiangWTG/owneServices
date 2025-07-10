using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415DeclarationTypeTransportInformationProvider : IIM413AndIM415DeclarationTypeTransportInformation
	{
		public IM413AndIM415DeclarationTypeTransportInformationProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public string BorderTransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);

		public string ActiveBorderTransportMeansNationality => declaration.JE_RN_NKTransportNationality;
	}
}
