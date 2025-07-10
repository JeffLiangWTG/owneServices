using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsIEOfficeCode : NctsEuOfficeCode, Integration.Customs.IENCTS.INctsEuOfficeCode
	{
		public NctsIEOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[ReadOnlyMember(nameof(DateUnavailable))]
		public override ZDateTime CY_Date { get => base.CY_Date; set => base.CY_Date = value; }

		public bool DateUnavailable =>
			CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture
			|| CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination
			|| CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
	}
}
