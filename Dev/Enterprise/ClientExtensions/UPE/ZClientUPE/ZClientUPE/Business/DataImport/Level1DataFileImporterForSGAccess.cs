using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Business.SGAccess;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using ASYCUDAAddInfoConstants = Enterprise.Customs.ASYCUDA.Business.AddInfoConstants;
using AsycudaBill = Enterprise.Customs.SG.Access.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using SGAccessAddInfoConstants = Enterprise.Customs.SG.Access.Business.AddInfoConstants;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1DataFileImporterForSGAccess : Level1DataFileImporterCore, ISimpleLogger, IUnitConverterDataProvider
	{
		public Level1DataFileImporterForSGAccess(Level1DataImport level1DataImport, INotifications notify)
			: base(level1DataImport, notify)
		{
		}

		public static class Constants
		{
			public const decimal InsurancePercentage = 0.01m;
			public const string StandardPreference = "STD";

			public static class TransportMode
			{
				public const string Air = Core.Constants.TransportModes.Air;
				public const string Road = Core.Constants.TransportModes.Road;
			}

			public static class GoodsType
			{
				public const string ControlledGoods = Customs.SG.Access.Business.Constants.GoodsType.ControlledGoods;
				public const string DutiableGoods = Customs.SG.Access.Business.Constants.GoodsType.DutiableGoods;
				public const string MajorExporter = Customs.SG.Access.Business.Constants.GoodsType.MajorExporter;
				public const string NormalGoods = Customs.SG.Access.Business.Constants.GoodsType.NormalGoods;
			}

			public static class ManifestType
			{
				public const string Export = Customs.SG.Access.Business.Constants.ManifestType.Export;
				public const string Import = Customs.SG.Access.Business.Constants.ManifestType.Import;
			}

			public static class Nature
			{
				public const string Export = "EXP";
				public const string Import = "IMP";
			}

			public static class CustomizedFieldConstants
			{
				public static class Bill
				{
					public const string BillingTerms = "Billing Terms";
					public const string LongTrackingNumber = "Long Tracking Number";
					public const string ServiceLevel = "Service Level";
				}
			}

			public static class AddInfoConstants
			{
				public static class HeaderCountry
				{
					public const string AHC_Nature = "AHC_Nature";
				}

				public static class Bill
				{
					public const string ABL_PrepaidCollect = AsycudaBillSchema.Constants.ABL_PrepaidCollect;
					public const string MatchingReference = ASYCUDAAddInfoConstants.Bill.MatchingReference;
				}

				public static class BillCountry
				{
					public const string TaxAmount = ASYCUDAAddInfoConstants.Bill.TaxAmount;
					public const string DutyAmount = ASYCUDAAddInfoConstants.Bill.DutyAmount;
					public const string PayeeIndicator = SGAccessAddInfoConstants.BillCountry.PayeeIndicator;
					public const string CycleDate = SGAccessAddInfoConstants.BillCountry.CycleDate;
					public const string CycleNumber = SGAccessAddInfoConstants.BillCountry.CycleNumber;
				}

				public static class Pack
				{
					public const string PackAddInfoType = ASYCUDAAddInfoConstants.Pack.PackAddInfoType;
					public const string MatchingReference = ASYCUDAAddInfoConstants.Pack.MatchingReference;
				}

				public static class PackedItem
				{
					public const string PackingItemAddInfoType = ASYCUDAAddInfoConstants.PackedItem.PackingItemAddInfoType;
					public const string Country = ASYCUDAAddInfoConstants.PackedItem.Country;
					public const string CustomsValue = ASYCUDAAddInfoConstants.PackedItem.CustomsValue;
					public const string TaxValue = ASYCUDAAddInfoConstants.PackedItem.TaxValue;
					public const string DutyValue = ASYCUDAAddInfoConstants.PackedItem.DutyValue;
					public const string CountryOfDestination = ASYCUDAAddInfoConstants.PackedItem.CountryOfDestination;
					public const string GoodsType = SGAccessAddInfoConstants.PackedItem.GoodsType;
					public const string CustomsQty = ASYCUDAAddInfoConstants.PackedItem.CustomsQty;
					public const string CustomsUQ = ASYCUDAAddInfoConstants.PackedItem.CustomsUQ;
				}
			}

			public static class ChargeType
			{
				public const string CustomsValue = Customs.ASYCUDA.Business.Constants.CustomsChargeType.CustomsChargeCode;
				public const string ExWorks = CustomsChargeTypeList.Codes.ExWorks;
				public const string InternationalFreight = CustomsChargeTypeList.Codes.OverseasFreight;
				public const string InternationalInsurance = CustomsChargeTypeList.Codes.OverseasInsurance;
				public const string Discount = CustomsChargeTypeList.Codes.Discount;
				public const string OtherCharges = CustomsChargeTypeList.Codes.OtherCharges;
			}

			public static class Unit
			{
				public const string Package = Core.Constants.PkgUnit.Package;
			}
		}

		#region Implementation

		protected override void PrepareForLoadCore()
		{
			ResetDuplicateAsycudaBillNumberList();
			ResetNegativeBillNumberList();
			ResetBillsToCreateTradenetDeclarationFromList();
			ResetShipmentsNotImported();
			ResetSuccessfullyCreatedManifestHeaders();
		}

		public override ZString GetErrorMessage()
		{
			var result = base.GetErrorMessage();
			if (DuplicateAsycudaBillNumberList.Count > 0)
			{
				ZStringBuilder billNumbers = new ZStringBuilder();
				foreach (ZString billNumber in DuplicateAsycudaBillNumberList.Distinct())
				{
					billNumbers.Append(billNumber);
				}

				result = result + "Bill Numbers are duplicated in the file: \r\n" + billNumbers.ToStringWithDelimiterBetweenAppends(", ");
			}

			return result;
		}

		public override ZString GetSummaryInformation()
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.Append(FlightDetailsSummary);

			var fileDetails = Invariant($@"
File Summary
============
File Name : {level1DataImport.FileName}
Total Number of Shipments : {TotalNoOfShipments}
Shipping Agent : {FactoryProvider.Current.Load<OrgAddress>(level1DataImport.ShippingAgentAddress)?.GetFullAddressString() ?? string.Empty}
");

			stringBuilder.Append(fileDetails);
			stringBuilder.Append(IncorrectPortOfDestinationSummary.ToString());
			stringBuilder.Append(EmptyShipmentsSummary.ToString());
			return stringBuilder.ToString();
		}

		protected override bool CheckForDuplicateShipment(Level1Record record)
		{
			var billNumber = record._200000 != null ? new ZString(record._200000.TrackingNumber) : ZString.Empty;
			if (AsycudaBillNumberList.Any(x => x == billNumber))
			{
				DuplicateAsycudaBillNumberList.Add(billNumber);
				return true;
			}
			else
			{
				AsycudaBillNumberList.Add(billNumber);
				return false;
			}
		}

		public List<ZString> DuplicateAsycudaBillNumberList => fDuplicateAsycudaBillNumberList ?? (fDuplicateAsycudaBillNumberList = new List<ZString>());
		List<ZString> fDuplicateAsycudaBillNumberList;

		List<ZString> AsycudaBillNumberList => fAsycudaBillNumberList ?? (fAsycudaBillNumberList = new List<ZString>());
		List<ZString> fAsycudaBillNumberList;

		void ResetDuplicateAsycudaBillNumberList()
		{
			fAsycudaBillNumberList = null;
			fDuplicateAsycudaBillNumberList = null;
		}

		List<ZString> NegativeBillNumberList => fNegativeBillNumberList ?? (fNegativeBillNumberList = new List<ZString>());
		List<ZString> fNegativeBillNumberList;

		void ResetNegativeBillNumberList()
		{
			fNegativeBillNumberList = null;
		}

		List<ZString> BillsToCreateTradenetDeclarationFromList => fBillsToCreateTradenetDeclarationFromList ?? (fBillsToCreateTradenetDeclarationFromList = new List<ZString>());
		List<ZString> fBillsToCreateTradenetDeclarationFromList;

		ZStringBuilder SuccessfullTradenetDeclarationList => fSuccessfullTradenetDeclarationList ?? (fSuccessfullTradenetDeclarationList = new ZStringBuilder());
		ZStringBuilder fSuccessfullTradenetDeclarationList;

		void ResetBillsToCreateTradenetDeclarationFromList()
		{
			fBillsToCreateTradenetDeclarationFromList = null;
			fSuccessfullTradenetDeclarationList = null;
		}

		ZStringBuilder ShipmentsNotImported => fShipmentsNotImported ?? (fShipmentsNotImported = new ZStringBuilder());
		ZStringBuilder fShipmentsNotImported;

		void ResetShipmentsNotImported()
		{
			fShipmentsNotImported = null;
		}

		List<AsycudaManifestHeader> SuccessfullyCreatedManifestHeaders => fSuccessfullyCreatedManifestHeaders ?? (fSuccessfullyCreatedManifestHeaders = new List<AsycudaManifestHeader>());
		List<AsycudaManifestHeader> fSuccessfullyCreatedManifestHeaders;

		void ResetSuccessfullyCreatedManifestHeaders()
		{
			fSuccessfullyCreatedManifestHeaders = null;
		}

		protected override bool RecordShouldNotBeLoaded(Level1Record record) => RecordShouldNotBeLoadedCore(record);

		public static bool RecordShouldNotBeLoadedCore(Level1Record record) => record.IsGCCChild() ||
			(record.IsGCCLead() && (record._401000 == null || record._401000.LeadTrackingNumberForGCCShipment.IsEmpty)) ||
			record._200000 == null ||
			record._202000 == null ||
			record._202000.PackageTrackingNumber.IsEmpty ||
			record._200000.TrackingNumber.IsEmpty ||
			record._200000.ShipmentType == ShipmentTypeCodeDescriptionPairList.Codes.Documents ||
			record._200000.ShipmentType == ShipmentTypeCodeDescriptionPairList.Codes.Letter;

		UPEOrganisationMatching OrganisationMatching
		{
			get { return fUPEOrganisationMatching ?? (fUPEOrganisationMatching = new UPEOrganisationMatching(FactoryProvider.Current, false)); }
		}
		UPEOrganisationMatching fUPEOrganisationMatching;

		protected override void ProcessData()
		{
			var progressNotification = new ProgressNotification(0);
			var billUpdateMap = CreateSubShipmentData(progressNotification);
			var portOfLoading = new UNLOCO() { Code = level1DataImport.PortOfLoading };
			var portOfDischarge = new UNLOCO() { Code = level1DataImport.PortOfDischarge };
			var transportMode = new CodeDescriptionPair() { Code = level1DataImport.IsRoad ? Constants.TransportMode.Road : Constants.TransportMode.Air };
			var voyageFlightNo = level1DataImport.FlightNumber;
			var masterBillNumber = level1DataImport.MasterBill;
			var masterBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			var dateCollection = CreateDateData();
			var entryHeaderCollection = CreateEntryHeaderData();
			var writerStrategy = DefaultDataObjectWriterStrategy.Instance;
			var entryInstructionCollection = CreateEntryInstructionData(writerStrategy);

			if (billUpdateMap.Any())
			{
				AddToLog("Creating Global Manifest Jobs. This may take some time.");
				progressNotification.PercentageComplete = 1;
				foreach (var billDetails in billUpdateMap)
				{
					var factory = new BusinessObjectFactory { NameForDebugging = "SG Access" };
					using (factory.AddDisposableService())
					{
						var accessShipment = CreateShipmentToUpdate(writerStrategy, billDetails, portOfLoading, portOfDischarge, transportMode, voyageFlightNo, masterBillNumber, masterBillType, dateCollection, entryHeaderCollection, entryInstructionCollection);
						var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", this);
						var accessEvents = PublishAccessEvents(factory, accessShipment, xmlSessionTracker);
						if (accessEvents.Any())
						{
							var publishResult = PublishToUniversalResult.New(accessEvents, DataContextType.AsycudaManifest, ZString.Empty);
							var job = FindManifestHeaderIfExists(publishResult);
							var manifestHeader = job as AsycudaManifestHeader;
							if (manifestHeader != null)
							{
								SuccessfullyCreatedManifestHeaders.Add(manifestHeader);
								AddToLog(Invariant($"Successfully saved Manifest Header {manifestHeader.AMA_JobReference} with {manifestHeader.Bills.Count} Bills.\r\n"));
								var currentRecord = 0;
								var totalNoOfRecords = BillsToCreateTradenetDeclarationFromList.Count;
								foreach (var housebillNumber in BillsToCreateTradenetDeclarationFromList)
								{
									AddToLog("Creating TradeNet Job for " + housebillNumber);
									var declaration = manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == housebillNumber)?.CreateCustomsDeclaration();
									SuccessfullTradenetDeclarationList.Append(Invariant($"{housebillNumber} => {declaration?.JE_DeclarationReference ?? ZString.Empty}"));
									currentRecord++;
									progressNotification.PercentageComplete = (currentRecord * 100) / totalNoOfRecords;
									UpdateProgress(progressNotification);
								}
							}
							else
							{
								AddToLog("Could not find new created Manifest Header.");
								if (!publishResult.ErrorMessage.IsEmpty)
								{
									AddToLog(Invariant($"Failed to publish Universal Data. Error message: {publishResult.ErrorMessage}"));
								}

								if (job != null)
								{
									AddToLog(Invariant($"Could not cast found job to AsycudaManifestHeader, class type of the job is {job.GetType().FullName}"));
								}

								var xmlSessionLogs = xmlSessionTracker.Logs;
								if (xmlSessionLogs.Any())
								{
									AddToLog("XML Session Tracker logs:");
									xmlSessionLogs.ForEach(x => { AddToLog(Invariant($"{x.Type}: {x.Message}")); });
								}
							}
						}
						else
						{
							AddToLog("Failed to publish Universal XML.");
							var ex = accessEvents.FailureException;
							if (ex != null)
							{
								AddToLog(Invariant($"Inner exception: {ex.Message}\r\nStack trace: {ex.StackTrace}"));
							}

							var xmlSessionLogs = xmlSessionTracker.Logs;
							if (xmlSessionLogs.Any())
							{
								AddToLog("XML Session Tracker logs:");
								xmlSessionLogs.ForEach(x => { AddToLog(Invariant($"{x.Type}: {x.Message}")); });
							}
						}
						// This is wrong: Should have one save for all updates, but this preserves existing behaviour and I'm not trying to fix this now.
						ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, () => { });
					}
				}
			}
			NotifyShipmentsNotImportedAndDeclarationDetails();
		}

		internal virtual PublishUniversalXmlResult PublishAccessEvents(BusinessObjectFactory factory, ITopLevelDataObject accessShipment, IXmlSessionTracker xmlSessionTracker) => UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(factory, null, accessShipment, EDIMessageSubTypeList.Codes.XmlUniversalShipment, null, xmlSessionTracker);

		internal virtual BusinessObject FindManifestHeaderIfExists(PublishToUniversalResult publishResult) => publishResult.FindJobIfExists();

		protected override void SaveSummaryTextToEDocs()
		{
			base.SaveSummaryTextToEDocs();
			foreach (var manifestHeader in SuccessfullyCreatedManifestHeaders)
			{
				var docSupportInfo = ((IDocManagerSupport)manifestHeader).DocManagerInfo;
				var bytes = System.Text.Encoding.ASCII.GetBytes(NotificationLogger.ToStringWithNewLineBetweenAppends());
				var doc = docSupportInfo.AddFileOrDocument(bytes, "Level1ImportSummary.txt", Core.Constants.RefDocTypes.MiscellaneousDocument);
				doc.Description = "Level 1 Import Summary Log";
				try
				{
					docSupportInfo.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		Shipment CreateShipmentToUpdate(IDataObjectWriterStrategy writerStrategy, KeyValuePair<ZString, DataObjectList<Shipment>> billDetails, UNLOCO portOfLoading, UNLOCO portOfDischarge, CodeDescriptionPair transportMode, ZString voyageFlightNo, ZString masterBillNumber,
			WayBillType masterBillType, List<Date> dateCollection, List<EntryHeader> entryHeaderCollection, List<EntryInstruction> entryInstructionCollection)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var manifestReference = billDetails.Key;
			dataContext.AddDataTarget(DataContextType.AsycudaManifest, !manifestReference.IsEmpty ? manifestReference : null);
			var updateActionPurpose = AllowMultipleLevel1LoadsForMasterBill && !manifestReference.IsEmpty;
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = new CodeDescriptionPair()
				{
					Code = updateActionPurpose ? Customs.ASYCUDA.Business.AsycudaEventMessageConstants.ActionPurpose.UPD : Customs.ASYCUDA.Business.AsycudaEventMessageConstants.ActionPurpose.ADD,
					Description = updateActionPurpose ? "ASYCUDA Manifest Update" : "ASYCUDA Manifest Add"
				}
			});
			return GetShipment(writerStrategy, dataContext, portOfLoading, portOfDischarge, transportMode, voyageFlightNo, masterBillNumber, masterBillType, dateCollection, entryHeaderCollection, entryInstructionCollection, billDetails.Value);
		}

		Shipment GetShipment(IDataObjectWriterStrategy writerStrategy, IDataContextDataObject dataContext, UNLOCO portOfLoading, UNLOCO portOfDischarge, CodeDescriptionPair transportMode, ZString voyageFlightNo, ZString masterBillNumber,
			WayBillType masterBillType, List<Date> dateCollection, List<EntryHeader> entryHeaderCollection, List<EntryInstruction> entryInstructionCollection, DataObjectList<Shipment> subShipmentCollection)
		{
			var shipment = new Shipment(writerStrategy)
			{
				DataContext = dataContext,
				PortOfLoading = portOfLoading,
				PortOfDischarge = portOfDischarge,
				TransportMode = transportMode,
				VoyageFlightNo = voyageFlightNo,
				WayBillNumber = masterBillNumber,
				WayBillType = masterBillType,
			};
			shipment.SetDateCollection(() => dateCollection);
			shipment.SetEntryHeaderCollection(() => entryHeaderCollection);
			shipment.SetEntryInstructionCollection(() => entryInstructionCollection);
			shipment.SetSubShipmentCollection(() => subShipmentCollection);
			return shipment;
		}

		List<EntryInstruction> CreateEntryInstructionData(IDataObjectWriterStrategy writerStrategy)
		{
			return new List<EntryInstruction>
			{
				new EntryInstruction()
				{
					Link = 1,
					CustomsOffice = new CodeDescriptionPair5Char() { Code = ZString.Empty },
					Style = level1DataImport.IsExport ? Constants.ManifestType.Export : Constants.ManifestType.Import,
					DateAtCustomsOffice = ZDateTime.Empty,
					FirstArrival = new UNLOCO() { Code = level1DataImport.PortOfDischarge },
					AddInfoCollection = CreateEntryInstructionAddInfoData(),
					OrganizationAddressCollection = CreateEntryInstructionOrganizationData(writerStrategy)
				}
			};
		}

		List<OrganizationAddress> CreateEntryInstructionOrganizationData(IDataObjectWriterStrategy writerStrategy)
		{
			List<OrganizationAddress> result = null;
			var shippingAgentAddressPk = level1DataImport.ShippingAgentAddress;
			if (!shippingAgentAddressPk.IsEmpty)
			{
				var shippingAgentAddress = FactoryProvider.Current.Load<OrgAddress>(shippingAgentAddressPk);
				if (shippingAgentAddress?.Header != null)
				{
					result = new List<OrganizationAddress>()
					{
						new OrganizationAddress(writerStrategy)
						{
							AddressType = nameof(MasterFiles.Integration.DocAddressType.ControllingAgent),
							OrganizationCode = shippingAgentAddress.Header.OH_Code,
							AddressShortCode = shippingAgentAddress.OA_Code
						}
					};
				}
			}
			return result;
		}

		List<AddInfo> CreateEntryInstructionAddInfoData()
		{
			return new List<AddInfo>
			{
				AddInfo.New(Constants.AddInfoConstants.HeaderCountry.AHC_Nature, level1DataImport.IsExport ? Constants.Nature.Export : Constants.Nature.Import)
			};
		}

		List<EntryHeader> CreateEntryHeaderData()
		{
			return new List<EntryHeader>
			{
				new EntryHeader()
				{
					Type = new EntryType() { Code = Core.Constants.CountryCodes.Singapore },
					EntryInstructionLink = 1
				}
			};
		}

		List<Date> CreateDateData()
		{
			return new List<Date>
			{
				{ DateType.Departure, ZBool.False, level1DataImport.DepartureDate },
				{ DateType.Arrival, ZBool.False, level1DataImport.ArrivalDate }
			};
		}

		readonly Dictionary<_500000Line, TariffView> lineTariffMap = new Dictionary<_500000Line, TariffView>();
		readonly Dictionary<Level1Record, OrganizationAddressesForRecord> organizationAddressesDictionary = new Dictionary<Level1Record, OrganizationAddressesForRecord>();

		Dictionary<ZString, DataObjectList<Shipment>> CreateSubShipmentData(ProgressNotification progressNotification)
		{
			var billUpdateMap = new Dictionary<ZString, DataObjectList<Shipment>>();
			var currentRecord = 0;
			var masterBillNumber = level1DataImport.MasterBill;
			var flightNo = level1DataImport.FlightNumber;
			AddToLog("Starting Level 1 to Universal XML process");

			var level1RecordList = Level1RecordList.OfType<Level1Record>().ToArray();
			new Level1DataFileImporterForSGAccessFetchStrategy(this).ExecuteFetchStrategy(level1RecordList, progressNotification, lineTariffMap, organizationAddressesDictionary);
			var writerStrategy = DefaultDataObjectWriterStrategy.Instance;

			foreach (Level1Record record in level1RecordList)
			{
				if (!record.IsEmpty)
				{
					if (!RecordShouldNotBeLoaded(record))
					{
						var houseBillNumber = record.IsGCCLead() ? record._401000.LeadTrackingNumberForGCCShipment : record._200000.TrackingNumber;
						var portOfOrigin = GetUNLOCOFromUPSMapping(record._200000.OriginPort)?.RL_Code ?? ZString.Empty;
						var portOfDestination = GetPortOfDestinationCode(record);
						OrgHeader consignee;
						ZDecimal totalLineCustomsValue;
						if (!level1DataImport.DisableDecisionProvider)
						{
							if (RecordShouldBeImported(masterBillNumber, flightNo, portOfOrigin, portOfDestination, houseBillNumber))
							{
								var shipment = CreateNewShipmentUXMLAndNotify(writerStrategy, record, billUpdateMap, houseBillNumber, out consignee, out totalLineCustomsValue);
								var isTradeNetRequired = SGDecisionSupporter.IsTradeNet(shipment, consignee, FactoryProvider.Current);
								if (isTradeNetRequired)
								{
									BillsToCreateTradenetDeclarationFromList.Add(houseBillNumber);
									NotifyDecisionSupporterReason(houseBillNumber);
								}
								PostCalculationProcessing(isTradeNetRequired, shipment, consignee, totalLineCustomsValue);
							}
						}
						else
						{
							CreateNewShipmentUXMLAndNotify(writerStrategy, record, billUpdateMap, houseBillNumber, out consignee, out totalLineCustomsValue);
						}
					}
					currentRecord++;
					progressNotification.PercentageComplete = (currentRecord * 100) / totalNoOfRecords;
					UpdateProgress(progressNotification);
				}
			}
			AddToLog("Universal XML successfully generated.");
			return billUpdateMap;
		}

		void PostCalculationProcessing(bool isTradeNetRequired, Shipment shipment, OrgHeader consignee, ZDecimal totalLineCustomsValue)
		{
			var isHighValueShipment = IsHighValueShipment(totalLineCustomsValue, level1DataImport.IsExport);
			if (isHighValueShipment)
			{
				SetHighValueRequirements(consignee, shipment);
			}
			else
			{
				SetLowValueRequirements(isTradeNetRequired, shipment);
			}

			if (ShouldCycleNumberBeAutoNominated(shipment, consignee)
				|| (!isHighValueShipment && !isTradeNetRequired))
			{
				PopulateCycleDetails(shipment, consignee);
			}
		}

		bool ShouldCycleNumberBeAutoNominated(Shipment shipment, OrgHeader consignee)
		{
			return !level1DataImport.IsExport
					&& SGDecisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee);
		}

		Shipment CreateNewShipmentUXMLAndNotify(IDataObjectWriterStrategy writerStrategy, Level1Record record, Dictionary<ZString, DataObjectList<Shipment>> billUpdateMap, ZString houseBillNumber, out OrgHeader consignee, out ZDecimal totalLineCustomsValue)
		{
			var shipment = CreateShipmentData(writerStrategy, record, houseBillNumber, out consignee, out totalLineCustomsValue);
			AddAccessShipment(shipment, billUpdateMap, houseBillNumber);
			NotifyAccessShipmentDetails(houseBillNumber);
			return shipment;
		}

		ZString GetPortOfDestinationCode(Level1Record record)
		{
			var record_200000 = record._200000;
			return record_200000.DestinationCountry != Core.Constants.CountryCodes.Singapore ?
				GetUNLOCOFromUPSMapping(record_200000.DestinationPort)?.RL_Code ?? ZString.Empty :
				level1DataImport.PortOfDischarge;
		}

		bool RecordShouldBeImported(ZString masterBillNumber, ZString flightNo, ZString origin, ZString destination, ZString houseBillNumber)
		{
			var result = SGDecisionSupporter.ImportThisShipment(FactoryProvider.Current, masterBillNumber, flightNo, houseBillNumber, origin, destination);
			if (!result)
			{
				ShipmentsNotImported.Append(Invariant($"{houseBillNumber} Reason: {SGDecisionSupporter.DecisionReason}"));
			}
			return result;
		}

		SGDecisionSupporter SGDecisionSupporter => sgDecisionSupporter ?? (sgDecisionSupporter = new SGDecisionSupporter());
		SGDecisionSupporter sgDecisionSupporter;

		void NotifyAccessShipmentDetails(ZString wayBillNumber)
		{
			AddToLog(Invariant($"Universal XML created for Bill {wayBillNumber}"));
		}

		void NotifyDecisionSupporterReason(ZString wayBillNumber)
		{
			AddToLog(Invariant($"{wayBillNumber} TradeNet Reason: {SGDecisionSupporter.DecisionReason}"));
		}

		void AddAccessShipment(Shipment shipment, Dictionary<ZString, DataObjectList<Shipment>> billUpdateMap, ZString matchingReference)
		{
			var manifestReference = ZString.Empty;

			if (AllowMultipleLevel1LoadsForMasterBill)
			{
				var masterbill = level1DataImport.MasterBill;
				var flightNo = level1DataImport.FlightNumber;
				var factory = FactoryProvider.Current;

				var manifest = SGDecisionSupporter.GetMatchingBill(factory, masterbill, flightNo, matchingReference)?.Header ??
					SGDecisionSupporter.LoadManifestHeadersForMasterBillAndFlightNo(factory, masterbill, flightNo).FirstOrDefault();

				if (manifest != null)
				{
					manifestReference = manifest.AMA_JobReference;
				}
			}

			billUpdateMap.GetOrAdd(manifestReference, () => new DataObjectList<Shipment>()).Add(shipment);
		}

		bool AllowMultipleLevel1LoadsForMasterBill
		{
			get
			{
				if (!allowMultipleLevel1LoadsForMasterBill.HasValue)
				{
					allowMultipleLevel1LoadsForMasterBill = UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill;
				}
				return allowMultipleLevel1LoadsForMasterBill.Value;
			}
		}
		bool? allowMultipleLevel1LoadsForMasterBill;

		bool EnableFreightAutoRatingInLevelOneImport
		{
			get
			{
				if (!enableFreightAutoRatingInLevelOneImport.HasValue)
				{
					enableFreightAutoRatingInLevelOneImport = UPEDataRegistry.Instance.EnableFreightAutoRatingInLevelOneImport;
				}
				return enableFreightAutoRatingInLevelOneImport.Value;
			}
		}
		bool? enableFreightAutoRatingInLevelOneImport;

		bool EnableAutoPopulateCycleDetailsToImportGlobalManifestBills
		{
			get
			{
				if (!enableAutoPopulateCycleDetailsToImportGlobalManifestBills.HasValue)
				{
					enableAutoPopulateCycleDetailsToImportGlobalManifestBills = UPEDataRegistry.Instance.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills;
				}
				return enableAutoPopulateCycleDetailsToImportGlobalManifestBills.Value;
			}
		}
		bool? enableAutoPopulateCycleDetailsToImportGlobalManifestBills;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		Shipment CreateShipmentData(IDataObjectWriterStrategy writerStrategy, Level1Record record, ZString houseBillNumber, out OrgHeader consignee, out ZDecimal totalLineCustomsValue)
		{
			consignee = null;
			var dataContext = DataContextFactory.New();
			dataContext.AddDataTarget(DataContextType.AsycudaBill, null);
			var organizationAddresses = organizationAddressesDictionary[record];
			var organizationAddressCollection = organizationAddresses.OrganizationAddresses;
			var consignor = organizationAddresses.Consignor;
			consignee = organizationAddresses.Consignee;

			var shipmentWeightUnit = record._200000.ShipmentWeightUnit;
			var weight = record._202000.Weight;
			weight = shipmentWeightUnit == Core.Constants.Weight.Kilograms ? weight / 10.0m : Core.Constants.Weight.ConvertSafe(weight, shipmentWeightUnit, Core.Constants.Weight.Kilograms);

			var packingLineDutyRateMap = new Dictionary<ZInt, ExciseDutyData>();
			var packingLineCollection = CreatePackingLineData(record, packingLineDutyRateMap, writerStrategy, out var totalLinePrice, out var totalPackQty);

			var result = new Shipment(writerStrategy)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = level1DataImport.IsRoad ? Constants.TransportMode.Road : Constants.TransportMode.Air },
				CommercialInfo = CreateCommercialInfo(record, record._200000.Discount, totalLinePrice, consignor, consignee, out var totalCustomsValue),
				GoodsDescription = CreateGoodsDescription(record._500000Lines.OfType<_500000Line>().FirstOrDefault(x => !x.Description.IsEmpty), record._200000.GoodsDescription),
				PortOfOrigin = CreatePortOfOrigin(record),
				PortOfDestination = CreatePortOfDestination(record),
				TotalNoOfPacks = CreateTotalNoOfPacks(record.IsGCCLead()
					? record._401000.TotalPackageCountForGCCShipment
					: !record._202000.PiecesManifested.IsEmpty
						? record._202000.PiecesManifested : totalPackQty),
				TotalNoOfPacksPackageType = new PackageType() { Code = Constants.Unit.Package },
				WayBillNumber = houseBillNumber,
				WayBillType = new WayBillType() { Code = Core.Constants.ShipmentTypes.StandardHouse },
				TotalWeight = weight,
				TotalWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }
			};
			result.SetAddInfoCollection(() => CreateAddInfoDataForBill(record));
			result.SetCustomizedFieldCollection(() => CreateCustomizedFieldDataForBill(record));
			result.SetEntryHeaderCollection(() => CreateEntryHeaderData());
			result.SetOrganizationAddressCollection(() => organizationAddressCollection);
			result.SetPackingLineCollection(() => packingLineCollection);

			var dimensionalWeight = record._200000.DimensionalWeight;
			if (dimensionalWeight != 0)
			{
				result.TotalVolume = Core.Constants.Volume.ConvertSafe(dimensionalWeight, Core.Constants.Weight.Kilograms, Core.Constants.Volume.CubicMetres);
				result.TotalVolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			}

			var isZeroLinePrice = totalLinePrice.IsEmpty;
			totalLineCustomsValue = ZDecimal.Zero;
			var totalDuty = ZDecimal.Zero;
			var totalGST = ZDecimal.Zero;
			if (packingLineCollection.Count > 0)
			{
				foreach (var packingLine in packingLineCollection)
				{
					var customsValue = CalculateLineCustomsValue(packingLine, isZeroLinePrice, totalLinePrice, totalCustomsValue);
					var dutyValue = CalculateDutyValue(packingLine, packingLineDutyRateMap, customsValue, level1DataImport.IsExport);
					var gstValue = CalculateGST(customsValue + dutyValue);
					var packingLineAddInfos = packingLine.PackingLineCollection[0].AddInfoGroupCollection[0].AddInfoCollection;
					packingLineAddInfos.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.CustomsValue, customsValue.ToString(2)));
					packingLineAddInfos.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.DutyValue, dutyValue.ToString(2)));
					packingLineAddInfos.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.TaxValue, gstValue.ToString(2)));

					totalLineCustomsValue += customsValue;
					totalDuty += dutyValue;
					totalGST += gstValue;
				}
			}
			else
			{
				totalLineCustomsValue = totalCustomsValue;
				totalGST = CalculateGST(totalLineCustomsValue);
			}

			AddCharge(result.CommercialInfo.CommercialChargeCollection, Constants.ChargeType.CustomsValue, totalLineCustomsValue, LocalCurrency.RX_Code, houseBillNumber);
			var cons = consignee;
			result.SetEntryInstructionCollection(() => CreateEntryInstructionDataForBill(totalDuty, totalGST, cons, consignor));
			return result;
		}

		internal ZDecimal CalculateGST(ZDecimal amount)
		{
			var valuationDate = level1DataImport.IsExport ? ZDateTime.Today : level1DataImport.CycleDate;
			var gstRate = RefCusTaxOrFeeLoader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, valuationDate)?.ZZF_Value ?? ZDecimal.Zero;

			return new ZDecimal(amount * gstRate).Round(2);
		}

		ZDecimal CalculateLineCustomsValue(PackingLine packingLine, bool isZeroLinePrice, ZDecimal totalLinePrice, ZDecimal totalCustomsValue)
		{
			var ratio = isZeroLinePrice ? ZDecimal.Zero : new ZDecimal(packingLine.LinePrice.Value / totalLinePrice);
			return new ZDecimal(totalCustomsValue * ratio).Round(2);
		}

		ZDecimal CalculateDutyValue(PackingLine packingLine, Dictionary<ZInt, ExciseDutyData> packingLineDutyRateMap, ZDecimal customsValue, bool isExport)
		{
			var dutyValue = ZDecimal.Zero;
			if (packingLineDutyRateMap != null)
			{
				var exciseDutyData = packingLineDutyRateMap[packingLine.Link.Value];
				if (exciseDutyData.GoodsType != Constants.GoodsType.NormalGoods || IsHighValueShipment(customsValue, isExport))
				{
					var calculatedDuty = GetCalculatedExciseOrDuty(exciseDutyData.DutyPerUnitRate, exciseDutyData.DutyUnitOfQuantity, exciseDutyData.DutyPercentRate, customsValue, packingLine.PackQty.Value, packingLine.PackType.Code) +
						GetCalculatedExciseOrDuty(exciseDutyData.ExcisePerUnitRate, exciseDutyData.ExciseUnitOfQuantity, exciseDutyData.ExcisePercentRate, customsValue, packingLine.PackQty.Value, packingLine.PackType.Code);
					dutyValue = calculatedDuty;
				}
			}
			return dutyValue;
		}

		List<AddInfo> CreateAddInfoDataForBill(Level1Record record)
		{
			return new List<AddInfo>
			{
				AddInfo.New(Constants.AddInfoConstants.Bill.MatchingReference, record._202000.PackageTrackingNumber),
				AddInfo.New(AsycudaBill.Schema.ABL_BolType, Core.Constants.ShipmentTypes.StandardHouse)
			};
		}

		List<CustomizedField> CreateCustomizedFieldDataForBill(Level1Record record)
		{
			return new List<CustomizedField>
			{
				CustomizedField.New(Constants.CustomizedFieldConstants.Bill.BillingTerms, record._200000.BillingTerms),
				CustomizedField.New(Constants.CustomizedFieldConstants.Bill.ServiceLevel, record._200000.ServiceLevel),
				CustomizedField.New(Constants.CustomizedFieldConstants.Bill.LongTrackingNumber, record._202000.PackageTrackingNumber)
			};
		}

		DataObjectList<PackingLine> CreatePackingLineData(Level1Record record, Dictionary<ZInt, ExciseDutyData> packingLineDutyRateMap, IDataObjectWriterStrategy writerStrategy, out ZDecimal totalLinePrice, out ZInt totalPackQty)
		{
			totalLinePrice = ZDecimal.Zero;
			totalPackQty = ZInt.Zero;
			var list = new DataObjectList<PackingLine> { Content = CollectionContent.Complete };
			var packingLineLink = ZInt.Zero;
			foreach (_500000Line line500000 in record._500000Lines)
			{
				var linePrice = line500000.Price;
				var exciseDutyData = new ExciseDutyData();
				packingLineDutyRateMap.Add(++packingLineLink, exciseDutyData);
				list.Add(CreatePackingLineData(line500000, packingLineLink, exciseDutyData, linePrice, line500000.Quantity.ToZLong(), writerStrategy));
				totalLinePrice += linePrice;
				totalPackQty += line500000.Quantity.ToZInt();
			}
			return list;
		}

		class ExciseDutyData
		{
			public ZDecimal DutyPercentRate;
			public ZDecimal DutyPerUnitRate;
			public ZString DutyUnitOfQuantity;
			public ZDecimal ExcisePercentRate;
			public ZDecimal ExcisePerUnitRate;
			public ZString ExciseUnitOfQuantity;
			public ZString GoodsType;
		}

		ZDecimal GetCalculatedExciseOrDuty(ZDecimal perUnitRate, ZString unitOfQuantity, ZDecimal percentageRate, ZDecimal lineValue, ZLong lineQty, ZString? lineUnitOfQuantity)
		{
			var value = ZDecimal.Zero;
			if (!percentageRate.IsEmpty)
			{
				value = (percentageRate / 100) * lineValue;
			}
			else
			{
				ZString lineUOQ = lineUnitOfQuantity.HasValue ? (ZString)lineUnitOfQuantity : ZString.Empty;
				if (unitOfQuantity == lineUOQ)
				{
					value = perUnitRate * lineQty;
				}
			}

			return value.Round(2);
		}

		void SetHighValueRequirements(OrgHeader consignee, Shipment shipment)
		{
			if (!level1DataImport.IsExport && SGDecisionSupporter.IsMajorExporter(consignee))
			{
				foreach (var packingLine in shipment.PackingLineCollection)
				{
					var countryPackLineGoodsType = packingLine.PackingLineCollection[0].AddInfoGroupCollection[0].AddInfoCollection.SingleOrDefault(x => x.Key.Value == Constants.AddInfoConstants.PackedItem.GoodsType && x.Value.Value == Constants.GoodsType.NormalGoods);
					if (countryPackLineGoodsType != null)
					{
						countryPackLineGoodsType.Value = Constants.GoodsType.MajorExporter;
					}
				}
			}
		}

		void SetLowValueRequirements(bool isTradeNetRequired, Shipment shipment)
		{
			if (!isTradeNetRequired)
			{
				var billGoodsDescription = shipment.GoodsDescription ?? ZString.Empty;
				var updatedTotalCustomsValue = RemoveInsuranceAndUpdateCustomsValue(shipment);
				var totalLinePrice = GetShipmentCommercialCharge(shipment, Constants.ChargeType.ExWorks).Amount.Value;
				var totalLineCustomsValue = ZDecimal.Zero;
				var totalLineGST = ZDecimal.Zero;
				if (shipment.PackingLineCollection.Count > 0)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						RemoveTariffAndUpdateCustomsPackageDetails(packingLine);
						UpdateEmptyGoodsDescription(packingLine, billGoodsDescription);
						if (!level1DataImport.IsExport)
						{
							var customsValue = CalculateLineCustomsValue(packingLine, totalLinePrice.IsEmpty, totalLinePrice, updatedTotalCustomsValue);
							var gstValue = CalculateGST(customsValue);
							UpdatePackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.CustomsValue, customsValue.ToString(2));
							UpdatePackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.DutyValue, "0.00");
							UpdatePackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.TaxValue, gstValue.ToString(2));

							totalLineCustomsValue += customsValue;
							totalLineGST += gstValue;
						}
					}
				}
				else
				{
					totalLineCustomsValue = updatedTotalCustomsValue;
					totalLineGST = CalculateGST(totalLineCustomsValue);
				}
				UpdateChargeAndEntryInstruction(shipment, totalLineCustomsValue, totalLineGST);
				UpdateEntryInstructionAddInfo(shipment, SGAccessAddInfoConstants.BillCountry.PartyIndicator, ZString.Empty);
			}
		}

		void PopulateCycleDetails(Shipment shipment, OrgHeader consignee)
		{
			if (EnableAutoPopulateCycleDetailsToImportGlobalManifestBills && !level1DataImport.IsExport)
			{
				AddEntryInstructionAddInfo(shipment, Constants.AddInfoConstants.BillCountry.CycleDate, Customs.Business.BaseAddInfo.GetStringRepresentation(level1DataImport.CycleDate));
				AddEntryInstructionAddInfo(shipment, Constants.AddInfoConstants.BillCountry.CycleNumber, GetCycleNumber(shipment, consignee));
			}
		}

		ZString GetCycleNumber(Shipment shipment, OrgHeader consignee)
		{
			return ShouldCycleNumberBeAutoNominated(shipment, consignee)
				? (ZString)"1"
				: level1DataImport.CycleNumber;
		}

		void UpdateChargeAndEntryInstruction(Shipment shipment, ZDecimal totalLineCustomsValue, ZDecimal totalLineGST)
		{
			if (!level1DataImport.IsExport)
			{
				var totalCustomsValueCommercialCharge = GetShipmentCommercialCharge(shipment, Constants.ChargeType.CustomsValue);
				totalCustomsValueCommercialCharge.Amount = totalLineCustomsValue;
				UpdateEntryInstructionAddInfo(shipment, Constants.AddInfoConstants.BillCountry.DutyAmount, ZDecimal.Zero);
				UpdateEntryInstructionAddInfo(shipment, Constants.AddInfoConstants.BillCountry.TaxAmount, totalLineGST);
			}
		}

		void UpdateEmptyGoodsDescription(PackingLine packingLine, ZString billGoodsDescription)
		{
			var packLineGoodsDescription = packingLine.GoodsDescription;
			if (!packLineGoodsDescription.HasValue || packLineGoodsDescription.Value.IsEmpty)
			{
				packingLine.GoodsDescription = billGoodsDescription;
				packingLine.PackingLineCollection[0].GoodsDescription = billGoodsDescription;
			}
		}

		void RemoveTariffAndUpdateCustomsPackageDetails(PackingLine packingLine)
		{
			var tariff = packingLine.PackingLineCollection[0].HarmonisedCode;
			if (tariff.HasValue && !tariff.Value.IsEmpty)
			{
				packingLine.PackingLineCollection[0].HarmonisedCode = ZString.Empty;
				var customsPackDetails = SGPackQuantityAndUnitConverter.GetCustomsPackDetails(FactoryProvider.Current, UnitConverter, null, packingLine.PackQty.Value, packingLine.PackType.Code.Value);
				UpdatePackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.CustomsQty, !customsPackDetails.Quantity.IsEmpty ? customsPackDetails.Quantity.ToString(5) : "1");
				UpdatePackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.CustomsUQ, customsPackDetails.UnitOfQuantity);
			}
			else
			{
				ZDecimal customsQty;
				if (ZDecimal.TryParse(GetPackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.CustomsQty), out customsQty) && customsQty.IsEmpty)
				{
					UpdatePackingLineValue(packingLine, Constants.AddInfoConstants.PackedItem.CustomsQty, "1");
				}
			}
		}

		ZDecimal RemoveInsuranceAndUpdateCustomsValue(Shipment shipment)
		{
			var localCurrencyCode = LocalCurrencyCode;
			var totalCustomsValueCommercialCharge = GetShipmentCommercialCharge(shipment, Constants.ChargeType.CustomsValue);
			var totalCustomsValue = totalCustomsValueCommercialCharge.Amount.Value;
			var insuranceCommercialCharge = GetShipmentCommercialCharge(shipment, Constants.ChargeType.InternationalInsurance);
			var insuranceCurrencyCode = insuranceCommercialCharge.Currency.Code.Value;
			var insuranceAmountInLocalCurrency = insuranceCurrencyCode == localCurrencyCode ? insuranceCommercialCharge.Amount.Value : GetValueInLocalCurrency(insuranceCommercialCharge.Amount.Value, insuranceCurrencyCode);
			insuranceCommercialCharge.Amount = 0m;
			insuranceCommercialCharge.Currency = new Currency() { Code = localCurrencyCode };
			if (!level1DataImport.IsExport && totalCustomsValue > 0)
			{
				var newTotalCustomsValue = Math.Max(totalCustomsValue - insuranceAmountInLocalCurrency, ZDecimal.Zero);
				totalCustomsValueCommercialCharge.Amount = newTotalCustomsValue;
				totalCustomsValue = newTotalCustomsValue;
			}
			return totalCustomsValue;
		}

		CommercialCharge GetShipmentCommercialCharge(Shipment shipment, ZString chargeCode) => shipment.CommercialInfo.CommercialChargeCollection.Single(x => x.ChargeType.Code.Value == chargeCode);

		ZString GetPackingLineValue(PackingLine packingLine, string addInfoKey) => packingLine.PackingLineCollection[0].AddInfoGroupCollection[0].AddInfoCollection.Single(x => x.Key.Value == addInfoKey).Value.Value;

		void UpdatePackingLineValue(PackingLine packingLine, string addInfoKey, ZString updatedValue) => packingLine.PackingLineCollection[0].AddInfoGroupCollection[0].AddInfoCollection.Single(x => x.Key.Value == addInfoKey).Value = updatedValue;

		void UpdateEntryInstructionAddInfo(Shipment shipment, string addInfoKey, ZDecimal updatedValue) => shipment.EntryInstructionCollection[0].AddInfoCollection.Single(x => x.Key.Value == addInfoKey).Value = updatedValue.ToString(2);

		void UpdateEntryInstructionAddInfo(Shipment shipment, string addInfoKey, ZString updatedValue)
		{
			var addInfo = shipment.EntryInstructionCollection[0].AddInfoCollection.FirstOrDefault(x => x.Key.Value == addInfoKey);
			if (addInfo != null)
			{
				addInfo.Value = updatedValue;
			}
		}

		void AddEntryInstructionAddInfo(Shipment shipment, string addInfoKey, ZString updatedValue) => shipment.EntryInstructionCollection[0].AddInfoCollection.Add(AddInfo.New(addInfoKey, updatedValue));

		PackingLine CreatePackingLineData(_500000Line line500000, ZInt packingLineLink, ExciseDutyData exciseDutyData, ZDecimal linePrice, ZLong packQty, IDataObjectWriterStrategy writerStrategy)
		{
			var packType = line500000.UnitOfQuantity;
			var description = line500000.Description;
			var result = new PackingLine(writerStrategy)
			{
				Link = packingLineLink,
				PackQty = packQty,
				PackType = new PackageType() { Code = packType },
				GoodsDescription = description,
				LinePrice = linePrice,
				LinePriceCurrency = new Currency() { Code = line500000.CurrencyCode }
			};

			result.SetPackingLineCollection(() => CreatePackingLineDataForPackage(line500000, exciseDutyData, description, packQty, packType, writerStrategy));
			return result;
		}

		List<PackingLine> CreatePackingLineDataForPackage(_500000Line line500000, ExciseDutyData exciseDutyData, ZString description, ZLong packQty, ZString packType, IDataObjectWriterStrategy writerStrategy)
		{
			var goodsType = Constants.GoodsType.NormalGoods;
			lineTariffMap.TryGetValue(line500000, out var tariff);
			var customsPackDetails = SGPackQuantityAndUnitConverter.GetCustomsPackDetails(FactoryProvider.Current, UnitConverter, tariff, packQty, packType);
			var countryOfOrigin = new Country() { Code = !line500000.CountryOfOrigin.IsEmpty ? line500000.CountryOfOrigin : line500000.OriginCountry };
			var countryOfOriginCode = countryOfOrigin.Code.Value.IsEmpty ? (ZString)Core.Constants.CountryCodes.Singapore : countryOfOrigin.Code.Value;
			if (tariff != null)
			{
				goodsType = GetGoodsType(tariff);
				var dutyRate = GetDutyRate(tariff, countryOfOriginCode);
				var exciseRate = tariff.GetExciseRate(ZDateTime.Today);

				if (dutyRate != null)
				{
					var tariffRate = tariff.GetRate(dutyRate);
					exciseDutyData.DutyPercentRate = tariffRate.PercentageRate;
					exciseDutyData.DutyPerUnitRate = tariffRate.UnitRate;
					exciseDutyData.DutyUnitOfQuantity = tariffRate.UnitQty;
				}

				exciseDutyData.ExcisePercentRate = exciseRate.PercentageRate;
				exciseDutyData.ExcisePerUnitRate = exciseRate.UnitRate;
				exciseDutyData.ExciseUnitOfQuantity = exciseRate.UnitQty;

				exciseDutyData.GoodsType = goodsType;
			}

			var packageLine = new PackingLine(writerStrategy)
			{
				CountryOfOrigin = countryOfOrigin,
				GoodsDescription = description,
				HarmonisedCode = tariff?.ZZ1_TariffCode ?? ZString.Empty,
				PackQty = packQty,
				PackType = new PackageType() { Code = packType },
			};
			packageLine.SetAddInfoGroupCollection(() => CreateAddInfoGroupForPackageCountry(line500000, goodsType, customsPackDetails.Quantity, customsPackDetails.UnitOfQuantity));

			var list = new List<PackingLine>();
			list.Add(packageLine);
			return list;
		}

		RateView GetDutyRate(TariffView tariff, ZString tradeGroupCountry)
		{
			var rateCriteria = new SpecificRateSelectionCriteria(
				tradeGroupCountry,
				dataGrouping: Core.Constants.CountryCodes.Singapore,
				primaryPreference: Constants.StandardPreference,
				concessionOrder: ZString.Empty,
				additionalCodes: null,
				effectiveDate: ZDateTime.Today,
				rateType: ZString.Empty,
				rateCode: Customs.Universal.Constants.RateTypes.Duty);
			return tariff.GetApplicableRates(rateCriteria).FirstOrDefault();
		}

		ZString GetGoodsType(TariffView tariff)
		{
			var goodsType = Constants.GoodsType.NormalGoods;
			if (level1DataImport.IsExport ? tariff.IsUnderExportControl(EffectiveDateForDutyRate) : tariff.IsUnderImportControl(EffectiveDateForDutyRate))
			{
				goodsType = Constants.GoodsType.ControlledGoods;
			}
			else
			{
				var commodityType = tariff.GetAttribute(SGConstants.Attributes.Names.CommodityType)?.ZZ3_Value ?? string.Empty;
				if (commodityType == CommodityTypeList.Codes.Vehicle || commodityType == CommodityTypeList.Codes.Alcohol || commodityType == CommodityTypeList.Codes.Tobacco)
				{
					goodsType = Constants.GoodsType.DutiableGoods;
				}
			}

			return goodsType;
		}

		public ZDateTime EffectiveDateForDutyRate
		{
			get
			{
				if (!effectiveDateForDutyRate.HasValue)
				{
					var effectiveDate = level1DataImport.IsExport ? level1DataImport.DepartureDate : level1DataImport.ArrivalDate;
					effectiveDateForDutyRate = effectiveDate.IsValid ? effectiveDate : ZDateTime.Today;
				}

				return effectiveDateForDutyRate.Value;
			}
		}
		ZDateTime? effectiveDateForDutyRate;

		List<AddInfoGroup> CreateAddInfoGroupForPackageCountry(_500000Line line500000, ZString goodsType, ZDecimal customsQty, ZString customsUnitOfQty)
		{
			var list = new List<AddInfoGroup>();
			list.Add(new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.AddInfoConstants.PackedItem.PackingItemAddInfoType },
				AddInfoCollection = CreateAddInfoDataForPackageCountry(line500000, goodsType, customsQty, customsUnitOfQty)
			});

			return list;
		}

		List<AddInfo> CreateAddInfoDataForPackageCountry(_500000Line line500000, ZString goodsType, ZDecimal customsQty, ZString customsUnitOfQty)
		{
			var list = new List<AddInfo>();
			list.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.Country, Core.Constants.CountryCodes.Singapore));
			list.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.GoodsType, goodsType));
			list.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.CountryOfDestination, line500000.CountryOfUltimateDestination));
			list.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.CustomsQty, customsQty.ToString(5)));
			list.Add(AddInfo.New(Constants.AddInfoConstants.PackedItem.CustomsUQ, customsUnitOfQty));
			return list;
		}

		public OrganizationAddressesForRecord CreateOrganizationAddressDataForBill(Level1Record record)
		{
			var houseBillNumber = record.IsGCCLead() ? record._401000.LeadTrackingNumberForGCCShipment : record._200000.TrackingNumber;
			AddToLog(Invariant($"Matching Organizations for Bill: {houseBillNumber}"));
			var list = new List<OrganizationAddress>();
			var consignor = CreateConsignor(list, record._300000);
			var consignee = CreateConsignee(list, record._400000, record._401000);
			return new OrganizationAddressesForRecord(list, consignor, consignee);
		}

		public class OrganizationAddressesForRecord
		{
			public List<OrganizationAddress> OrganizationAddresses { get; }
			public OrgHeader Consignee { get; }
			public OrgHeader Consignor { get; }

			public OrganizationAddressesForRecord(List<OrganizationAddress> organizationAddresses, OrgHeader consignor, OrgHeader consignee)
			{
				OrganizationAddresses = organizationAddresses;
				Consignor = consignor;
				Consignee = consignee;
			}
		}

		OrgHeader CreateConsignee(List<OrganizationAddress> list, _400000Line line400000, _401000Line line401000)
		{
			var organisationLine = line401000 != null && line401000.Country == Core.Constants.CountryCodes.Singapore ? (_OrganisationLine)line401000 : line400000;
			return CreateOrganisation(list, nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress), organisationLine, (org) => org.Country == org.DestinationCountry ? org.DestinationPort : string.Empty);
		}

		OrgHeader CreateOrganisation(List<OrganizationAddress> list, ZString addressType, _OrganisationLine organisationLine, Func<_OrganisationLine, string> getPortCode)
		{
			OrgHeader org = null;
			if (organisationLine != null)
			{
				var portCode = getPortCode(organisationLine);
				list.Add(CreateOrganisation(addressType, organisationLine.Name, organisationLine.Street1, organisationLine.Street2, organisationLine.City, organisationLine.PostCode, organisationLine.State, organisationLine.Country, organisationLine.Phone, organisationLine.Fax, organisationLine.ContactName, organisationLine.AccountNumber, portCode, out org));
			}
			return org;
		}

		OrgHeader CreateConsignor(List<OrganizationAddress> list, _300000Line line300000)
		{
			return CreateOrganisation(list, nameof(MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress), line300000, (org) => org.Country == org.OriginCountry ? org.OriginPort : string.Empty);
		}

		OrganizationAddress CreateOrganisation(ZString addressType, ZString name, ZString street1, ZString street2, ZString city, ZString postCode, ZString state, ZString country, ZString phone, ZString fax, ZString contactName, ZString accountNumber, ZString portCode, out OrgHeader org)
		{
			var unloco = GetUNLOCOFromUPSMapping(portCode)?.RL_Code ?? ZString.Empty;
			org = OrganisationMatching.TryMatchOrganisation(name, unloco, city, phone, fax, postCode, state, street1, street2, country, accountNumber);
			var organisation = CreateOrganisation(addressType, name, street1, street2, city, postCode, state, country, phone, fax, contactName, accountNumber, org);
			return organisation;
		}

		OrganizationAddress CreateOrganisation(ZString addressType, ZString name, ZString street1, ZString street2, ZString city, ZString postCode, ZString state, ZString country, ZString phone, ZString fax, ZString contactName, ZString accountNumber, OrgHeader org)
		{
			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance)
			{
				AddressType = addressType,
				CompanyName = name,
				Address1 = street1,
				Address2 = street2,
				City = city,
				Postcode = postCode,
				State = state,
				Country = new Country() { Code = country },
				Phone = phone,
				Fax = fax,
				Contact = contactName
			};

			if (org != null)
			{
				result.OrganizationCode = org.OH_Code;
			}
			else
			{
				result.AddressOverride = true;
			}

			if (!accountNumber.IsEmpty)
			{
				result.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>(new[]
				{
					new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber },
						CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Singapore },
						Value = accountNumber
					}
				}));
			}

			return result;
		}

		List<EntryInstruction> CreateEntryInstructionDataForBill(ZDecimal totalDuty, ZDecimal totalGST, OrgHeader consignee, OrgHeader consignor)
		{
			var result = new List<EntryInstruction>();
			result.Add(new EntryInstruction()
			{
				Style = level1DataImport.IsExport ? Constants.Nature.Export : Constants.Nature.Import,
				Link = 1,
				AddInfoCollection = CreateEntryInstructionAddInfoDataForBill(totalDuty, totalGST, consignee, consignor)
			});

			return result;
		}

		List<AddInfo> CreateEntryInstructionAddInfoDataForBill(ZDecimal totalDuty, ZDecimal totalGST, OrgHeader consignee, OrgHeader consignor)
		{
			var list = new List<AddInfo>();
			list.Add(AddInfo.New(Constants.AddInfoConstants.BillCountry.DutyAmount, totalDuty.ToString(2)));
			list.Add(AddInfo.New(Constants.AddInfoConstants.BillCountry.TaxAmount, totalGST.ToString(2)));
			if (!level1DataImport.IsExport)
			{
				if (consignee == null)
				{
					list.Add(AddInfo.New(SGAccessAddInfoConstants.BillCountry.PartyStatus, SGPartyStatusList.Codes.N));
				}
				else
				{
					var consigneeCustomsCodes = consignee.CustomsCodes;
					var partyIndicator = consigneeCustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, Core.Constants.CountryCodes.Singapore);
					var partyStatusType = consigneeCustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.PartyStatusType, Core.Constants.CountryCodes.Singapore);
					if (!consignee.MiscServ?.OM_IMPaymentMethod.IsEmpty ?? false)
					{
						list.Add(AddInfo.New(SGAccessAddInfoConstants.BillCountry.PayeeIndicator, consignee.MiscServ.OM_IMPaymentMethod));
					}
					if (!partyStatusType.IsEmpty)
					{
						list.Add(AddInfo.New(SGAccessAddInfoConstants.BillCountry.PartyStatus, partyStatusType));
					}
					if (!partyIndicator.IsEmpty)
					{
						list.Add(AddInfo.New(SGAccessAddInfoConstants.BillCountry.PartyIndicator, partyIndicator));
					}
				}
			}
			if (level1DataImport.IsExport && consignor != null)
			{
				var partyIndicator = consignor.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, Core.Constants.CountryCodes.Singapore);
				if (!partyIndicator.IsEmpty)
				{
					list.Add(AddInfo.New(SGAccessAddInfoConstants.BillCountry.PartyIndicator, partyIndicator));
				}
			}
			return list;
		}

		protected override ZString GetCountryCodeForUPSMapping()
		{
			return Core.Constants.CountryCodes.Singapore;
		}

		ZInt? CreateTotalNoOfPacks(ZInt piecesManifested)
		{
			return piecesManifested.IsEmpty ? (ZInt)1 : piecesManifested;
		}

		UNLOCO CreatePortOfOrigin(Level1Record record)
		{
			var originPort = record._200000.OriginPort;
			var originUNLoco = GetUNLOCOFromUPSMapping(originPort);
			var code = ZString.Empty;
			ZString? name = null;
			if (originUNLoco == null)
			{
				AddToLog(string.Format(CultureInfo.InvariantCulture, "Unable to locate UNLOCO for {0} OriginCountry: {1} OriginPort: {2}", record._202000.PackageTrackingNumber, record._200000.OriginCountry, originPort));
				code = originPort.PadRight(5, '*');
				name = string.Format(CultureInfo.InvariantCulture, "Country: {0} Port: {1}", record._200000.OriginCountry, originPort);
			}
			else
			{
				code = originUNLoco.RL_Code;
			}

			return new UNLOCO() { Code = code, Name = name };
		}

		UNLOCO CreatePortOfDestination(Level1Record record)
		{
			ZString code;
			ZString? name = null;
			if (record._200000.DestinationCountry != Core.Constants.CountryCodes.Singapore)
			{
				var destinationPort = record._200000.DestinationPort;
				var destinationUNLoco = GetUNLOCOFromUPSMapping(destinationPort);
				if (destinationUNLoco == null)
				{
					AddToLog(string.Format(CultureInfo.InvariantCulture, "Unable to locate UNLOCO for {0} DestinationCountry: {1} DestinationPort: {2}", record._202000.PackageTrackingNumber, record._200000.DestinationCountry, destinationPort));
					code = destinationPort.PadRight(5, '*');
					name = string.Format(CultureInfo.InvariantCulture, "Country: {0} Port: {1}", record._200000.DestinationCountry, destinationPort);
				}
				else
				{
					code = destinationUNLoco.RL_Code;
				}
			}
			else
			{
				code = level1DataImport.PortOfDischarge;
			}

			return new UNLOCO() { Code = code, Name = name };
		}

		ZString? CreateGoodsDescription(_500000Line line500000, string goodsDescription)
		{
			return line500000 == null ? (goodsDescription == null ? goodsDescription : null) : (string)line500000.Description;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		CommercialInfo CreateCommercialInfo(Level1Record record, ZDecimal discount, ZDecimal totalLinePrice, OrgHeader consignor, OrgHeader consignee, out ZDecimal totalCustomsValue)
		{
			var charges = new List<CommercialCharge>();
			var currencyCodeForInvoiceTotal = record._200000.CurrencyCodeForInvoiceTotal;
			var billNumber = record._200000 != null ? new ZString(record._200000.TrackingNumber) : ZString.Empty;
			var goodsValue = AddChargeAndGetValueInLocalCurrency(charges, Constants.ChargeType.ExWorks, totalLinePrice, currencyCodeForInvoiceTotal, billNumber);
			var transportValue = AddChargeAndGetValueInLocalCurrency(charges, Constants.ChargeType.InternationalFreight, record._200000.Freight, currencyCodeForInvoiceTotal, billNumber);
			var transportValueReCalculated = false;
			if (transportValue.IsEmpty && EnableFreightAutoRatingInLevelOneImport)
			{
				var freightCharge = charges.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Constants.ChargeType.InternationalFreight);
				if (freightCharge != null)
				{
					JobDeclaration tradeNetDeclaration = null;
					try
					{
						tradeNetDeclaration = new TradeNetDeclarationCreator(FactoryProvider.Current, level1DataImport, record, level1DataImport.IsExport, consignee, consignor, goodsValue, discount, currencyCodeForInvoiceTotal, lineTariffMap).CreateTradeNet(ReadOnlyFactoryForCreateTradeNet);
						var freightAmountAndCurrency = new FreightRateCalculator().CalculateFreightRateOnDeclaration(tradeNetDeclaration);
						if (!freightAmountAndCurrency.Value.IsEmpty)
						{
							var valueInInvoiceCurrency = ConvertValueInCurrencies(freightAmountAndCurrency.Key, freightAmountAndCurrency.Value, currencyCodeForInvoiceTotal);
							if (!valueInInvoiceCurrency.IsEmpty)
							{
								transportValue = valueInInvoiceCurrency;
								freightCharge.Amount = transportValue;
								transportValue = GetValueInLocalCurrency(transportValue, currencyCodeForInvoiceTotal);
								transportValueReCalculated = true;
							}
						}
					}
					finally
					{
						if (tradeNetDeclaration != null)
						{
							if (transportValueReCalculated)
							{
								AddToLog(ZString.Format("Freight and Insurance values have been recalculated for bill '{0}' based on {1} Freight rating system.", billNumber, BrandingFactory.Instance.ProductName));
							}
							else
							{
								AddToLog(ZString.Format("System is unable to recalculate Freight and Insurance values from Level 1 for bill '{0}', please check your {1} Freight rating system.", billNumber, BrandingFactory.Instance.ProductName));
							}
						}
						else
						{
							AddToLog(ZString.Format("Unable to create TradeNet from Level 1 for bill '{0}', system is unable to recalculate Freight and Insurance values based on {1} Freight rating system.", billNumber, BrandingFactory.Instance.ProductName));
						}
					}
				}
			}
			var discountValue = AddChargeAndGetValueInLocalCurrency(charges, Constants.ChargeType.Discount, discount, currencyCodeForInvoiceTotal, billNumber);
			var otherChargesValue = AddChargeAndGetValueInLocalCurrency(charges, Constants.ChargeType.OtherCharges, record._200000.OtherCharges, currencyCodeForInvoiceTotal, billNumber);
			var insurance = record._200000.Insurance;
			var insuranceCurrency = currencyCodeForInvoiceTotal;
			if (insurance.IsEmpty || transportValueReCalculated)
			{
				insurance = new ZDecimal((goodsValue + transportValue) * Constants.InsurancePercentage).Round(2);
				insuranceCurrency = LocalCurrencyCode;
			}

			var insuranceValue = AddChargeAndGetValueInLocalCurrency(charges, Constants.ChargeType.InternationalInsurance, insurance, insuranceCurrency, billNumber);
			var totalValue = goodsValue + otherChargesValue - discountValue + (level1DataImport.IsExport ? 0m : transportValue + insuranceValue);
			if (totalValue < 0)
			{
				if (!NegativeBillNumberList.Contains(billNumber))
				{
					NegativeBillNumberList.Add(billNumber);
					string valueToDisplay = string.Format(CultureInfo.CurrentCulture, totalValue.ToString());
					AddToLog(string.Format(CultureInfo.CurrentCulture, "Bill {0} had a negative total value calculation that has been set to zero: (calculated value was {1})", billNumber, valueToDisplay));
				}

				totalValue = 0;
			}

			totalCustomsValue = totalValue;
			return new CommercialInfo()
			{
				CommercialChargeCollection = charges
			};
		}

		void AddCharge(List<CommercialCharge> charges, ZString chargeType, ZDecimal amount, ZString currencyCode, ZString billNumber)
		{
			if (amount < 0)
			{
				if (!NegativeBillNumberList.Contains(billNumber))
				{
					NegativeBillNumberList.Add(billNumber);
					string valueToDisplay = string.Format(CultureInfo.CurrentCulture, amount.ToString());
					AddToLog(string.Format(CultureInfo.CurrentCulture, "Bill {0} had a negative charge value calculation that has been set to zero: (calculated value was {1})", billNumber, valueToDisplay));
				}

				amount = 0;
			}

			charges.Add(new CommercialCharge()
			{
				ChargeType = new CodeDescriptionPair() { Code = chargeType },
				Amount = amount > 0 ? amount : 0,
				Currency = new Currency() { Code = currencyCode }
			});
		}

		ZDecimal AddChargeAndGetValueInLocalCurrency(List<CommercialCharge> charges, ZString chargeType, ZDecimal amount, ZString currencyCode, ZString billNumber)
		{
			AddCharge(charges, chargeType, amount, currencyCode, billNumber);
			return GetValueInLocalCurrency(amount, currencyCode);
		}

		protected override CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = CurrencyConverter.New(FactoryProvider.Current, ZDateTime.Now, ZArchitecture.Core.ExchangeRateType.Customs, 0);
				}

				return fCurrencyConverter;
			}
		}

		void NotifyShipmentsNotImportedAndDeclarationDetails()
		{
			if (!SuccessfullTradenetDeclarationList.IsEmpty)
			{
				var tradenetDetails = string.Format(
					CultureInfo.InvariantCulture,
					"\r\nTradeNet Declarations\r\n" +
					"===================\r\n" +
					"Successfully created for the following bills:\r\n" +
					"{0}\r\n" +
					"\r\n",
					SuccessfullTradenetDeclarationList.ToStringWithNewLineBetweenAppends());
				AddToLog(tradenetDetails);
			}

			if (!ShipmentsNotImported.IsEmpty)
			{
				var notImported = string.Format(
					CultureInfo.InvariantCulture,
					"Shipments Not Imported\r\n" +
					"=================================\r\n" +
					"{0}\r\n" +
					"\r\n",
					ShipmentsNotImported.ToStringWithNewLineBetweenAppends());
				AddToLog(notImported);
			}
		}

		bool IsHighValueShipment(ZDecimal customsValue, bool isExport = false)
		{
			var factory = level1DataImport == null ? new BusinessObjectFactory() : level1DataImport.Factory;
			var deminimusValue = isExport ? SGDecisionSupporter.GetSGCustomsExportDeminimusValue(factory) : SGDecisionSupporter.GetCurrentSGCustomsDeminimusValue(factory);
			return customsValue >= deminimusValue;
		}

		ReadOnlyBusinessObjectFactory ReadOnlyFactoryForCreateTradeNet => readOnlyFactory ?? (readOnlyFactory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = "Create TradeNet" });
		ReadOnlyBusinessObjectFactory readOnlyFactory;

		RefCusTaxOrFee.Loader RefCusTaxOrFeeLoader => refCusTaxOrFeeLoader ?? (refCusTaxOrFeeLoader = new RefCusTaxOrFee.Loader(FactoryProvider.Current));
		RefCusTaxOrFee.Loader refCusTaxOrFeeLoader;

		#region ISimpleLogger

		void ISimpleLogger.Log(LogType type, string message) => AddToLog(message);

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs => Enumerable.Empty<ISimpleLog>();

		protected override string LocalCurrencyCode => Core.Constants.CurrencyCodes.Singapore;

		#endregion

		#endregion

		#region UnitConverters

		public UnitConverter UnitConverter => fUnitConverter ?? (fUnitConverter = new UnitConverter(this));
		UnitConverter fUnitConverter;

		public SGPackQuantityAndUnitConverter SGPackQuantityAndUnitConverter => fSGPackQuantityAndUnitConverter ?? (fSGPackQuantityAndUnitConverter = new SGPackQuantityAndUnitConverter());
		SGPackQuantityAndUnitConverter fSGPackQuantityAndUnitConverter;

		#endregion

		#region IUnitConverterDataProvider

		ZString IUnitConverterDataProvider.CountryCode => Core.Constants.CountryCodes.Singapore;

		BusinessObjectFactory IUnitConverterDataProvider.Factory => FactoryProvider.Current;

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits() => Array.Empty<IUnitConverter>();

		MasterFiles.Business.OrgSupplierPart IUnitConverterDataProvider.Product => null;

		ZGuid IUnitConverterDataProvider.SupplierFK => ZGuid.Empty;

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions => false;

		ZString IUnitConverterDataProvider.Type => RPTypeList.Codes.GlobalManifestLine;

		#endregion

	}
}
