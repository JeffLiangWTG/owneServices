using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsESOfficeCode : NctsEuOfficeCode, Integration.Customs.ES.INctsEuOfficeCode
	{
		public NctsESOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

		bool IsOfficeDepartureAndCustomsStatusPREAndOrder1 => (MovementHeader?.CustomsStatusIsPRE ?? ZBool.False) && IsOfficeDeparture && CY_Order == 1;

		[ReadOnlyMember(nameof(IsOfficeDepartureAndCustomsStatusPREAndOrder1))]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[ReadOnlyMember(nameof(IsOfficeDepartureAndCustomsStatusPREAndOrder1))]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		[ReadOnlyMember(nameof(IsOfficeDepartureAndCustomsStatusPREAndOrder1))]
		public override ZDateTime CY_Date { get => base.CY_Date; set => base.CY_Date = value; }
	}
}
