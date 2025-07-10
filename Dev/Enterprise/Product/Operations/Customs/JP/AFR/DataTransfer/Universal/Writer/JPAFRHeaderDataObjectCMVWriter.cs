using System.Collections.Generic;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRHeaderDataObjectCMVWriter : JPAFRHeaderDataObjectWriter
	{
		public JPAFRHeaderDataObjectCMVWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override void PopulateAddInfosData(JPAFRHeader headerBO, Shipment headerData, JPManifestDataObjectWriterHelper headerHelper)
		{
			base.PopulateAddInfosData(headerBO, headerData, headerHelper);
			PopulateAddInfoForNewVesselDetails(headerBO, headerData.AddInfoCollection);
		}

		static void PopulateAddInfoForNewVesselDetails(JPAFRHeader headerBO, List<AddInfo> list)
		{
			var newVesselVoyage = headerBO.NewVesselVoyage;
			if (newVesselVoyage != null)
			{
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.CarrierCodeNew,
					Value = newVesselVoyage.JP_CarrierCode
				});
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.VesselNameNew,
					Value = newVesselVoyage.JP_VesselName
				});
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.VesselCallSignNew,
					Value = newVesselVoyage.JP_VesselCallSign
				});
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.VesselCountryNew,
					Value = newVesselVoyage.JP_VesselCountry
				});

				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.VoyageNumberNew,
					Value = newVesselVoyage.JP_VoyageNumber
				});

				if (headerBO.JPH_IsShippingLineEntry)
				{
					list.Add(new AddInfo
					{
						Key = AddInfoConstants.Header.OperationalCarrierVoyageNoNew,
						Value = newVesselVoyage.JP_OperationalCarrierVoyageNo
					});
				}

				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.PortOfLoadingSuffixNew,
					Value = newVesselVoyage.JP_PortOfLoadingSuffix
				});
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.PortOfLoadingCodeNew,
					Value = newVesselVoyage.JP_PortOfLoadingCode
				});
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.PortOfLoadingNameNew,
					Value = newVesselVoyage.JP_PortOfLoadingName
				});

				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.IsDepartureFromRelaxedAreaNew,
					Value = newVesselVoyage.JP_IsDepartureFromRelaxedArea ? AddInfoConstants.True : AddInfoConstants.False
				});

				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.EstimatedDateTimeOfDepartureNew,
					Value = newVesselVoyage.JP_EstimatedDateTimeOfDeparture.ToISO8601String()
				});
				list.Add(new AddInfo
				{
					Key = AddInfoConstants.Header.BlanketChange,
					Value = newVesselVoyage.JP_BlanketChange ? AddInfoConstants.True : AddInfoConstants.False
				});
			}
		}
	}
}
