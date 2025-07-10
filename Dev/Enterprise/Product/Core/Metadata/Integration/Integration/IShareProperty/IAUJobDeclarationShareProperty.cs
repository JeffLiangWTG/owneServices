using CargoWise.Types;

namespace Enterprise.Metadata.Integration
{
	public interface IAUJobDeclarationShareProperty
	{
		bool IsQuarantine { get; set; }
		bool IsImportCMR { get; set; }
		ZBool IsExWarehouse { get; set; }
		bool IsTransportModeOther { get; set; }
	}
}
