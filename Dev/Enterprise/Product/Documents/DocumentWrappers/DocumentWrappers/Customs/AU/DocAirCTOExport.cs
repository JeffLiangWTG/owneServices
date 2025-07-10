using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocAirCTOExport : DocumentWrapper
	{
		public static DocAirCTOExport New(AirCTOExportCustomsManifestHeader header, BusinessObjectFactory factory)
		{
			return new DocAirCTOExport(header, factory);
		}

		DocAirCTOExport(AirCTOExportCustomsManifestHeader header, BusinessObjectFactory factory)
			: base(header, factory)
		{
		}

		#region AirlineName

		public ZString AirlineName
		{
			get
			{
				ZString result = "";

				ZString flightprefix = FlightNo.SubstringSafe(0, 2);

				if (!flightprefix.IsEmpty)
				{
					RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Factory, flightprefix);
					if (airline != null)
					{
						result = airline.RM_AirlineName1;
					}
				}

				return result;
			}
		}

		#endregion

		#region DepartureDate

		public ZDateTime DepartureDate
		{
			get { return Header.ED_DepartureDate; }
		}

		#endregion

		#region FlightNo

		public ZString FlightNo
		{
			get { return Header.ED_FlightNumber; }
		}

		#endregion

		#region LoadPort

		public DocUNLOCO LoadPort
		{
			get { return DocUNLOCO.New(Header.PortOfDeparture, Factory); }
		}

		#endregion

		#region TotalPiecesManifested

		public ZInt TotalPiecesManifested
		{
			get
			{
				ZInt total = 0;

				foreach (ExportCustomsManifestLines line in Header.Lines)
				{
					total += line.EL_NumberOfPackages;
				}

				return total;
			}
		}

		#endregion

		#region TotalWeight

		public ZDecimal TotalWeight
		{
			get
			{
				ZDecimal total = 0m;
				var unit = TotalWeightUnit;

				foreach (ExportCustomsManifestLines line in Header.Lines)
				{
					total += Core.Constants.Weight.Convert(line.EL_Weight, line.EL_WeightUQ, unit);
				}

				return total;
			}
		}

		#endregion

		#region TotalWeightUnit

		public ZString TotalWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		#endregion

		#region TotalVolume

		public ZDecimal TotalVolume
		{
			get
			{
				ZDecimal total = 0m;
				var unit = TotalVolumeUnit;

				foreach (ExportCustomsManifestLines line in Header.Lines)
				{
					total += Core.Constants.Volume.Convert(line.EL_Volume, line.EL_VolumeUQ, unit);
				}

				return total;
			}
		}

		#endregion

		#region TotalVolumeUnit

		public ZString TotalVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		#endregion

		#region Lines

		public DocExportCustomsManifestLineCollection Lines
		{
			get { return lines ?? (lines = new DocExportCustomsManifestLineCollection(Header.Lines)); }
		}
		DocExportCustomsManifestLineCollection lines;

		#endregion

		#region Implementation

		AirCTOExportCustomsManifestHeader Header
		{
			get { return (AirCTOExportCustomsManifestHeader)this.WrappedObject; }
		}

		#endregion
	}
}
