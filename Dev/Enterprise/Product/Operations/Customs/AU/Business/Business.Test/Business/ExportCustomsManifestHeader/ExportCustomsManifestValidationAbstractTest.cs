using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ExportCustomsManifestValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			line = header.Lines.AddNew();
		}

		internal void SetupSeaEMM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
		}

		internal void SetupAirEMM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
		}

		internal void SetupSeaDEP()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
		}

		internal void SetupSeaESM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
		}

		internal void SetupSeaSlotESM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
		}

		internal void SetupAirESM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
		}

		internal void SetupLowValueExemptLine()
		{
			line.EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
		}

		internal void SetupCANLine()
		{
			line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			line.EL_CAN = "AAAAAAMP7";
		}

		internal ExportCustomsManifestHeader header;
		internal ExportCustomsManifestLines line;
	}
}
