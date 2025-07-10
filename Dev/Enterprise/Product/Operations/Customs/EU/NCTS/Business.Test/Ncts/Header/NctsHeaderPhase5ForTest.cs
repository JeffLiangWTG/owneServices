using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class NctsHeaderPhase5ForTest : NctsHeader
	{
		public NctsHeaderPhase5ForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString DefaultApplicationCode => CusInBondApplicationCodeList.Codes.NCTS5;

		internal CusInBondMoveHeader GetNewMovementHeaderExposed() => GetNewMovementHeader();

		public new NctsDepartureMovementHeader MovementHeader => IsDepartureMovement ? (NctsDepartureMovementHeaderForTest)base.MovementHeader : null;

		protected override NctsDepartureMovementHeader GetNewDepartureMovementHeader() => NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeaderForTest>(this, Common.EU.NctsMoveHeaderType.Codes.Departure);
	}
}
