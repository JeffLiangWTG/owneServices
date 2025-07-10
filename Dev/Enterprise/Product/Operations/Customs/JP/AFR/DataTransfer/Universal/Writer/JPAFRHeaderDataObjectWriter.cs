using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRHeaderDataObjectWriter : TopLevelDataObjectWriter<JPAFRHeader, Shipment>
	{
		public JPAFRHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(JPAFRHeader headerBO, Shipment headerData)
		{
			var headerHelper = new JPManifestDataObjectWriterHelper(headerBO);
			headerData.Branch = Branch.New(headerBO.Branch);
			headerData.WayBillNumber = headerBO.JPH_MasterBillNumber;
			headerData.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			headerData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(Core.Constants.TransportModes.Sea, headerHelper.ForwardingTransportTypeList);
			headerData.VesselName = headerBO.JPH_VesselName;

			var vessel = headerBO.Vessel;
			if (vessel != null)
			{
				headerData.LloydsIMO = vessel.RV_LloydsNumber;
			}

			var country = headerBO.CountryOfReg;
			if (country != null)
			{
				headerData.VesselCountryOfRegistration = Country.New(country);
			}

			headerData.VoyageFlightNo = headerBO.JPH_Voyage;

			var refUNLOCOList = headerBO.Factory.GetRefUNLOCOList();
			headerData.PortOfLoading = ListHelper.GetWithName(headerBO.JPH_RL_NKLoading, refUNLOCOList);
			headerData.PortOfDischarge = ListHelper.GetWithName(headerBO.JPH_RL_NKDischarge, refUNLOCOList);
			PopulateAddInfosData(headerBO, headerData, headerHelper);
			PopulateNotes(headerBO, headerData, headerHelper);
			PopulateDates(headerBO, headerData, headerHelper);
			headerData.SetSubShipmentCollection(() => PopulateSubShipmentCollection(headerBO, headerHelper));

			headerData.SetOrganizationAddressCollection(() => ProcessCollection(headerBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
		}

		protected virtual void PopulateAddInfosData(JPAFRHeader headerBO, Shipment headerData, JPManifestDataObjectWriterHelper headerHelper)
		{
			headerData.SetAddInfoCollection(() =>
			{
				var list = new List<AddInfo>();
				list.Add(new AddInfo()
				{
					Key = AddInfoConstants.Header.CarrierCode,
					Value = headerBO.JPH_CarrierCode
				});
				list.Add(new AddInfo()
				{
					Key = AddInfoConstants.Header.PortOfLoadingSuffix,
					Value = headerBO.JPH_LoadingPortSuffix
				});
				if (headerBO.JPH_IsShippingLineEntry)
				{
					list.Add(new AddInfo()
					{
						Key = AddInfoConstants.Header.PortOfDischargeSuffix,
						Value = headerBO.JPH_DischargePortSuffix
					});
				}
				list.Add(new AddInfo()
				{
					Key = AddInfoConstants.Header.IsDepartureFromRelaxedArea,
					Value = headerBO.JPH_RelaxedAppId ? AddInfoConstants.True : AddInfoConstants.False
				});
				list.Add(new AddInfo()
				{
					Key = AddInfoConstants.Header.VesselCallSign,
					Value = headerBO.JPH_RadioCallSign
				});
				if (!headerBO.JPH_IsShippingLineEntry && headerBO.JPH_VesselDetailsChanged)
				{
					list.Add(new AddInfo
					{
						Key = AddInfoConstants.Header.VesselDetailsChanged,
						Value = AddInfoConstants.True
					});
				}
				if (headerBO.JPH_IsShippingLineEntry)
				{
					list.Add(new AddInfo
					{
						Key = AddInfoConstants.Header.OperationalCarrierVoyageNo,
						Value = headerBO.JPH_OperationalCarrierVoyageNo
					});
				}
				return list;
			});
		}

		DataObjectList<Shipment> PopulateSubShipmentCollection(JPAFRHeader headerBO, JPManifestDataObjectWriterHelper headerHelper)
		{
			var bills = headerHelper.Load<JPAFRBills>(headerBO.Bills.CompleteFilter);
			var data = ProcessCollection(bills, new JPAFRBillsDataObjectWriter(writeManager, headerHelper));
			return data != null ? new DataObjectList<Shipment>(data) : null;
		}

		void PopulateNotes(JPAFRHeader headerBO, Shipment headerData, JPManifestDataObjectWriterHelper headerHelper)
		{
			headerData.SetNoteCollection(() =>
			{
				var notes = headerBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateDates(JPAFRHeader headerBO, Shipment headerData, JPManifestDataObjectWriterHelper headerHelper)
		{
			headerData.SetDateCollection(() =>
			{
				var list = new List<Date>();
				list.Add(DateType.Departure, ZBool.False, headerBO.JPH_ETD);
				list.Add(DateType.Arrival, ZBool.False, headerBO.JPH_ETA);
				return list;
			});
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AFRHeader;
		}
	}
}
