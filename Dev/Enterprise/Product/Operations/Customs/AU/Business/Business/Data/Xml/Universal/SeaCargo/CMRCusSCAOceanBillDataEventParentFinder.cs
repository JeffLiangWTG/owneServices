using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class CMRCusSCAOceanBillDataEventParentFinder : CusSCAOceanBillDataEventParentFinder
	{
		public CMRCusSCAOceanBillDataEventParentFinder(BusinessObjectFactory factory, CusSCAOceanBillDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override ZString GetApplicationCode(IXmlEventValueObject valueObject) => Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
	}
}
