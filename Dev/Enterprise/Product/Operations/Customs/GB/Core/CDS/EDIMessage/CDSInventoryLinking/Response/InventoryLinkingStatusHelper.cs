using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	internal static class InventoryLinkingStatusHelper
	{
		internal static ZString GetROEStatusDescription(ZString code) => new RouteOfEntryList().GetWithDescription(code);

		internal static ZString GetSOEStatusDescription(ZString code) => new CDSExportSOE().GetWithDescription(code);

		internal static ZString GetSOEMasterStatusDescription(ZString code) => new CDSExportMasterSOE().GetWithDescription(code);
	}
}
