using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CalcExportManifestHeader : AutoCalcExportManifestHeader
	{
		public CalcExportManifestHeader(BusinessObjectFactory factory, ZString manifestType)
			: base(factory)
		{
			switch (manifestType)
			{
				case ManifestTypeList.Codes.ExportMainManifest:
				case ManifestTypeList.Codes.SlotExportSubManifest:
					this.manifestType = manifestType;
					break;

				default:
					throw new ArgumentException("Invalid Manifest Type: " + manifestType);
			}
		}

		public ExportCustomsManifestHeader ApplyTo(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ExportCustomsManifestHeader manifest = factory.New<ExportCustomsManifestHeader>();
			manifest.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			manifest.ED_DepartureDate = DepartureDate;
			manifest.ED_RL_NKPortOfDeparture = Departure;
			manifest.ED_RN_NKCountryOfDestination = DischargeCountry;
			manifest.ED_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.ED_VesselName = VesselName;
			manifest.ED_VoyageNumber = VoyageNumber;

			ApplyTo(manifest);
			return manifest;
		}

		public ExportCustomsManifestHeader ApplyTo(ExportCustomsManifestHeader manifest)
		{
			if (manifest == null)
			{
				throw new ArgumentNullException(nameof(manifest));
			}

			manifest.ED_NoOfEmptyContainers = (ZShort)EmptyContainerCount;
			manifest.ED_ManifestType = manifestType;

			Dictionary<ExportCustomsManifestLines, bool> linesToDelete = new Dictionary<ExportCustomsManifestLines, bool>();

			foreach (ExportCustomsManifestLines line in manifest.Lines)
			{
				linesToDelete.Add(line, true);
			}

			foreach (CalcExportManifestLine line in Lines)
			{
				ExportCustomsManifestLines newLine = GetOrCreateLine(manifest.Lines, line);

				newLine.EL_NumberOfContainers = (ZShort)line.ContainerCount;
				newLine.EL_NumberOfPackages = line.PackageCount;

				linesToDelete.Remove(newLine);
			}

			foreach (ExportCustomsManifestLines line in linesToDelete.Keys)
			{
				line.Delete();
			}

			return manifest;
		}

		#region Related Business Objects

		public CalcExportManifestLineCollection Lines
		{
			get { return lines ?? (lines = new CalcExportManifestLineCollection(this)); }
		}

		#endregion

		#region Implementation

		CalcExportManifestLineCollection lines;

		ExportCustomsManifestLines GetOrCreateLine(ExportCustomsManifestLinesCollection collection, CalcExportManifestLine calcLine)
		{
			Predicate<ExportCustomsManifestLines> comparer = ExportManifestFromSailingCreator.GetExportManifestLinesPredicate(IsMainManifest, calcLine);

			foreach (ExportCustomsManifestLines line in collection)
			{
				if (comparer(line))
				{
					line.EL_GoodsDescription = calcLine.GoodsDescription;
					line.EL_RN_NKCountryOfDestination = calcLine.DischargeCountry;

					return line;
				}
			}

			ExportCustomsManifestLines result = collection.AddNew();
			result.EL_TypeOfCAN = calcLine.EntryType;
			result.EL_CAN = calcLine.EntryNumber.Left(ExportCustomsManifestLines.Schema.EL_CANMaxLength);
			result.EL_GoodsDescription = calcLine.GoodsDescription;

			if (!IsMainManifest)
			{
				result.EL_OH_Owner = calcLine.OH_Owner;
				result.EL_GoodsOwner = calcLine.GoodsOwner;
				result.EL_RN_NKCountryOfDestination = calcLine.DischargeCountry;
			}

			return result;
		}

		readonly ZString manifestType;

		bool IsMainManifest
		{
			get { return manifestType == ManifestTypeList.Codes.ExportMainManifest; }
		}

		#endregion
	}
}
