using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDeparturePayInfo : Customs.Business.CusInBondPayInfo
	{
		public NctsDeparturePayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new NctsDeparturePayInfoTypeDecider();

		[RelatedBusinessObject(nameof(MoveHeader))]
		public override ZGuid BPI_BM { get => base.BPI_BM; set => base.BPI_BM = value; }

		public NctsDepartureMovementHeader MoveHeader => Factory.Load<NctsDepartureMovementHeader>(BPI_BM);
	}
}
