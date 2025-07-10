using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for FileHeaderLine.
	/// </summary>
	public class FileHeaderLine : DataLine
	{
		public FileHeaderLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		#region Implementation

		protected override void DoProcessing()
		{
			SetManifestType(SafeSubstring(line, 3, 10));
			SetTransportMode(SafeSubstring(line, 198, 2));
			SetVoyageNumber(SafeSubstring(line, 200, 10));
			SetVessel(SafeSubstring(line, 108, 35), SafeSubstring(line, 143, 10));
			SetPortOfDeparture(SafeSubstring(line, 211, 5));
			SetPortOfDestination(SafeSubstring(line, 265, 5));
			SetETD(SafeSubstring(line, 251, 14));
			SetCTOCode(SafeSubstring(line, 319, 15));
		}

		protected void SetCTOCode(ZString cTOCode)
		{
			if (header.ED_OA_CTOAddress.IsEmpty)
			{
				if (!cTOCode.IsEmpty)
				{
					OrgAddress cTOAddress = GetHeader(cTOCode);
					if (cTOAddress != null)
					{
						header.ED_OA_CTOAddress = cTOAddress.PK;
					}
				}
			}
		}

		internal void SetETD(ZString eTDString)
		{
			if (header.ED_DepartureDate.IsEmpty)
			{
				string formatString = "yyyyMMddhhmmss";
				if (eTDString.Length == 8)
				{
					formatString = "yyyyMMdd";
				}
				else if (eTDString.Length == 14)
				{
					formatString = "yyyyMMddHHmmss";
				}
				else
				{
					formatString = null;
				}

				if (formatString != null)
				{
					try
					{
						header.ED_DepartureDate = DateTime.ParseExact(eTDString, formatString, System.Threading.Thread.CurrentThread.CurrentCulture);
					}
					catch (FormatException) { }
				}
			}
		}

		internal void SetPortOfDestination(string portCode)
		{
			if (header.ED_RL_NKPortOfDestination.IsEmpty)
			{
				header.ED_RL_NKPortOfDestination = portCode;
			}
		}

		internal void SetPortOfDeparture(ZString portCode)
		{
			if (header.ED_RL_NKPortOfDeparture.IsEmpty)
			{
				header.ED_RL_NKPortOfDeparture = portCode;
			}
		}

		internal void SetManifestType(string code)
		{
			if (header.ED_ManifestType.IsEmpty)
			{
				if (code.StartsWith("ESM"))
				{
					header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
				}
				else if (code.StartsWith("EMM"))
				{
					header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
				}
				else
				{
					header.ED_ManifestType = ZString.Empty;
				}
			}
		}

		protected void SetTransportMode(string transportModeCode)
		{
			if (header.ED_TransportMode.IsEmpty && transportModeCode == "11")
			{
				header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			}
		}

		protected void SetVoyageNumber(string voyageNumber)
		{
			if (header.ED_VoyageNumber.IsEmpty)
			{
				header.ED_VoyageNumber = voyageNumber;
			}
		}

		internal void SetVessel(string vesselName, string lloydsNumber)
		{
			if (header.ED_VesselName.IsEmpty)
			{
				if (lloydsNumber.Length == 7)
				{
					var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, vesselName);
					vesselQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, lloydsNumber);
					var vessel = header.Factory.LoadTop1<RefVessel>(vesselQuery);
					if (vessel == null)
					{
						vessel = header.Factory.New<RefVessel>();
						vessel.RV_Code = vesselName.ToUpper();
						vessel.RV_LloydsNumber = lloydsNumber;
						vessel.RV_VesselType = "CV";
					}

					header.ED_VesselName = vessel.RV_Code;
					header.ED_LloydsIMO = vessel.RV_LloydsNumber;
				}
			}
		}

		protected OrgAddress GetHeader(ZString cTOCode)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
			filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, cTOCode);
			filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			OrgCusCode[] codes = (OrgCusCode[])header.Factory.Load(typeof(OrgCusCode), filter);
			if (codes.Length == 1)
			{
				return codes[0].PremisesAddress;
			}

			return null;
		}

		#endregion
	}
}
