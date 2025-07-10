using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin
{
	public class TransportDetailWrapper : EU.Business.Documents.CertificateOfOrigin.TransportDetailWrapper
	{
		public TransportDetailWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString GetVoyage()
		{
			switch (Declaration.TransportMode)
			{
				case Core.Constants.TransportModes.Sea:
					return Declaration.JE_VesselName;

				default:
					return base.GetVoyage();
			}
		}
	}
}
