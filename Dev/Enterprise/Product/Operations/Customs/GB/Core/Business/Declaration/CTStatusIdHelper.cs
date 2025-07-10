using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class CTStatusIdHelper
	{
		public static ZString GetNewCtStatusId(BaseJobDeclaration declaration)
		{
			var ct = ZString.Empty;
			var origin = declaration.Origin;
			if (declaration.IsExport)
			{
				var destination = declaration.FinalDestination;
				if (destination != null && origin != null)
				{
					if (origin.IsInNorthernIreland)
					{
						// NIP
						if (destination.IsInEUSpecialTerritory())
						{
							ct = ExportCommunityTransitStatusList.Codes.TF;
						}
						else if (destination.IsInEU)
						{
							ct = ExportCommunityTransitStatusList.Codes.C;
						}
						else
						{
							ct = ExportCommunityTransitStatusList.Codes.X;
						}
					}
					else
					{
						// GB mainland
						ct = ExportCommunityTransitStatusList.Codes.X;
					}
				}
			}
			else if (declaration.IsImport)
			{
				if (origin != null)
				{
					if (origin.IsInEUSpecialTerritory())
					{
						ct = ImportCommunityTransitStatusList.Codes.T2LF; //It is assumed that this module is not being used for transit, so goods to special terr must be TSLF and not T2F
					}
					else if (origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.SanMarino)
					{
						ct = ImportCommunityTransitStatusList.Codes.T2LSM;  //It is assumed that this module is not being used for transit, so goods to SM must be TSLSM and not T2SM
					}
					// No other cases such as transit procedures. It is assumed that this module is not being used for transit. 
				}
			}
			return ct;
		}
	}
}
