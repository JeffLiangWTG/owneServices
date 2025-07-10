using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirManifestDataObjectReaderHelper : DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper
	{
		public AirManifestDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, string dataProviderForCodeMapping = null) : base(factory, targetCountryCode, dataProviderForCodeMapping)
		{
		}

		protected override void HandleBillNotAbleToDelete(Customs.Business.CusHAWB hawb, IXmlImportLogger logger)
		{
			base.HandleBillNotAbleToDelete(hawb, logger);
			if (hawb is CusHAWB auHawb)
			{
				auHawb.Logs.AddNew(AutoEvents.AuthorisationWithdrawn);
			}
		}
	}
}
