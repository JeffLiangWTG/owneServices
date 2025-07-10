using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestHeaderValidation : Customs.Business.ExportCustomsManifestHeaderValidation
	{
		public ExportCustomsManifestHeaderValidation(ExportCustomsManifestHeader parent)
			: base(parent)
		{
			header = parent;
			messageValidation = new MessageValidation(header);
		}

		protected override void CheckED_DepartureDate()
		{
			base.CheckED_DepartureDate();
			if (!header.IsCTO)
			{
				messageValidation.CheckEntered(header.ED_DepartureDateInfo);
				if ((header.IsMainManifest || header.IsSlot) && header.ED_DepartureDate < ZDateTime.Today.AddDays(-3))
				{
					header.ED_DepartureDateInfo.AddMessageError("The departure date should not be more than 3 days in the past for Main and Slot Manifests");
				}
				else if (!(header.IsMainManifest || header.IsSlot) && header.ED_DepartureDate < ZDateTime.Today)
				{
					header.ED_DepartureDateInfo.AddMessageError("The departure date should not be in the past for Consolidation Sub Manifests");
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(header.ED_DepartureDateInfo);
			}
		}

		protected override void CheckED_FlightNumber()
		{
			base.CheckED_FlightNumber();
			if ((header.IsMainManifest || header.IsDeparture) && header.IsAir)
			{
				messageValidation.CheckEntered(header.ED_FlightNumberInfo);
				if (!header.ED_FlightNumber.IsEmpty)
				{
					//do some more flight number validation
				}
			}
		}

		protected override void CheckED_VesselName()
		{
			base.CheckED_VesselName();
			if (header.IsSea)
			{
				if (header.IsMainManifest || header.IsDeparture)
				{
					if (header.Vessel == null)
					{
						header.ED_VesselNameInfo.AddMessageError("A valid vessel is required");
					}
					else if (header.Vessel.RV_LloydsNumber.IsEmpty)
					{
						header.ED_VesselNameInfo.AddMessageError("The vessel must have a lloyds number entered");
					}
				}

				if (header.IsMainManifest)
				{
					if (header.LoadFromVesselVoyageDepartureDestination().Length > 0)
					{
						header.ED_VesselNameInfo.AddError("An Export Main Manifest with the same Vessel, Voyage, Departure Port and Destination Country/Region has already been registered.");
					}
				}
			}
		}

		protected override void CheckED_VoyageNumber()
		{
			base.CheckED_VoyageNumber();
			if (header.IsSea)
			{
				if (header.IsMainManifest || header.IsDeparture)
				{
					messageValidation.CheckEntered(header.ED_VoyageNumberInfo);
				}
				ValidateED_VesselName();
			}
		}

		protected override void CheckED_ManifestType()
		{
			base.CheckED_ManifestType();
			if (header.ED_ManifestType.IsEmpty)
			{
				header.ED_ManifestTypeInfo.AddError("You must enter a manifest type");
			}

			ListValidation.ErrorIfInvalidCode(header.ED_ManifestTypeInfo, header.Lookups.ManifestTypeList);
			CheckForDuplicatedDepartureReport();
			if (header.IsSea)
			{
				ValidateED_VesselName();
			}
		}

		void CheckForDuplicatedDepartureReport()
		{
			if (header.ED_ManifestType == ManifestTypeList.Codes.DepartureReport)
			{
				ZQuery query = new ZQuery(ExportCustomsManifestHeaderSchema.ED_ManifestType, ManifestTypeList.Codes.DepartureReport);
				query.AddToFilter(ExportCustomsManifestHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.PK);
				query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_TransportMode, header.ED_TransportMode);
				query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_OA_CTOAddress, header.ED_OA_CTOAddress);
				if (!header.ED_DepartureDate.IsEmpty && header.ED_DepartureDate.IsValid)
				{
					query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_DepartureDate, SQLComparisonOperator.EqualToDatePartOnly, header.ED_DepartureDate.Date);
				}
				if (header.ED_TransportMode == Core.Constants.TransportModes.Air)
				{
					query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_FlightNumber, header.ED_FlightNumber);
				}
				else
				{
					query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VesselName, header.ED_VesselName);
					query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VoyageNumber, header.ED_VoyageNumber);
				}
				query.MaximumRows = 5;
				ZString duplicatedReferences = ZString.Empty;
				foreach (ExportCustomsManifestHeader dupHeader in header.Factory.Load<ExportCustomsManifestHeader>(query))
				{
					duplicatedReferences += dupHeader.ED_BGMReference + ",";
				}
				if (!duplicatedReferences.IsEmpty)
				{
					header.ED_ManifestTypeInfo.AddError(string.Format("This is a duplicate departure report, the same CTO, Departure Date and Transport details are recorded on {0}", duplicatedReferences.TrimEnd(',')));
				}
			}
		}

		protected override void CheckED_NoOfContainer()
		{
			base.CheckED_NoOfContainer();
			if (!header.IsDeparture)
			{
				if (header.ED_NoOfContainer < 0)
				{
					header.ED_NoOfContainerInfo.AddMessageError("There should not be a negative number of containers");
				}
				else if (header.IsAir && header.ED_NoOfContainer > 0)
				{
					header.ED_NoOfContainerInfo.AddMessageError("There should be 0 containers for air manifests.");
				}
				else if (!header.IsMainManifest && header.IsSea && header.ED_NoOfPacks == 0 && header.ED_NoOfContainer == 0)
				{
					header.ED_NoOfContainerInfo.AddMessageError("There should be > 0 containers if there are 0 packages on a sub manifest with a transport mode of sea.");
				}
				else if (header.ED_NoOfContainer != header.Lines.TotalContainers)
				{
					header.ED_NoOfContainerInfo.AddMessageError("The number of containers should add up to the total of the containers on the lines.");
				}
				ValidateED_NoOfPacks();
			}
		}

		protected override void CheckED_NoOfPacks()
		{
			base.CheckED_NoOfPacks();
			if (!header.IsDeparture)
			{
				if (header.ED_NoOfPacks < 0)
				{
					header.ED_NoOfPacksInfo.AddMessageError("There should not be a negative number of packages");
				}
				else if (!header.IsMainManifest && header.IsAir && !header.IsCTO && header.ED_NoOfPacks == 0)
				{
					header.ED_NoOfPacksInfo.AddMessageError("There should be > 0 packages for air sub manifests.");
				}
				else if (!header.IsMainManifest && header.IsSea && header.ED_NoOfPacks == 0 && header.ED_NoOfContainer == 0)
				{
					header.ED_NoOfPacksInfo.AddMessageError("There should be > 0 packages if there are 0 containers on the sub manifest.");
				}
				else if (header.ED_NoOfPacks != header.Lines.TotalPackages)
				{
					header.ED_NoOfPacksInfo.AddMessageError("The number of packages does not equal the sum of the packages at the line level.");
				}
				ValidateED_NoOfContainer();
			}
		}

		protected override void CheckED_NoOfEmptyContainers()
		{
			base.CheckED_NoOfEmptyContainers();
			if (!header.IsDeparture)
			{
				if (header.ED_NoOfEmptyContainers < 0)
				{
					header.ED_NoOfEmptyContainersInfo.AddMessageError("There should not be a negative number of empty containers.");
				}
				else if (header.IsConsolidation && header.ED_NoOfEmptyContainers > 0)
				{
					header.ED_NoOfEmptyContainersInfo.AddMessageError("You should not have empty containers on a Consolidation Sub Manifest");
				}
				else if (header.IsAir && header.ED_NoOfEmptyContainers > 0)
				{
					header.ED_NoOfEmptyContainersInfo.AddMessageError("There should be 0 containers for air manifests.");
				}
			}
		}

		protected override void CheckED_TransportMode()
		{
			base.CheckED_TransportMode();
			ListValidation.MessageErrorIfInvalidCode(header.ED_TransportModeInfo, header.Lookups.ModeOfTransportList);
			if (header.IsSlot && !header.IsSea)
			{
				header.ED_TransportModeInfo.AddMessageError("Slot Sub Manifests must have a transport mode of sea");
			}
		}

		protected override void CheckED_RL_NKPortOfDeparture()
		{
			base.CheckED_RL_NKPortOfDeparture();

			if (header.IsMainManifest)
			{
				messageValidation.CheckEntered(header.ED_RL_NKPortOfDepartureInfo);
			}

			if (header.IsSlot)
			{
				List<ZString> references = new List<ZString>();
				foreach (ExportCustomsManifestHeader duplicatedHeader in header.LoadFromVesselVoyageDepartureDestination())
				{
					references.Add(ZString.Format("'{0}'", duplicatedHeader.ED_FolioReference));
				}

				if (references.Count > 0)
				{
					references.Sort();
					ZString duplicatedReferences = ZString.Join(", ", references.ToArray());
					header.ED_RL_NKPortOfDepartureInfo.AddWarning(string.Format("Customs Export Manifest for '{0}'-'{1}'-'{2}' already exists with Folio References: {3}.\r\nWe recommend to use a single manifest for this '{0}'-'{1}'-'{2}'", header.Vessel.RV_Code, header.ED_VoyageNumber, header.ED_RL_NKPortOfDeparture, duplicatedReferences));
				}
			}

			ListValidation.MessageErrorIfInvalidCode(header.ED_RL_NKPortOfDepartureInfo, header.Lookups.PortOfDepartures);

			if (header.IsSea)
			{
				ValidateED_VesselName();
			}
		}

		protected override void CheckED_RN_NKCountryOfDestination()
		{
			base.CheckED_RN_NKCountryOfDestination();
			if (header.IsMainManifest)
			{
				messageValidation.CheckEntered(header.ED_RN_NKCountryOfDestinationInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(header.ED_RN_NKCountryOfDestinationInfo, header.Lookups.CountryOfDestinations);
			if (header.IsSea)
			{
				ValidateED_VesselName();
			}
		}

		protected override void CheckED_RL_NKPortOfDestination()
		{
			base.CheckED_RL_NKPortOfDestination();
			if (header.IsDeparture)
			{
				messageValidation.CheckEntered(header.ED_RL_NKPortOfDestinationInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(header.ED_RL_NKPortOfDestinationInfo, header.Lookups.PortOfDestinations);
		}

		protected override void CheckED_OA_CTOAddress()
		{
			base.CheckED_OA_CTOAddress();
			if (header.IsDeparture || header.IsCTO || ((header.IsMainManifest || header.IsConsolidation) && header.IsAirCTOHeader))
			{
				MandatoryValidation.CheckEntered(header.ED_OA_CTOAddressInfo);
			}

			if (header.CTOAddress != null && header.CTOAddress.LocalControlledPremisesID.IsEmpty)
			{
				header.ED_OA_CTOAddressInfo.AddMessageError("This address is missing a Premise ID (CCP). Please enter one on the Organisation's Config tab.");
			}
		}

		protected MessageValidation messageValidation;
		protected ExportCustomsManifestHeader header;
	}
}
