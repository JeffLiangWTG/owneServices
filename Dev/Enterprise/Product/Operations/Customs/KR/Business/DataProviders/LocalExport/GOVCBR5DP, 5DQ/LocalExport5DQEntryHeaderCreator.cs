using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExport5DQEntryHeaderCreator : LocalExportCommonCreator
	{
		protected override void PopulateHeader(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			base.PopulateHeader(entry, localExportData);
			var declaration = entry.Declaration;
			localExportData.BondedAreaCode = declaration.JE_SubLocationOfGoods;
			if (declaration.JE_MessageSubType == LocalExportTransactionNatureCodeList.Codes._08 && !declaration.JE_EntryDate.IsEmpty)
			{
				localExportData.DeclarationDate = declaration.JE_EntryDate.ToDateTime();
			}
			if (declaration.IsLocalExportToSeaVessel)
			{
				localExportData.CrewCount = declaration.JE_NoOfCrew;
				localExportData.ScheduledSailingDays = declaration.JE_VoyageDuration;
				localExportData.FlightNoOrVesselName = declaration.JE_VesselName;
				localExportData.MRNNo = declaration.MRNJ3_ReferenceNumber;
				localExportData.VesselRadioCallSign = declaration.Vessel?.RV_RadioCallSign ?? ZString.Empty;
			}
			else if (declaration.IsLocalExportToAirplane)
			{
				localExportData.FlightNoOrVesselName = declaration.JE_VoyageFlightNo;
			}
		}

		protected override void PopulateMoreOrganisations(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			base.PopulateMoreOrganisations(entry, localExportData);
			PopulateStevedore(entry, localExportData);
			PopulateOtherTransportMeans(entry, localExportData);
			PopulateExporter(entry, localExportData);
		}
		void PopulateStevedore(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			var persons = entry.Declaration.Persons;
			var stevedoreList = new List<LocalExportStevedore>();

			if (entry.Declaration.IsLocalExportToSeaVessel)
			{
				var i = 1;
				foreach (CusPerson person in persons)
				{
					var stevedore = entry.Declaration.StevedoreCompany;

					if (stevedore != null)
					{
						var item = new LocalExportStevedore()
						{
							SequenceNo = i,
							FullName = person.Person?.PER_FullName ?? ZString.Empty,
							RoadNameCode = stevedore.Address?.GetRoadNameCode(),
							BuildingNumber = stevedore.Address?.GetBuildingNumber(),
							Postcode = stevedore.Address?.Postcode,
							AddressLine1 = stevedore.Address?.Address1,
							AddressLine2 = stevedore.Address?.Address2
						};

						if (person.Person != null && !person.Person.PER_BirthDate.IsEmpty)
						{
							item.Birthday = person.Person.PER_BirthDate.ToDateTime();
						}

						i++;
						stevedoreList.Add(item);
					}
				}
			}
			localExportData.Stevedores = stevedoreList.ToArray();
		}

		void PopulateOtherTransportMeans(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			var transportMeans = entry.Declaration.TransportMeans;
			var otherTransportMeansList = new List<LocalExportOtherTransportMeans>();

			if (entry.Declaration.IsLocalExportToSeaVessel)
			{
				foreach (TransportMeans transportMean in transportMeans)
				{
					var vessel = entry.Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, transportMean.CY_Code));
					var item = new LocalExportOtherTransportMeans()
					{
						SequenceNo = transportMean.CY_Order,
						WorkingVesselName = transportMean.CY_Code,
						WorkingVesselLloydsNumber = transportMean.Description,
						TransportVehicleRegNo = transportMean.CY_Data
					};

					otherTransportMeansList.Add(item);
				}
			}
			localExportData.OtherTransportMeans = otherTransportMeansList.ToArray();
		}

		void PopulateExporter(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			if (entry.Declaration.Exporter != null)
			{
				localExportData.Exporter = new Organisation(RoleType.Exporter);
				var idNumbers = entry.Declaration.Exporter.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo });
				localExportData.Exporter.SetRegistrationIDNumbers(idNumbers);
			}
			else
			{
				if (localExportData.Supplier != null && !string.IsNullOrEmpty(localExportData.Supplier.BusinessRegNo))
				{
					localExportData.Exporter = new Organisation(RoleType.Exporter);
					localExportData.Exporter.BusinessRegNo = localExportData.Supplier.BusinessRegNo;
				}
			}
		}
	}
}
