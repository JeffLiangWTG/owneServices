using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1DataFileImporterForAU : Level1DataFileImporterCore
	{
		public Level1DataFileImporterForAU(Level1DataImport level1DataImport, INotifications notify)
			: base(level1DataImport, notify)
		{
		}

		#region Implementation

		protected override void PrepareForLoadCore()
		{
			ResetDuplicateShipmentDict();
		}

		void ResetDuplicateShipmentDict()
		{
			duplicateShipmentDict = null;
		}

		[SuppressMessage("Microsoft.Performance", "CA1815", Justification = "Instances are not compared")]
		public struct ShipmentInfo
		{
			public ShipmentInfo(string houseBill, string masterBill)
			{
				HouseBill = houseBill;
				MasterBill = masterBill;
			}

			public readonly string HouseBill;
			public readonly string MasterBill;
		}

		public Dictionary<string, ShipmentInfo> DuplicateShipmentDict
		{
			get
			{
				if (duplicateShipmentDict == null)
				{
					duplicateShipmentDict = new Dictionary<string, ShipmentInfo>();
				}
				return duplicateShipmentDict;
			}
		}
		Dictionary<string, ShipmentInfo> duplicateShipmentDict;

		protected override int GetDuplicatePercentage()
		{
			return TotalNoOfShipments == 0 ? 0 : (DuplicateShipmentDict.Count * 100) / TotalNoOfShipments;
		}

		protected override bool CheckForDuplicateShipment(Level1Record record)
		{
			if (!record.IsGCCChild())
			{
				string longTrackingNumber = record._202000 != null ? record._202000.PackageTrackingNumber : ZString.Empty;
				string shortTrackingNumber = record._200000 != null ? record._200000.TrackingNumber : ZString.Empty;

				if (!string.IsNullOrEmpty(longTrackingNumber) && !string.IsNullOrEmpty(shortTrackingNumber))
				{
					if (DuplicateShipmentDict.ContainsKey(longTrackingNumber))
					{
						return true;
					}

					return CheckForAPreviouslyLoadedShipment(longTrackingNumber, shortTrackingNumber);
				}
			}
			return false;
		}

		protected override ZString GetCountryCodeForUPSMapping()
		{
			return Core.Constants.CountryCodes.Australia;
		}

		protected override void ProcessData()
		{
			AirCargoValueObjectDataAdapter adapter = new AirCargoValueObjectDataAdapter();
			CusMAWB mawb = GetCusMAWB(adapter);
			SaveLog(mawb);
			Thread.Sleep(0);

			SaveCore(adapter, mawb);

			FactoryProvider.SaveCurrentAndCreateNew();
			ProcessGCCChildren();
		}

		#region Save Tasks
		protected bool ContinueProcessingHouseBill(CusMAWB cusMAWB, Level1Record record, ZString housebill)
		{
			bool isLeadOrNotConsolidated = record._200000 != null && (record._200000.IsGCCLead || !record._200000.IsConsolidated);

			return isLeadOrNotConsolidated && !IsHouseBillMasterBillInDatabase(housebill, cusMAWB.PK);
		}

		void SaveLog(CusMAWB cusMAWB)
		{
			cusMAWB.Logs.AddNew(Events.DataImport, Path.GetFileName(level1DataImport.FileName));
		}

		CusMAWB GetCusMAWB(AirCargoValueObjectDataAdapter dataAdapter)
		{
			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, level1DataImport.MasterBill);
			CusMAWB result = FactoryProvider.Current.LoadTop1<CusMAWB>(mAWBFilter);
			if (result == null)
			{
				result = FactoryProvider.Current.New<CusMAWB>();
				Xsd.Consol consol = CreateConsol();

				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider.Current, new NotificationBuffer());
				dataAdapter.ImportMAWB(result, consol, importContext);
				result.CM_MasterHouseBill = level1DataImport.CoLoadMasterBill;
			}
			AddCusMAWBNotes(result);

			return result;
		}

		bool IsHouseBillMasterBillInDatabase(string houseBill, ZGuid masterBillPK)
		{
			ZQuery houseBillMasterBillFilter = new ZQuery(CusHAWBSchema.CS_HAWB, houseBill);
			houseBillMasterBillFilter.AddToFilter(CusHAWBSchema.CS_CM, masterBillPK);
			return FactoryProvider.Current.Exists(typeof(UPECusHAWB), houseBillMasterBillFilter);
		}

		#endregion

		#region Load UPS Specific Data

		protected void LoadUPSSpecificData(UPECusHAWB upeCusHAWB, Level1Record record)
		{
			LoadTrackingNumbers(upeCusHAWB, record);

			upeCusHAWB.ShipmentType = record._200000.ShipmentType;
			upeCusHAWB.DutyType = record._200000.DutyType;
			upeCusHAWB.BillingTerms = record._200000.BillingTerms;

			upeCusHAWB.CS_OtherSystemConsignorCode = (record._300000 != null) ? record._300000.AccountNumber : "";
			upeCusHAWB.CS_OtherSystemConsigneeCode = (record._400000 != null) ? record._400000.AccountNumber : "";
		}

		void AddCusMAWBNotes(CusMAWB cusMAWB)
		{
			AddCustomNote(cusMAWB, "LoadSummaryInformationNote", level1DataImport.LoadSummaryInformation);
			AddCustomNote(cusMAWB, "DuplicateHAWBNote", level1DataImport.DuplicateHAWBsNote);
			AddCustomNote(cusMAWB, Level1DataImport.Schema.SurplusIndicatedNote, level1DataImport.SurplusIndicatedNote);
			AddCustomNote(cusMAWB, Level1DataImport.Schema.FlightNotInScheduleNote, level1DataImport.FlightNotInScheduleNote);
			AddCustomNote(cusMAWB, Level1DataImport.Schema.MasterbillWarningNote, level1DataImport.MasterbillWarningNote);
			AddCustomNote(cusMAWB, Level1DataImport.Schema.ArrivalDateWarningNote, level1DataImport.ArrivalDateWarningNote);
			AddCustomNote(cusMAWB, "FileNameWarningNote", level1DataImport.UnmatchedFilenameNote);
		}

		void AddCustomNote(CusMAWB cusMAWB, string description, string noteData)
		{
			if (!string.IsNullOrEmpty(noteData))
			{
				StmNote note = cusMAWB.Notes.AddNew();
				note.ST_Description = description;
				note.ST_NoteDataAsText = noteData;
				note.ST_IsCustomDescription = true;
			}
		}

		#endregion

		#region Create Xsd

		Xsd.Consol CreateConsol()
		{
			Xsd.Consol result = new Xsd.Consol();
			Xsd.ConsolIdentifier consolIdentifier = result.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = level1DataImport.MasterBill;
			result.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
			Xsd.FlightWithFlightNumber flightInfo = new Xsd.FlightWithFlightNumber();
			flightInfo.FlightNoJourneyNoTruckRegNo = level1DataImport.FlightNumber;
			result.ConsolDetail.Item = flightInfo;
			flightInfo.ETA = level1DataImport.ArrivalDate;
			result.ConsolDetail.PortOfLoading.Port.Value = level1DataImport.PortOfLoading;
			result.ConsolDetail.PortOfDischarge.Port.Value = level1DataImport.PortOfDischarge;

			return result;
		}

		protected virtual Xsd.Shipment CreateShipment(Level1Record record, string housebill)
		{
			Xsd.Shipment result = null;

			if (record._200000 != null)
			{
				result = new Xsd.Shipment();
				Xsd.ShipmentIdentifier shipmentIdentifier = result.ShipmentIdentifier.AddNew();
				shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;
				shipmentIdentifier.Value = level1DataImport.CoLoadMasterBill;

				result.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;

				RefUNLOCO originUNLoco = GetUNLOCOFromUPSMapping(record._200000.OriginPort);
				result.ShipmentDetails.PortOfOrigin.Port = originUNLoco != null ? Xsd.UNLOCO.FromPort(originUNLoco) : Xsd.UNLOCO.FromPortCode(FactoryProvider.Current, record._200000.OriginPort);

				if (record._200000.DestinationCountry != Core.Constants.CountryCodes.Australia)
				{
					RefUNLOCO destinationUNLoco = GetUNLOCOFromUPSMapping(record._200000.DestinationPort);
					result.ShipmentDetails.PortofDestination.Port = destinationUNLoco != null ? Xsd.UNLOCO.FromPort(destinationUNLoco) : Xsd.UNLOCO.FromPortCode(FactoryProvider.Current, record._200000.DestinationPort);
				}
				else
				{
					result.ShipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(FactoryProvider.Current, level1DataImport.PortOfDischarge);
				}

				PopulateGoodsDescription(record, result);
				ZDecimal goodsValue = GetValueInLocalCurrency(record._200000.GoodsValue, record._200000.CurrencyCodeForInvoiceTotal);
				string currencyCode = goodsValue == 0 ? record._200000.CurrencyCodeForInvoiceTotal : Core.Constants.CurrencyCodes.Australia;
				result.ShipmentDetails.GoodsValue = Xsd.FinancialValue.FromAmountAndCurrencyCode(goodsValue, currencyCode);
				result.ShipmentDetails.ServiceLevel = GetServiceLevelForAU(record._200000.ServiceLevel);

				if (record._202000 != null)
				{
					shipmentIdentifier = result.ShipmentIdentifier.AddNew();
					shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
					shipmentIdentifier.Value = housebill;

					if (!record._200000.IsGCCLead)
					{
						ZDecimal weight = record._200000.ShipmentWeightUnit == Core.Constants.Weight.Kilograms ? record._202000.Weight / 10.0m : (decimal)record._202000.Weight;
						result.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(weight, record._200000.ShipmentWeightUnit);
						result.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit(record._202000.PiecesManifested, null);

						if (record._200000.DimensionalWeight != 0)
						{
							result.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(record._200000.DimensionalWeight, null);
						}
						else
						{
							ZDecimal chargableWeight = Core.Constants.Weight.Convert(weight, record._200000.ShipmentWeightUnit, Core.Constants.Weight.Kilograms);
							result.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(chargableWeight, null);
						}
					}
				}

				if (record._200000.IsGCCLead && record._401000 != null)
				{
					ZString weightUnit = record._401000.WeightUnitOfQuantityForGCCShipment;
					ZDecimal weight = weightUnit == Core.Constants.Weight.Kilograms ? record._401000.TotalWeightForGCCShipment / 10.0m : (decimal)record._401000.TotalWeightForGCCShipment;
					weight = ExcludeWeightIfVirtual(record, weight, weightUnit);
					result.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(weight, weightUnit);
					result.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(weight, null);

					result.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit(record._401000.TotalPackageCountForGCCShipment, null);
				}

				Xsd.Organisation organisation = CreateOrganisation(record._300000);
				if (organisation != null)
				{
					result.ShipmentDetails.Consignor = organisation;
				}

				organisation = CreateOrganisation(record._400000);
				if (organisation != null)
				{
					result.ShipmentDetails.Consignee = organisation;
				}

				organisation = CreateOrganisation(record._401000);
				if (organisation != null)
				{
					result.Declaration.Importer = organisation;
				}

				PopulateDeclaration(record, result.Declaration);
			}
			return result;
		}

		ZString GetServiceLevelForAU(ZString originalValue)
		{
			ZString result;
			switch (originalValue)
			{
				case "S":
					result = "28";
					break;
				case "L":
					result = "21";
					break;
				case "5":
					result = "5";
					break;
				default:
					result = "1";
					break;
			}
			return result;
		}

		ZDecimal ExcludeWeightIfVirtual(Level1Record record, ZDecimal weight, ZString weightUnit)
		{
			if (record._200000.IsVirtual && record._202000 != null)
			{
				string virualWeightUnit = record._200000.ShipmentWeightUnit;
				ZDecimal virtualWeight = virualWeightUnit == Core.Constants.Weight.Kilograms ? record._202000.Weight / 10.0m : (decimal)record._202000.Weight;

				if (virualWeightUnit != weightUnit)
				{
					virtualWeight = Core.Constants.Weight.Convert(virtualWeight, virualWeightUnit, weightUnit);
				}

				weight -= virtualWeight;
			}

			return weight;
		}

		Xsd.Organisation CreateOrganisation(_OrganisationLine organisationLine)
		{
			Xsd.Organisation result = null;
			if (organisationLine != null)
			{
				result = new Xsd.Organisation();
				result.OrganisationDetails.Name = organisationLine.Name;
				RefCountry country = RefCountry.LoadFromCountryCode(FactoryProvider.Current, organisationLine.Country);
				if (country != null)
				{
					result.OrganisationDetails.Location.Country = country.RN_DescMultilingual;
				}
				Xsd.OrgAddress address = result.OrganisationDetails.Addresses.AddNew();
				Xsd.AddressCapability capability = address.AddressCapabilities.AddNew();
				capability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
				capability.AddressTypeSpecified = true;
				address.AddressLine1 = organisationLine.Street1;
				address.AddressLine2 = organisationLine.Street2;
				address.PostCode = organisationLine.PostCode;
				address.StateOrProvince = organisationLine.State;
				address.CityOrSuburb = organisationLine.City;

				Xsd.TelephoneNumber telephoneNumber = address.TelephoneNumbers.AddNew();
				telephoneNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
				telephoneNumber.Value = organisationLine.Phone;

				Xsd.OrgContact contact = result.OrganisationDetails.Contacts.AddNew();
				contact.Name = organisationLine.ContactName;
			}
			return result;
		}

		void PopulateDeclaration(Level1Record record, Xsd.Declaration declaration)
		{
			declaration.IsSurplus = level1DataImport.IsSurplus ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			declaration.IsDocuments = (record._200000.ShipmentType == ShipmentTypeCodeDescriptionPairList.Codes.Letter || record._200000.ShipmentType == ShipmentTypeCodeDescriptionPairList.Codes.Documents) ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			declaration.PaymentTerms = record._200000.FreightCollect ? "CCX" : "PPD";
		}

		void PopulateGoodsDescription(Level1Record record, Xsd.Shipment shipment)
		{
			if (record._200000 != null)
			{
				string goodsDescription = record._200000.GoodsDescription;
				if (record._500000Lines.Count > 0)
				{
					if (!record._500000Lines.GoodsDescription.IsEmpty)
					{
						goodsDescription = record._500000Lines.GoodsDescription;
					}
				}
				shipment.ShipmentDetails.GoodsDescription = goodsDescription;
			}
		}

		#endregion

		#region Non Merge Clearance Code
		protected virtual void SaveCore(AirCargoValueObjectDataAdapter adapter, CusMAWB mawb)
		{
			var progressNotification = new ProgressNotification(0);
			int currentRecord = 0;
			foreach (Level1Record record in Level1RecordList)
			{
				if (!record.IsEmpty)
				{
					ProcessHousebills(mawb, record, adapter);

					currentRecord++;
					progressNotification.PercentageComplete = (currentRecord * 100) / totalNoOfRecords;
					UpdateProgress(progressNotification);

					if (currentRecord % ClearFactoryAfterNRecords == 0)
					{
						FactoryProvider.SaveCurrentAndCreateNew();
					}
				}
				Thread.Sleep(0);
			}
		}

		void ProcessHousebills(CusMAWB masterBill, Level1Record record, AirCargoValueObjectDataAdapter dataAdapter)
		{
			if (record._202000 == null || record._202000.PackageTrackingNumber.IsEmpty)
			{
				return;
			}

			ZString housebill = record._202000.PackageTrackingNumber;
			ShipmentInfo duplicateShipment = DuplicateShipmentDict.GetValueSafe(housebill);

			if (housebill.Length < longTrackingNumberLength && !duplicateShipment.HouseBill.IsNullOrEmpty())
			{
				housebill = duplicateShipment.HouseBill;
			}

			if (ContinueProcessingHouseBill(masterBill, record, housebill))
			{
				Xsd.Shipment shipmentValueObj = CreateShipment(record, housebill);
				if (shipmentValueObj != null)
				{
					UPECusHAWB cusHAWB = FactoryProvider.Current.New<UPECusHAWB>();
					cusHAWB.Level1Record = record;
					cusHAWB.CS_CM = masterBill.PK;

					ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider.Current, new NotificationBuffer());
					dataAdapter.ImportHAWB(cusHAWB, shipmentValueObj, importContext);

					LoadUPSSpecificData(cusHAWB, record);

					SetSplitShipment(duplicateShipment.HouseBill, cusHAWB);

					cusHAWB.SetRemarksIfThisIsANewHAWBAndInInterventionQueue();
					SetVendorIdentifier(record, cusHAWB);

					if (housebill.Length == longTrackingNumberLength || cusHAWB.CS_PiecesManifested == 1 && cusHAWB.CS_PiecesLanded == 1 && regex.IsMatch(housebill))
					{
						cusHAWB.CreateFormalDecAndMatchIfRequired();
						SendAirCargoMessageIfNecessary(cusHAWB);
					}
				}
			}
		}
		static readonly Regex regex = new Regex(@"^[a-zA-Z]\d{10}$");

		void SendAirCargoMessageIfNecessary(UPECusHAWB cusHAWB)
		{
			if (!cusHAWB.CS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.Australia, StringComparison.OrdinalIgnoreCase))
			{
				cusHAWB.SendAirCargoMessage();
			}
			else
			{
				cusHAWB.CS_IsSelfAssessedClearance = cusHAWB.IsValidForSAC;
				if (cusHAWB.IsFormalDecRequired || cusHAWB.CS_IsSelfAssessedClearance)
				{
					cusHAWB.SendAirCargoMessage();
				}
			}
		}

		#endregion

		#region Split Shipments Feature

		const int longTrackingNumberLength = 18;

		bool CheckForAPreviouslyLoadedShipment(string longTrackingNumber, string shortTrackingNumber)
		{
			ZDBOnlyQuery query = UPEUtility.GetDuplicateShipmentQuery(shortTrackingNumber);

			CusHAWB previousShipment = FactoryProvider.Current.LoadTop1<CusHAWB>(query);
			if (previousShipment != null)
			{
				ShipmentInfo previousShipmentInfo = new ShipmentInfo(previousShipment.CS_HAWB, previousShipment.MAWB.CM_MAWB);
				DuplicateShipmentDict.Add(longTrackingNumber, previousShipmentInfo);
				return true;
			}

			return false;
		}

		void SetSplitShipment(string housebill, UPECusHAWB cusHAWB)
		{
			if (!housebill.IsNullOrEmpty())
			{
				cusHAWB.SetSplitShipment();
			}
		}

		void LoadTrackingNumbers(UPECusHAWB houseBill, Level1Record record)
		{
			if (record._200000 != null & record._202000 != null)
			{
				UPEJobRelatedWayBill.CreateJobRelatedWaybill(FactoryProvider.Current, houseBill.PK, houseBill.CS_HAWB, record._200000.TrackingNumber,
					JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			}

			foreach (_600000Line childPackage in record._600000Lines)
			{
				string shortTrackingNumber = childPackage.ShortTrackingNumber == "99999999999" ? "" : childPackage.ShortTrackingNumber;
				UPEJobRelatedWayBill.CreateJobRelatedWaybill(FactoryProvider.Current, houseBill.PK, childPackage.LongTrackingNumber, shortTrackingNumber,
					JobRelatedWayBill.Constants.RelatedWayBillType.Child);
			}
		}

		#endregion

		#region Process GCC Children

		void ProcessGCCChildren()
		{
			foreach (Level1Record record in Level1RecordList)
			{
				if (record._200000 != null && record._200000.IsGCCChild)
				{
					JobRelatedWayBill parentWayBill = LeadPackageParentWayBill(record);

					if (parentWayBill != null)
					{
						if (!IsChildPackageInDatabase(parentWayBill, record))
						{
							UPEJobRelatedWayBill.CreateJobRelatedWaybill(FactoryProvider.Current, parentWayBill.EB_ParentID, record._202000.PackageTrackingNumber, record._200000.TrackingNumber, JobRelatedWayBill.Constants.RelatedWayBillType.Child);
						}
						AddInvoiceLinesToLeadLevel1Record(parentWayBill, record);
					}
				}
			}

			FactoryProvider.SaveCurrentAndCreateNew();
		}

		JobRelatedWayBill LeadPackageParentWayBill(Level1Record record)
		{
			JobRelatedWayBill result = null;

			if (record._401000 != null)
			{
				ZQuery leadPackageParentWayBillFilter = new ZQuery(JobRelatedWayBillSchema.EB_WaybillShortNumber, record._401000.LeadTrackingNumberForGCCShipment);
				leadPackageParentWayBillFilter.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
				result = (JobRelatedWayBill)FactoryProvider.Current.LoadTop1(typeof(JobRelatedWayBill), leadPackageParentWayBillFilter);
			}

			return result;
		}

		bool IsChildPackageInDatabase(JobRelatedWayBill parentWayBill, Level1Record record)
		{
			ZQuery childAlreadyExistsFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, parentWayBill.PK);
			childAlreadyExistsFilter.AddToFilter(JobRelatedWayBillSchema.EB_WaybillNumber, record._202000.PackageTrackingNumber);
			return FactoryProvider.Current.Exists(typeof(JobRelatedWayBill), childAlreadyExistsFilter);
		}

		void AddInvoiceLinesToLeadLevel1Record(JobRelatedWayBill parentWayBill, Level1Record childLevel1Record)
		{
			UPECusHAWB uPECusHAWB = FactoryProvider.Current.Load<UPECusHAWB>(parentWayBill.EB_ParentID);
			if (uPECusHAWB != null && uPECusHAWB.Level1Record != null)
			{
				foreach (_500000Line line in childLevel1Record._500000Lines)
				{
					uPECusHAWB.Level1Record._500000Lines.Add(line);
				}
			}
		}

		#endregion

		protected override string LocalCurrencyCode => Core.Constants.CurrencyCodes.Australia;

		#endregion

		public override ZString GetSummaryInformation()
		{
			ZStringBuilder stringBuilder = new ZStringBuilder();
			stringBuilder.Append(FlightDetailsSummary);

			string fileDetails = string.Format(CultureInfo.InvariantCulture,
				"File Summary\r\n" +
				"============\r\n" +
				"File Name                         : {0}\r\n" +
				"Total Number of Shipments         : {1}\r\n" +
				"Total Number of Pieces Manifested : {2}\r\n" +
				"Total Number of Child Packages    : {3}\r\n" +
				"\r\n",
				level1DataImport.FileName,
				TotalNoOfShipments,
				TotalPiecesManifested,
				TotalNoOfChildPackages);

			stringBuilder.Append(fileDetails);

			stringBuilder.Append(DuplicateHousebillsSummary.ToString());
			stringBuilder.Append(IncorrectChildPacksSummary.ToString());
			stringBuilder.Append(IncorrectPortOfDestinationSummary.ToString());
			stringBuilder.Append(EmptyShipmentsSummary.ToString());
			return stringBuilder.ToString();
		}

		ZStringBuilder DuplicateHousebillsSummary
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				string header = "Duplicate HAWB %                  : " + PercentageOfDuplicateHAWBs + "\r\n" +
					"\r\n" +
					"Duplicate HAWB's\r\n" +
					"================\r\n";
				result.Append(header);

				foreach (var shipment in DuplicateShipmentDict)
				{
					result.Append(string.Format(CultureInfo.InvariantCulture, "{0} Matched Shipment: House Bill[{1}]; Master Bill[{2}]", shipment.Key, shipment.Value.HouseBill, shipment.Value.MasterBill));
					result.Append("\r\n");
				}

				result.Append("\r\n");

				return result;
			}
		}

		ZStringBuilder IncorrectChildPacksSummary
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				result.Append(
					"No of Pieces Manifested <> No of Child Packs\r\n" +
					"============================================\r\n");

				if (IncorrectChildPacks.Count > 0)
				{
					foreach (string s in IncorrectChildPacks)
					{
						result.Append(s);
						result.Append("\r\n");
					}
				}

				result.Append("\r\n");

				return result;
			}
		}
		static void SetVendorIdentifier(Level1Record record, UPECusHAWB cusHAWB)
		{
			cusHAWB.CS_VendorIdentifier = cusHAWB.IsValidForSAC && record._900000 != null ? record._900000.ARNNumber : string.Empty;
		}
	}
}

