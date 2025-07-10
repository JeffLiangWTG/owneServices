using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDepartureMovementHeaderCollection<out TMoveHeader> : IActiveBusinessObjectCollection<TMoveHeader>
		where TMoveHeader : NctsDepartureMovementHeader
	{
		new TMoveHeader this[int index] { get; }
	}

	public class NctsDepartureMovementHeaderCollection<TMoveHeader> : ActiveBusinessObjectCollection<TMoveHeader>, INctsDepartureMovementHeaderCollection<TMoveHeader>
		where TMoveHeader : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderCollection(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			if (nctsHeader.IsPhase5Departure)
			{
				AdditionalFilter.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Common.EU.NctsMoveHeaderType.Codes.Departure);
			}
			else
			{
				AdditionalFilter.AddToFilter(ZQuery.NoResultQuery);
			}
		}

		protected override bool AllowNew => Relationship.Master is NctsHeader { IsPhase5: true, Configuration.IsMultipleMovementsEnabled: true };
	}
}
