using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public interface IAsycudaManifestHeaderDataObjectWriter : ITopLevelDataObjectWriter
	{
		Shipment GetDataObject(AsycudaManifestHeader sourceBO);
	}

	public class AsycudaManifestHeaderDataObjectWriter<T> : TopLevelDataObjectWriter<T, Shipment>, IAsycudaManifestHeaderDataObjectWriter
		where T : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter, T sourceBO)
		{
			base.AddTableFetchHintCreators(externalFetchHintSupporter, sourceBO);
			sourceBO?.FetchStrategy.FetchForTreeTableStrategy(externalFetchHintSupporter);
		}

		protected override void PopulateDataObject(T sourceManifest, Shipment uxml)
		{
			var headerHelper = CreateAsycudaManifestHeaderDataObjectWriterHelper(sourceManifest);
			uxml.Branch = Branch.New(GlbBranch.CurrentBranch);   // TODO: Will move Branch to EntryHeader/EntryInstructuction later. Maps to AsycudaManifestHeader.AMA_GB_Branch
			uxml.CustomsBroker = Staff.New(GlbStaff.CurrentUser);
			uxml.TransportMode = new CodeDescriptionPair() { Code = sourceManifest.AMA_TransportMode, Description = sourceManifest.AMA_TransportMode };
			PopulateVoyageFlightNo(sourceManifest, uxml);
			uxml.PortOfLoading = ListHelper.GetWithName(sourceManifest.AMA_RL_NKPortOfLoading, sourceManifest.Factory.GetRefUNLOCOList());
			uxml.PortOfDischarge = ListHelper.GetWithName(sourceManifest.AMA_RL_NKPortOfDischarge, sourceManifest.Factory.GetRefUNLOCOList());
			uxml.WayBillNumber = sourceManifest.AMA_MasterBill;
			uxml.IsBuyersConsol = sourceManifest.AMA_IsBuyersConsolidation;
			uxml.MessagingApplicationCode = new CodeDescriptionPair() { Code = sourceManifest.AMA_ApplicationCode, Description = new ApplicationCodeTypeList()[sourceManifest.AMA_ApplicationCode].Description };

			if (sourceManifest.FeatureProvider?.SupportsCustomsPorts(sourceManifest) ?? false)
			{
				var loadingPortList = (IFindBoxListProvider)sourceManifest.Lookups.CustomsLoadingPortList;
				uxml.CustomsLoadPort = CreateCustomsPort(sourceManifest.AMA_CustomsLoadPort, loadingPortList);
				var dischargePortList = (IFindBoxListProvider)sourceManifest.Lookups.CustomsDischargePortList;
				uxml.CustomsDischargePort = CreateCustomsPort(sourceManifest.AMA_CustomsDischargePort, dischargePortList);
			}

			uxml.DeclarantType = new CodeDescriptionPair()
			{
				Code = sourceManifest.AMA_AgentType,
				Description = sourceManifest.Lookups.AgentTypeList.GetDescriptionFromCode(sourceManifest.AMA_AgentType)
			};

			uxml.ContainerMode = new ContainerMode()
			{
				Code = sourceManifest.AMA_ContainerMode,
				Description = AsycudaManifestHeaderLookups.ContainerModeList.GetDescriptionFromCode(sourceManifest.AMA_ContainerMode)
			};

			uxml.CustomsOffice = new CodeDescriptionPair10Char()
			{
				Code = sourceManifest.AMA_CustomsOffice,
				Description = sourceManifest.AMA_CustomsOfficeDescription
			};

			PopulateAddInfosData(sourceManifest, uxml, headerHelper);
			AddVesselAsAddInfo(uxml, sourceManifest);
			PopulateNotes(sourceManifest, uxml);
			PopulateDates(sourceManifest, uxml, headerHelper);
			PopulateCustomsSupportingInfo(sourceManifest, uxml, headerHelper);
			PopulateManifestSpecificData(sourceManifest, uxml, headerHelper);

			uxml.AddOrgAddress(writeManager, sourceManifest.Carrier, DocAddressType.Carrier);
			uxml.AddOrgAddress(writeManager, sourceManifest.ShippingAgent, DocAddressType.ControllingAgent);
			uxml.AddOrgAddress(writeManager, GlbBranch.CurrentBranch.OrgProxy, Constants.AddressType.Branch);
			uxml.AddOrgAddress(writeManager, sourceManifest.DeconsolidateAddress, DocAddressType.CustomsContainerYardAddress);
			uxml.AddOrgAddress(writeManager, sourceManifest.DischargeTerminalAddress, DocAddressType.CustomsContainerTerminalOperatorAddress);

			AddRegistrationNumberDetails(sourceManifest, uxml);
			headerHelper.ClearEntryInstructionLinkMap();
			var manifestCountries = new[] { sourceManifest };
			uxml.SetEntryInstructionCollection(() => ProcessCollection(manifestCountries, CreateNewAsycudaManifestHeaderEntryInstructionDataObjectWriter(headerHelper)));
			uxml.SetEntryHeaderCollection(() => ProcessCollection(manifestCountries, CreateNewAsycudaManifestHeaderEntryHeaderDataObjectWriter(headerHelper)));
			uxml.SetContainerCollection(() => CreateContainerCollection(sourceManifest.Containers, headerHelper));
			uxml.SetSubShipmentCollection(() => PopulateSubShipmentCollection(sourceManifest, headerHelper)); // Bills
		}

		protected virtual AsycudaManifestHeaderDataObjectWriterHelper CreateAsycudaManifestHeaderDataObjectWriterHelper(T header)
		{
			return header.ApplicationBusinessProvider.GetAsycudaManifestHeaderDataObjectWriterHelper(header);
		}

		protected virtual AsycudaManifestHeaderEntryInstructionDataObjectWriter<T> CreateNewAsycudaManifestHeaderEntryInstructionDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper headerHelper) => new AsycudaManifestHeaderEntryInstructionDataObjectWriter<T>(writeManager, headerHelper);
		protected virtual AsycudaManifestHeaderEntryHeaderDataObjectWriter<T> CreateNewAsycudaManifestHeaderEntryHeaderDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper headerHelper) => new AsycudaManifestHeaderEntryHeaderDataObjectWriter<T>(writeManager, headerHelper);
		DataObjectList<Container> CreateContainerCollection(IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> containers, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			var writer = CreateNewAsycudaContainerDataObjectWriter(headerHelper);
			var list = new DataObjectList<Container>(containers.OfType<AsycudaContainer>().Select(c => writer.GetDataObject(c)));
			list.Content = CollectionContent.Complete;
			return list;
		}

		protected virtual IAsycudaContainerDataObjectWriter CreateNewAsycudaContainerDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper headerHelper) => new AsycudaContainerDataObjectWriter<AsycudaContainer>(writeManager, headerHelper);

		void AddRegistrationNumberDetails(T countryHelper, Shipment uxml)
		{
			if (countryHelper != null)
			{
				if (!countryHelper.RegistrationDate.IsEmpty)
				{
					uxml.SetEntryNumberCollection(() =>
					{
						var result = new List<EntryNumber>();
						if (!countryHelper.RegistrationDate.IsEmpty)
						{
							result.Add(new EntryNumber()
							{
								Type = new EntryType
								{
									Code = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration,
									Description = Constants.CodeTypeDescription.AsycudaRegistrationDescription,
								},
								Number = countryHelper.RegistrationNumber,
								IssueDate = countryHelper.RegistrationDate,
								EntryStatus = new EntryStatus() { Code = countryHelper.RegistrationStatus },
								CountryOfIssue = new Country() { Code = countryHelper.AMA_RN_NKCountry }
							});
						}
						return result;
					});
				}
			}
		}

		void AddVesselAsAddInfo(Shipment uxml, T sourceManifest)
		{
			var vessel = sourceManifest.Vessel;
			if (vessel != null)
			{
				uxml.LloydsIMO = vessel.RV_LloydsNumber;
				uxml.VesselCountryOfRegistration = Country.New(vessel.CountryOfReg);
				uxml.VesselName = vessel.RV_Name;
				var listOfVesselDetails = new List<AddInfo>
				{
					new AddInfo { Key = AddInfoConstants.Header.VesselCarrierCode, Value = vessel.RV_CarrierCode },
					new AddInfo { Key = AddInfoConstants.Header.RadioCallSign, Value = vessel.RV_RadioCallSign },
					new AddInfo { Key = AddInfoConstants.Header.VesselScreeningStatus, Value = vessel.RV_ScreeningStatus },
					new AddInfo { Key = AddInfoConstants.Header.VesselVesselType, Value = vessel.RV_VesselType },
					new AddInfo { Key = AddInfoConstants.Header.VesselYearOfConstruction, Value = vessel.RV_YearOfConstruction.ToString() },
					new AddInfo { Key = AddInfoConstants.Header.VesselNetTonnage, Value = vessel.RV_NetRegisterTon.ToString() }
				};
				uxml.AddInfoCollection.AddRange(listOfVesselDetails);
			}
			else
			{
				uxml.LloydsIMO = sourceManifest.AMA_LloydsNumber;
				uxml.VesselCountryOfRegistration = Country.New(sourceManifest.ConveyanceNationality);
				uxml.VesselName = sourceManifest.AMA_VesselName;
				var listOfVesselDetails = new List<AddInfo>
				{
					new AddInfo { Key = AddInfoConstants.Header.VesselCarrierCode, Value = ZString.Empty },
					new AddInfo { Key = AddInfoConstants.Header.RadioCallSign, Value = sourceManifest.AMA_RadioCallSign },
					new AddInfo { Key = AddInfoConstants.Header.VesselScreeningStatus, Value = ZString.Empty },
					new AddInfo { Key = AddInfoConstants.Header.VesselVesselType, Value = ZString.Empty },
					new AddInfo { Key = AddInfoConstants.Header.VesselYearOfConstruction, Value = ZString.Empty },
					new AddInfo { Key = AddInfoConstants.Header.VesselNetTonnage, Value = ZString.Empty }
				};
				uxml.AddInfoCollection.AddRange(listOfVesselDetails);
			}
		}

		protected virtual void PopulateVoyageFlightNo(T headerBO, Shipment headerData)
		{
			switch (headerBO.AMA_TransportMode)
			{
				case Core.Constants.TransportModes.Road:
					headerData.VoyageFlightNo = headerBO.AMA_VehicleRegistration;
					break;
				default:
					headerData.VoyageFlightNo = headerBO.AMA_Voyage;
					break;
			}
		}

		void PopulateAddInfosData(T headerBO, Shipment headerData, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			headerData.SetAddInfoCollection(() =>
			{
				var list = new List<AddInfo>();
				list.Add(new AddInfo { Key = AddInfoConstants.Header.ConveyanceNationality, Value = headerBO.AMA_RN_NKConveyanceNationality });
				list.Add(new AddInfo { Key = AddInfoConstants.Header.ManifestNumber, Value = headerBO.AMA_MasterBill });
				list.Add(new AddInfo { Key = AddInfoConstants.Header.MasterInformation, Value = headerBO.AMA_MasterInformation });
				list.Add(new AddInfo { Key = AddInfoConstants.Header.Trailer1, Value = headerBO.AMA_Trailer1RegNo });
				list.Add(new AddInfo { Key = AddInfoConstants.Header.Trailer1CountryOfRegistration, Value = headerBO.AMA_RN_NKTrailer1RegCountry });
				list.Add(new AddInfo { Key = AddInfoConstants.Header.Trailer2, Value = headerBO.AMA_Trailer2RegNo });
				list.Add(new AddInfo { Key = AddInfoConstants.Header.Trailer2CountryOfRegistration, Value = headerBO.AMA_RN_NKTrailer2RegCountry });
				foreach (var pair in headerHelper.GetHeaderAdditionalAddInfos(headerBO))
				{
					if (!pair.Value.IsEmpty)
					{
						list.Add(AddInfo.New(pair.Key, pair.Value));
					}
				}
				return list;
			});
		}

		DataObjectList<Shipment> PopulateSubShipmentCollection(T headerBO, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			var billsForMessage = (headerBO.GetCurrentManifestContext()?.ManifestSendBills ?? System.Array.Empty<AsycudaBill>()).ToList();
			var bills = billsForMessage.Any() ? billsForMessage : headerHelper.Load<AsycudaBill>(headerBO.Bills.CompleteFilter).OfType<AsycudaBill>();
			var writer = CreateNewAsycudaBillDataObjectWriter(headerHelper);
			var data = bills.Select(b => writer.GetDataObject(b));
			return new DataObjectList<Shipment>(data);
		}

		protected virtual IAsycudaBillDataObjectWriter CreateNewAsycudaBillDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper headerHelper) => new AsycudaBillDataObjectWriter<AsycudaBill>(writeManager, headerHelper);

		void PopulateNotes(T headerBO, Shipment headerData)
		{
			headerData.SetNoteCollection(() =>
			{
				var notes = headerBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateDates(T headerBO, Shipment headerData, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			headerData.SetDateCollection(() =>
			{
				var list = new List<Date>();
				list.Add(DateType.Departure, ZBool.False, headerBO.AMA_E_DEP);
				list.Add(DateType.Arrival, ZBool.False, headerBO.AMA_E_ARV);
				list.Add(DateType.BillIssued, ZBool.False, headerBO.AMA_MasterBillIssueDate);

				foreach (var date in headerHelper.GetHeaderAdditionalDateAddInfos(headerBO))
				{
					list.Add(date);
				}

				return list;
			});
		}

		void PopulateCustomsSupportingInfo(T headerBO, Shipment headerData, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			headerData.SetCustomsSupportingInformationCollection(() =>
			{
				var customsSupportingInfos = headerHelper.GetHeaderCustomsSupportingInfos(headerBO);
				var customsSupportingInfosList = customsSupportingInfos.Where(info => info != null).Select(info => CustomsSupportingInformationCollectionCreator.Create(info, null, writeManager)).ToList();
				return customsSupportingInfosList;
			});
		}

		protected virtual void PopulateManifestSpecificData(T headerBO, Shipment headerData, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
		}

		CodeDescriptionPair10Char CreateCustomsPort(ZString code, IFindBoxListProvider list)
		{
			var result = new CodeDescriptionPair10Char() { Code = code };
			string description = list?.DescriptionFromCode(code);
			if (description != null)
			{
				result.Description = description;
			}
			return result;
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.AsycudaManifest;

		Shipment IAsycudaManifestHeaderDataObjectWriter.GetDataObject(AsycudaManifestHeader sourceBO) => GetDataObject(sourceBO as T);
	}
}
