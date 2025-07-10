using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderDocManagerInfo : DocManagerInfo
	{
		public NctsHeaderDocManagerInfo(NctsHeader header)
			: base(header, Core.Constants.DocManagerCodes.NctsInBond)
		{ }

		protected NctsHeader NctsHeader => (NctsHeader)BusinessEntity;

		protected override BusinessObject[] GetRelatedObjects()
		{
			var businessObjects = base.GetRelatedObjects();

			return NctsHeader.IsPhase5Departure
				? businessObjects.Concat(NctsHeader.DepartureMovementHeaders).ToArray()
				: businessObjects;
		}
	}
}
