using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class ExportManifestFromSailingCreator
	{
		public ExportManifestFromSailingCreator(BusinessObjectFactory factory, CustomsJobVoyageWrapper voyageWrapper)
		{
			this.factory = factory;
			this.voyageWrapper = voyageWrapper;

			if (voyageWrapper.Voyage.JV_VoyageType.IsEmpty)
			{
				throw new ArgumentException("The Voyage Type should be set up before creating Export Manifest");
			}

			switch (voyageWrapper.Voyage.JV_VoyageType)
			{
				case Core.Constants.VoyageType.MainVoyage:
					manifestType = ManifestTypeList.Codes.ExportMainManifest;
					break;

				case Core.Constants.VoyageType.SlotVoyage:
					manifestType = ManifestTypeList.Codes.SlotExportSubManifest;
					break;

				default:
					throw new ArgumentException("Invalid Voyage Type: " + voyageWrapper.Voyage.JV_VoyageType);
			}
		}

		public TemporaryManifestHolder CreateExportManifests()
		{
			var holder = new TemporaryManifestHolder(factory);
			holder.VesselName = voyageWrapper.Voyage.JV_RV_NKVessel;
			holder.VoyageNumber = voyageWrapper.Voyage.JV_VoyageFlight;

			GenerateExportManifests();

			foreach (var pair in resultExportManifests)
			{
				foreach (var manifest in pair.Value)
				{
					holder.Manifests.Add(manifest);
				}
			}

			return holder;
		}

		void GenerateExportManifests()
		{
			GetExistingManifests();
			CalculateExportManifests(voyageWrapper.Voyage.JV_RV_NKVessel, voyageWrapper.Voyage.JV_VoyageFlight);

			foreach (JobSailing sailing in voyageWrapper.Voyage.Sailings)
			{
				var loadCountry = sailing.JX_JA_RL_NKPortOfLoading.Left(2);
				var dischargeCountry = sailing.JX_JB_RL_NKPortOfDischarge.Left(2);

				if (!dischargeCountry.IsEmpty && loadCountry == Core.Constants.CountryCodes.Australia && dischargeCountry != Core.Constants.CountryCodes.Australia)
				{
					var key = GetKey(sailing);

					if (!resultExportManifests.ContainsKey(key))
					{
						TemporaryManifest manifest;

						List<ExportCustomsManifestHeader> list;
						if (existingManifests.TryGetValue(key, out list))
						{
							ZBool isDuplicated = list.Count > 1;
							foreach (var header in list)
							{
								manifest = new TemporaryManifest(factory, GetOrCreateCalcHeader(key, sailing));
								manifest.ExportManifest = header;
								manifest.IsDuplicated = isDuplicated;
								manifest.ShouldSave = !isDuplicated;

								AddResultManifest(key, manifest);
							}

							existingManifests.Remove(key);
						}
						else
						{
							manifest = new TemporaryManifest(factory, GetOrCreateCalcHeader(key, sailing));
							AddResultManifest(key, manifest);
						}
					}
				}
			}

			foreach (var pair in existingManifests)
			{
				ZBool isDuplicated = pair.Value.Count > 1;
				foreach (var header in pair.Value)
				{
					var calcManifest = CreateNewCalcHeader(header.Vessel.RV_Code, header.ED_VoyageNumber, header.ED_RL_NKPortOfDeparture, header.ED_DepartureDate, header.ED_RN_NKCountryOfDestination);
					var manifest = new TemporaryManifest(factory, calcManifest);
					manifest.ExportManifest = header;
					manifest.IsDuplicated = isDuplicated;
					manifest.ShouldSave = !isDuplicated;

					AddResultManifest(pair.Key, manifest);
				}
			}
		}

		void GetExistingManifests()
		{
			ZString transportMode = Core.Constants.TransportModes.Sea;
			var vessel = voyageWrapper.Voyage.Vessel.RV_Code;
			var voyageNumber = voyageWrapper.Voyage.JV_VoyageFlight;

			var manifestQuery = new ZQuery();
			manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_TransportMode, transportMode);
			manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VesselName, vessel);
			manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VoyageNumber, voyageNumber);
			manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_ManifestType, this.manifestType);

			existingManifests.Clear();
			foreach (var header in factory.Load<ExportCustomsManifestHeader>(manifestQuery))
			{
				AddExistingManifest(GetKey(header), header);
			}
		}

		void AddExistingManifest(ZString key, ExportCustomsManifestHeader header)
		{
			List<ExportCustomsManifestHeader> list;

			if (!existingManifests.TryGetValue(key, out list))
			{
				list = new List<ExportCustomsManifestHeader>();
				existingManifests.Add(key, list);
			}

			list.Add(header);
		}

		void AddResultManifest(ZString key, TemporaryManifest manifest)
		{
			List<TemporaryManifest> list;

			if (!resultExportManifests.TryGetValue(key, out list))
			{
				list = new List<TemporaryManifest>();
				resultExportManifests.Add(key, list);
			}

			list.Add(manifest);
		}

		void CalculateExportManifests(ZString vesselName, ZString voyageNumber)
		{
			ZString departure;
			ZDateTime departureDate;
			ZString dischargeCountry;
			CalcExportManifestHeader header = null;
			ZString key = "---";

			calcManifests.Clear();
			foreach (var obj in GetDynamicBusinessObjectCollection(vesselName, voyageNumber))
			{
				departure = new ZString(obj[CalcExportManifestLine.Schema.Departure]);
				departureDate = new ZDateTime(obj[CalcExportManifestLine.Schema.DepartureDate]);
				dischargeCountry = new ZString(obj[CalcExportManifestLine.Schema.DischargeCountry]);

				string newKey = GetKey(departure, dischargeCountry);
				if (key != newKey)
				{
					header = CreateNewCalcHeader(vesselName, voyageNumber, departure, departureDate, dischargeCountry);
					calcManifests.Add(newKey, header);

					key = newKey;
				}

				var entryType = CMRExportExemptionCodes.Get4CharCode(new ZString(obj[CalcExportManifestLine.Schema.EntryType]));
				var entryNumber = new ZString(obj[CalcExportManifestLine.Schema.EntryNumber]);
				var goodsDescription = new ZString(obj[CalcExportManifestLine.Schema.GoodsDescription]);
				var containerCount = new ZInt(obj[CalcExportManifestLine.Schema.ContainerCount]);
				var packageCount = new ZInt(obj[CalcExportManifestLine.Schema.PackageCount]);
				var emptyContainerCount = new ZInt(obj["EmptyContainerCount"]);
				var owner = new ZGuid(obj["OH_Owner"]); // not a defined schema column
				var goodsOwner = new ZString(obj["GoodsOwner"]);

				var line = GetOrCreateManifestLine(header, departure, departureDate, dischargeCountry, entryType, entryNumber, goodsDescription, owner, goodsOwner);

				line.ContainerCount += containerCount;
				line.PackageCount += packageCount;

				header.EmptyContainerCount += emptyContainerCount;
				header.ContainerCount += containerCount;
				header.PackageCount += packageCount;
			}
		}

		CalcExportManifestLine GetOrCreateManifestLine(CalcExportManifestHeader header, ZString departure, ZDateTime departureDate, ZString dischargeCountry, ZString entryType, ZString entryNumber, ZString goodsDescription, ZGuid owner, ZString goodsOwner)
		{
			var comparer = GetExportManifestLinesPredicateCalc(IsMainManifest, entryType, entryNumber, goodsDescription, owner, goodsOwner, dischargeCountry);

			foreach (CalcExportManifestLine line in header.Lines)
			{
				if (comparer(line))
				{
					if (line.GoodsDescription != goodsDescription)
					{
						line.GoodsDescription = ZString.Empty;
					}

					if (line.DischargeCountry != dischargeCountry)
					{
						line.DischargeCountry = ZString.Empty;
					}

					return line;
				}
			}

			var result = header.Lines.AddNew();
			result.Departure = departure;
			result.DepartureDate = departureDate;
			result.DischargeCountry = dischargeCountry;
			result.EntryType = entryType;
			result.EntryNumber = entryNumber;
			result.GoodsDescription = goodsDescription;

			if (!IsMainManifest)
			{
				result.OH_Owner = owner;
				result.GoodsOwner = goodsOwner.Left(result.GoodsOwnerInfo.MaxLength);
			}

			return result;
		}

		CalcExportManifestHeader GetOrCreateCalcHeader(ZString key, JobSailing sailing)
		{
			CalcExportManifestHeader header;

			if (!calcManifests.TryGetValue(key, out header))
			{
				header = CreateNewCalcHeader(sailing.JX_JV_NKVessel, sailing.JX_JV_VoyageFlight, sailing.JX_JA_RL_NKPortOfLoading, sailing.JX_JA_E_DEP, sailing.JX_Calc_DischargeCountry);
				calcManifests.Add(key, header);
			}

			return header;
		}

		CalcExportManifestHeader CreateNewCalcHeader(ZString vesselName, ZString voyageNumber, ZString departure, ZDateTime departureDate, ZString dischargeCountry)
		{
			var header = new CalcExportManifestHeader(factory, this.manifestType);
			header.VesselName = vesselName;
			header.VoyageNumber = voyageNumber;
			header.Departure = departure;
			header.DepartureDate = departureDate;
			header.DischargeCountry = IsMainManifest ? dischargeCountry : ZString.Empty;

			return header;
		}

		DynamicBusinessObjectCollection GetDynamicBusinessObjectCollection(ZString vesselName, ZString voyageNumber)
		{
			const string sql = @"
					Select * From dbo.CalcExportCustomsManifest(@VesselName, @VoyageNumber) Order by Departure, DischargeCountry
				";

			var paramCollection = new ZSqlParameterCollection();
			paramCollection.Add("@VesselName", vesselName, JobVoyageSchema.JV_RV_NKVessel);
			paramCollection.Add("@VoyageNumber", voyageNumber, JobVoyageSchema.JV_VoyageFlight);

			var matchingManifests = new DynamicBusinessObjectCollection(factory);
			matchingManifests.Load(sql, paramCollection);
			return matchingManifests;
		}

		readonly Dictionary<ZString, List<TemporaryManifest>> resultExportManifests = new Dictionary<ZString, List<TemporaryManifest>>();
		readonly Dictionary<ZString, CalcExportManifestHeader> calcManifests = new Dictionary<ZString, CalcExportManifestHeader>();
		readonly Dictionary<ZString, List<ExportCustomsManifestHeader>> existingManifests = new Dictionary<ZString, List<ExportCustomsManifestHeader>>();
		readonly BusinessObjectFactory factory;
		readonly CustomsJobVoyageWrapper voyageWrapper;
		readonly ZString manifestType;

		bool IsMainManifest
		{
			get { return this.manifestType == ManifestTypeList.Codes.ExportMainManifest; }
		}

		ZString GetKey(JobSailing sailing)
		{
			return GetKey(sailing.JX_JA_RL_NKPortOfLoading, sailing.JX_Calc_DischargeCountry);
		}
		ZString GetKey(ExportCustomsManifestHeader manifestHeader)
		{
			return GetKey(manifestHeader.ED_RL_NKPortOfDeparture, manifestHeader.ED_RN_NKCountryOfDestination);
		}
		ZString GetKey(ZString portOfLoading, ZString countryOfDestination)
		{
			return IsMainManifest ? ZString.Format("{0}|{1}", portOfLoading, countryOfDestination) : portOfLoading;
		}

		static Predicate<ExportCustomsManifestLines> GetExportManifestLinesPredicateCustoms(bool isMainManifest, ZString entryType, ZString entryNumber, ZString goodsDescription, ZGuid owner, ZString goodsOwner, ZString dischargeCountry)
		{
			Predicate<ExportCustomsManifestLines> result;

			if (isMainManifest)
			{
				if (entryType == "CAN" || entryType == "CCN")
				{
					result = (l) => l.EL_TypeOfCAN == entryType && l.EL_CAN == entryNumber;
				}
				else
				{
					result = (l) => l.EL_TypeOfCAN == entryType && l.EL_CAN == entryNumber && l.EL_GoodsDescription == goodsDescription;
				}
			}
			else
			{
				if (entryType == "CAN" || entryType == "CCN")
				{
					result = (l) => l.EL_TypeOfCAN == entryType && l.EL_CAN == entryNumber && l.EL_OH_Owner == owner && l.EL_GoodsOwner == goodsOwner;
				}
				else
				{
					result = (l) => l.EL_TypeOfCAN == entryType && l.EL_CAN == entryNumber && l.EL_GoodsDescription == goodsDescription && l.EL_OH_Owner == owner && l.EL_GoodsOwner == goodsOwner && l.EL_RN_NKCountryOfDestination == dischargeCountry;
				}
			}

			return result;
		}
		static Predicate<CalcExportManifestLine> GetExportManifestLinesPredicateCalc(bool isMainManifest, ZString entryType, ZString entryNumber, ZString goodsDescription, ZGuid owner, ZString goodsOwner, ZString dischargeCountry)
		{
			Predicate<CalcExportManifestLine> result;

			if (isMainManifest)
			{
				if (entryType == "CAN" || entryType == "CCN")
				{
					result = (r) => r.EntryType == entryType && r.EntryNumber == entryNumber;
				}
				else
				{
					result = (r) => r.EntryType == entryType && r.EntryNumber == entryNumber && r.GoodsDescription == goodsDescription;
				}
			}
			else
			{
				if (entryType == "CAN" || entryType == "CCN")
				{
					result = (r) => r.EntryType == entryType && r.EntryNumber == entryNumber && r.OH_Owner == owner && r.GoodsOwner == goodsOwner;
				}
				else
				{
					result = (r) => r.EntryType == entryType && r.EntryNumber == entryNumber && r.GoodsDescription == goodsDescription && r.OH_Owner == owner && r.GoodsOwner == goodsOwner && r.DischargeCountry == dischargeCountry;
				}
			}

			return result;
		}

		internal static Predicate<ExportCustomsManifestLines> GetExportManifestLinesPredicate(bool isMainManifest, CalcExportManifestLine line)
		{
			return GetExportManifestLinesPredicateCustoms(isMainManifest, line.EntryType, line.EntryNumber, line.GoodsDescription, line.OH_Owner, line.GoodsOwner, line.DischargeCountry);
		}
	}
}
