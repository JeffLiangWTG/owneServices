using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[UniversalCopyAddInfo(JobDeclarationSchema.Constants.Prefix, AUAddInfo.Schema.Prefix)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.AUJobDeclaration)]
	public partial class JobDeclaration :
		TypeSafeJobDeclaration,
		IAddInfo,
		IAddInfoManager,
		ICMRMessageRespondee,
		ICMRControlMessageRespondee,
		ILandedCostHeader,
		IProcessQueueParent,
		ICPQAAttacheeHolder,
		IAQIS,
		IMessageAttacheeParent,
		IDocAddresses,
		IStatusNeedsRecalculationProvider,
		ICMROtherMessageHeader,
		ICPQAAttachee,
		IBackDoorSavingSupportableBizObj,
		IInvoicesProvider,
		IApportionInvoiceHolder,
		Integration.Customs.AU.IJobDeclaration,
		IDataExportCSVFileNameProvider,
		ICMRMessageRespondeeReference,
		IOnUniversalEventAddedHandler,
		IEDocDeliveryAuthorization
	{
		#region Constants

		#region MessageType

		public static class MessageSubType
		{
			public const string FormalEntry = "FRM";
			public const string RequestForACEAN = "ACE";
			public const string PeriodDeclarationType1 = "PD1";
			public const string PeriodDeclarationType2 = "PD2";
			public const string SimplifiedEntry = "SIM";
			public const string SelfAssessedClearance = "SAC";
			public const string SelfAssessedClearanceWithLines = "SWL";
			public const string RequestForCargoRelease = "RCR";
			public const string Drawback = "DRW";
			public const string Confirming = "CFM";
			public const string NonConfirming = "NCF";
			public const string Manual = "MAN";
		}
		#endregion

		#region ClassificationType
		public static class ClassificationType
		{
			public const string IMP = "IMP";
			public const string EXP = "EXP";
		}
		#endregion ClassificationType

		#region PaymentMethods
		public abstract class PaymentMethods
		{
			public const string Importer = "IMP";
			public const string Broker = "BRK";
			public const string Default = "DEF";
			public const string SecondBroker = "BK2";
			public const string Cash = "CSH";
			public const string DrawbackClaimant = "DBC";
		}
		#endregion

		#region ExportGoodsType

		public abstract class ExportGoodsType
		{
			public const string GeneralConsignedCargo = "OT";
			public const string Stores = "ST";
			public const string SpareParts = "SP";
			public const string OwnPower = "OP";
			public const string AccompaniedBaggage = "AB";
			public const string Postal = "PO";
		}

		#endregion

		#region Drawback

		public abstract class DrawbackAmberReasonTypes
		{
			public const string Calculation = "C";
			public const string Declaration = "D";
			public const string Time = "T";
			public const string LegacyMigration = "L";
		}

		public abstract class DrawbackAssessmentMethods
		{
			public const string ActualShipment = "A";
			public const string RepresentativeShipment = "B";
			public const string Imputation = "C";
			public const string OtherMethod = "O";
		}

		#endregion Drawback

		#region Schema

		public new class Schema : BaseJobDeclaration.Schema
		{
			public const string JE_GoodsOwnerPartyID = "JE_GoodsOwnerPartyID";
			public const string JE_ForcePrimeEnclosure = "JE_ForcePrimeEnclosure";

			public const string JE_SendCreateMessage = "JE_SendCreateMessage";
			public const string JE_SendCPDecMessage = "JE_SendCPDecMessage";
			public const string JE_SendLodgeMessage = "JE_SendLodgeMessage";
			public const string JE_SendPayMessage = "JE_SendPayMessage";
			public const string ContingencyCAN = "ContingencyCAN";
			public const string IsSubjectToExciseOrDuty = "IsSubjectToExciseOrDuty";
			public const string JE_ToOrder = "JE_ToOrder";
			public const string JE_ToOrderComment = "JE_ToOrderComment";
			public const string ZA_GoodsOwnerPartyIDHidden = AUAddInfoSchema.Constants.ZA_GoodsOwnerPartyIDHidden;
			public const string ZA_ConsigneeNameHidden = AUAddInfoSchema.Constants.ZA_ConsigneeNameHidden;
			public const string ZA_ConsigneeCityHidden = AUAddInfoSchema.Constants.ZA_ConsigneeCityHidden;
			public const string ZA_CustShipNo_Hidden = AUAddInfoSchema.Constants.ZA_CustShipNo_Hidden;
			public const string ZA_CustShipNoOverride_Hidden = AUAddInfoSchema.Constants.ZA_CustShipNoOverride_Hidden;

			public const string JE_PartShipConsignmentReference = "JE_PartShipConsignmentReference";
			public const int JE_PartShipConsignmentReferenceMaxLength = 35;
		}

		#endregion

		#endregion

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			calculator = new ExportsOtherMessagesStatusCalculator(this);
		}
		public readonly ExportsOtherMessagesStatusCalculator calculator;

		public new AUJobDeclarationLookups Lookups
		{
			get { return (AUJobDeclarationLookups)base.Lookups; }
		}

		protected override Customs.Business.MostInterestingLegProvider MostInterestingLegProviderCore
		{
			get { return new MostInterestingLegProvider(this); }
		}

		public static new JobDeclaration New(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>();
		}

		public static new JobDeclaration LoadFirstMatchingInCurrentCompanyIncludingInActive(BusinessObjectFactory factory, ZString declarationReference)
		{
			return (JobDeclaration)BaseJobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(factory, declarationReference);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		protected override IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get { return new ZString[] { ApplicationCodeList.Codes.AUCMR }; }
		}

		protected override bool DoesCustomsEntryStatusAllowCancellation
		{
			get
			{
				var result = true;
				if (IsExport)
				{
					if (!string.IsNullOrWhiteSpace(JE_EntryStatus)
							&& !JE_EntryStatus.Equals(CustomsEntryStatus.NotSent.Code)
							&& !JE_EntryStatus.Equals(CustomsEntryStatus.FailOriginal.Code)
							&& !JE_EntryStatus.Equals(CustomsEntryStatus.ClearWithdrawal.Code))
					{
						result = false;
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(JE_EntryStatus)
						&& !JE_EntryStatus.Equals(CustomsEntryStatus.NotSent.Code)
						&& !JE_EntryStatus.Equals(CMRImportEntryAdvice.Withdrawn.Code)
						|| JE_MessageStatus.StartsWith("W"))
					{
						result = false;
					}
				}
				return result;
			}
		}

		public JobDeclaration RelatedDeclarationForTransferEDN { get; set; }

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		protected override bool SupportUseOwnerRefAsQuarantineRefUsageCore => true;

		public ISendsMessagesToCustoms GetMessageInitiatorSafe()
		{
			return fMessageInitiator as ISendsMessagesToCustoms;
		}

		public Guid BranchOfEmailGroupRegistry
		{
			get
			{
				var job = new JobHeader.Loader(this).Load();
				if (job != null && job.JH_GB.IsValid)
				{
					return job.JH_GB.ToGuid();
				}
				return base.RegistryBranchPK;
			}
		}

		public new Bill PrimaryMasterBill
		{
			get { return (Bill)base.PrimaryMasterBill; }
		}

		public new Bill PrimaryHouseBill
		{
			get { return (Bill)base.PrimaryHouseBill; }
		}

		[ChildEditable(true)]
		public new DeclarationLevelPackageCollection Packages
		{
			get { return (DeclarationLevelPackageCollection)base.Packages; }
		}

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection()
		{
			return new DeclarationLevelPackageCollection(this);
		}

		public ZInt PackagesOuterPackageCount
		{
			get
			{
				ZInt result = 0;
				foreach (Package package in Packages)
				{
					result += package.CW_OuterPacks;
				}

				return result;
			}
		}

		#region vessel

		public ZString VesselNumber
		{
			get
			{
				return (!ZA_CustShipNo_Hidden.IsEmpty ? ZA_CustShipNo_Hidden :
						Vessel != null ? Vessel.RV_LloydsNumber : ZString.Empty).ToUpper();
			}
		}

		public ZString ZA_CustShipNo_Hidden
		{
			get { return AddInfo.ZA_CustShipNo_Hidden; }
			set { AddInfo.ZA_CustShipNo_Hidden = value; }
		}

		public ZPropertyInfo ZA_CustShipNo_HiddenInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_CustShipNo_Hidden, x => AddInfo.ZA_CustShipNo_HiddenInfo); }
		}

		public ZBool ZA_CustShipNoOverride_Hidden
		{
			get { return (AddInfo.ZA_CustShipNoOverride_Hidden == "Y"); }
			set
			{
				if (AddInfo.ZA_CustShipNoOverride_Hidden == "Y" ^ value)
				{
					AddInfo.ZA_CustShipNoOverride_Hidden = value ? "Y" : "";
				}
				if (value)
				{
					JE_VesselName = ZString.Empty;
				}
				else
				{
					ZA_CustShipNo_Hidden = ZString.Empty;
				}
			}
		}
		public ZPropertyInfo ZA_CustShipNoOverride_HiddenInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_CustShipNoOverride_Hidden, x => AddInfo.ZA_CustShipNoOverride_HiddenInfo); }
		}

		bool JE_VesselNameReadOnly
		{
			get { return ZA_CustShipNoOverride_Hidden; }
		}

		[ReadOnlyMember(nameof(JE_VesselNameReadOnly))]
		public override ZString JE_VesselName
		{
			get { return base.JE_VesselName; }
			set { base.JE_VesselName = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString JE_RN_NKTransportNationality
		{
			get => Vessel?.RV_RN_NKCountryOfReg ?? ZString.Empty;
		}

		#endregion

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			if (IsImportCMR || IsEXPDeclaration)
			{
				if (fCMRMergeManager == null)
				{
					fCMRMergeManager = new CMRMergeManager(this);
				}
				return fCMRMergeManager;
			}
			else
			{
				if (fMergeManager == null)
				{
					fMergeManager = new MergeManager(this);
				}
				return fMergeManager;
			}
		}
		CMRMergeManager fCMRMergeManager;

		protected override void MiscServ_OnMiscServUpdatedByDataRefresh()
		{
			base.MiscServ_OnMiscServUpdatedByDataRefresh();
			if (IsImportCMR)
			{
				Validation.ValidateJE_PaymentMethod();
			}
		}

		public bool IsOther
		{
			get { return JE_TransportMode == Enterprise.Core.Constants.TransportModes.Other; }
		}

		public bool IsSACWithoutLines
		{
			get { return IsImport && (JE_MessageSubType == MessageSubType.SelfAssessedClearance); }
		}

		public bool IsSACWithLines
		{
			get { return IsImport && (JE_MessageSubType == MessageSubType.SelfAssessedClearanceWithLines); }
		}

		public bool IsSAC
		{
			get { return IsSACWithoutLines || IsSACWithLines; }
		}

		public bool IsPrimeEnclosureEntry
		{
			get
			{
				var primeEntryExists = false;
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					if (entryHeader.IsPrimeEntry)
					{
						primeEntryExists = true;
						break;
					}
				}
				return primeEntryExists;
			}
		}

		public bool HasNature20Entry
		{
			get
			{
				foreach (var groupHeader in JobComInvoiceGroupHeaders)
				{
					foreach (var invoiceHeader in groupHeader.AllJobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
					{
						foreach (var invoiceLine in invoiceHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>())
						{
							if (invoiceLine.Nature == JobComInvoiceHeader.NatureString.Nature20)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public bool HasNature10Entry
		{
			get
			{
				foreach (var groupHeader in JobComInvoiceGroupHeaders)
				{
					foreach (var invoiceHeader in groupHeader.AllJobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
					{
						foreach (var invoiceLine in invoiceHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>())
						{
							if (invoiceLine.Nature == JobComInvoiceHeader.NatureString.Nature10)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public bool HasNature10And20Entries
		{
			get { return HasNature10Entry && HasNature20Entry; }
		}

		public bool HaveAllInvoicesBeenAssignedAPackCount
		{
			get
			{
				var result = true;
				foreach (var header in JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
				{
					if (!header.HasAPackCountHasBeenEnteredForThisInvoice)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		public bool HaveAllInvoicesBeenAssignedAPiecesCount
		{
			get
			{
				var result = true;
				foreach (var header in JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
				{
					if (!header.HasAPiecesCountHasBeenEnteredForThisInvoice)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		public bool DeclarationMustBeSentPrimeEnclosure
		{
			get
			{
				return WillThereBeMultipleEntryHeaders && (JE_ForcePrimeEnclosure || (JE_TotalNoOfPacks > 0 && !HaveAllInvoicesBeenAssignedAPackCount) || (JE_TotalNoOfPieces > 0 && !HaveAllInvoicesBeenAssignedAPiecesCount));
			}
		}

		public bool ImplementUPE
		{
			get
			{
				return AUCustomsDataRegistry.Instance.UPEImplementationDate.Value <= ZDateTime.Today &&
					(CustomsEntryHeaders.Any(x => x.DeclarationDate.IsEmpty || !x.DeclarationDate.IsValid ||
						x.DeclarationDate > AUCustomsDataRegistry.Instance.UPEImplementationDate.Value) ||
					CustomsEntryHeaders.Count == 0);
			}
		}

		public bool IsUPEDeclaration
		{
			get
			{
				return AddInfo.ZA_UPEIndicator_Hidden;
			}
		}

		public bool IsSOFADeclaration
		{
			get { return IsSOFAVisible && AddInfo.ZA_SOFAIndicator_Hidden; }
		}

		public bool IsSOFAVisible
		{
			get { return IsImport && IsNature10; }
		}

		public bool HasMultipleWarehouses => Factory.GetValue(ref hasMultipleWarehousesCached, GetHasMultipleWarehouses);

		CachedProperty<bool> hasMultipleWarehousesCached;

		bool GetHasMultipleWarehouses()
		{
			var result = false;

			var previousWarehouseCCP = ZString.Empty;
			var shouldCheckForEmpty = IsWHSUniversalXMLActive && IsExWarehouse;
			foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
			{
				var lineWarehouseCCP = line.WarehouseCCP;

				if (shouldCheckForEmpty || !lineWarehouseCCP.IsEmpty)
				{
					if (previousWarehouseCCP.IsEmpty)
					{
						previousWarehouseCCP = lineWarehouseCCP;
					}
					else if (lineWarehouseCCP != previousWarehouseCCP)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		protected override JobDeclarationLookups GetNewLookups()
		{
			return new AUJobDeclarationLookups(this);
		}

		protected override Customs.Business.WarehouseInvoiceLink GetNewWarehouseInvoiceLink()
		{
			return new WarehouseInvoiceLink(this);
		}

		protected override bool DoMergeCore(Customs.Business.ISendsMessagesToCustoms notifier)
		{
			bool result;
			if (IsImportEdifice)
			{
				result = Globals.IsTest && base.DoMergeCore(notifier);
			}
			else
			{
				result = base.DoMergeCore(notifier);
			}

			return result;
		}

		internal bool HasAtLeastOneLineFeeOfType(string feeType)
		{
			foreach (var header in ActiveEntryHeaders.Cast<CusEntryHeader>())
			{
				foreach (CusEntryLine line in header.MergedLines)
				{
					var fee = line.Fees.GetElementWithThisCode(feeType);
					if (fee != null && !fee.CF_ChargeAmount.IsEmpty)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool ShouldCreateDummyInvoiceLinesForMerge
		{
			get { return IsImportCMR && IsSACWithoutLines; }
		}

		public JobComInvoiceLine[] GetInvoiceLinesMarkedForBondedWarehousingWithDifferentWarehouseAddressToDeclaration() => Factory.GetValue(ref invoiceLinesMarkedForBondedWarehousingWithDifferentWarehouseAddressCached, delegate
		{
			var foundInvoiceLines = new List<JobComInvoiceLine>();
			if (SupportsBondedWarehousing)
			{
				var declarationWarehouseAddress = WarehouseDocAddress.E2_OA_Address;
				if (!declarationWarehouseAddress.IsEmpty)
				{
					foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
					{
						var invoiceLineWarehouseAddressPK = invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden;
						if (!invoiceLineWarehouseAddressPK.IsEmpty && declarationWarehouseAddress != invoiceLineWarehouseAddressPK)
						{
							foundInvoiceLines.Add(invoiceLine);
						}
					}
				}
			}
			return foundInvoiceLines.ToArray();
		});
		CachedProperty<JobComInvoiceLine[]> invoiceLinesMarkedForBondedWarehousingWithDifferentWarehouseAddressCached;

		#region QuarantineExdocLineCollection

		[ChildEditable(true)]
		public QuarantineExdocLineCompleteCollection QuarantineExdocLineLines
		{
			get
			{
				if (fQuarantineExdocLineLines == null)
				{
					fQuarantineExdocLineLines = GetNewQuarantineExdocLineCompleteCollection();
					fQuarantineExdocLineLines.Load();
					RegisterEditableChildObject(fQuarantineExdocLineLines);
				}

				return fQuarantineExdocLineLines;
			}
		}

		QuarantineExdocLineCompleteCollection fQuarantineExdocLineLines;

		protected virtual QuarantineExdocLineCompleteCollection GetNewQuarantineExdocLineCompleteCollection()
		{
			return new QuarantineExdocLineCompleteCollection(this);
		}
		#endregion

#if DEBUG

		internal bool ForceValidationInTest;
		public void SetupMergedForTest()
		{
			InitialiseDeclarationForMergeForTest();
			MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			DoMerge();
		}

		internal void InitialiseDeclarationForMergeForTest()
		{
			JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			var invoiceHeader = JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
		}

#endif

		protected override ZString GetMessageTypeForDocumentFilter()
		{
			ZString result;

			if (JE_MessageType == JobMessageTypeList.Codes.Quarantine)
			{
				result = JobMessageTypeList.Codes.Quarantine;
			}
			else
			{
				result = base.GetMessageTypeForDocumentFilter();
			}

			return result;
		}

		public bool ShouldWeCompareDeclarations
		{
			get
			{
				var result = false;
				result |= IsSavedByFactory;
				if (!result)
				{
					result |= JobComInvoiceGroupHeaders.HasChanges;
					if (!result)
					{
						foreach (var groupHeader in JobComInvoiceGroupHeaders)
						{
							result |= ((IBusinessObjectCollection)groupHeader.JobComInvoiceHeaders).HasChanges;
							if (!result)
							{
								foreach (var header in groupHeader.JobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
								{
									result |= header.JobComInvoiceLines.HasChanges;
								}
							}
						}
					}
				}
				if (!result)
				{
					result |= CusContainers.HasChanges;
				}
				return result;
			}
		}

		#region delivery address post code

		ZString DeliveryAddressPostCode
		{
			get
			{
				var result = ZString.Empty;
				if (ImporterDeliveryAddress != null)
				{
					if (!ImporterDeliveryAddress.E2_AddressOverride)
					{
						var address = ImporterDeliveryAddress.Address;
						if (address != null)
						{
							result = address.OA_PostCode;
						}
					}
					else
					{
						result = ImporterDeliveryAddress.E2_Postcode;
					}
				}
				return result;
			}
		}

		bool RuralAQISConcernTypeSpecified
		{
			get
			{
				return AQISConcernTypes.ContainsConcernType(AQISConcernType.AQISRuralConcernTypeCode);
			}
		}

		public void DeliveryAddressPostCodeMessages(out ZString postCodeMessageErrors, out ZString postCodeWarnings)
		{
			var postCodeMessageErrorsResult = ZString.Empty;
			var postCodeWarningsResult = ZString.Empty;
			if (IsSea && IsFCLorFCX)
			{
				var postCode = DeliveryAddressPostCode;
				if (!postCode.IsEmpty)
				{
					while (postCode.Length < 4)
					{
						postCode = "0" + postCode;
					}

					var postcodeDeliveryClassification = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AqisPostCodes, ZDateTime.Today, postCode, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.PostcodeDeliveryClassification);
					if (AddInfo.ZA_AQISInspectLocation_Hidden.IsEmpty || !RuralAQISConcernTypeSpecified)
					{
						if (postcodeDeliveryClassification.IsNullOrEmpty() || postcodeDeliveryClassification.FirstOrDefault().EqualsIgnoringCase("RURAL"))
						{
							postCodeMessageErrorsResult = "Delivery address post code is listed as RURAL. Both Quarantine Concern Type and Quarantine Inspection Location are required.";
						}
						else if (postcodeDeliveryClassification.FirstOrDefault().EqualsIgnoringCase("SPLIT"))
						{
							postCodeMessageErrorsResult = @"Delivery address post code is listed as SPLIT, so you must determine from the actual street address
whether this is a Rural or a Metro address. If it is a Rural address then both Quarantine Concern Type and Quarantine Inspection Location are required.";
						}
					}
					else
					{
						if (postcodeDeliveryClassification.IsNullOrEmpty() || postcodeDeliveryClassification.FirstOrDefault().EqualsIgnoringCase("RURAL"))
						{
							postCodeWarningsResult = "Delivery address post code is listed as RURAL, tailgate inspection is required.";
						}
						else if (postcodeDeliveryClassification.FirstOrDefault().EqualsIgnoringCase("SPLIT"))
						{
							postCodeWarningsResult = "Delivery address post code is listed as SPLIT, tailgate inspection is required if the address is Rural.";
						}
					}
				}
			}
			postCodeMessageErrors = postCodeMessageErrorsResult;
			postCodeWarnings = postCodeWarningsResult;
		}
		#endregion

		#region CTO

		//public enum CTOStatuss
		//{
		//  None, WaitingForResponse, Load, DoNotLoad, HoldForCustoms
		//}

		//public bool IsCTOMessagePending
		//{
		//  get
		//  {
		//    CTOStatuss Status = CTOStatus;
		//    return Status == CTOStatuss.WaitingForResponse;
		//  }
		//}

		//public CTOStatuss CTOStatus
		//{
		//  get
		//  {
		//    CTOStatuss Result = CTOStatuss.None;
		//    EDIMessage LastCTOMessage = null;
		//    EDIMessage[] View = Messages.GetMatchingMessages(CMRMessage.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.CTOREC }, ZString.Empty);
		//    if (View.Length > 0) LastCTOMessage = View[0];

		//    if (LastCTOMessage != null)
		//    {
		//      if (LastCTOMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
		//      {
		//        Result = CTOStatuss.WaitingForResponse;
		//      }
		//      else
		//      {
		//        if (LastCTOMessage.EM_MessageSubType == CMRMessage.MovementStatusResponseSubTypes.DoNotLoad)
		//        {
		//          Result = CTOStatuss.DoNotLoad;
		//        }
		//        else if (LastCTOMessage.EM_MessageSubType == CMRMessage.MovementStatusResponseSubTypes.HoldForCustoms)
		//        {
		//          Result = CTOStatuss.HoldForCustoms;
		//        }
		//        else if (LastCTOMessage.EM_MessageSubType == CMRMessage.MovementStatusResponseSubTypes.Load)
		//        {
		//          Result = CTOStatuss.Load;
		//        }
		//      }
		//    }
		//    return Result;
		//  }
		//}

		#endregion

		#region Depot

		//public enum DepotStatuses
		//{
		//  NotSent,
		//  ReceivalPending,
		//  ReceivalCleared,
		//  ReceivalErrors,
		//  ReceivalRejected,
		//  ReceivalReplacementPending,
		//  ReceivalReplacementRejected,
		//  ReceivalWithdrawalPending,
		//  ReceivalWithdrawn,
		//  ReleasePending,
		//  ReleaseCleared,
		//  ReleaseErrors,
		//  ReleaseRejected,
		//  ReleaseReplacementPending,
		//  ReleaseReplacementRejected,
		//  ReleaseWithdrawalPending,
		//  ReleaseWithdrawn
		//}

		//public bool IsDepotMessagePending
		//{
		//  get
		//  {
		//    DepotStatuses Status = DepotStatus;
		//    return Status == DepotStatuses.ReceivalPending
		//      || Status == DepotStatuses.ReceivalReplacementPending
		//      || Status == DepotStatuses.ReceivalWithdrawalPending
		//      || Status == DepotStatuses.ReleasePending
		//      || Status == DepotStatuses.ReleaseReplacementPending
		//      || Status == DepotStatuses.ReleaseWithdrawalPending;
		//  }
		//}

		//public bool IsDepotMessageRejected
		//{
		//  get
		//  {
		//    DepotStatuses Status = DepotStatus;
		//    return Status == DepotStatuses.ReceivalRejected
		//      || Status == DepotStatuses.ReceivalReplacementRejected
		//      || Status == DepotStatuses.ReleaseRejected
		//      || Status == DepotStatuses.ReleaseReplacementRejected;
		//  }
		//}

		//public DepotStatuses DepotStatus
		//{
		//  get
		//  {
		//    DepotStatuses Result = DepotStatuses.NotSent;
		//    EDIMessage LastDepotMessage = null;
		//    EDIMessage SecondLastDepotMessage = null;
		//    EDIMessage[] View = Messages.GetMatchingMessages(CMRMessage.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.DEPREC, CMRMessage.CMRMessageTypes.DEPREL }, ZString.Empty);
		//    if (View.Length > 0) LastDepotMessage = View[0];
		//    if (View.Length > 1) SecondLastDepotMessage = View[1];
		//    if (LastDepotMessage != null)
		//    {
		//      if (LastDepotMessage.EM_MessageType == CMRMessage.CMRMessageTypes.DEPREC)
		//      {
		//        if (LastDepotMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
		//        {
		//          if (LastDepotMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Withdraw)
		//          {
		//            Result = DepotStatuses.ReceivalWithdrawalPending;
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment)
		//          {
		//            Result = DepotStatuses.ReceivalReplacementPending;
		//          }
		//          else
		//          {
		//            Result = DepotStatuses.ReceivalPending;
		//          }
		//        }
		//        else
		//        {
		//          if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Clear)
		//          {
		//            Result = DepotStatuses.ReceivalCleared;
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Rejected)
		//          {
		//            if (SecondLastDepotMessage != null && SecondLastDepotMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment)
		//            {
		//              Result = DepotStatuses.ReceivalReplacementRejected;
		//            }
		//            else
		//            {
		//              Result = DepotStatuses.ReceivalRejected;
		//            }
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Error)
		//          {
		//            Result = DepotStatuses.ReceivalErrors;
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Withdrawn)
		//          {
		//            Result = DepotStatuses.ReceivalWithdrawn;
		//          }
		//        }
		//      }
		//      else if (LastDepotMessage.EM_MessageType == CMRMessage.CMRMessageTypes.DEPREL)
		//      {
		//        if (LastDepotMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
		//        {
		//          if (LastDepotMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Withdraw)
		//          {
		//            Result = DepotStatuses.ReleaseWithdrawalPending;
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment)
		//          {
		//            Result = DepotStatuses.ReleaseReplacementPending;
		//          }
		//          else
		//          {
		//            Result = DepotStatuses.ReleasePending;
		//          }
		//        }
		//        else
		//        {
		//          if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Clear)
		//          {
		//            Result = DepotStatuses.ReleaseCleared;
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Rejected)
		//          {
		//            if (SecondLastDepotMessage != null && SecondLastDepotMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment)
		//            {
		//              Result = DepotStatuses.ReleaseReplacementRejected;
		//            }
		//            else
		//            {
		//              Result = DepotStatuses.ReleaseRejected;
		//            }
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Error)
		//          {
		//            Result = DepotStatuses.ReleaseErrors;
		//          }
		//          else if (LastDepotMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Withdrawn)
		//          {
		//            Result = DepotStatuses.ReleaseWithdrawn;
		//          }
		//        }
		//      }
		//    }
		//    return Result;
		//  }
		//}

		#endregion

		#region Warehouse

		protected override JobDocAddressRequirement GetNewWarehouseDocAddressRequirement()
		{
			var result = base.GetNewWarehouseDocAddressRequirement();
			result.ValidateOrganisationPK = ValidationWarehouseDocAddress;
			return result;
		}

		public const string EnterAWarehouseMessageError = "A bonded warehouse address is required when there is a Nature 20/30 line entered.";
		public const string NoCCPMessageError = "The bonded warehouse address doesn't have a warehouse code (CCP) entered.";

		void ValidationWarehouseDocAddress(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var declaration = parent.Parent as JobDeclaration;
			if (declaration != null && declaration.IsWHSUniversalXMLActive)
			{
				if (declaration.IsExWarehouse || declaration.HasLineGoingIntoABondedWarehouse)
				{
					var warehouseAddress = declaration.IsExWarehouse ? declaration.DeclarationOrFirstInvoiceLineWarehouseAddress : declaration.WarehouseAddress;
					if (warehouseAddress == null)
					{
						parent.OrganisationPKInfo.AddMessageError(EnterAWarehouseMessageError);
					}
					else if (warehouseAddress.LocalControlledPremisesID.IsEmpty && (!declaration.IsExWarehouse || declaration.WarehouseAddress != null))
					{
						parent.OrganisationPKInfo.AddMessageError(NoCCPMessageError);
					}
				}
			}
		}

		//public enum WarehouseStatuss
		//{
		//  NotSent,
		//  ReleasePending,
		//  ReleaseCleared,
		//  ReleaseRejected,
		//  ReleaseReplacementPending,
		//  ReleaseReplacementRejected,
		//  ReleaseWithdrawalPending,
		//  ReleaseWithdrawn,
		//  ReturnPending,
		//  ReturnClear,
		//  ReturnRejected
		//}

		//public bool IsWarehouseMessagePending
		//{
		//  get
		//  {
		//    WarehouseStatuss Status = WarehouseStatus;
		//    return Status == WarehouseStatuss.ReleasePending || Status == WarehouseStatuss.ReturnPending || Status == WarehouseStatuss.ReleaseReplacementPending || Status == WarehouseStatuss.ReleaseWithdrawalPending;
		//  }
		//}

		//public bool IsWarehouseMessageRejected
		//{
		//  get
		//  {
		//    WarehouseStatuss Status = WarehouseStatus;
		//    return Status == WarehouseStatuss.ReleaseRejected || Status == WarehouseStatuss.ReturnRejected || Status == WarehouseStatuss.ReleaseReplacementRejected;
		//  }
		//}

		//public WarehouseStatuss WarehouseStatus
		//{
		//  get
		//  {
		//    WarehouseStatuss Result = WarehouseStatuss.NotSent;
		//    EDIMessage LastWarehouseMessage = null;
		//    EDIMessage SecondLastWarehouseMessage = null;
		//    EDIMessage[] View = Messages.GetMatchingMessages(CMRMessage.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.WARREL, CMRMessage.CMRMessageTypes.WARRET }, ZString.Empty);
		//    if (View.Length > 0) LastWarehouseMessage = View[0];
		//    if (View.Length > 1) SecondLastWarehouseMessage = View[1];
		//    if (LastWarehouseMessage != null)
		//    {
		//      if (LastWarehouseMessage.EM_MessageType == CMRMessage.CMRMessageTypes.WARREL)
		//      {
		//        if (LastWarehouseMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
		//        {
		//          if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Withdraw)
		//          {
		//            Result = WarehouseStatuss.ReleaseWithdrawalPending;
		//          }
		//          else if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment)
		//          {
		//            Result = WarehouseStatuss.ReleaseReplacementPending;
		//          }
		//          else
		//          {
		//            Result = WarehouseStatuss.ReleasePending;
		//          }
		//        }
		//        else
		//        {
		//          if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Clear)
		//          {
		//            Result = WarehouseStatuss.ReleaseCleared;
		//          }
		//          else if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Rejected)
		//          {
		//            if (SecondLastWarehouseMessage != null && SecondLastWarehouseMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment)
		//            {
		//              Result = WarehouseStatuss.ReleaseReplacementRejected;
		//            }
		//            else
		//            {
		//              Result = WarehouseStatuss.ReleaseRejected;
		//            }
		//          }
		//          else if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Withdrawn)
		//          {
		//            Result = WarehouseStatuss.ReleaseWithdrawn;
		//          }
		//        }
		//      }
		//      else if (LastWarehouseMessage.EM_MessageType == CMRMessage.CMRMessageTypes.WARRET)
		//      {
		//        if (LastWarehouseMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
		//        {
		//          Result = WarehouseStatuss.ReturnPending;
		//        }
		//        else
		//        {
		//          if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Clear)
		//          {
		//            Result = WarehouseStatuss.ReturnClear;
		//          }
		//          else if (LastWarehouseMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Rejected)
		//          {
		//            Result = WarehouseStatuss.ReturnRejected;
		//          }
		//        }
		//      }
		//    }
		//    return Result;
		//  }
		//}

		public ZString FirstWarehouseCCP => Factory.GetValue(ref cachedFirstWarehouseCCP, GetFirstWarehouseCCP);

		CachedProperty<ZString> cachedFirstWarehouseCCP;

		ZString GetFirstWarehouseCCP()
		{
			ZString result = "";
			foreach (var line1 in InvoiceLines.OrderBy(line => line.JI_LineNo).Cast<JobComInvoiceLine>())
			{
				if (!line1.WarehouseCCP.IsEmpty)
				{
					result = line1.WarehouseCCP;
					break;
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		protected override bool IsInvoiceQuantityRequiredForBondedWarehouse
		{
			get { return true; }
		}

		protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse
		{
			get { return false; }
		}

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new InventorySelectionHeader(this);
		}

		protected override Customs.Business.BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		public const string WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing = "Warehouse Address on invoice line level cannot be different to declaration level for Bonded Warehousing.";
		public const string InvoiceLineMarkedForBondedWarehousingRequiresWRNAndWRL = "An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.";

		OrgAddress DeclarationOrFirstInvoiceLineWarehouseAddress
		{
			get
			{
				var warehouseAddress = WarehouseAddress;
				if (warehouseAddress == null)
				{
					foreach (var invoiceLine in InvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(line => line.JI_LineNo).Cast<JobComInvoiceLine>())
					{
						warehouseAddress = invoiceLine.AddInfo.WarehouseAddress;
						if (warehouseAddress != null)
						{
							break;
						}
					}
				}
				return warehouseAddress;
			}
		}

		protected override bool HasBondedWarehouse()
		{
			return HasABondedWarehouseFor(DeclarationOrFirstInvoiceLineWarehouseAddress);
		}

		protected override bool IsUNDGSupportedOnInvoiceLines
		{
			get { return IsExport || IsQuarantine; }
		}

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection(this, Factory);

		protected void DefaultContainerModeIfNeeded()
		{
			if (!IsCopying && !fIsImportingData)
			{
				var containerMode = DefaultContainerMode;

				if (JE_ContainerMode != containerMode)
				{
					JE_ContainerMode = containerMode;
				}
			}
		}

		public ZString DefaultContainerMode
		{
			get
			{
				ZString result = "";
				if (UseEXD && IsPost)
				{
					result = Core.Constants.ContainerModes.NonContainerised;
				}
				else if (IsTransportModeOther)
				{
					result = Core.Constants.ContainerModes.Other;
				}
				else if ((!IsImport || !IsSea) && Lookups.CargoIdTypeList.Count > 0)
				{
					result = Lookups.CargoIdTypeList[0].Code;
				}

				return result;
			}
		}

		#endregion

		#region Merge Lines

		public new ISendsMessagesToCustoms MessageInitiator
		{
			get { return base.MessageInitiator as ISendsMessagesToCustoms; }
			set { base.MessageInitiator = value; }
		}

		public new bool HasMessageInitiator
		{
			get { return fMessageInitiator is ISendsMessagesToCustoms; }
		}

		CachedProperty<ZBool> willThereBeMultipleEntryHeadersCache;
		public override bool WillThereBeMultipleEntryHeaders => Factory.GetValue(ref willThereBeMultipleEntryHeadersCache, delegate
		{
			var merger = new LineMerger(this);
			return merger.WillThereBeMultipleEntryHeaders;
		});

		public bool CanThrowAwayMerge
		{
			get
			{
				return (JE_EntryStatus == CustomsEntryStatus.NotSent.Code || JE_EntryStatus.IsEmpty || //TODO: Change this back when the Not Sent code is 'NOT'
					JE_EntryStatus == CustomsEntryStatus.AwaitingCreate.Code ||
					JE_EntryStatus == CustomsEntryStatus.FailCreate.Code ||
					JE_EntryStatus == CustomsEntryStatus.ClearCreate.Code ||
					JE_EntryStatus == CustomsEntryStatus.AwaitingCPDec.Code ||
					JE_EntryStatus == CustomsEntryStatus.FailCPDec.Code ||
					JE_EntryStatus == CustomsEntryStatus.ClearCPDec.Code ||
					JE_EntryStatus == CustomsEntryStatus.FailLodge.Code);
			}
		}

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection(this, Factory);

		#endregion

		#region Properties
		public ZBool IsNEXDOCSActive => IsQuarantine && Invoices.Count == 1 && Invoices[0].IsNEXDOCSActive;

		public override ZString JE_MasterBill
		{
			get { return base.JE_MasterBill; }
			set
			{
				base.JE_MasterBill = value;
				DefaultPiecesToPackingGroupOuterPacksIfRequired();
			}
		}

		[ResourceStringData("8C81B24C-8946-4319-AD19-72DD552DB452", Caption = "Parcel Post Number", IsApplicableMember = nameof(IsImportAndIsPost))]
		public override ZString JE_HouseBill
		{
			get { return base.JE_HouseBill; }
			set
			{
				base.JE_HouseBill = value;
				DefaultPiecesToPackingGroupOuterPacksIfRequired();
			}
		}
		public bool IsImportAndIsPost => IsImport && IsPost;

		protected override bool IsWeightApportionmentSupportedCore
		{
			get { return !IsExWarehouse; }
		}

		public JobComInvoiceHeader QuarantineInvoice
		{
			get
			{
				if (quarantineInvoice == null && IsQuarantine && Invoices.Count > 0)
				{
					quarantineInvoice = Invoices[0];
				}
				return quarantineInvoice;
			}
		}
		JobComInvoiceHeader quarantineInvoice;

		internal void ResetQuarantineInvoiceCache()
		{
			quarantineInvoice = null;
		}

		public ZString Details
		{
			get
			{
				var builder = new StringBuilder();
				builder.Append(DeclarationReferenceDetail);
				if (!JE_HouseBill.IsEmpty)
				{
					builder.Append("Housebill: " + JE_HouseBill + "\r\n");
				}

				if (!JE_MasterBill.IsEmpty)
				{
					builder.Append("Masterbill: " + JE_MasterBill + "\r\n");
				}

				if (!DeclarationNumber.IsEmpty)
				{
					if (IsExport)
					{
						builder.Append(EntryType + ": " + DeclarationNumber + "\r\n");
					}
					else if (IsDrawback)
					{
						builder.Append("Drawback Claim Identifier: " + DeclarationNumber + "\r\n");
					}
					else
					{
						builder.Append("Entry Number: " + DeclarationNumber + "\r\n");
					}
				}
				if (Supplier != null)
				{
					builder.Append("Consignor: " + Supplier.OH_FullNameTruncated + "\r\n");
				}

				if (Importer != null)
				{
					builder.Append("Consignee: " + Importer.OH_FullNameTruncated + "\r\n");
				}

				if (Origin != null)
				{
					builder.Append("Origin: " + Origin.Code + "\r\n");
				}

				if (FinalDestination != null)
				{
					builder.Append("Destination: " + FinalDestination.Code + "\r\n");
				}

				return builder.ToString();
			}
		}

		ZString DeclarationReferenceDetail
		{
			get { return "Declaration Reference: " + JE_DeclarationReference + "\r\n"; }
		}

		public ZString ShortDescription
		{
			get
			{
				return new ZString("Declaration Reference: " + JE_DeclarationReference);
			}
		}

		public new JobComInvoiceGroupHeaderSingleElementCollection ActiveGroupHeader
		{
			get { return (JobComInvoiceGroupHeaderSingleElementCollection)base.ActiveGroupHeader; }
		}

		protected override BaseJobComInvoiceGroupHeaderSingleElementCollection GetSingleElementCollection()
		{
			return new JobComInvoiceGroupHeaderSingleElementCollection(Factory);
		}

		public new JobComInvoiceLine[] SortedInvoiceLines
		{
			get
			{
				var unsortedLines = new JobComInvoiceLine[InvoiceLines.Count];
				var counter = 0;
				foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					unsortedLines[counter] = line;
					counter++;
				}
				var result = new JobComInvoiceLine[unsortedLines.Length];
				var myComparer = new JobComInvoiceLine.LineComparer();
				for (var j = 0; j < unsortedLines.Length; j++)
				{
					var minLineNum = -1;
					for (var i = 0; i < unsortedLines.Length; i++)
					{
						if (unsortedLines[i] != null && (minLineNum == -1 || myComparer.Compare(unsortedLines[i], unsortedLines[minLineNum]) < 0))
						{
							minLineNum = i;
						}
					}
					result[j] = unsortedLines[minLineNum];
					unsortedLines[minLineNum] = null;
				}
				return result;
			}
		}

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		#region GuidFindBox Collection

		// Until the AmbiguousMatch Exception is resolved in Z this property must stay for binding - Is this comment still valid?
		[ChildEditable(true)] public ICusContainerCollection<CusContainer> AUCusContainers => CusContainers;

		[ChildEditable(true)]
		public new ICusContainerCollection<CusContainer> CusContainers
		{
			get { return (ICusContainerCollection<CusContainer>)base.CusContainers; }
		}

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new BaseCusContainerCollection<CusContainer>(this, Factory);
		}

		#endregion

		public override ZBool IsExport
		{
			get
			{
				return new ZBool(JE_MessageType == JobMessageTypeList.Codes.Export || JE_MessageType == JobMessageTypeList.Codes.Quarantine
									|| JE_MessageType == JobMessageTypeList.Codes.ExportDeclarationByExternalBroker);
			}
		}

		public ZBool IsEXPDeclaration => JE_MessageType == JobMessageTypeList.Codes.Export;

		public override ZBool IsImport
		{
			get
			{
				return new ZBool(JE_MessageType == JobMessageTypeList.Codes.Import || JE_MessageType == JobMessageTypeList.Codes.ExWarehouse ||
									JE_MessageType == JobMessageTypeList.Codes.WarehousedByExternalAgent || JE_MessageType == JobMessageTypeList.Codes.ImportDeclarationByExternalBroker);
			}
		}

		public override bool IsNonTransportDeclarationType
		{
			get { return IsDrawback; }
		}

		public TransportModeEnum TransportModeEnum
		{
			get
			{
				if (IsAir)
				{
					return TransportModeEnum.Air;
				}

				if (IsSea)
				{
					return TransportModeEnum.Sea;
				}

				if (IsPost)
				{
					return TransportModeEnum.Post;
				}

				if (IsTransportModeOther)
				{
					return TransportModeEnum.Other;
				}

				return TransportModeEnum.Undefined;
			}
		}

		public bool IsTransportModeOther
		{
			get { return JE_TransportMode == Core.Constants.TransportModes.Other; }
		}

		public bool IsImportCMR => IsImport && (JE_ApplicationCode == Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages || JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced);

		public bool IsImportEdifice
		{
			get { return IsImport && JE_ApplicationCode == Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages; }
		}

		public bool IsQuarantine
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Quarantine; }
		}

		public bool IsImportOrDrawback
		{
			get { return IsImport || JE_MessageType == JobMessageTypeList.Codes.Drawback; }
		}

		public override ZBool ContainersAlwaysRequired
		{
			get { return IsQuarantine && JE_TransportMode == Enterprise.Core.Constants.TransportModes.Air; }
		}

		protected override void ThrowAwayMergeCore()
		{
			base.ThrowAwayMergeCore();
			JE_MessageStatus = ZString.Empty;
			OutstandingAmendmentLogManger.CancelAllOutstandingAmendments();
			CancelQueuedMessageLogs();
			foreach (var currentPack in PackingGroups.Cast<PackingGroup>())
			{
				currentPack.CR_HouseContainerNumber = ZShort.Zero;
			}
		}

		protected override void RemoveAndDeleteAllRequiredEntryHeaders()
		{
			var colsHeader = QuarantineCOLSHeader;
			if (colsHeader == null)
			{
				base.RemoveAndDeleteAllRequiredEntryHeaders();
			}
			else
			{
				var entryHeaders = CustomsEntryHeaders.Where(x => x.PK != colsHeader.QCH_CH_CusEntryHeader).ToArray();
				foreach (var entryHeader in entryHeaders)
				{
					CustomsEntryHeaders.RemoveAndDelete(entryHeader);
				}
			}
		}

		public bool IsNature10 => (isNature10 ?? (isNature10 = new RecalculableCachedValue<bool>(() =>
		{
			var potentialN10Entry = JE_MessageType == JobMessageTypeList.Codes.Import &&
									 JE_MessageSubType == MessageSubType.FormalEntry;
			if (potentialN10Entry)
			{
				potentialN10Entry = !InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_IsPackToBondForLine);
			}

			return potentialN10Entry;
		}))).Value;
		RecalculableCachedValue<bool> isNature10;

		internal void InvalidateIsNature10Cache() => isNature10?.InvalidateCache();

		public bool IsNature30
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.ExWarehouse; }
		}

		public bool IsNonConfirming
		{
			get { return IsExport && JE_MessageSubType == MessageSubType.NonConfirming; }
		}

		public override ZInt JE_TotalNoOfPieces
		{
			get { return base.JE_TotalNoOfPieces; }
			set
			{
				var hasChanges = base.JE_TotalNoOfPieces != value;
				base.JE_TotalNoOfPieces = value;
				if (hasChanges)
				{
					DefaultPiecesToPackingGroupOuterPacksIfRequired();
				}
			}
		}

		void DefaultPiecesToPackingGroupOuterPacksIfRequired()
		{
			if (!IsCopying && IsSea && LowestBills.Count == 1 && JE_TotalNoOfPieces > 0 && IsPackingInformationRelevant)
			{
				if (CusContainers.Count > 1)
				{
					DefaultOuterPacksForContainerisedJob();
				}
				else
				{
					var package = Packages.Count == 0 ? Packages.AddNew() : Packages[0];
					package.CW_OuterPacks = JE_TotalNoOfPieces;
					SetDefaultValueToPackage(package);
				}
			}
		}

		void SetDefaultValueToPackage(Package package)
		{
			var bill = LowestBills[0];
			package.CW_HouseBill = bill.CU_BillUniqueCode;
		}

		void DefaultOuterPacksForContainerisedJob()
		{
			foreach (Package package in Packages)
			{
				package.CW_OuterPacks = 1;
			}
		}

		public override ZString JE_TotalNoOfPacksPackType
		{
			get { return base.JE_TotalNoOfPacksPackType; }
			set
			{
				var hasChanges = value != JE_TotalNoOfPacksPackType;
				base.JE_TotalNoOfPacksPackType = value;
				if (hasChanges && !IsCopying)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZInt JE_TotalNoOfPacks
		{
			get
			{
				return base.JE_TotalNoOfPacks;
			}
			set
			{
				var hasChanges = value != JE_TotalNoOfPacks;
				base.JE_TotalNoOfPacks = value;
				if (hasChanges && !IsCopying)
				{
					Invoices.MarkAsNeedingValidation();
					Packages.MarkAsNeedingValidation();
					Packages.RunPreSaveValidation();
				}
			}
		}

		public override ZBool JE_OverrideFreightDefaults
		{
			get
			{
				return base.JE_OverrideFreightDefaults;
			}
			set
			{
				var hasChange = JE_OverrideFreightDefaults != value;
				base.JE_OverrideFreightDefaults = value;

				if (hasChange)
				{
					UpdateJE_ToOrder();
				}
			}
		}

		#endregion

		#region IAddInfo Members

		public ZDateTime EffectiveDutyDate
		{
			get
			{
				if (!isEffectiveDutyDateValid)
				{
					isEffectiveDutyDateValid = true;
					fEffectiveDutyDate = ZDateTime.Today;

					var entrySubmittedDateForPostLodgedDeclaration = EntrySubmittedDateForPostLodgedDeclaration;
					if (entrySubmittedDateForPostLodgedDeclaration.IsValid)
					{
						if (IsExWarehouse)
						{
							fEffectiveDutyDate = entrySubmittedDateForPostLodgedDeclaration;
						}
						else if (TypeValidation.IsWithinValidZDateTimeRangeWithoutError(JE_DateOfFirstArrival) && JE_DateOfFirstArrival.IsInThePastDatePartOnly)
						{
							fEffectiveDutyDate = JE_DateOfFirstArrival < entrySubmittedDateForPostLodgedDeclaration
												? entrySubmittedDateForPostLodgedDeclaration
												: JE_DateOfFirstArrival;
						}
					}
				}
				return fEffectiveDutyDate;
			}
		}
		ZDateTime fEffectiveDutyDate;
		bool isEffectiveDutyDateValid;

		public void NotifyEffectiveDutyDateDirty()
		{
			isEffectiveDutyDateValid = false;
			Invoices.NotifyEffectiveDutyDateDirty();
		}

		public ZDateTime EntrySubmittedDateForPostLodgedDeclaration
		{
			get { return CustomsEntryHeaders.HasEntryWithPostLodgeStatus ? JE_EntrySubmittedDate : ZDateTime.Empty; }
		}

		public override ZDateTime JE_EntrySubmittedDate
		{
			get { return base.JE_EntrySubmittedDate; }
			set
			{
				var hasChanges = base.JE_EntrySubmittedDate != value;
				base.JE_EntrySubmittedDate = value;
				if (hasChanges)
				{
					NotifyEffectiveDutyDateDirty();
				}
			}
		}

		public override ZDateTime JE_ExportDate
		{
			get
			{
				if (IsExWarehouse)
				{
					return ZDateTime.Empty;
				}
				else
				{
					return base.JE_ExportDate;
				}
			}
			set
			{
				base.JE_ExportDate = value;
			}
		}

		public ZString AggregatedZA_ORG
		{
			get { return ZString.Empty; }
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get { return ZString.Empty; }
		}

		public IZType AggregatedValue(string propertyName)
		{
			return (IZType)AddInfo[propertyName];
		}

		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AUAddInfo(this, JE_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					using (fAddInfo.SuspendSettingHasChanges())
					{
						((ILightValidationInternals)fAddInfo).IsValid = ((ILightValidationInternals)this).IsValid;
					}
				}
				return fAddInfo;
			}
		}

		bool IAggregatedAddInfo.IsCopying
		{
			get
			{
				return IsCopying;
			}
		}

		#endregion

		#region Quarantine To Order

		public bool JE_ToOrder_ReadOnly
		{
			get { return ShouldSynchroniseWithShipment(); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_ImporterToOrder_Hidden)]
		public ZBool JE_ToOrder
		{
			get { return AddInfo.ZA_ImporterToOrder_Hidden; }
			set
			{
				AddInfo.ZA_ImporterToOrder_Hidden = value;
				if (AddInfo.ZA_ImporterToOrder_Hidden)
				{
					JE_OH_Importer = ZGuid.Empty;
					JE_ToOrderComment = "UNKNOWN";
				}
				else
				{
					JE_ToOrderComment = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo JE_ToOrderInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_ToOrder, x => AddInfo.ZA_ImporterToOrder_HiddenInfo); }
		}

		void UpdateJE_ToOrder()
		{
			if (JE_OH_Importer != ZGuid.Empty || JE_MessageType != JobMessageTypeList.Codes.Quarantine || (Shipment != null && !JE_OverrideFreightDefaults))
			{
				JE_ToOrder = false;
			}
		}

		#endregion

		#region Quarantine To Order Comment
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_ImporterToOrderComment_Hidden)]
		public ZString JE_ToOrderComment
		{
			get
			{
				return AddInfo.ZA_ImporterToOrderComment_Hidden;
			}

			set
			{
				AddInfo.ZA_ImporterToOrderComment_Hidden = value;
			}
		}

		public ZPropertyInfo JE_ToOrderCommentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_ToOrderComment, x => AddInfo.ZA_ImporterToOrderComment_HiddenInfo); }
		}

		#endregion

		#region Declaration Status

		#region JE_EntryStatus

		protected override void DeriveExportDeclarationStatus()
		{
			var status = CustomsEntryStatus.NotSent;
			if (IsHolding)
			{
				status = CustomsEntryStatus.HoldAwaiting;
			}
			else if (IsDeclarationWorkFinished)
			{
				status = CustomsEntryStatus.DeclarationWorkComplete;
			}
			JE_EntryStatus = status.Code;
		}

		public void ResetDeclarationStatus()
		{
			DeriveImportDeclarationStatus();
		}

		protected override void DeriveImportDeclarationStatus()
		{
			if (IsImportCMR)
			{
				if (IsDeclarationWorkFinished)
				{
					JE_MessageStatus = CustomsEntryStatus.DeclarationWorkComplete.Code;
					JE_EntryStatus = CustomsEntryStatus.DeclarationWorkComplete.Code;
				}
				else
				{
					var summaryMessageStatus = SummaryEntryStatusCalculator.SummaryMessageStatus;
					var summaryEntryStatus = SummaryEntryStatusCalculator.SummaryEntryStatus;

					if (!MergeManager.RequiresMerge && IsMergeDone)
					{
						#pragma warning disable IDE0001 // Prevent simplification to base class
						using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(this))
						#pragma warning restore IDE0001 // Prevent simplification to base class
						{
							JE_MessageStatus = summaryMessageStatus;
							JE_EntryStatus = summaryEntryStatus;
						}
					}
					else
					{
						JE_MessageStatus = summaryMessageStatus;
						JE_EntryStatus = summaryEntryStatus;
					}
				}
			}
			else
			{
				#region Edifice
				var redLineEntry = false;
				var amberLineEntry = false;
				var communityProtectionCheckEntry = false;
				var cargoExaminationEntry = false;

				var hasPendingCreates = false;
				var hasPendingCPDecs = false;
				var hasPendingLodges = false;
				var hasPendingPays = false;

				var hasCreateFailed = false;
				var hasCPDecFailed = false;
				var hasLodgeFailed = false;
				var hasPayFailed = false;

				var allEntriesAreCreated = CustomsEntryHeaders.Count > 0;
				var allEntriesAreCPDeced = CustomsEntryHeaders.Count > 0;
				var allEntriesAreLodged = CustomsEntryHeaders.Count > 0;
				var allEntriesArePaid = CustomsEntryHeaders.Count > 0;

				var impediments = false;

				var entryNotReleased = false;

				var withdrawPending = false;
				var applicableStatuses = new ArrayList();
				if (HaveAllEntriesHadTheirGoodsReleased)
				{
					applicableStatuses.Add(CustomsEntryStatus.CargoCleared);
				}

				if (hasCreateFailed)
				{
					applicableStatuses.Add(CustomsEntryStatus.FailCreate);
				}

				if (hasCPDecFailed)
				{
					applicableStatuses.Add(CustomsEntryStatus.FailCPDec);
				}

				if (hasLodgeFailed)
				{
					applicableStatuses.Add(CustomsEntryStatus.FailLodge);
				}

				if (hasPayFailed)
				{
					applicableStatuses.Add(CustomsEntryStatus.FailPay);
				}

				if (hasPendingPays && !allEntriesArePaid)
				{
					applicableStatuses.Add(CustomsEntryStatus.AwaitingPay);
				}

				if (hasPendingLodges && !allEntriesAreLodged)
				{
					applicableStatuses.Add(CustomsEntryStatus.AwaitingLodge);
				}

				if (hasPendingCPDecs && !allEntriesAreCPDeced)
				{
					applicableStatuses.Add(CustomsEntryStatus.AwaitingCPDec);
				}

				if (hasPendingCreates && !allEntriesAreCreated)
				{
					applicableStatuses.Add(CustomsEntryStatus.AwaitingCreate);
				}

				if (allEntriesArePaid)
				{
					applicableStatuses.Add(CustomsEntryStatus.ClearPay);
				}

				if (allEntriesAreLodged)
				{
					applicableStatuses.Add(CustomsEntryStatus.ClearLodge);
				}

				if (allEntriesArePaid && impediments)
				{
					applicableStatuses.Add(CustomsEntryStatus.ClearPayWithImpediments);
				}

				if (allEntriesAreLodged && impediments)
				{
					applicableStatuses.Add(CustomsEntryStatus.ClearLodgeWithImpediments);
				}

				if (allEntriesAreCPDeced)
				{
					applicableStatuses.Add(CustomsEntryStatus.ClearCPDec);
				}

				if (allEntriesAreCreated)
				{
					applicableStatuses.Add(CustomsEntryStatus.ClearCreate);
				}

				if (redLineEntry)
				{
					applicableStatuses.Add(CustomsEntryStatus.RedLine);
				}

				if (amberLineEntry)
				{
					applicableStatuses.Add(CustomsEntryStatus.AmberLine);
				}

				if (communityProtectionCheckEntry)
				{
					applicableStatuses.Add(CustomsEntryStatus.CommunityProtectionCheck);
				}

				if (cargoExaminationEntry)
				{
					applicableStatuses.Add(CustomsEntryStatus.SelectedForCargoExamination);
				}

				if (entryNotReleased)
				{
					applicableStatuses.Add(CustomsEntryStatus.CargoNotCleared);
				}

				if (IsHolding)
				{
					applicableStatuses.Add(CustomsEntryStatus.HoldAwaiting);
				}

				if (IsDeclarationWorkFinished)
				{
					applicableStatuses.Add(CustomsEntryStatus.DeclarationWorkComplete);
				}

				if (withdrawPending)
				{
					applicableStatuses.Add(CustomsEntryStatus.AwaitingWithdrawal);
				}

				JE_EntryStatus = CustomsEntryStatus.MostImportantStatusForEdifice((CustomsEntryStatus[])applicableStatuses.ToArray(typeof(CustomsEntryStatus))).Code;
				if (JE_EntryStatus == "NOT")
				{
					JE_EntryStatus = ZString.Empty;//TODO: Change this back when the Not Sent code is 'NOT' CustomsEntryStatus.NotSent.Code
				}

				#endregion
			}
		}

		public SummaryEntryStatusCalculator SummaryEntryStatusCalculator
		{
			get
			{
				if (fSummaryEntryStatusCalculator == null)
				{
					fSummaryEntryStatusCalculator = new SummaryEntryStatusCalculator(this);
				}
				return fSummaryEntryStatusCalculator;
			}
		}
		SummaryEntryStatusCalculator fSummaryEntryStatusCalculator;

		protected enum MessageStatus
		{
			NotSent,
			Awaiting,
			Clear,
			Fail
		}

		protected MessageStatus GetMessageStatus(CusEntryHeader header, ZString messageType)
		{
			var lastMessage = LastMessageOfType(header.Messages, messageType);
			if (lastMessage == null)
			{
				return MessageStatus.NotSent;
			}
			else if (lastMessage.EM_Status == EDIMessage.Status.Failed)
			{
				return MessageStatus.Fail;
			}
			else
			{
				return MessageStatus.Awaiting;
			}
		}

		protected EDIMessage LastMessageOfType(EDIMessageCollection messages, ZString messageType)
		{
			EDIMessage result = null;
			foreach (var message in messages.Cast<EDIMessage>())
			{
				if (message.EM_MessageType == messageType && message.EM_Status != EDIMessage.Status.Cancelled && message.EM_Status != EDIMessage.Status.Withdrawn && (result == null || message.EM_SystemCreateTimeUtc > result.EM_SystemCreateTimeUtc))
				{
					result = message;
				}
			}
			return result;
		}

		ZString ICMRControlMessageRespondee.UpdateStatusWhenControlMessageSyntaxError(EDIMessage incomingMessage, EDIMessage outgoingMessage)
		{
			var logText = ZString.Empty;
			var newStatus = ZString.Empty;
			if (IsExport)
			{
				switch (outgoingMessage.EM_MessageType)
				{
					case CMRMessage.CMRMessageTypes.EXD:
						EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, this);
						logText = "Export JobDeclaration to: " + JE_EntryStatus;
						break;
					case CMRMessage.CMRMessageTypes.WARREL:
						switch (outgoingMessage.EM_MessageSubType)
						{
							case CMRMessage.MessageSubTypes.Original:
								newStatus = CustomsEntryStatus.FailWARRELOriginal.Code;
								break;
							case CMRMessage.MessageSubTypes.Change:
							case CMRMessage.MessageSubTypes.Amendment:
								newStatus = CustomsEntryStatus.FailWARRELReplacement.Code;
								break;
							case CMRMessage.MessageSubTypes.Withdraw:
								newStatus = CustomsEntryStatus.FailWARRELWithdrawal.Code;
								break;
							default:
								throw new InvalidOperationException("Unexpected WARREL Message Sub Type In JobDeclaration UpdateStatusWhenControlMessageSyntaxError: " + outgoingMessage.EM_MessageSubType);
						}
						break;
					case CMRMessage.CMRMessageTypes.WARRET:
						switch (outgoingMessage.EM_MessageSubType)
						{
							case CMRMessage.MessageSubTypes.Original:
								newStatus = CustomsEntryStatus.FailWARRETOriginal.Code;
								break;
							case CMRMessage.MessageSubTypes.Change:
							case CMRMessage.MessageSubTypes.Amendment:
								newStatus = CustomsEntryStatus.FailWARRETReplacement.Code;
								break;
							default:
								throw new InvalidOperationException("Unexpected WARRET Message Sub Type In JobDeclaration UpdateStatusWhenControlMessageSyntaxError: " + outgoingMessage.EM_MessageSubType);
						}
						break;
					case CMRMessage.CMRMessageTypes.DEPREC:
						switch (outgoingMessage.EM_MessageSubType)
						{
							case CMRMessage.MessageSubTypes.Original:
								newStatus = CustomsEntryStatus.FailDEPRECOriginal.Code;
								break;
							case CMRMessage.MessageSubTypes.Change:
							case CMRMessage.MessageSubTypes.Amendment:
								newStatus = CustomsEntryStatus.FailDEPRECReplacement.Code;
								break;
							case CMRMessage.MessageSubTypes.Withdraw:
								newStatus = CustomsEntryStatus.FailDEPRECWithdrawal.Code;
								break;
							default:
								throw new InvalidOperationException("Unexpected DEPREC Message Sub Type In JobDeclaration UpdateStatusWhenControlMessageSyntaxError: " + outgoingMessage.EM_MessageSubType);
						}
						break;
					case CMRMessage.CMRMessageTypes.DEPREL:
						switch (outgoingMessage.EM_MessageSubType)
						{
							case CMRMessage.MessageSubTypes.Original:
								newStatus = CustomsEntryStatus.FailDEPRELOriginal.Code;
								break;
							case CMRMessage.MessageSubTypes.Change:
							case CMRMessage.MessageSubTypes.Amendment:
								newStatus = CustomsEntryStatus.FailDEPRELReplacement.Code;
								break;
							case CMRMessage.MessageSubTypes.Withdraw:
								newStatus = CustomsEntryStatus.FailDEPRELWithdrawal.Code;
								break;
							default:
								throw new InvalidOperationException("Unexpected DEPREL Message Sub Type In JobDeclaration UpdateStatusWhenControlMessageSyntaxError: " + outgoingMessage.EM_MessageSubType);
						}
						break;
				}
				if (newStatus != ZString.Empty)
				{
					JE_MessageStatus = newStatus;
					logText = "Export JobDeclaration Message Status to: " + newStatus;
				}
			}
			return logText;
		}

		#endregion

		#region Hold-Awaiting Status
		public void PlaceHold(ZString reason)
		{
			var maxLength = StmALogSchema.SL_Reference.MaxLength - 30;
			HoldAwaitingLogs.AddNew("Reason: '" + reason.Left(maxLength) + "'");
			fJE_MessageStatusDescription = ZString.Empty;
			DeriveImportDeclarationStatus();
		}

		public void RemoveHold()
		{
			var mostRecentHold = MostRecentHold;
			if (mostRecentHold != null)
			{
				mostRecentHold.Cancel();
				fJE_MessageStatusDescription = ZString.Empty;
				fRefreshCurrentEntryMessageStatus = true;
				DeriveImportDeclarationStatus();
				fRefreshCurrentEntryMessageStatus = false;
			}
		}

		protected internal StmALog MostRecentHold
		{
			get { return HoldAwaitingLogs.MostRecentLog; }
		}

		public ZString GetHoldReason()
		{
			var mostRecentHold = MostRecentHold;
			return mostRecentHold != null ? mostRecentHold.SL_Reference : ZString.Empty;
		}

		public bool IsHolding
		{
			get
			{
				var mostRecentHold = MostRecentHold;
				return mostRecentHold != null && !mostRecentHold.SL_IsCancelled;
			}
		}

		public LogsForNominatedEvent HoldAwaitingLogs => Factory.GetValue(ref fHoldAwaitingLogs, delegate
		{
			return new LogsForNominatedEvent(Logs, Events.HoldAwaiting);
		});

		CachedProperty<LogsForNominatedEvent> fHoldAwaitingLogs;

		public bool RefreshCurrentEntryMessageStatus
		{
			get
			{
				return fRefreshCurrentEntryMessageStatus;
			}
		}
		protected bool fRefreshCurrentEntryMessageStatus;

		#endregion

		#region Declaration Work Finished Status
		public void PlaceDeclarationWorkComplete(ZString reason)
		{
			var maxLength = StmALogSchema.SL_Reference.MaxLength - 30;
			DeclarationWorkCompleteLogs.AddNew("Reason: '" + reason.Left(maxLength) + "'");
			DeriveImportDeclarationStatus();
		}

		public void RemoveDeclarationWorkComplete()
		{
			var mostRecentDWC = MostRecentDWC;
			if (mostRecentDWC != null)
			{
				mostRecentDWC.Cancel();
				DeriveImportDeclarationStatus();
			}
		}

		protected internal StmALog MostRecentDWC
		{
			get { return DeclarationWorkCompleteLogs.MostRecentLog; }
		}

		public string GetDeclarationWorkCompleteReason()
		{
			var mostRecentDWC = MostRecentDWC;
			if (mostRecentDWC != null)
			{
				return mostRecentDWC.SL_Reference;
			}
			else
			{
				return "";
			}
		}

		public bool IsDeclarationWorkFinished
		{
			get
			{
				var mostRecentDWC = MostRecentDWC;
				return mostRecentDWC != null && !mostRecentDWC.SL_IsCancelled && !mostRecentDWC.SL_IsEstimate;
			}
		}

		protected LogsForNominatedEvent fDeclarationWorkCompleteLogs;
		public LogsForNominatedEvent DeclarationWorkCompleteLogs
		{
			get
			{
				if (fDeclarationWorkCompleteLogs == null)
				{
					fDeclarationWorkCompleteLogs = new LogsForNominatedEvent(Logs, Events.DeclarationWorkComplete);
				}
				return fDeclarationWorkCompleteLogs;
			}
		}
		#endregion

		#region Authority to pay

		internal LogsForNominatedEvent AuthorityToPayLogs
		{
			get
			{
				if (fAuthorityToPayLog == null)
				{
					fAuthorityToPayLog = new LogsForNominatedEvent(Logs, Events.HoldStatusOverride);
				}
				return fAuthorityToPayLog;
			}
		}
		LogsForNominatedEvent fAuthorityToPayLog;

		public StmALog LiveAuthorityToPayLog
		{
			get
			{
				StmALog result = null;
				foreach (var aLog in AuthorityToPayLogs.Cast<StmALog>())
				{
					if (!aLog.SL_IsCancelled)
					{
						result = aLog;
						break;
					}
				}
				return result;
			}
		}

		public void AddAuthorityToPayLog()
		{
			if (LiveAuthorityToPayLog == null)
			{
				AuthorityToPayLogs.AddNew("EFP Payment Authority given by the importer");
			}
		}

		#endregion

		#endregion

		#region Weekly settlement fields

		public ZBool SettlementTypeSelected => !AddInfo.ZA_SettlementPeriodType_Hidden.IsEmpty;

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_SettlementPeriodType_Hidden)]
		public ZString JE_SettlementPeriodType
		{
			get => AddInfo.ZA_SettlementPeriodType_Hidden;
			set
			{
				var oldValue = JE_SettlementPeriodType;
				AddInfo.ZA_SettlementPeriodType_Hidden = value;
				if (oldValue.IsEmpty ^ JE_SettlementPeriodType.IsEmpty)
				{
					ResetIsWHSUniversalXMLActive();
				}
			}
		}
		public ZPropertyInfo JE_SettlementPeriodTypeInfo => GetWrappedZPropertyInfo(nameof(JE_SettlementPeriodType), c => AddInfo.ZA_SettlementPeriodType_HiddenInfo);

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_NilReturnInd_Hidden)]
		public ZBool NilReturnInd
		{
			get { return (AddInfo.ZA_NilReturnInd_Hidden == "Y"); }
			set
			{
				if (NilReturnInd ^ value)
				{
					AddInfo.ZA_NilReturnInd_Hidden = value ? "Y" : "";
					ResetIsWHSUniversalXMLActive();
				}
			}
		}
		public ZPropertyInfo NilReturnIndInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NilReturnInd), x => AddInfo.ZA_NilReturnInd_HiddenInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_SettlementPeriodStartDate_Hidden)]
		public ZDateTime SettlementPeriodStartDate
		{
			get { return AddInfo.ZA_SettlementPeriodStartDate_Hidden; }
			set { AddInfo.ZA_SettlementPeriodStartDate_Hidden = value; }
		}
		public ZPropertyInfo SettlementPeriodStartDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SettlementPeriodStartDate), x => AddInfo.ZA_SettlementPeriodStartDate_HiddenInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_SettlementPeriodEndDate_Hidden)]
		public ZDateTime JE_SettlementPeriodEndDate
		{
			get => AddInfo.ZA_SettlementPeriodEndDate_Hidden;
			set => AddInfo.ZA_SettlementPeriodEndDate_Hidden = value;
		}
		public ZPropertyInfo JE_SettlementPeriodEndDateInfo => GetWrappedZPropertyInfo(nameof(JE_SettlementPeriodEndDate), x => AddInfo.ZA_SettlementPeriodEndDate_HiddenInfo);

		public bool SubmitWeeklyNilReturnN30 => SettlementTypeSelected && NilReturnInd;

		#endregion

		#region NewProperties

		public IOrgHeaderWrapper SupplierWrapper
		{
			get
			{
				IOrgHeaderWrapper result = null;
				var supplier = Supplier;
				if (supplier != null)
				{
					if (supplier.IsMiscellaneous)
					{
						result = new MiscOrganisationWrapper(this);
					}
					else
					{
						result = new OrgHeaderWrapper(supplier);
					}
				}
				return result;
			}
		}

		public ZBool SentWithMessageErrors
		{
			get
			{
				return AddInfo.ZA_SentWithMessageErrors_Hidden == "Y";
			}
			set
			{
				if (SentWithMessageErrors != value)
				{
					AddInfo.ZA_SentWithMessageErrors_Hidden = value ? "Y" : "N";
				}
			}
		}

		public ZString ZA_GoodsOwnerPartyIDHidden
		{
			get { return AddInfo.ZA_GoodsOwnerPartyIDHidden; }
			set { AddInfo.ZA_GoodsOwnerPartyIDHidden = value; }
		}

		public ZPropertyInfo ZA_GoodsOwnerPartyIDHiddenInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_GoodsOwnerPartyIDHidden, x => AddInfo.ZA_GoodsOwnerPartyIDHiddenInfo); }
		}

		public ZString ZA_ConsigneeNameHidden
		{
			get { return AddInfo.ZA_ConsigneeNameHidden; }
			set { AddInfo.ZA_ConsigneeNameHidden = value; }
		}

		public ZPropertyInfo ZA_ConsigneeNameHiddenInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ConsigneeNameHidden, x => AddInfo.ZA_ConsigneeNameHiddenInfo); }
		}

		public ZString ZA_ConsigneeCityHidden
		{
			get { return AddInfo.ZA_ConsigneeCityHidden; }
			set { AddInfo.ZA_ConsigneeCityHidden = value; }
		}

		public ZPropertyInfo ZA_ConsigneeCityHiddenInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ConsigneeCityHidden, x => AddInfo.ZA_ConsigneeCityHiddenInfo); }
		}

		public override ZString JE_SupplierMiscFields
		{
			get
			{
				if (JE_OH_Supplier == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation)
				{
					if (IsExport)
					{
						return AUAddInfoSchema.ZA_GoodsOwnerPartyIDHidden.Name;
					}
					else
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public override ZString JE_ImporterMiscFields
		{
			get
			{
				if (JE_OH_Importer == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation)
				{
					if (IsExport)
					{
						return AUAddInfoSchema.ZA_ConsigneeNameHidden.Name + "," + AUAddInfoSchema.ZA_ConsigneeCityHidden.Name;
					}
					else
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}
		public ZBool JE_ForcePrimeEnclosure
		{
			get { return (AddInfo.ZA_ForcePrimeEnclosureIfMultipleEntries_Hidden == "Y"); }
			set
			{
				if (JE_ForcePrimeEnclosure != value)
				{
					AddInfo.ZA_ForcePrimeEnclosureIfMultipleEntries_Hidden = value ? "Y" : "N";
					HasChanges = true;
					JE_ForcePrimeEnclosureInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo JE_ForcePrimeEnclosureInfo
		{
			get { return GetZPropertyInfo(Schema.JE_ForcePrimeEnclosure); }
		}

		public virtual ZString EntryPrinter => AddInfo.ZA_PrinterNumber_Hidden;

		public virtual ZString EFTReceiptPrinter => AddInfo.ZA_EFTReceiptPrinter_Hidden;

		public virtual ZString EntryClearancePrinter => AddInfo.ZA_ClearanceAdvicePrinter_Hidden;

		public ZString OwnerCode
		{
			get
			{
				var ownerCode = ZString.Empty;
				if (Importer != null)
				{
					ownerCode = IsImportCMR ? Importer.GetCustomsClientID() : Importer.LocalCustomsClientCode;
				}
				return ownerCode;
			}
		}

		public virtual ZString BranchBoxNo => ZString.Empty;

		public ZString WarehouseLocationCode => WarehouseAddress != null ? WarehouseAddress.LocalControlledPremisesID : ZString.Empty;

		public ZString CRN
		{
			get
			{
				var result = ZString.Empty;
				if (Shipment != null)
				{
					foreach (var consol in Shipment.Consols.Cast<ForwardingConsol>())
					{
						var wrapper = new FreightConsolWrapper(consol);
						var entryNum = wrapper.GetPermit();
						if (entryNum != null && !entryNum.CE_EntryNum.IsEmpty)
						{
							result = entryNum.CE_EntryNum;
							break;
						}
					}
				}
				return result;
			}
		}

		public ZString DepotID
		{
			get { return DepotDocAddress.Address != null ? DepotDocAddress.Address.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZString CTOID
		{
			get { return ContainerTerminalOperatorDocAddress.Address != null ? ContainerTerminalOperatorDocAddress.Address.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZString DepotOrCTOID
		{
			get { return DepotOrCTOAddress != null ? DepotOrCTOAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZString WarehouseID
		{
			get { return WarehouseAddress != null ? WarehouseAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZString CleanVoyageNumber
		{
			get { return JE_VoyageFlightNo.KeepChars("1234567890").TrimStart('0'); }
		}

		public override ZString JE_MessageStatus
		{
			get { return base.JE_MessageStatus; }
			set
			{
				if (JE_MessageStatus != value)
				{
					base.JE_MessageStatus = value;

					SetMessageStatusDescription();
					readOnlyNeedsRecalculation = true;
					RefreshBindingAndChildrenReadOnly();
				}
			}
		}

		public void SetMessageStatusDescription()
		{
			if (Lookups.MessageStatusList != null && Lookups.MessageStatusList.ContainsCode(JE_MessageStatus))
			{
				fJE_MessageStatusDescription = Lookups.MessageStatusList.GetDescriptionFromCode(JE_MessageStatus);

				if (IsQueuedEntryLodgement)
				{
					fJE_MessageStatusDescription += $" message to be sent {JE_EDITransmitDate.ToShortDateString()}";
				}

				if (IsQueuedEntryPayment)
				{
					var scheduledPaymentDate = CustomsEntryHeaders.FirstOrDefault(x => x.CH_Status == CustomsEntryStatus.ScheduledPayment.Code)?.ScheduledPaymentDate.ToBestReadableDateTimeString() ?? string.Empty;
					fJE_MessageStatusDescription += $" to be sent {scheduledPaymentDate}";
				}
			}
		}

		public override ZString JE_MessageStatusDescription
		{
			get
			{
				if (fJE_MessageStatusDescription == "")
				{
					SetMessageStatusDescription();
				}
				return fJE_MessageStatusDescription;
			}
		}
		ZString fJE_MessageStatusDescription;

		/// <summary>
		/// This property depends on Logs. Don't expose it on Module grid.
		/// </summary>
		public ZString JE_MessageStatusDescriptionIncludingOustandingAmendments => Factory.GetValue(ref jE_MessageStatusDescriptionIncludingOustandingAmendmentsCached, delegate
		{
			var result = JE_MessageStatusDescription;
			if (HasOutstandingAmendment)
			{
				result += " " + OutstandingAmendmentDescription;
			}
			else if (HasOutstandingAmendmentNotQueued)
			{
				result += " " + OutstandingAmendmentNotQueuedDescription;
			}
			else if (HasOutstandingManualAmendments)
			{
				result += " " + OutstandingManualAmendmentDescription;
			}
			else if (HasOutstandingFailedAmendments)
			{
				result += " " + AmendmentFailedDescription;
			}
			else if (HasConsolidatedEntryChanges)
			{
				result += " " + ConsolidatedEntryChangedDescription;
			}

			return result;
		});

		CachedProperty<ZString> jE_MessageStatusDescriptionIncludingOustandingAmendmentsCached;
		internal string OutstandingAmendmentDescription => ResString.GetMultilingualString("AU|JobDeclaration|OutstandingAmendmentDescription", "(Amendment detected, NOT Lodged. See 'Events' tab or 'Details' button.)");
		internal string OutstandingAmendmentNotQueuedDescription => ResString.GetMultilingualString("AU|JobDeclaration|OutstandingAmendmentNotQueuedDescription", "(Saved without sending Amendment, NOT Lodged. See 'Events' tab.)");
		internal string OutstandingManualAmendmentDescription => ResString.GetMultilingualString("AU|JobDeclaration|OutstandingManualAmendmentDescription", "(Amended through CI, Details may not match ICS.)");
		internal string AmendmentFailedDescription => ResString.GetMultilingualString("AU|JobDeclaration|AmendmentFailedDescription", "(Amendment Failed, Details may not match ICS.)");
		internal string ConsolidatedEntryChangedDescription => ResString.GetMultilingualString("AU|JobDeclaration|ConsolidatedEntryChangedDescription", "(Changed, Amendment may be required.)");

		public ZPropertyInfo JE_MessageStatusDescriptionIncludingOustandingAmendmentsInfo
		{
			get { return GetZPropertyInfo(nameof(JE_MessageStatusDescriptionIncludingOustandingAmendments)); }
		}

		#region Paid Under Protest

		[MaxLength(4000)]
		public ZString JE_PaidUnderProtestStatement
		{
			get
			{
				return (PaidUnderProtestNote != null && !PaidUnderProtestNote.IsDeleted) ? PaidUnderProtestNote.ST_NoteDataAsText : ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JE_PaidUnderProtestStatementInfo, value);

					if (PaidUnderProtestNote == null)
					{
						PaidUnderProtestNote = CreateNoteAndMakeReadOnly(PaidUnderProtestDescription);
						RegisterEditableChildObject(PaidUnderProtestNote);
					}

					PaidUnderProtestNote.ST_NoteDataAsText = value;
				}
				else
				{
					if (PaidUnderProtestNote != null)
					{
						PaidUnderProtestNote.Delete();
						PaidUnderProtestNote = null;
					}
				}
				JE_PaidUnderProtestStatementInfo.RefreshBinding();
				Validation.ValidateJE_PaidUnderProtestStatement();
			}
		}

		StmNote PaidUnderProtestNote
		{
			get
			{
				if (fPaidUnderProtestNote == null)
				{
					fPaidUnderProtestNote = LoadNoteAndMakeReadOnly(PaidUnderProtestDescription);
					if (fPaidUnderProtestNote != null)
					{
						RegisterEditableChildObject(fPaidUnderProtestNote);
					}
				}

				return fPaidUnderProtestNote;
			}
			set
			{
				fPaidUnderProtestNote = value;
			}
		}
		StmNote fPaidUnderProtestNote;
		const string PaidUnderProtestDescription = "PAID UNDER PROTEST";

		public ZPropertyInfo JE_PaidUnderProtestStatementInfo
		{
			get { return GetZPropertyInfo(nameof(JE_PaidUnderProtestStatement)); }
		}

		#endregion

		#region Amber Statement

		[MaxLength(2560)]
		public ZString JE_AmberStatement
		{
			get
			{
				return (AmberStatementNote != null && !AmberStatementNote.IsDeleted) ? AmberStatementNote.ST_NoteDataAsText : ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JE_AmberStatementInfo, value);

					if (AmberStatementNote == null)
					{
						AmberStatementNote = CreateNoteAndMakeReadOnly(AmberStatementDescription);
						RegisterEditableChildObject(AmberStatementNote);
					}
					AmberStatementNote.ST_NoteDataAsText = value;
				}
				else
				{
					if (AmberStatementNote != null)
					{
						AmberStatementNote.Delete();
						AmberStatementNote = null;
					}
				}
				JE_AmberStatementInfo.RefreshBinding();
				Validation.ValidateJE_AmberStatement();
			}
		}

		StmNote AmberStatementNote
		{
			get
			{
				if (fAmberStatementNote == null)
				{
					fAmberStatementNote = LoadNoteAndMakeReadOnly(AmberStatementDescription);
					if (fAmberStatementNote != null)
					{
						RegisterEditableChildObject(fAmberStatementNote);
					}
				}

				return fAmberStatementNote;
			}
			set
			{
				fAmberStatementNote = value;
			}
		}
		StmNote fAmberStatementNote;
		const string AmberStatementDescription = "AMBER STATEMENT";

		public ZPropertyInfo JE_AmberStatementInfo
		{
			get { return GetZPropertyInfo(nameof(JE_AmberStatement)); }
		}

		#endregion

		StmNote CreateNoteAndMakeReadOnly(string description)
		{
			StmNote result;
			if (Shipment != null)
			{
				result = Shipment.Notes.AddNew();
			}
			else
			{
				result = Notes.AddNew();
			}

			result.ST_Description = description;
			result.ST_IsCustomDescription = true;
			result.ST_NoteType = nameof(StmNoteVisibility.INT);

			return result;
		}

		StmNote LoadNoteAndMakeReadOnly(string description)
		{
			var filter = new ZQuery(StmNoteSchema.ST_Description, description);

			if (Shipment != null)
			{
				filter.AddToFilter(StmNoteSchema.ST_ParentID, Shipment.PK);
				filter.AddToFilter(StmNoteSchema.ST_Table, "JobShipment");
				filter.FetchOnlyFromLocalCache = !Shipment.IsInDatabase;
			}
			else
			{
				filter.AddToFilter(StmNoteSchema.ST_ParentID, PK);
				filter.AddToFilter(StmNoteSchema.ST_Table, "JobDeclaration");
				filter.FetchOnlyFromLocalCache = !IsInDatabase;
			}

			return Factory.LoadTop1<StmNote>(filter);
		}

		#endregion

		#region Declaration - Other Export Customs messages

		public enum ExportOtherMessageType { WARREL, WARRET, DEPREC, DEPREL }

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get { return Messages.HasChanges; }
		}

		bool OtherMessagesCommonValidationError
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Messages cannot be sent to customs without a Customs Registration Number. Please enter a Customs Registration Number on the Company Details form.");
				}
				else if (IsImport)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("This type of message can only be sent for Export Declarations.");
				}
				else if (CMRExportOtherMessageStatusList.IsAwaitingResponse(JE_MessageStatus))
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The system is waiting for responses from customs.");
				}
				else if (!IsExportDeclarationClear)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Export Declaration is not clear.");
				}
				else if (HasMessageStatusBeenChangedSinceLoading())
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("It is not possible to send this message as another person in your company or a batch processor has changed the message status.");
				}
				else
				{
					return false;
				}
				return true;
			}
		}

		public void SendWarrelMessage()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (DepotOrCTOID == ZString.Empty || WarehouseID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Bonded Warehouse and either a CFS or CTO must be specified.");
				}
				else if (!JE_EstimatedDeliveryOrPickup.IsValid)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; Estimated Pickup Date and Time is required, this must be in the future but no more then 24 hours. See Pickup Tab on Declaration Tab.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.WARREL, IsWARRELMessageLodged ? Common.MessageBuilders.MessageSubTypes.Replace : Common.MessageBuilders.MessageSubTypes.Create);
					}
				}
			}
		}

		public void SendWarrelWithdrawl()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (DepotOrCTOID == ZString.Empty || WarehouseID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Bonded Warehouse and either a CFS or CTO must be specified.");
				}
				else if (!JE_EstimatedDeliveryOrPickup.IsValid)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; Estimated Pickup Date and Time is required, this must be in the future but no more then 24 hours. See Pickup Tab on Declaration Tab.");
				}
				else if (!IsWARRELMessageLodged)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; There is no message to withdraw.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.WARREL, Common.MessageBuilders.MessageSubTypes.Withdraw);
					}
				}
			}
		}

		public void SendWarretMessage()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (WarehouseID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Bonded Warehouse must be specified.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.WARRET, IsWARRETMessageLodged ? Common.MessageBuilders.MessageSubTypes.Replace : Common.MessageBuilders.MessageSubTypes.Create);
					}
				}
			}
		}

		public void SendDeprecMessage()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (DepotID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Deport (CFS) must be specified.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.DEPREC, IsDEPRECMessageLodged ? Common.MessageBuilders.MessageSubTypes.Replace : Common.MessageBuilders.MessageSubTypes.Create);
					}
				}
			}
		}

		public void SendDeprecWithdrawl()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (DepotID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Deport (CFS) must be specified.");
				}
				else if (!IsDEPRECMessageLodged)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; There is no message to withdraw.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.DEPREC, Common.MessageBuilders.MessageSubTypes.Withdraw);
					}
				}
			}
		}

		public void SendDeprelMessage()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (DepotID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Deport (CFS) must be specified.");
				}
				else if (CTOID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The CTO must be specified.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.DEPREL, IsDEPRELMessageLodged ? Common.MessageBuilders.MessageSubTypes.Replace : Common.MessageBuilders.MessageSubTypes.Create);
					}
				}
			}
		}

		public void SendDeprelWithdrawl()
		{
			if (!OtherMessagesCommonValidationError)
			{
				if (DepotID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The Deport (CFS) must be specified.");
				}
				else if (CTOID == ZString.Empty)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; The CTO must be specified.");
				}
				else if (!IsDEPRELMessageLodged)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Cannot send message; There is no message to withdraw.");
				}
				else
				{
					LoadChildEditableObjects();
					RunPreSaveValidation();
					var continueWithSend = true;
					if (HasErrors)
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
						continueWithSend = false;
					}
					else if (HasMessageErrors)
					{
						if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
						{
							continueWithSend = false;
						}
					}
					if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
					{
						SendOtherCustomsMessage(ExportOtherMessageType.DEPREL, Common.MessageBuilders.MessageSubTypes.Withdraw);
					}
				}
			}
		}

		void SendOtherCustomsMessage(ExportOtherMessageType otherMessageType, Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			try
			{
				CMRMessageBuilder otherExportMessageBuilder = null;
				if (otherMessageType == ExportOtherMessageType.WARREL)
				{
					otherExportMessageBuilder = !ActiveEntryHeaders.IsNullOrEmpty() ? new WARRELMessageBuilder(EntryHeader) : new WARRELMessageBuilder(this);
				}
				else if (otherMessageType == ExportOtherMessageType.WARRET)
				{
					otherExportMessageBuilder = !ActiveEntryHeaders.IsNullOrEmpty() ? new WARRETMessageBuilder(EntryHeader) : new WARRETMessageBuilder(this);
				}
				else if (otherMessageType == ExportOtherMessageType.DEPREC)
				{
					otherExportMessageBuilder = new DEPRECMessageBuilder(this);
				}
				else if (otherMessageType == ExportOtherMessageType.DEPREL)
				{
					otherExportMessageBuilder = new DEPRELMessageBuilder(this);
				}
				else
				{
					throw new ArgumentException("Invalid Other Export Message Type: " + otherMessageType.ToString());
				}
				otherExportMessageBuilder.MessageSubType = messageSubType;
				otherExportMessageBuilder.PopulateMessages();
				try
				{
					Factory.Save();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(e);
				}
				Messages.Load(MessageFilter);
				if (messageSubType == Common.MessageBuilders.MessageSubTypes.Withdraw)
				{
					MessageInitiator.NotifyUserOfASuccessfulSend(otherMessageType.ToString() + " withdrawal message sent.");
				}
				else if (messageSubType == Common.MessageBuilders.MessageSubTypes.Create)
				{
					MessageInitiator.NotifyUserOfASuccessfulSend("Original " + otherMessageType.ToString() + " message sent.");
				}
				else if (messageSubType == Common.MessageBuilders.MessageSubTypes.Replace)
				{
					MessageInitiator.NotifyUserOfASuccessfulSend("Replace " + otherMessageType.ToString() + " message sent.");
				}
				else
				{
					throw new ArgumentException("Invalid Message Sub Type: " + messageSubType.ToString());
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);
			}
		}

		public
#if DEBUG
 virtual
#endif
 bool IsWARRELMessageLodged
		{
			get
			{
				if (fLastWarrelReply == null)
				{
					fLastWarrelReply = Messages.GetLastClearReceivedMessage(CMRMessage.CMRMessageTypes.WARREL);
				}
				return fLastWarrelReply != null && fLastWarrelReply.EM_MessageSubType != EDIMessage.Status.Withdrawn;
			}
		}
		EDIMessage fLastWarrelReply;

#if DEBUG
		public void ResetfLastWarrelReplyForTesting()
		{
			fLastWarrelReply = null;
		}
#endif

		public
#if DEBUG
 virtual
#endif
 bool IsWARRETMessageLodged
		{
			get
			{
				if (fLastWarretReply == null)
				{
					fLastWarretReply = Messages.GetLastClearReceivedMessage(CMRMessage.CMRMessageTypes.WARRET);
				}
				return fLastWarretReply != null && fLastWarretReply.EM_MessageSubType != EDIMessage.Status.Withdrawn;
			}
		}
		EDIMessage fLastWarretReply;

#if DEBUG
		public void ResetfLastWarretReplyForTesting()
		{
			fLastWarretReply = null;
		}
#endif

		public
#if DEBUG
 virtual
#endif
 bool IsDEPRECMessageLodged
		{
			get
			{
				if (fLastDeprecReply == null)
				{
					fLastDeprecReply = Messages.GetLastClearReceivedMessage(CMRMessage.CMRMessageTypes.DEPREC);
				}
				return fLastDeprecReply != null && fLastDeprecReply.EM_MessageSubType != EDIMessage.Status.Withdrawn;
			}
		}
		EDIMessage fLastDeprecReply;

#if DEBUG
		public void ResetfLastDeprecReplyForTesting()
		{
			fLastDeprecReply = null;
		}
#endif

		public
#if DEBUG
 virtual
#endif
 bool IsDEPRELMessageLodged
		{
			get
			{
				if (fLastDeprelReply == null)
				{
					fLastDeprelReply = Messages.GetLastClearReceivedMessage(CMRMessage.CMRMessageTypes.DEPREL);
				}
				return fLastDeprelReply != null && fLastDeprelReply.EM_MessageSubType != EDIMessage.Status.Withdrawn;
			}
		}
		EDIMessage fLastDeprelReply;

#if DEBUG
		public void ResetfLastDeprelReplyForTesting()
		{
			fLastDeprelReply = null;
		}
#endif

		#endregion

		#region Declaration

		public void SendExportDeclaration()
		{
			if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("Messages cannot be sent to customs without a Customs Registration Number. Please enter a Customs Registration Number on the Company Details form.");
			}
			else if (IsImport)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("Only export declarations can be sent this way.");
			}
			else if (EntryType == "ECN")
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("Exit1 is no longer available. It is not possible to send any further messages for this declaration.");
			}
			else if (HasStatusBeenChangedSinceLoading())
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("It is not possible to send this Declaration as another person in your company or a batch processor has changed the status of the declaration.");
			}
			else if (IsWaitingForExportResponse)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("No messages can be sent as the system is waiting for responses from customs.");
			}
			else if (IsWithdrawn)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("The declaration has been withdrawn. No messages can be sent.");
			}
			else if (!IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("Please ensure that there is at least one Invoice Header and that each Invoice Header has at least one Invoice Line.");
			}
			else
			{
				if (ApportionmentDirty)
				{
					ResumeApportionment();
				}

				LoadChildEditableObjects();
				RunPreSaveValidation();
				var continueWithSend = true;
				if (HasErrors)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix errors before sending a message to customs.");
					continueWithSend = false;
				}
				else if (HasMessageErrors)
				{
					if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
					{
						continueWithSend = false;
					}
					else
					{
						SentWithMessageErrors = true;
					}
				}
				else
				{
					SentWithMessageErrors = false;
				}

				if (continueWithSend && ShouldWeContinueToSendTestDeclarationMessage)
				{
					var decType = GetExportDeclarationType();
					if (decType != ExportDeclarationType.Undefined)
					{
						LogCustomsCommencedIfNeeded();
						SendDeclarationMessage(decType);
					}
				}
			}
		}

		protected bool ShouldWeContinueToSendExportDeclarationDespiteMessageErrors
		{
			get
			{
				return MessageInitiator.ContinueWithAction("The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", "Continue with send?");
			}
		}

		protected bool ShouldWeContinueToSendTestDeclarationMessage
		{
			get
			{
				return !Env.Registry.CMRTestMode || MessageInitiator.ContinueWithAction(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText, "Continue with send?");
			}
		}

		protected bool CheckDeniedParty()
		{
			return MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(this);
		}

		bool CheckDeniedPartyIfRequired(ExportDeclarationType decType)
		{
			var result = decType != ExportDeclarationType.Original && decType != ExportDeclarationType.Replacement;
			result |= CheckDeniedParty();
			return result;
		}

		bool CheckServiceTaskIsHealthy()
		{
			var serviceError = CMRMessageManager.CheckMessageSendingServiceTaskIsRunning();
			if (!serviceError.IsEmpty)
			{
				return MessageInitiator.ContinueWithAction(serviceError, "Service Task Not Running");
			}
			return true;
		}

		internal ExportDeclarationType GetExportDeclarationType()
		{
			var decType = ExportDeclarationType.Undefined;

			if (JE_MessageSubType == JobDeclaration.MessageSubType.Confirming)
			{
				var messageType = MessageInitiator.GetExit1MessageType();

				if (messageType != null)
				{
					if (messageType.ZX_IsConfirmed)
					{
						decType = ExportDeclarationType.Confirmed;
					}
					else
					{
						decType = ExportDeclarationType.Confirming;
					}
				}
			}
			else
			{
				if (DeclarationNumber.IsEmpty)
				{
					decType = ExportDeclarationType.Original;
				}
				else
				{
					decType = ExportDeclarationType.Replacement;
				}
			}

			return decType;
		}

		protected ExportDeclarationType GetExportDeclarationTypeForNextMessage()
		{
			var result = ExportDeclarationType.Undefined;

			if (IsExport)
			{
				if (IsWaitingForExportResponse || IsExportDeclarationSent)
				{
					if (JE_MessageSubType == MessageSubType.Confirming)
					{
						var messageType = MessageInitiator.GetExit1MessageType();
						if (messageType == null)
						{
							result = ExportDeclarationType.Undefined;
						}
						else if (messageType.ZX_IsConfirmed)
						{
							result = ExportDeclarationType.Confirmed;
						}
						else
						{
							result = ExportDeclarationType.Confirming;
						}
					}
					else
					{
						result = ExportDeclarationType.Replacement;
					}
				}
			}

			return result;
		}

		internal bool IsStyleDifferent
		{
			get { return !JE_MessageSubType.Equals(JE_MessageSubTypeInfo.OriginalValue); }
		}

		#region SaveWithAmendment /For import jobs, courses of actions and decision making logics are moved to and initiated by base Customs GUI
		protected virtual internal bool CanContinueWithSaveSendingExportAmendmentIfNeeded()
		{
			var result = true;
			if (ShouldWeCompareDeclarations)
			{
				if (IsWaitingForExportResponse)
				{
					if (DoChangesResultInADifferentMessage(ExportDeclarationType.Undefined))
					{
						MessageInitiator.NotifyUserOfAnInvalidOperation("You can't save the declaration because you are awaiting a response from Customs and changes you have made effect the message currently in progress.");
						result = false;
					}
				}
				else if (IsExportDeclarationSent)
				{
					if (DoChangesResultInADifferentMessage(ExportDeclarationType.Undefined))
					{
						if (!MessageInitiator.ContinueWithAction("Declaration changes have been made. There are system detected changes, which may result in different message content. This may require an amendment to be sent to Customs. If you are sure the changes made do NOT result in different message content, then select 'YES' and the system will let you save the changes. If you need to send an amendment message, please select 'NO'.", "Continue With Save?"))
						{
							if (!MessageInitiator.ContinueWithAction("This declaration has already been sent to Customs. An amendment message will result from saving this form. Do you wish to continue?", "Send Amendment"))
							{
								result = false;
							}
							else
							{
								LoadChildEditableObjects();
								RunPreSaveValidation();
								if (HasErrors)
								{
									MessageInitiator.NotifyUserOfAnInvalidOperation("Please fix the Errors before sending message to customs");
									result = false;
								}
								else if (HasMessageErrors)
								{
									if (!ShouldWeContinueToSendExportDeclarationDespiteMessageErrors)
									{
										result = false;
									}
									else
									{
										SentWithMessageErrors = true;
									}
								}
								else
								{
									SentWithMessageErrors = false;
								}

								result &= ShouldWeContinueToSendTestDeclarationMessage;
								result &= CheckServiceTaskIsHealthy();
								result &= CheckDeniedParty();

								if (result)
								{
									var decType = GetExportDeclarationTypeForNextMessage();
									if (decType == ExportDeclarationType.Undefined)
									{
										result = false;
									}
									else
									{
										SendDeclarationAmendment(decType);
										MessageInitiator.NotifyUserOfASuccessfulSend("Amendment message sent.");
										result = true;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		#region CanContinueWithSaveSendingImportAmendmentIfNeeded

		internal bool IsCurrentStatusAnAmendableOne
		{
			get
			{
				return
					JE_EntryStatus == CMRImportEntryAdvice.Held.Code
					|| JE_EntryStatus == CMRImportEntryAdvice.Clear.Code
					|| JE_EntryStatus == CMRImportEntryAdvice.Finalised.Code
					|| JE_EntryStatus == CMRImportEntryAdvice.MultiStatus.Code
					|| JE_EntryStatus == CMRImportEntryAdvice.Rejected.Code
					|| JE_EntryStatus == CMRImportEntryAdvice.ATDReceived.Code
					|| JE_MessageStatus == CustomsEntryStatus.FailAmendment.Code
					|| JE_MessageStatus == CustomsEntryStatus.FailWithdrawal.Code;
			}
		}

		public CPQAManager CPQAManager
		{
			get
			{
				if (fCPQAManager == null)
				{
					fCPQAManager = new CPQAManager(this);
				}
				return fCPQAManager;
			}
		}
		CPQAManager fCPQAManager;

		#endregion

		#endregion

		public
#if DEBUG
 virtual
#endif
 bool CanContinueWithSaveSendingAnAmendmentIfNeeded()
		{
			var result = true;
			if (IsExport && !IsQuarantine)
			{
				result = CanContinueWithSaveSendingExportAmendmentIfNeeded();
			}
			return result;
		}

		public bool HaveAllEntriesBeenLodged
		{
			get
			{
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					if ((IsImportCMR && !entryHeader.IsStatusPostLodge) ||
						(!IsImportCMR && !EntryHeaderStatus.IsPostLodgeStatus(entryHeader.CH_Status)))
					{
						return false;
					}
				}
				return true;
			}
		}
		public bool HaveAllEntriesHadTheirGoodsReleased
		{
			get { return CustomsEntryHeaders.Count > 0 && HaveAllEntryHeadersBeenAssignedThisStatus(EntryHeaderStatus.GoodsReleased.Code); }
		}

		protected bool HaveAllEntryHeadersBeenAssignedThisStatus(params string[] statusList)
		{
			var result = true;
			CustomsEntryHeaders.Load();
			foreach (var entryHeader in CustomsEntryHeaders)
			{
				var wasOfCorrectStatus = false;
				foreach (var status in statusList)
				{
					if (entryHeader.CH_Status == status)
					{
						wasOfCorrectStatus = true;
						break;
					}
				}
				if (!wasOfCorrectStatus)
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public bool IsWaitingForExportResponse
		{
			get
			{
				return JE_EntryStatus == CustomsEntryStatus.AwaitingReplacement.Code ||
					JE_EntryStatus == CustomsEntryStatus.AwaitingWithdrawal.Code ||
					JE_EntryStatus == CustomsEntryStatus.AwaitingOriginal.Code;
			}
		}

		public bool IsExportDeclarationSent
		{
			get
			{
				return (IsExport && (JE_EntryStatus == CustomsEntryStatus.AwaitingReplacement.Code ||
					JE_EntryStatus == CustomsEntryStatus.AwaitingWithdrawal.Code ||
					JE_EntryStatus == CustomsEntryStatus.AwaitingOriginal.Code ||
					JE_EntryStatus == CustomsEntryStatus.ClearReplacement.Code ||
					JE_EntryStatus == CustomsEntryStatus.ClearOriginal.Code ||
					JE_EntryStatus == CustomsEntryStatus.ErrorReplacement.Code ||
					JE_EntryStatus == CustomsEntryStatus.ErrorOriginal.Code ||
					JE_EntryStatus == CustomsEntryStatus.FailReplacement.Code ||
					JE_EntryStatus == CustomsEntryStatus.FailWithdrawal.Code ||
					JE_EntryStatus == CustomsEntryStatus.Embargoed.Code ||
					JE_EntryStatus == CustomsEntryStatus.ReleasedFromEmbargo.Code));
			}
		}

		public bool IsExportDeclarationClear
		{
			get
			{
				return (IsExport && (JE_EntryStatus == CustomsEntryStatus.ClearReplacement.Code ||
					JE_EntryStatus == CustomsEntryStatus.ClearOriginal.Code ||
					JE_EntryStatus == CustomsEntryStatus.ReleasedFromEmbargo.Code));
			}
		}

		public ZBool PendingManualAmendmentResponse
		{
			get
			{
				var result = false;
				if (IsQuarantine)
				{
					var quarantineExDocHeader = QuarantineInvoice?.QuarantineExDocHeader;
					if (quarantineExDocHeader != null)
					{
						var lastMessageSent = quarantineExDocHeader.Messages?.LastOutgoingMessage;
						if (lastMessageSent != null && lastMessageSent.EM_Status != EDIMessage.Status.Failed)
						{
							result = quarantineExDocHeader.AddInfo.ZH_AmendmentResponseStatus == RFPMessage.Status.AwaitingResponse;
						}
					}
				}

				return result;
			}
		}

		protected StringCollection AllWarningsOfDeclarationAndInvoices()
		{
			var result = new StringCollection();

			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				var warningCount = info.GetWarnings().Count();
				for (var i = 0; i < warningCount; i++)
				{
					result.Add(info.GetWarnings().GetFirstMessage());
				}
			}

			foreach (var invHeader in JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Cast<JobComInvoiceHeader>())
			{
				foreach (ZPropertyInfo info in invHeader.ZPropertyInfoHash)
				{
					foreach (var warning in info.GetWarnings())
					{
						result.Add(warning.Message);
					}
				}
			}
			return result;
		}

		public StringCollection GetExchangeRateRelatedWarnings()
		{
			var result = new StringCollection();
			foreach (var warning in AllWarningsOfDeclarationAndInvoices())
			{
				if (warning.IndexOf("exchange rate") != -1)
				{
					result.Add(warning);
				}
			}
			return result;
		}

		public bool HasStatusBeenChangedSinceLoading()
		{
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(PK);
			return declaration != null && declaration.JE_EntryStatus != (ZString)JE_EntryStatusInfo.OriginalValue;
		}

		bool HasMessageStatusBeenChangedSinceLoading()
		{
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(PK);
			return declaration != null && declaration.JE_MessageStatus != (ZString)JE_MessageStatusInfo.OriginalValue;
		}

		#region Send Declaration Entry

		public void WithdrawDeclaration()
		{
			if (IsWaitingForExportResponse)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("You can't send a message to Customs because you are waiting for a reply.");
			}
			else if (EntryType == "ECN")
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("You can't send any further messages for this declaration as Exit1 no longer exists.");
			}
			else if (JE_EntryStatus == CustomsEntryStatus.ErrorWithdrawal.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearWithdrawal.Code)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("You can't send a message to Customs because you have previously withdrawn this declaration.");
			}
			else if (JE_EntryStatus == CustomsEntryStatus.NotSent.Code ||
				JE_EntryStatus.IsEmpty || //TODO: Change this back when the Not Sent code is 'NOT'
				JE_EntryStatus == CustomsEntryStatus.FailOriginal.Code)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("You can't withdraw this declaration because it is has not been declared successfully yet.");
			}
			else if (JE_EntryStatus == CustomsEntryStatus.ClearReplacement.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearOriginal.Code ||
				JE_EntryStatus == CustomsEntryStatus.ErrorReplacement.Code ||
				JE_EntryStatus == CustomsEntryStatus.ErrorOriginal.Code ||
				JE_EntryStatus == CustomsEntryStatus.FailReplacement.Code ||
				JE_EntryStatus == CustomsEntryStatus.FailWithdrawal.Code ||
				JE_EntryStatus == CustomsEntryStatus.Embargoed.Code ||
				JE_EntryStatus == CustomsEntryStatus.ReleasedFromEmbargo.Code)
			{
				if (MessageInitiator.ContinueWithAction("Once you withdraw a declaration you wont be able to re-declare it. Are you sure you wish to continue?", "Withdraw Declaration?"))
				{
					SendDeclarationMessage(ExportDeclarationType.Withdrawal);
				}
			}
			else if (JE_EntryStatus == CustomsEntryStatus.ClearCPDec.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearCreate.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearLodge.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearLodgeWithImpediments.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearPay.Code ||
				JE_EntryStatus == CustomsEntryStatus.ClearPayWithImpediments.Code ||
				JE_EntryStatus == CustomsEntryStatus.FailCPDec.Code ||
				JE_EntryStatus == CustomsEntryStatus.FailCreate.Code ||
				JE_EntryStatus == CustomsEntryStatus.FailLodge.Code ||
				JE_EntryStatus == CustomsEntryStatus.FailPay.Code ||
				JE_EntryStatus == CustomsEntryStatus.LodgeImpediment.Code ||
				JE_EntryStatus == CustomsEntryStatus.RedLine.Code ||
				JE_EntryStatus == CustomsEntryStatus.AmberLine.Code ||
				JE_EntryStatus == CustomsEntryStatus.CommunityProtectionCheck.Code ||
				JE_EntryStatus == CustomsEntryStatus.SelectedForCargoExamination.Code ||
				JE_EntryStatus == CustomsEntryStatus.DeclarationWorkComplete.Code ||
				JE_EntryStatus == CustomsEntryStatus.HoldAwaiting.Code)
			{
				MessageInitiator.NotifyUserOfAnInvalidOperation("Withdrawing import message not implemented.");
			}
			else
			{
				throw new ApplicationException("Unhandled state.");
			}
		}

		public void SendDeclarationMessage(ExportDeclarationType decType)
		{
			if (CheckServiceTaskIsHealthy() && CheckDeniedPartyIfRequired(decType))
			{
				try
				{
					var additionalDetail = decType == ExportDeclarationType.Withdrawal ? "withdrawal " : "";
					switch (SendDeclarationOriginal(decType))
					{
						case SendResult.Failure:
							MessageInitiator.NotifyUserOfAnInvalidOperation($"Declaration {additionalDetail}message not sent. Please correct any errors and try again.");
							break;
						case SendResult.Error:
							MessageInitiator.NotifyUserOfAnInvalidOperation($"A system error has occured, declaration {additionalDetail}message not sent. Please reload the form and try again.");
							break;
						default:
							MessageInitiator.NotifyUserOfASuccessfulSend($"Declaration {additionalDetail}message sent.");
							break;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowDeveloperException(ex);
				}
			}
		}

		public bool IsWithdrawn
		{
			get
			{
				return JE_EntryStatus == CustomsEntryStatus.ErrorWithdrawal.Code ||
					JE_EntryStatus == CustomsEntryStatus.ClearWithdrawal.Code;
			}
		}

		public enum SendResult
		{
			Success,
			Failure,
			Error
		}

		internal SendResult SendDeclarationOriginal(ExportDeclarationType decType)
		{
			var sendDeclarationSuccess = SendResult.Failure;
			var myBuilder = GetMessageBuilder(this, decType);
			var buildresult = myBuilder.PopulateMessages();
			if (buildresult.IsSuccess)
			{
				try
				{
					PopulateEntrySubmittedDate(ZDateTime.Now);
					Factory.Save(); // This save should not be here.
					sendDeclarationSuccess = SendResult.Success;
				}
				catch (ZSaveException e)
				{
					foreach (var result in buildresult.GetBuilderResults())
					{
						result.Message.Delete();
					}
					ZExceptionReporting.HandleSaveException(e);
					sendDeclarationSuccess = SendResult.Error;
				}
			}
			Messages.Load(MessageFilter);
			return sendDeclarationSuccess;
		}

		public bool UseExit1
		{
			get { return !UseEXD; }
		}

		public bool UseEXD
		{
			get
			{
				var entryNum = ExportEntryNumber;
				return entryNum == null || entryNum.CE_EntryType != CusEntryNumberTypes.Australia.ECN;
			}
		}

		public void SendDeclarationAmendment(ExportDeclarationType decType)
		{
			if (IsWaitingForExportResponse)
			{
				throw new InvalidOperationException("You can't send an amendment message because you are still waiting on a response from Customs.");
			}

			if (!IsExportDeclarationSent)
			{
				throw new InvalidOperationException("You can't send an amendment message because you have nothing at Customs to update.");
			}

			var myBuilder = GetMessageBuilder(this, decType);
			myBuilder.PopulateMessages();
		}

		#endregion

		ZString[] GetMessageStrings(IMessageBuilderResult messageBuilderResult)
		{
			var result = new List<ZString>();
			foreach (var builderResult in messageBuilderResult.GetBuilderResults())
			{
				var message = builderResult.Message;
				result.Add(message.EM_MessageText);
				message.Delete();
			}
			return result.ToArray();
		}

		//Generate messages using the old and new data
		public bool DoChangesResultInADifferentMessage(object decType)
		{
			if (IsImport)
			{
				ErrorReporter.ReportOnce("DoChangesResultInADifferentMessage", "Do not use this method for import job");
				return false;
			}
			else
			{
				var result = false;
				if (EntryType != "ECN")
				{
					var databaseFactory = new BusinessObjectFactory();
					var databaseJobDeclaration = (JobDeclaration)databaseFactory.Load(GetType(), PK);
					var databaseBuilder = databaseJobDeclaration.GetMessageBuilder(databaseJobDeclaration, decType);
					var inDatabaseMessageResults = databaseBuilder.PopulateMessages();
					var inDatabaseMessageStrings = GetMessageStrings(inDatabaseMessageResults);

					var inMemoryBuilder = GetMessageBuilder(this, decType);
					var inMemoryMessageResults = inMemoryBuilder.PopulateMessages();
					var inMemoryMessageStrings = GetMessageStrings(inMemoryMessageResults);

					if (inDatabaseMessageStrings.Length != inMemoryMessageStrings.Length)
					{
						result = true;
					}
					else
					{
						for (var i = 0; i < inDatabaseMessageStrings.Length; i++)
						{
							if (inDatabaseMessageStrings[i] != inMemoryMessageStrings[i])
							{
								result = true;
								break;
							}
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region PackingGroups Collection

		[ChildEditableTestExclude]
		public new PackingGroupCollection PackingGroups
		{
			get { return (PackingGroupCollection)base.PackingGroups; }
		}

		protected override BaseDeclarationLevelPackingGroupCollection CreateNewPackingGroups()
		{
			return new PackingGroupCollection(this);
		}

		protected override bool IsPackingGroupCollectionRegisteredEditable
		{
			get { return IsPackingInformationRelevant; }
		}

		#endregion

		#region CMR Question Collection

		public CachedAnsweredQuestions CachedQuestions
		{
			get { return fCachedQuestions ?? (fCachedQuestions = new CachedAnsweredQuestions()); }
		}
		CachedAnsweredQuestions fCachedQuestions;

		#endregion

		#region Merging Radio Buttons Bound Properties

		public ZBool JE_IsMergeByNone
		{
			get { return JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMerge; }
		}

		public ZBool JE_IsMergeByPartNumber
		{
			get { return JE_MergeBy == OrgConstants.MergeInvoiceLines.PartNumber; }
		}

		public ZBool JE_IsMergeByLookupCode
		{
			get { return JE_MergeBy == OrgConstants.MergeInvoiceLines.Classification; }
		}

		public ZBool JE_IsMergeByTariff
		{
			get { return (JE_MergeBy == OrgConstants.MergeInvoiceLines.Tariff); }
		}

		#endregion

		#region Overridden Properties

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (QuarantineCOLSHeader != null)
				{
					result.Add(QuarantineCOLSHeader);
				}

				return result.ToArray();
			}
		}

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		internal static bool IsReciprocalRatesConstant
		{
			get { return false; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		internal static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.Australia; }
		}

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);
		}

		public override ZString ImporterCity
		{
			get
			{
				ZString result;
				if (IsExport && Importer != null && Importer.IsMiscellaneous)
				{
					result = ZA_ConsigneeCityHidden;
				}
				else
				{
					result = base.ImporterCity;
				}
				return result;
			}
		}

		protected override ZBool CloneBills
		{
			get { return !IsImportCMR; }
		}

		protected override ZString? GetImporterEquipment()
		{
			if (JE_TransportMode == Core.Constants.TransportModes.Other || JE_TransportMode == Core.Constants.TransportModes.Mail)
			{
				return ZString.Empty;
			}
			else
			{
				return base.GetImporterEquipment();
			}
		}

		protected override bool GetIsWHSUniversalXMLActive()
		{
			return !SubmitWeeklyNilReturnN30 && CustomsDataRegistry.IsWHSUniversalXMLActive(JE_SystemCreateTimeUtc);
		}

		protected override bool SupportsBondedWarehousingCore
		{
			get { return true; }
		}

		protected override bool SupportJE_PaymentMethodUsageCore
		{
			get { return true; }
		}

		public override ZString GetDefaultContainerisedContainerMode()
		{
			var result = ZString.Empty;
			if (IsSea)
			{
				result = (IsImportCMR) ? Core.Constants.ContainerModes.FCL : Core.Constants.ContainerModes.Containerised;
			}
			else if (IsAir && IsQuarantine)
			{
				result = Core.Constants.ContainerModes.AIR;
			}
			return result;
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				var normalEntries = 0;
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					if (entryHeader.IsNormalEntry)
					{
						normalEntries++;
					}
				}
				return normalEntries > 1;
			}
		}

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		protected override bool IsIntegrationWithAccountingSupported
		{
			get { return true; }
		}

		public override bool CannotUpdatePart
		{
			get { return HasLodgeBeenSent; }
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsAir)
			{
				JE_VesselName = ZString.Empty;
				JE_LloydsIMO = ZString.Empty;
				JE_ContainerCount = 0;
				JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.AIR;
			}

			if (Env.Registry.AUCustoms.AgentsReferenceDefaulting == Core.Constants.AgentsReferenceDefaulting.PAR && JE_AgentsReference.IsEmpty && !JE_DeclarationReference.IsEmpty)
			{
				JE_AgentsReference = JE_DeclarationReference;
			}

			if (IsImportCMR)
			{
				JE_ContainerCount = (ZShort)CusContainers.Count;
			}

			if (IsExWarehouse)
			{
				JE_RL_NKOriginInfo.PushValueIntoRow();
				JE_RL_NKPortOfArrivalInfo.PushValueIntoRow();
				JE_RL_NKPortOfFirstArrivalInfo.PushValueIntoRow();
				JE_RL_NKPortOfLoadingInfo.PushValueIntoRow();
				JE_DateAtFinalDestinationInfo.PushValueIntoRow();
				JE_DateAtOriginInfo.PushValueIntoRow();
				JE_DateOfArrivalInfo.PushValueIntoRow();
				JE_DateOfFirstArrivalInfo.PushValueIntoRow();
				JE_ExportDateInfo.PushValueIntoRow();
			}
			else if (IsWHSUniversalXMLActive && JE_MessageTypeWasChanged)
			{
				ClearZA_OA_WarehouseAddressOnInvoiceLines();
			}

			var quarantineInvoice = QuarantineInvoice;
			if (quarantineInvoice != null)
			{
				if (!IsQuarantine)
				{
					quarantineInvoice.ResetQuarantineProperties();
					QuarantineExdocLineLines.DeleteAll();
					ResetQuarantineInvoiceCache();
				}
				else
				{
					quarantineInvoice.CleanUnnecessaryQuarantineValuesOnSaving();
				}
			}

			UpdateInvoiceInsuranceGroupCharges();
		}

		void UpdateInvoiceInsuranceGroupCharges()
		{
			foreach (var groupHeader in JobComInvoiceGroupHeaders)
			{
				foreach (var groupCharge in groupHeader.Charges)
				{
					if (groupCharge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance && groupCharge.J7_IsCalculated)
					{
						groupCharge.ApplyApplicableInsuranceRate();
					}
				}
			}
		}

		void ClearZA_OA_WarehouseAddressOnInvoiceLines()
		{
			foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
			{
				line.AddInfo.ClearZA_OA_WarehouseAddress();
			}
		}

		bool JE_MessageTypeWasChanged
		{
			get
			{
				return !IsInDatabase || !JE_MessageTypeInfo.OriginalValue.Equals(JE_MessageType);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				WasSaved = saveSucceeded;
				fMergeManager = null;
				AddInfo.OnSaved(saveSucceeded);
			}
		}

		public override void RecoverFromUnsuccessfulSave()
		{
			base.RecoverFromUnsuccessfulSave();
			if (!IsInDatabase)
			{
				JE_DeclarationReference = ZString.Empty;
			}
			WasSaved = false;
			fMergeManager = null;
			AddInfo.OnSaved(false);
		}

		public bool HasCreateBeenSent
		{
			get
			{
				return HasCPDecBeenSent || (IsImport && (JE_EntryStatus == CustomsEntryStatus.AwaitingCreate.Code || JE_EntryStatus == CustomsEntryStatus.ClearCreate.Code || JE_EntryStatus == CustomsEntryStatus.FailCPDec.Code));
			}
		}
		public bool HasCPDecBeenSent
		{
			get
			{
				return HasLodgeBeenSent || (IsImport && (JE_EntryStatus == CustomsEntryStatus.AwaitingCPDec.Code || JE_EntryStatus == CustomsEntryStatus.ClearCPDec.Code || JE_EntryStatus == CustomsEntryStatus.FailLodge.Code));
			}
		}

		public bool HasLodgeBeenSent
		{
			get
			{
				return HasPayBeenSent || (IsImport && (JE_EntryStatus == CustomsEntryStatus.AwaitingLodge.Code || JE_EntryStatus == CustomsEntryStatus.ClearLodge.Code || JE_EntryStatus == CustomsEntryStatus.ClearLodgeWithImpediments.Code || JE_EntryStatus == CustomsEntryStatus.FailPay.Code || JE_EntryStatus == CustomsEntryStatus.LodgeImpediment.Code || JE_EntryStatus == CustomsEntryStatus.RedLine.Code || JE_EntryStatus == CustomsEntryStatus.AmberLine.Code || JE_EntryStatus == CustomsEntryStatus.CommunityProtectionCheck.Code || JE_EntryStatus == CustomsEntryStatus.SelectedForCargoExamination.Code || JE_EntryStatus == CustomsEntryStatus.PaymentStopped.Code));
			}
		}

		public bool HasPayBeenSent
		{
			get
			{
				return (IsImport && (JE_EntryStatus == CustomsEntryStatus.AwaitingPay.Code || JE_EntryStatus == CustomsEntryStatus.ClearPay.Code || JE_EntryStatus == CustomsEntryStatus.ClearPayWithImpediments.Code || JE_EntryStatus == CustomsEntryStatus.CargoNotCleared.Code || JE_EntryStatus == CustomsEntryStatus.CargoCleared.Code));
			}
		}

		public void WarnUserLodgeHasBeenDoneIfNeeded()
		{
			if (HasLodgeBeenSent && MessageInitiator != null)
			{
				MessageInitiator.WarnUserAboutSomething("This declaration has already been lodged.  Changes you make will not be able to be saved if they contradict information already sent to Customs.", "Declaration Already Lodged");
			}
		}

		protected bool readOnlyNeedsRecalculation = true;

		public override bool ReadOnly
		{
			get
			{
				if (readOnlyNeedsRecalculation)
				{
					ReadOnly = IsHolding || IsDeclarationWorkFinished || IsQueuedEntryLodgement || IsQueuedEntryPayment;
					if (JE_MessageStatus == CustomsEntryStatus.HoldAwaiting.Code || JE_EntryStatus == CustomsEntryStatus.HoldAwaiting.Code)
					{
						NotesOfDeclarationOrShipment.GetAllNotes().SetReadOnlyIncludingChildren(false);
					}
				}
				return base.ReadOnly;
			}
			set
			{
				readOnlyNeedsRecalculation = false;
				base.ReadOnly = value;
			}
		}

		public bool WasSaved;

		[BusinessObjectTestExclude()]
		public override ZString JE_AddInfo
		{
			get { return !IsDeleted ? base.JE_AddInfo : ZString.Empty; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
				if (JE_AddInfo != value)
				{
					base.JE_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(value);
					}
				}
			}
		}

		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set
			{
				base.JE_EntryStatus = value;
				if (IsHolding || IsDeclarationWorkFinished)
				{
					SetReadOnlyIncludingChildren(true);
					readOnlyNeedsRecalculation = true;
					RefreshBindingAndChildrenReadOnly();
				}
				else
				{
					SetReadOnlyIncludingChildren(false);
					ReadOnly = false;
				}
			}
		}

		public override ZString JE_EntryStatusDescription
		{
			get
			{
				var result = base.JE_EntryStatusDescription;
				if (JE_AdditionalStatusInformation.Length > 0)
				{
					string additionalStatusInfo = JE_AdditionalStatusInformation.Replace("\t", "").Replace("\r", "").Replace("\n", "");
					result = result.IsEmpty ? additionalStatusInfo : result + ": " + additionalStatusInfo;
				}

				return result;
			}
		}

		public override ZString CustomsClearanceStatus => JE_ConsolidatedCargoStatus;

		public override ZString JE_ConsolidatedCargoStatus
		{
			get { return base.JE_ConsolidatedCargoStatus; }
			set
			{
				base.JE_ConsolidatedCargoStatus = value;
				LogCSHForHVLVStandAloneDeclaration();
			}
		}

		public override ZString ConsolidatedCargoStatusDescription => Factory.GetValue(ref consolidatedCargoStatusDescriptionCached, delegate
		{
			var result = ZString.Empty;
			if (!JE_ConsolidatedCargoStatus.IsEmpty)
			{
				if (CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(JE_ConsolidatedCargoStatus) && PackingGroups.Count > 0)
				{
					result = PackingGroups[0].AbbreviatedCargoStatusDescription;
				}
				else
				{
					result = CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(JE_ConsolidatedCargoStatus);
				}
			}
			return result;
		});

		CachedProperty<ZString> consolidatedCargoStatusDescriptionCached;

		public override void OnAppliedToConsolidatedDeclaration()
		{
			base.OnAppliedToConsolidatedDeclaration();
			EntryHeader?.Questions?.RemoveAndDeleteAll();
		}

		protected override Customs.Business.JobDeclarationConsolidatedEntryProvider GetConsolidatedEntryProvider() => new JobDeclarationConsolidatedEntryProvider(this, new ConsolidatedEntryDeclarationRemover());

		public bool IsAggregateDeclaration => Factory is ReadOnlyBusinessObjectFactory;

		public bool isCargoStatusAvailableAndCargoNotClear
		{
			get { return !JE_ConsolidatedCargoStatus.IsEmpty && !CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(JE_ConsolidatedCargoStatus); }
		}

		public bool isCargoStatusAvailableAndCargoClear
		{
			get { return !JE_ConsolidatedCargoStatus.IsEmpty && CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(JE_ConsolidatedCargoStatus); }
		}

		protected override bool JE_MessageType_ReadOnlyCore
		{
			get
			{
				var result = base.JE_MessageType_ReadOnlyCore;
				if (!result)
				{
					var areThereAnyActiveMessages =
						!(JE_EntryStatus == CustomsEntryStatus.ClearWithdrawal.Code ||
						JE_EntryStatus == CustomsEntryStatus.FailOriginal.Code ||
						JE_EntryStatus == CustomsEntryStatus.FailCreate.Code ||
						JE_EntryStatus == CustomsEntryStatus.NotSent.Code ||
						JE_EntryStatus == ZString.Empty);
					result = areThereAnyActiveMessages || (IsQuarantine && !(QuarantineInvoice?.QuarantineExDocHeader?.QH_RequestForPermitNumber.IsEmpty ?? true));
				}
				return result;
			}
		}

		public void ResetDeclaration()
		{
			JE_EntryStatus = ZString.Empty; //TODO: Change this back when the Not Sent code is 'NOT' CustomsEntryStatus.NotSent.Code;
			var entryNum = ExportEntryNumber;
			if (entryNum != null)
			{
				entryNum.Delete();
			}

			JE_EntrySubmittedDate = ZDateTime.Empty;

			DefaultJE_ApplicationCode();

			if (IsExport)
			{
				JE_MessageStatus = ZString.Empty;
				var declarationMessages = Messages.ToArray<EDIMessage>();
				foreach (var message in declarationMessages)
				{
					message.EM_Status = EDIMessage.Status.Discarded;
				}
			}

			if (IsQuarantine)
			{
				var exDocHeader = QuarantineInvoice?.QuarantineExDocHeader;
				if (exDocHeader != null)
				{
					exDocHeader.AddInfo.ZH_AmendmentResponseStatus = ZString.Empty;
				}
			}
		}

		void ClearPartShipConsignmentRefNumberIfRequired()
		{
			if (!IsPartShipConsignmentReferenceRelevant)
			{
				foreach (var bill in Bills.Cast<Bill>())
				{
					bill.CU_fPartShipConsignmentReference = ZString.Empty;
				}
			}
		}

		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				var hasChanges = base.JE_ApplicationCode != value;
				base.JE_ApplicationCode = value;
				if (hasChanges && !IsCopying)
				{
					RefreshIncotermAndChargeFactory();
					Packages.MarkAsNeedingValidation();
					MarkAllChargesAsNeedingValidation();

					if (IsImportEdifice)
					{
						JE_ForcePrimeEnclosure = true;
					}
				}
			}
		}

		protected override void DefaultJE_ApplicationCode()
		{
			if (!IsImportEdifice)
			{
				base.DefaultJE_ApplicationCode();
			}
		}

		protected override string SubmissionTypeBuiltinCode => IsImport ? ApplicationCodeList.Codes.AUCMR : string.Empty;

		protected override bool ShowSubmitMenuItemCore() => IsInterface;

		protected override bool IsDeclarationIntegratedCore() => IsInterface;

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				if (JE_MessageType != value)
				{
					var oldValue = JE_MessageType;
					var oldIsQuarantine = IsQuarantine;
					base.JE_MessageType = value;

					if (!IsCopying)
					{
						InvalidateIsNature10Cache();
						DefaultJE_ApplicationCode();

						if (IsExport)
						{
							if (JE_MessageSubType != MessageSubType.NonConfirming)
							{
								JE_MessageSubType = MessageSubType.NonConfirming;
							}

							JE_RL_NKPortOfFirstArrival = ZString.Empty;
							if (oldValue == JobMessageTypeList.Codes.ExportDeclarationByExternalBroker)
							{
								DeclarationNumber = ZString.Empty;
								ManualClearanceDate = ZDateTime.Empty;
							}
						}
						else
						{
							JE_MessageSubType = MessageSubType.FormalEntry;
							if (IsImport)
							{
								if (oldValue == JobMessageTypeList.Codes.ImportDeclarationByExternalBroker)
								{
									ClearAllEntryNumbers();
									ManualClearanceDate = ZDateTime.Empty;
								}

								if (!IsImportCMR)
								{
									JE_ForcePrimeEnclosure = true;
								}

								if (IsExWarehouse)
								{
									JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
									foreach (var header1 in Invoices.Cast<JobComInvoiceHeader>())
									{
										header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
									}
								}
							}
							else if (IsDrawback)
							{
								JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
								JE_TotalNoOfPacksPackType = ZString.Empty;
							}
						}

						customsAuthorityNumber = null;

						ClearPartShipConsignmentRefNumberIfRequired();

						if (!IsValidationSuspended)
						{
							ValidateInvoiceLinesClassifications();
						}

						DeclarationNumberInfo.RefreshBinding();
						DefaultContainerModeIfNeeded();
						if (IsExport && !IsEXPDeclaration && IsMergeDone)
						{
							ThrowAwayMerge();
						}

						RefreshIncotermAndChargeFactory();
						MarkAllChargesAsNeedingValidation();
						Packages.MarkAsNeedingValidation();
						UpdateJE_ToOrder();

						if (IsQuarantine)
						{
							QuarantineInvoice?.CreateQuarantineHeaderIfRequired();
						}

						var shouldResetInvoiceLineLookupsCached = oldIsQuarantine != IsQuarantine;
						foreach (var header in Invoices.Cast<JobComInvoiceHeader>())
						{
							header.JobComInvoiceLines.MarkQuarantineLineAsNeedingValidation();
							header.ClearInvoiceLineLineNumberGeneratorCache();
							if (shouldResetInvoiceLineLookupsCached)
							{
								ResetInvoiceLineLookupsCached(header);
							}
						}
					}
				}
			}
		}

		void ResetInvoiceLineLookupsCached(JobComInvoiceHeader header)
		{
			foreach (var invoiceLine in header.JobComInvoiceLines.Cast<JobComInvoiceLine>())
			{
				invoiceLine.ResetIsLookupsCached();
			}
		}

		public override ZString JE_MessageSubType
		{
			get { return base.JE_MessageSubType; }
			set
			{
				var hasChanges = base.JE_MessageSubType != value;
				base.JE_MessageSubType = value;
				InvalidateIsNature10Cache();

				if (hasChanges && !IsCopying)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				if (value != JE_TransportMode)
				{
					base.JE_TransportMode = value;
					DefaultContainerModeIfNeeded();
					if (!IsCopying && IsPost && UseEXD && JE_ExportGoodsType != ExportGoodsType.Postal)
					{
						JE_ExportGoodsType = ExportGoodsType.Postal;
					}

					Packages.MarkAsNeedingValidation();
					if (IsImporterDeliveryAddressInitialised || IsInDatabase)
					{
						ImporterDeliveryAddress.MarkAsNeedingValidation();
					}

					ClearPartShipConsignmentRefNumberIfRequired();
					DefaultPiecesToPackingGroupOuterPacksIfRequired();
				}
			}
		}

		public override ZString JE_ContainerMode
		{
			get { return base.JE_ContainerMode; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_ContainerMode))
				{
					base.JE_ContainerMode = value;
					if (IsImporterDeliveryAddressInitialised || IsInDatabase)
					{
						ImporterDeliveryAddress.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool JE_ContainerMode_ReadOnly
		{
			get { return ReadOnly || IsAir; }
		}

		public override ZString JE_ExportGoodsType
		{
			get { return base.JE_ExportGoodsType; }
			set
			{
				base.JE_ExportGoodsType = value;
				if (!IsCopying && !IsPost && UseEXD && JE_ExportGoodsType == ExportGoodsType.Postal)
				{
					JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
				}
			}
		}

		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				var hasChanged = JE_OH_Importer != value;
				base.JE_OH_Importer = value;
				if (hasChanged)
				{
					DefaultValuationBasisIfPossible();
					DefaultServiceLevelFromImporter();
					CommodityCodeDefaulter.AttemptDefaultFromImporter(Importer, InvoiceLines);
					UpdateJE_ToOrder();
				}
			}
		}

		void DefaultValuationBasisIfPossible()
		{
			foreach (var header in Invoices.Cast<JobComInvoiceHeader>())
			{
				header.DefaultValuationBasisIfPossible();
			}
		}

		public override ZString JE_RL_NKOrigin
		{
			get { return IsExWarehouse ? ZString.Empty : base.JE_RL_NKOrigin; }
			set { base.JE_RL_NKOrigin = value; }
		}

		public override ZString JE_RL_NKPortOfArrival
		{
			get { return IsExWarehouse ? ZString.Empty : base.JE_RL_NKPortOfArrival; }
			set { base.JE_RL_NKPortOfArrival = value; }
		}

		public override ZString JE_RL_NKPortOfFirstArrival
		{
			get { return IsExWarehouse ? ZString.Empty : base.JE_RL_NKPortOfFirstArrival; }
			set { base.JE_RL_NKPortOfFirstArrival = value; }
		}

		public override ZString JE_RL_NKPortOfLoading
		{
			get { return IsExWarehouse ? ZString.Empty : base.JE_RL_NKPortOfLoading; }
			set { base.JE_RL_NKPortOfLoading = value; }
		}

		public override ZDateTime JE_DateAtFinalDestination
		{
			get { return IsExWarehouse ? ZDateTime.Empty : base.JE_DateAtFinalDestination; }
			set { base.JE_DateAtFinalDestination = value; }
		}

		public override ZDateTime JE_DateAtOrigin
		{
			get { return IsExWarehouse ? ZDateTime.Empty : base.JE_DateAtOrigin; }
			set { base.JE_DateAtOrigin = value; }
		}

		public override ZDateTime JE_DateOfArrival
		{
			get { return IsExWarehouse ? ZDateTime.Empty : base.JE_DateOfArrival; }
			set { base.JE_DateOfArrival = value; }
		}

		public override ZDateTime JE_DateOfFirstArrival
		{
			get { return IsExWarehouse ? ZDateTime.Empty : base.JE_DateOfFirstArrival; }
			set
			{
				var hasChanges = base.JE_DateOfFirstArrival != value;
				base.JE_DateOfFirstArrival = value;
				if (hasChanges)
				{
					MarkAsNeedingValidationForMajorDataChange();
					NotifyEffectiveDutyDateDirty();
				}
			}
		}

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				var hasChanged = base.JE_GB != value;
				base.JE_GB = value;
				if (hasChanged)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				var hasChanged = base.JE_GC != value;
				base.JE_GC = value;
				if (hasChanged)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination && IsQuarantine)
				{
					QuarantineExdocLineLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override void MarkAsNeedingValidationForMajorDataChangeCore()
		{
			base.MarkAsNeedingValidationForMajorDataChangeCore();
			AddInfo.MarkAsNeedingValidationIncludingChildren();
		}

		[ReadOnlyMember(nameof(IsDrawback))]
		public override ZString JE_MergeBy
		{
			get { return base.JE_MergeBy; }
			set { base.JE_MergeBy = value; }
		}

		protected override ZString GetMergeCustomsInvoiceLinesBy(OrgMiscServ miscServ) => IsEXPDeclaration ? miscServ.OM_EXMergeCustomsInvoiceLinesBy : base.GetMergeCustomsInvoiceLinesBy(miscServ);

		public void MarkAllGroupChargesAsNeedingValidation()
		{
			foreach (var groupHeader in AllGroupHeaders.Cast<JobComInvoiceGroupHeader>())
			{
				groupHeader.Charges.MarkAsNeedingValidation();
			}
		}

		public void MarkAllChargesAsNeedingValidation()
		{
			MarkAllGroupChargesAsNeedingValidation();

			foreach (var invoice in Invoices.Cast<JobComInvoiceHeader>())
			{
				invoice.MarkAllChargesIncludingInvoiceLinesOnesAsNeedingValidation();
			}
		}

		protected override JobDeclarationIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider() => new AUDSBJobDeclarationIAccIntegrationDataProvider(this, false);

		public bool IsQuarantineChargeRatingSeparated
		{
			get
			{
				if (!fIsQuarantineChargeRatingSeparated.HasValue)
				{
					var aspChargeCodeRegistryItem = RatingDataRegistry.Instance.CustomsQuarantineChargeCode.GetFallBackValueAtAllLevels(CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
					fIsQuarantineChargeRatingSeparated = aspChargeCodeRegistryItem.ChargeCode.IsValid
						&& (!aspChargeCodeRegistryItem.ActiveTimeUtc.IsValid || JE_SystemCreateTimeUtc > aspChargeCodeRegistryItem.ActiveTimeUtc);
				}
				return fIsQuarantineChargeRatingSeparated.Value;
			}
		}
		bool? fIsQuarantineChargeRatingSeparated;

		#endregion

		#region Calculated Fields

		public bool HasOutstandingAmendment => OutstandingAmendmentLogManger.HasOutstandingAmendments;

		public bool HasOutstandingAmendmentNotQueued => OutstandingAmendmentLogManger.HasOutstandingAmendmentsNotQueued;

		public bool HasOutstandingManualAmendments => OutstandingAmendmentLogManger.HasOutstandingManualAmendments;

		public bool HasOutstandingFailedAmendments => OutstandingAmendmentLogManger.HasOutstandingFailedAmendments;

		public bool HasConsolidatedEntryChanges => OutstandingAmendmentLogManger.HasConsolidatedEntryChanges;

		public OutstandingAmendmentLogManager OutstandingAmendmentLogManger
		{
			get
			{
				return foutstandingAmendmentLogManger ??= new OutstandingAmendmentLogManager(this);
			}
		}
		OutstandingAmendmentLogManager foutstandingAmendmentLogManger;

		public ZString MessageStatusExtraDetails
		{
			get
			{
				var result = new ZStringBuilder();
				result.Append(LastHeldMessageExtraDetails);
				var oustandingAmendmentDetails = OutstandingAmendmentLogManger.AllOutstandingAmendmentDetailsIncludingEventTimes;
				if (!oustandingAmendmentDetails.IsEmpty)
				{
					result.Append("\r\nOutstanding Amendment Details:\r\n");
					result.Append(oustandingAmendmentDetails);
				}

				return result.ToString();
			}
		}

		public ZString CombinedConsolidatedCargoStatusDetails
		{
			get
			{
				if (fCombinedConsolidatedCargoStatusDetails.IsEmpty)
				{
					var result = new ZStringBuilder();
					var includeAdditionalDSAStatusSection = true;
					foreach (var pack in PackingGroups.Cast<PackingGroup>())
					{
						result.Append(pack.LineConsolidatedCargoStatusDescription(ref includeAdditionalDSAStatusSection));
					}

					if (string.IsNullOrEmpty(result.ToString().Trim()))
					{
						result.Append("No Consolidated Cargo Status Information is available.");
					}

					fCombinedConsolidatedCargoStatusDetails = result.ToString();
				}

				return fCombinedConsolidatedCargoStatusDetails;
			}
		}
#if DEBUG
		public
#endif
 ZString fCombinedConsolidatedCargoStatusDetails;

		public ZString JE_AdditionalStatusInformation
		{
			get
			{
				var resultBuilder = new StringBuilder();
				if (IsHolding)
				{
					resultBuilder.Append("\t" + GetHoldReason() + "\r\n");
				}

				if (IsDeclarationWorkFinished)
				{
					resultBuilder.Append("\t" + GetDeclarationWorkCompleteReason() + "\r\n");
				}

				foreach (var entryHeader in CustomsEntryHeaders)
				{
					var impediments = entryHeader.GetImpediments();
					if (!string.IsNullOrEmpty(impediments))
					{
						resultBuilder.Append("Entry " + entryHeader.EntryNumber + " impediment(s):\r\n");
						resultBuilder.Append(impediments);
					}
				}

				return resultBuilder.ToString();
			}
		}

		public ZString LastHeldMessageExtraDetails
		{
			get
			{
				const string NoAddInfo = "No additional information is available.";
				ZString result = "";
				EDIMessage lastMessage = null;
				if (IsExport)
				{
					foreach (var message in Messages.Cast<EDIMessage>())
					{
						if (!message.IsTransmitMessage && message.EM_Status != EDIMessage.Status.Discarded && message.EM_MessageType != CMRMessage.CMRMessageTypes.EXDR && (lastMessage == null || message.EM_SystemCreateTimeUtc > lastMessage.EM_SystemCreateTimeUtc))
						{
							lastMessage = message;
						}
					}
				}
				else
				{
					if (CustomsEntryHeaders.Count == 1)
					{
						foreach (var message in CustomsEntryHeaders[0].Messages.Cast<EDIMessage>())
						{
							if (!message.IsTransmitMessage && (message.EM_MessageType != CMRMessage.CMRMessageTypes.CARST) && (lastMessage == null || message.EM_SystemCreateTimeUtc > lastMessage.EM_SystemCreateTimeUtc))
							{
								lastMessage = message;
							}
						}
					}
				}

				if (lastMessage != null)
				{
					var errorPos = lastMessage.EM_MessageInterpretation.ToUpper().IndexOf("ERRORS");
					var warnPos = lastMessage.EM_MessageInterpretation.ToUpper().IndexOf("WARNINGS");
					if (errorPos != -1 && (errorPos < warnPos || warnPos == -1))
					{
						result = lastMessage.EM_MessageInterpretation.Right(lastMessage.EM_MessageInterpretation.Length - errorPos);
					}
					else if (warnPos != -1 && (warnPos < errorPos || errorPos == -1))
					{
						result = lastMessage.EM_MessageInterpretation.Right(lastMessage.EM_MessageInterpretation.Length - warnPos);
					}
					else
					{
						result = lastMessage.EM_MessageInterpretation;
					}
				}

				if (result.IsEmpty)
				{
					result = CustomsEntryHeaders.Count > 1 ? "Multiple Entries Found - Please see the Entries Tab for more information." : NoAddInfo;
				}

				return result;
			}
		}

		public ZString ImportEntryNumbers
		{
			get
			{
				var result = new StringBuilder();
				if (IsImport)
				{
					foreach (var header in CustomsEntryHeaders)
					{
						result.Append(header.EntryNumber + ",");
					}
				}
				return result.ToString().Trim(',');
			}
		}

		public void ClearAllEntryNumbers()
		{
			foreach (var header in CustomsEntryHeaders)
			{
				header.EntryNumber = ZString.Empty;
			}
		}

		public ZBool NeedsConfirmation
		{
			get
			{
				EDIMessage lastSentMessage = null;
				foreach (var message in Messages.Cast<EDIMessage>())
				{
					if (message.IsTransmitMessage)
					{
						if (lastSentMessage == null)
						{
							lastSentMessage = message;
						}
						else
						{
							if (message.EM_SystemCreateTimeUtc > lastSentMessage.EM_SystemCreateTimeUtc)
							{
								lastSentMessage = message;
							}
						}
					}
				}
				return lastSentMessage != null && lastSentMessage.IsAConfirmingEXDMessage;
			}
		}
		public ZPropertyInfo NeedsConfirmationInfo
		{
			get { return GetZPropertyInfo(nameof(NeedsConfirmation)); }
		}

		public ZString DeclarationReferenceOrHashesIfEmpty
		{
			get { return JE_DeclarationReference.IsEmpty ? new ZString("#########") : JE_DeclarationReference; }
		}

		public ZPropertyInfo DeclarationReferenceOrHashesIfEmptyInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarationReferenceOrHashesIfEmpty)); }
		}

		[ChildEditable(false)]
		public AQISConcernTypeCollection AQISConcernTypes
		{
			get
			{
				if (fAQISConcernTypes == null)
				{
					fAQISConcernTypes = new AQISConcernTypeCollection(Factory, this);
					fAQISConcernTypes.SplitAndAddAQISElements(AddInfo.ZA_AQISConcern_Hidden);
					fAQISConcernTypes.CountChanged += (object sender, CollectionCountChangedEventArgs e) =>
					{ ValidateAllCPDecQuestions(); };
					RegisterEditableChildObject(fAQISConcernTypes);
				}

				return fAQISConcernTypes;
			}
		}
		AQISConcernTypeCollection fAQISConcernTypes;

		public void ValidateAllCPDecQuestions()
		{
			foreach (var entryHeader in CustomsEntryHeaders)
			{
				// foreach (CMRCusEntryCPDec cpDec in entryHeader.AllCPDecQuestions)
				foreach (var cpDec in entryHeader.Questions.Cast<CMRCusEntryCPDec>())
				{
					cpDec.Validation.ValidateON_AnswerCode();
				}
			}
		}

		public ZBool IsConsignmentReferenceActive
		{
			get { return IsAir && IsImport && AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.Value; }
		}

		public bool IsPartShipConsignmentReferenceRelevant
		{
			get { return IsAir && IsImport && !IsExWarehouse; }
		}

		[MaxLength(Schema.JE_PartShipConsignmentReferenceMaxLength)]
		public ZString JE_PartShipConsignmentReference
		{
			get
			{
				var result = ZString.Empty;
				if (PrimaryHouseBill != null)
				{
					result = PrimaryHouseBill.CU_fPartShipConsignmentReference;
				}

				if (result.IsEmpty)
				{
					if (Shipment != null && Shipment.AUCusHAWB != null)
					{
						var hawb = (CusHAWBBase)Shipment.AUCusHAWB;
						if (hawb != null)
						{
							result = hawb.CS_fPartShipConsignmentReference;
						}
					}
				}

				return result;
			}
			set
			{
				var bill = PrimaryHouseBill;
				if (bill == null && !value.IsEmpty)
				{
					bill = Bills.CreatePrimaryBill(BillTypeList.Codes.HouseBill);
				}

				CheckMaximumLength(JE_PartShipConsignmentReferenceInfo, value);
				if (bill != null)
				{
					bill.CU_fPartShipConsignmentReference = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_PartShipConsignmentReference();
				}

				JE_PartShipConsignmentReferenceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_PartShipConsignmentReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.JE_PartShipConsignmentReference); }
		}

		public bool IsDestinedOrTransitingThroughEU => Factory.GetValue(ref cachedIsDestinedOrTransitingThroughEU, () =>
		{
			IEnumerable<ZString> GetCountryCodeOfPorts()
			{
				yield return JE_RL_NKFinalDestination.SubstringSafe(0, 2);

				var transports = Shipment?.TransportsIncludingRelated ?? TransportsIncludingRelated;

				foreach (var transport in transports.Cast<Transport>())
				{
					yield return transport.JW_RL_NKLoadPort.SubstringSafe(0, 2);
					yield return transport.JW_RL_NKDiscPort.SubstringSafe(0, 2);
				}
			}

			return GetCountryCodeOfPorts().Any(c => Factory.IsMemberOfEU(c));
		});

		CachedProperty<bool> cachedIsDestinedOrTransitingThroughEU;

		public CusEntryHeader EntryHeader => ActiveEntryHeaders?.Cast<CusEntryHeader>()?.FirstOrDefault();

		#endregion

		#region Overrides

		public override void UpdateJE_ContainerCount(BaseCusContainer container, bool removed)
		{
			if (!IsExport)
			{
				base.UpdateJE_ContainerCount(container, removed);
			}
		}

		protected override bool JE_ContainerCount_ReadOnly
		{
			get { return !IsExport && base.JE_ContainerCount_ReadOnly; }
		}

		public override bool IsNotificationRequiredIfDeliveryAddressChangedByFreight
		{
			get { return IsImportCMR && !DeclarationNumber.IsEmpty; }
		}

		public override string AdditionalMailTextWhenDeliveryAddressChangedByFreight
		{
			get { return "You should send an amendment message to Customs to reflect this change at Customs."; }
		}

		public override string[] MailRecipentsWhenDeliveryAddressChangedByFreight
		{
			get { return NotifyUserEmails; }
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get { return false; }
		}

		protected override bool IsCustomsLineAmendmentATotalReplacement
		{
			get { return false; }
		}

		protected override bool CurrentUserIsABroker
		{
			get
			{
				return IsImportCMR ? base.CurrentUserIsABroker && !GlbStaff.CurrentUser.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1).IsEmpty : base.CurrentUserIsABroker;
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (fAddInfo != null || fAQISConcernTypes != null)
			{
				//only Declaration has a concern type
				AddInfo.ZA_AQISConcern_Hidden = AQISConcernTypes.ReBuildAQISElements();
				AddInfo.SetAQISFieldsForSave();
			}

			if (!(Globals.IsWebService || Globals.IsWeb) && IsDeclarationWorkFinished)
			{
				readOnlyNeedsRecalculation = true;
				RefreshBindingAndChildrenReadOnly();
			}
		}

		public CollectionCache CollectionCache
		{
			get
			{
				if (fCollectionCache == null)
				{
					fCollectionCache = new CollectionCache(Factory);
				}
				return fCollectionCache;
			}
		}

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory factory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, factory);
		}

		protected override bool DeclarationMessagesHaveBeenSentCore(bool reloadMessages)
		{
			if (IsQuarantine)
			{
				foreach (var invoiceHeader in Invoices.Cast<JobComInvoiceHeader>())
				{
					if (invoiceHeader.QuarantineExDocHeader != null && invoiceHeader.QuarantineExDocHeader.Messages.HasNonDiscardedMessage())
					{
						return true;
					}
				}
			}
			return base.DeclarationMessagesHaveBeenSentCore(reloadMessages);
		}

		protected override ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			if (shipmentPackingMode == Enterprise.Core.Constants.ContainerModes.ShippersConsol && IsImport)
			{
				return Core.Constants.ContainerModes.LCL;
			}
			else
			{
				return base.GetContainerModeForDeclarationCore(transportMode, shipmentPackingMode);
			}
		}

		protected override ZString GetContainerModeForImportDeclaration(ZString shipmentPackingMode)
		{
			var result = ZString.Empty;

			if (shipmentPackingMode == Core.Constants.ContainerModes.BuyersConsol)
			{
				result = Core.Constants.ContainerModes.FCLMixedShipper;
			}
			else if (shipmentPackingMode == Core.Constants.ContainerModes.ShippersConsol)
			{
				result = Core.Constants.ContainerModes.LCL;
			}
			else if (shipmentPackingMode != Core.Constants.ContainerModes.RollOnRollOff)
			{
				result = shipmentPackingMode;
			}

			return result;
		}

		#endregion

		#region NotifyUserEmails

		string[] NotifyUserEmails
		{
			get
			{
				var list = new List<string>();
				if (IsImport)
				{
					foreach (var entryHeader in CustomsEntryHeaders)
					{
						AddEMailAddressFromMessages(entryHeader.Messages, list);
					}
				}
				else
				{
					AddEMailAddressFromMessages(Messages, list);
				}

				ZGuid groupToCopy = Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroupForBranch(RegistryCompanyPK, BranchOfEmailGroupRegistry);
				if (!groupToCopy.IsEmpty)
				{
					var group = Factory.Load<GlbGroup>(groupToCopy);
					if (group != null)
					{
						foreach (var staff in group.Staff.Cast<GlbStaff>())
						{
							var userToNotifyEmail = staff.GS_EmailAddress;
							if (!userToNotifyEmail.IsEmpty && !list.Contains(userToNotifyEmail))
							{
								list.Add(userToNotifyEmail);
							}
						}
					}
				}

				return list.ToArray();
			}
		}

		void AddEMailAddressFromMessages(EDIMessageCollection messages, List<string> list)
		{
			var lastMessage = messages.LastOutgoingMessage;
			if (lastMessage != null)
			{
				var userToNotify = lastMessage.UserWhoQueuedThisRecord;
				if (userToNotify != null)
				{
					var userToNotifyEmail = userToNotify.GS_EmailAddress;
					if (!userToNotifyEmail.IsEmpty && !list.Contains(userToNotifyEmail))
					{
						list.Add(userToNotifyEmail);
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetWarehouseEntries()
		{
			if (EntryHeader != null)
			{
				yield return EntryHeader;
			}
		}

		protected override ZString GetWarehouseTransactionStatus()
		{
			return SingleWarehouseEntry?.CH_WarehouseTransactionStatus ?? ZString.Empty;
		}

		protected override bool ShouldKeepDeletedLinesOnAmendmentCore
		{
			get { return true; }
		}

		protected AUAddInfo fAddInfo;

		protected CollectionCache fCollectionCache;

		public enum ValidationType { EXD, SAC, IMD, Drawback, AQS, Default }

		public ValidationType GetValidationType()
		{
			switch (JE_MessageType)
			{
				case JobMessageTypeList.Codes.Export:
					return ValidationType.EXD;
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.ExWarehouse:
				case JobMessageTypeList.Codes.WarehousedByExternalAgent:
					if (IsImportCMR)
					{
						if (IsSAC)
						{
							return ValidationType.SAC;
						}
						else
						{
							return ValidationType.IMD;
						}
					}
					else
					{
						return ValidationType.Default;
					}

				case JobMessageTypeList.Codes.Drawback:
					return ValidationType.Drawback;
				case JobMessageTypeList.Codes.Quarantine:
					return ValidationType.AQS;
				default:
					return ValidationType.Default;
			}
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			switch (GetValidationType())
			{
				case ValidationType.Drawback:
					return new DrawbackJobDeclarationValidation(this);
				case ValidationType.EXD:
					return new EXDJobDeclarationValidation(this);
				case ValidationType.IMD:
					return new IMDJobDeclarationValidation(this);
				case ValidationType.SAC:
					return new SACJobDeclarationValidation(this);
				case ValidationType.AQS:
					return new QuarantineJobDeclarationValidation(this);
				default:
					return new JobDeclarationValidation(this);
			}
		}

		protected override void SetDefaultValues()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Enterprise.Core.Constants.CountryCodes.Australia && !IsNull && !Factory.IsConstructingNullBusinessObject)
			{
				throw new OdysseyException("Attempted to create Australian Job Declaration in country code " + GlbCompany.CurrentCompany.GC_RN_NKCountryCode
					+ " if an Australian Job Declaration is required use : \r\n\r\n"
					+ "GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia); \r\n"
					+ "\r\n or \r\n"
					+ " Add the following using clause and lines to the Assembly Info \r\n\r\n"
					+ "#if DEBUG\r\n"
					+ "using Enterprise.MasterFiles.Business.Testing;\r\n"
					+ "#endif\r\n\r\n"
					+ " #if DEBUG \r\n"
					+ " [assembly: CountrySpecificTestAttribute(\"AU\")]\r\n"
					+ " #endif\r\n\r\n"
					+ " If the Assembly is not country specific please use BaseJobDeclaration Instead\r\n");
			}
			base.SetDefaultValues();
			JE_ExportGoodsType = "OT";
		}

		protected internal IMessageBuilder GetMessageBuilder(JobDeclaration jobDeclaration, object messageType)
		{
			IMessageBuilder result = null;

			if (IsExport)
			{
				result = !jobDeclaration.ActiveEntryHeaders.IsNullOrEmpty() ? new EXDMessageBuilder(jobDeclaration.EntryHeader, (ExportDeclarationType)messageType) : new EXDMessageBuilder(jobDeclaration, (ExportDeclarationType)messageType);
			}
			else if (IsImportEdifice)
			{
				ErrorReporter.ReportOnce("GetMessageBuilder", "Trying to send a message for an Import Edifice Job.");
			}

			return result;
		}

		CommodityCodeDefaulter CommodityCodeDefaulter
		{
			get
			{
				if (fCommodityCodeDefaulter == null)
				{
					fCommodityCodeDefaulter = new CommodityCodeDefaulter(this);
				}
				return fCommodityCodeDefaulter;
			}
		}
		CommodityCodeDefaulter fCommodityCodeDefaulter;

		public override void Delete()
		{
			ProcessQueueParentHelper.DeleteProcessQueue();
			base.Delete();
		}

		#region Calculated Values

		protected void ValidateInvoiceLinesClassifications()
		{
			foreach (var header in Invoices.Cast<JobComInvoiceHeader>())
			{
				header.JobComInvoiceLines.ValidateInvoiceLinesClassifications();
			}
		}

		#endregion
		#endregion

		#region DeclarationNumber

		public ZString EntryType
		{
			get
			{
				if (UseExit1)
				{
					return CusEntryNumberTypes.Australia.ECN;
				}
				else
				{
					return CANType.CustomsAuthorityNumber.Code;
				}
			}
		}

		#region Contingency CAN

		[MaxLength(14)]
		public ZString ContingencyCAN
		{
			get
			{
				var entryNum = GetCusEntryNumber(CANType.ContingencyCustomsAuthorityNumber.Code);
				return entryNum == null ? ZString.Empty : entryNum.CE_EntryNum;
			}
			set
			{
				if (ContingencyCAN != value)
				{
					CheckMaximumLength(ContingencyCANInfo, value);
					var entryNum = GetCusEntryNumber(CANType.ContingencyCustomsAuthorityNumber.Code);
					if (value.IsEmpty)
					{
						entryNum?.Delete();
					}
					else
					{
						if (entryNum == null)
						{
							CreateCusEntryNumber(CANType.ContingencyCustomsAuthorityNumber.Code, value);
						}
						else
						{
							entryNum.CE_EntryNum = value;
						}
					}

					HasChanges = true;
					ContingencyCANInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ContingencyCANInfo
		{
			get { return GetZPropertyInfo(Schema.ContingencyCAN); }
		}

		protected bool ContingencyCAN_ReadOnly
		{
			get { return !DeclarationNumber.IsEmpty; }
		}

		#endregion

		public ZString GetEntryNumberFromDeclarationReference(ZString declarationReference)
		{
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, declarationReference.TrimEnd(' ')));
			if (declaration != null)
			{
				return declaration.DeclarationNumber;
			}

			return ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString DeclarationNumber
		{
			get
			{
				var result = ZString.Empty;

				if (IsExport && ExportEntryNumber != null)
				{
					result = ExportEntryNumber.CE_EntryNum;
				}
				else if (IsImport)
				{
					result = ImportEntryNumber;
				}
				else if (IsDrawback && DrawbackClaimID != null)
				{
					result = DrawbackClaimID.CE_EntryNum;
				}

				return result;
			}
			set
			{
				if (IsExport)
				{
					UpdateExportNumber(value);
				}
				else if (IsDrawback)
				{
					UpdateDrawbackClaimNumber(value);
				}
				DeclarationNumberInfo.RefreshBinding();
			}
		}

		void UpdateExportNumber(ZString entryNumber)
		{
			var result = ExportEntryNumber;
			if (!entryNumber.IsEmpty)
			{
				if (result == null)
				{
					CreateExportCusEntryNumber(CANType.CustomsAuthorityNumber.Code, entryNumber);
				}
				else
				{
					result.CE_EntryNum = entryNumber;
				}
			}
			else if (result != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.DeletedARecordInTheSystem, "Dec#: " + result.CE_EntryNum);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				result.Delete();
			}
			customsAuthorityNumber = null;
		}

		void UpdateDrawbackClaimNumber(ZString entryNumber)
		{
			var result = DrawbackClaimID;
			if (!entryNumber.IsEmpty)
			{
				if (result == null)
				{
					CreateCusEntryNumber(CusEntryNumberTypes.Standard.DrawbackClaim, entryNumber);
				}
				else
				{
					result.CE_EntryNum = entryNumber;
				}
			}
			else if (result != null)
			{
				result.Delete();
			}
			drawbackClaimID = null;
		}

		protected override bool IsDeclarationNumberReadOnly
		{
			get { return (JE_MessageSubType != MessageSubType.Manual) && !IsExportByExternalBroker; }
		}

		public CusEntryNumber DrawbackClaimID => Factory.GetValue(ref drawbackClaimID, () => GetCusEntryNumber(CusEntryNumberTypes.Standard.DrawbackClaim));
		CachedProperty<CusEntryNumber> drawbackClaimID;

		public CusEntryNumber ExportEntryNumber
		{
			get
			{
				CusEntryNumber entryNum;
				if (IsEXPDeclaration && EntryHeader != null)
				{
					entryNum = EntryHeader.ExportCusEntryNumber;
				}
				else
				{
					entryNum = DeclarationExportCusEntryNumber;
				}

				return entryNum;
			}
		}

		public CusEntryNumber DeclarationExportCusEntryNumber
		{
			get
			{
				var entryNumbers = AUCusEntryNumber.LoadEntryNumber(Factory, PK, JobDeclaration.Schema.TableName, CountryCode, !IsInDatabase);
				return entryNumbers.FirstOrDefault(x => x.CE_EntryType == CusEntryNumberTypes.Australia.ECN || x.CE_EntryType == CANType.CustomsAuthorityNumber.Code);
			}
		}

		AUCusEntryNumber GetCusEntryNumber(ZString entryNumberType)
		{
			var entryNumbers = AUCusEntryNumber.LoadEntryNumber(Factory, PK, JobDeclaration.Schema.TableName, CountryCode, !IsInDatabase);
			return entryNumbers.FirstOrDefault(x => x.CE_EntryType == entryNumberType);
		}

		AUCusEntryNumber CreateExportCusEntryNumber(ZString entryNumberType, ZString entryNumber)
		{
			var result = CreateCusEntryNumber(entryNumberType, entryNumber);
			if (IsEXPDeclaration && EntryHeader != null)
			{
				result.Parent = EntryHeader;
			}
			return result;
		}

		AUCusEntryNumber CreateCusEntryNumber(ZString entryNumberType, ZString entryNumber)
		{
			var result = Factory.New<AUCusEntryNumber>();
			result.CE_EntryIsSystemGenerated = true;
			result.Parent = this;
			result.CE_RN_NKCountryCode = CountryCode;
			result.CE_EntryType = entryNumberType;
			result.CE_EntryNum = entryNumber;
			return result;
		}

		public bool IsECNNumberTransferred => IsQuarantine && JE_EntryStatus == CustomsEntryStatus.Transferred.Code;

		public ZString CustomsAuthorityNumber => Factory.GetValue(ref customsAuthorityNumber, () =>
		{
			var exportEntryNumber = IsExport ? ExportEntryNumber : null;
			return exportEntryNumber != null && exportEntryNumber.CE_EntryType == CANType.CustomsAuthorityNumber.Code ? exportEntryNumber.CE_EntryNum : ZString.Empty;
		});

		CachedProperty<ZString> customsAuthorityNumber;

		protected ZString ImportEntryNumber
		{
			get
			{
				var result = new StringBuilder();
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					result.Append(entryHeader.EntryNumber + ",");
				}
				return result.ToString().Trim(',');
			}
		}

		#endregion

		#region Drwaback members

		public ZString DrawbackHeaderAssesmentMethod
		{
			get { return AddInfo.ZA_DAM_Hidden; }
			set { AddInfo.ZA_DAM_Hidden = value; }
		}

		public ZString DrawbackHeaderAmberReasonCode
		{
			get { return AddInfo.ZA_DARC_Hidden; }
			set { AddInfo.ZA_DARC_Hidden = value; }
		}

		public ZString DrawbackContactPhoneNumber
		{
			get
			{
				var phone = GlbStaff.CurrentUser.GS_WorkPhone;
				if (phone.IsEmpty)
				{
					phone = GlbBranch.CurrentBranch.GB_Phone;
				}

				if (phone.IsEmpty)
				{
					phone = GlbCompany.CurrentCompany.GC_Phone;
				}

				return phone.KeepChars("1234567890");
			}
		}

		public ZString DrawbackContactPhoneNumber_Formatted
		{
			get
			{
				var phone = GlbStaff.CurrentUser.GS_WorkPhone_Formatted;
				if (phone.IsEmpty)
				{
					phone = GlbBranch.CurrentBranch.GB_Phone_Formatted;
				}

				if (phone.IsEmpty)
				{
					phone = GlbCompany.CurrentCompany.GC_Phone_Formatted;
				}

				return phone.KeepChars("1234567890");
			}
		}

		public ZString DrawbackContactEMail
		{
			get
			{
				var eMail = GlbStaff.CurrentUser.GS_EmailAddress;
				if (eMail.IsEmpty)
				{
					eMail = GlbBranch.CurrentBranch.GB_Email;
				}

				if (eMail.IsEmpty)
				{
					eMail = GlbCompany.CurrentCompany.GC_Email;
				}

				return eMail;
			}
		}

		ZDecimal totalDrawbackClaimAmount;
		ZDecimal totalDrawbackMethodAAmount;
		ZDecimal totalDrawbackMethodBAmount;
		ZDecimal totalDrawbackMethodCAmount;

		ZDecimal totalDrawbackClaimQuantity;
		ZDecimal totalDrawbackMethodAQuantity;
		ZDecimal totalDrawbackMethodBQuantity;
		ZDecimal totalDrawbackMethodCQuantity;
		bool drawbackTotalsCalculated;

		void CalculateDrawbackTotals()
		{
			if (!drawbackTotalsCalculated)
			{
				totalDrawbackClaimAmount = 0m;
				totalDrawbackMethodAAmount = 0m;
				totalDrawbackMethodBAmount = 0m;
				totalDrawbackMethodCAmount = 0m;

				totalDrawbackClaimQuantity = 0m;
				totalDrawbackMethodAQuantity = 0m;
				totalDrawbackMethodBQuantity = 0m;
				totalDrawbackMethodCQuantity = 0m;

				foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					totalDrawbackClaimAmount += line.DrawbackDutyAmount;
					totalDrawbackClaimQuantity += line.DrawbackClaimQuantity;
					if (line.DrawbackAssesmentMethod == DrawbackAssessmentMethods.ActualShipment)
					{
						totalDrawbackMethodAAmount += line.DrawbackDutyAmount;
						totalDrawbackMethodAQuantity += line.DrawbackClaimQuantity;
					}
					if (line.DrawbackAssesmentMethod == DrawbackAssessmentMethods.RepresentativeShipment)
					{
						totalDrawbackMethodBAmount += line.DrawbackDutyAmount;
						totalDrawbackMethodBQuantity += line.DrawbackClaimQuantity;
					}
					if (line.DrawbackAssesmentMethod == DrawbackAssessmentMethods.Imputation)
					{
						totalDrawbackMethodCAmount += line.DrawbackDutyAmount;
						totalDrawbackMethodCQuantity += line.DrawbackClaimQuantity;
					}
				}
				drawbackTotalsCalculated = true;
			}
		}

		public ZDecimal TotalDrawbackClaimAmount
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackClaimAmount;
			}
		}

		public ZDecimal TotalDrawbackMethodAAmount
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackMethodAAmount;
			}
		}

		public ZDecimal TotalDrawbackMethodBAmount
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackMethodBAmount;
			}
		}

		public ZDecimal TotalDrawbackMethodCAmount
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackMethodCAmount;
			}
		}

		public ZDecimal TotalDrawbackClaimQuantity
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackClaimQuantity;
			}
		}

		public ZDecimal TotalDrawbackMethodAQuantity
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackMethodAQuantity;
			}
		}

		public ZDecimal TotalDrawbackMethodBQuantity
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackMethodBQuantity;
			}
		}

		public ZDecimal TotalDrawbackMethodCQuantity
		{
			get
			{
				CalculateDrawbackTotals();
				return totalDrawbackMethodCQuantity;
			}
		}

		#endregion

		#region ICPQAAttachee members for Drawback Declaration Questions
		[BusinessObjectTestExclude()]
		public SchemaGuidColumn FKColumnInCusEntryCPDecTable
		{
			get { return null; }
		}

		[BusinessObjectTestExclude()]
		public CMRCusEntryCPDecCollection Questions
		{
			get { return null; }
		}

		public ZDateTime SelectionDate
		{
			get { return ZDateTime.Today; }
		}
		#endregion

		#region Drawback Declarations

		[ChildEditable(true)]
		public CMRDeclarationQuestionsCollection DrawbackQuestions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = new CMRDeclarationQuestionsCollection(this);
					fQuestions.Load();
					RegisterEditableChildObject(fQuestions);
				}
				return fQuestions;
			}
		}
		CMRDeclarationQuestionsCollection fQuestions;

		public void GenerateDrawbackQuestionsIfNecessary()
		{
			AddOneDrawbackQuestion(999);
			AddOneDrawbackQuestion(283);
			AddOneDrawbackQuestion(284);
			AddOneDrawbackQuestion(285);
			AddOneDrawbackQuestion(286);
			AddOneDrawbackQuestion(287);
		}

		void AddOneDrawbackQuestion(ZInt decNum)
		{
			var existingQuestion = DrawbackQuestions.GetQuestionWithID(decNum);
			if (existingQuestion == null)
			{
				existingQuestion = DrawbackQuestions.AddNew();
				existingQuestion.QuestionID = decNum.ToString();
			}
		}

		#endregion

		#region AddInvoiceFromExportDeclaration

		protected override BusinessObject AddInvoiceHeaderFromExportDeclarationCore(BaseJobComInvoiceHeader invoice)
		{
			var aUInvoiceHeader = (JobComInvoiceHeader)invoice;
			var newInvoiceHeader = Invoices.AddNew();
			newInvoiceHeader.JZ_InvoiceNumber = aUInvoiceHeader.JZ_InvoiceNumber;
			newInvoiceHeader.JZ_InvoiceAmount = aUInvoiceHeader.TotalLinePriceOfAllExportDrawbackLines;
			newInvoiceHeader.JZ_RX_NKInvoice_Currency = aUInvoiceHeader.JZ_RX_NKInvoice_Currency;
			newInvoiceHeader.JZ_IncoTerm = aUInvoiceHeader.JZ_IncoTerm;
			newInvoiceHeader.JZ_InvoiceDate = aUInvoiceHeader.JZ_InvoiceDate;
			newInvoiceHeader.AddInfo.ZA_EDN_Hidden = aUInvoiceHeader.JobDeclaration.DeclarationNumber.Left(AUAddInfo.Schema.ZA_EDN_HiddenMaxLength);
			newInvoiceHeader.AddInfo.ZA_DAM_Hidden = AddInfo.ZA_DAM_Hidden;
			return newInvoiceHeader;
		}
		protected override void AddInvoiceLineFromExportDeclarationCore(BaseJobComInvoiceLine invoiceLine, BaseJobComInvoiceHeader newInvoiceHeader)
		{
			var newHeader = (JobComInvoiceHeader)newInvoiceHeader;
			var exportInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (exportInvoiceLine.JI_Drawback)
			{
				var drawbackInvoiceLine = newHeader.JobComInvoiceLines.AddNew();
				drawbackInvoiceLine.JI_PartNo = exportInvoiceLine.JI_PartNo;
				drawbackInvoiceLine.JI_Description = exportInvoiceLine.JI_Description;
				drawbackInvoiceLine.JI_InvoiceUQ = exportInvoiceLine.JI_InvoiceUQ;
				drawbackInvoiceLine.JI_InvoiceQuantity = exportInvoiceLine.JI_InvoiceQuantity;
				drawbackInvoiceLine.AddInfo.ZA_EDN_Hidden = newHeader.AddInfo.ZA_EDN_Hidden;
				if (!drawbackInvoiceLine.JI_CustomsUnitQty.IsEmpty)
				{
					var customsQtyFromExportLine = DrawbackAmountCalculator.CustomsQuantityFromExportInvoiceLineIfPossible(exportInvoiceLine, drawbackInvoiceLine);
					if (!customsQtyFromExportLine.IsEmpty)
					{
						drawbackInvoiceLine.JI_CustomsQuantity = customsQtyFromExportLine;
					}
				}
			}
		}

		#endregion

		#region Manual Clearance Date
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_MCD_Hidden)]
		public ZDateTime ManualClearanceDate
		{
			get { return AddInfo.ZA_MCD_Hidden; }
			set { AddInfo.ZA_MCD_Hidden = value; }
		}

		protected bool ManualClearanceDate_ReadOnly
		{
			get { return !IsDeclarationByExternalBroker; }
		}

		public ZPropertyInfo ManualClearanceDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ManualClearanceDate), x => AddInfo.ZA_MCD_HiddenInfo); }
		}
		#endregion

		#region QueuedEntry

		[ReadOnlyMember(nameof(JE_EDITransmitDate_ReadOnly))]
		public virtual ZDateTime JE_EDITransmitDate
		{
			get => AddInfo.ZA_EDITransmitDate;
			set
			{
				AddInfo.ZA_EDITransmitDate = value;
				JE_EDITransmitDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_EDITransmitDateInfo => GetWrappedZPropertyInfo(nameof(JE_EDITransmitDate), x => AddInfo.ZA_EDITransmitDateInfo);

		public bool JE_EDITransmitDate_ReadOnly => ActiveEntryHeaders.Count > 0 && !ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.IsNextMessageOriginalForCMR);

		public bool IsQueuedEntryLodgementsEnabled => JE_MessageType == JobMessageTypeList.Codes.Import && IsQueuedEntriesFunctionEnabled;

		public bool IsQueuedEntryLodgement => JE_MessageStatus == CustomsEntryStatus.ScheduledLodgeWithPayment.Code || JE_MessageStatus == CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;

		public bool IsQueuedEntryPaymentsEnabled => JE_MessageType == JobMessageTypeList.Codes.Import && IsQueuedEntriesFunctionEnabled;

		public bool IsQueuedEntryPayment => JE_MessageStatus == CustomsEntryStatus.ScheduledPayment.Code;

		public bool IsQueuedEntriesFunctionEnabled => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, priorityToPilotFunctionality: false)
			|| ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.PQENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, priorityToPilotFunctionality: true);

		public void CancelQueuedMessageLogs()
		{
			var dsmLogs = new LogsForNominatedEvent(this.GetLogs(), Events.DeferredScheduledMessage);
			var activeQueuedEntryLogs = dsmLogs.Find(x => !x.IsCancelled && x.SL_EventTime >= ZDateTime.Now && IsQueuedMessageLogReference(x.SL_Reference.Trim())).ToArray();
			foreach (var log in activeQueuedEntryLogs)
			{
				log.Cancel();
			}
		}

		public static bool IsQueuedMessageLogReference(ZString logReference)
		{
			return logReference.EqualsIgnoringCase(nameof(CMRMessageTypes.LodgeWithPay))
				|| logReference.EqualsIgnoringCase(nameof(CMRMessageTypes.LodgeWithoutPay))
				|| logReference.EqualsIgnoringCase(nameof(CMRMessageTypes.Payment));
		}

		public bool DequeueScheduledMessages()
		{
			var result = false;

			if (IsQueuedEntryLodgement || IsQueuedEntryPayment)
			{
				try
				{
					CancelQueuedMessageLogs();

					foreach (var entryHeader in ActiveEntryHeaders.Cast<CusEntryHeader>())
					{
						var entryStatus = entryHeader.CH_Status;
						if (entryStatus == CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code
							|| entryStatus == CustomsEntryStatus.ScheduledLodgeWithPayment.Code
							|| entryStatus == CustomsEntryStatus.ScheduledPayment.Code)
						{
							entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
						}
					}
					JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
					Factory.Save();
					result = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}

			return result;
		}

		#endregion

		#region onApportioned

		protected override void OnApportioned()
		{
			if (IsImport)
			{
				new TILVApportionManager().ApportionTILV(this);
			}

			base.OnApportioned();
			if (IsDrawback)
			{
				foreach (var invoiceHeader in Invoices.Cast<JobComInvoiceHeader>())
				{
					invoiceHeader.OnDrawbackApportioned();
				}
			}
		}
		#endregion

		#region DocumentRequestedWithNoCusEntryHeaders Event
		public event EventHandler DocumentRequestedWithNoCusEntryHeaders;
		public virtual void OnDocumentRequestedWithNoCusEntryHeaders(EventArgs e)
		{
			if (DocumentRequestedWithNoCusEntryHeaders != null)
			{
				DocumentRequestedWithNoCusEntryHeaders(this, e);
			}
		}

		#endregion

		#region IDocumentSupportable Override
		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}
		#endregion

		#region ILandedCostHeader Members

		bool ILandedCostHeader.IsLCSupported
		{
			get { return IsImport && !IsSAC; }
		}

		public const string SACMessageShownWhenLCIsNotSupported = "Customs does not send line level duty/tax for SAC entries and Landed Costing does not support SAC entries due to the reason.\r\nIf you want Landed Costing functionality, you have to lodge this entry as a formal entry.\r\nCustoms will convert the formal entry to SAC entry at their end.";
		string ILandedCostHeader.MessageShownWhenLCIsNotSupported
		{
			get
			{
				var result = "";
				if (!((ILandedCostHeader)this).IsLCSupported)
				{
					if (IsSAC)
					{
						result = SACMessageShownWhenLCIsNotSupported;
					}
					else
					{
						result = BaseJobDeclaration.BaseMessageShownWhenLCIsNotSupported;
					}
				}
				return result;
			}
		}

		DutyTaxEntryFee ILandedCostHeader.TotalDutyTaxEntryFeeItems => Factory.GetValue(ref cachedTotalDutyTaxEntryFeeItems, GetTotalDutyTaxEntryFeeItems);

		CachedProperty<DutyTaxEntryFee> cachedTotalDutyTaxEntryFeeItems;

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee GetTotalDutyTaxEntryFeeItems()
		{
			var result = new DutyTaxEntryFee();
			foreach (var entryHeader in ActiveEntryHeaders.Cast<CusEntryHeader>())
			{
				result[CustomsDisbursementChargeCode.EntryFees] += entryHeader.AllEntryFeesExcludingAQISServiceFee;
				result[CustomsDisbursementChargeCode.QuarantineFees] += entryHeader.AQISServicePaymentAmount;

				result[CustomsDisbursementChargeCode.TotalDuty] += entryHeader.DutyAmountIncludingWHEstimate;
				result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] += entryHeader.FlatDutyPortion + entryHeader.AllOtherDuties;
				result[CustomsDisbursementChargeCode.SpecialTax1] += entryHeader.WETAmountIncludingWHEstimate;
				result[CustomsDisbursementChargeCode.SpecialTax2] += entryHeader.LCTAmountIncludingWHEstimate;
				result[CustomsDisbursementChargeCode.SpecialTax3] += entryHeader.WoodLevyIncludingWHEstimate;
			}
			return result;
		}

		protected override bool IsEntryClearCore
		{
			get
			{
				return IsImportCMR ? CMRImportEntryAdviceList.IsEntryStatusClearOrFinalisedOrATDReceived(JE_EntryStatus) : base.IsEntryClearCore;
			}
		}

		#endregion

		#region IProcessQueueParent Members

		public ActiveProcessQueueCollection ActiveProcessQueueForBinding
		{
			get { return ProcessQueueParentHelper.ActiveProcessQueueForBinding; }
		}

		public ProcessQueue CurrentQueue
		{
			get { return ProcessQueueParentHelper.CurrentQueue; }
		}

		protected virtual Type TypeOfProcessQueue
		{
			get { return typeof(ProcessQueue); }
		}

		ProcessQueueParentHelper ProcessQueueParentHelper
		{
			get
			{
				if (fProcessQueueParentHelper == null)
				{
					fProcessQueueParentHelper = new ProcessQueueParentHelper(this, TypeOfProcessQueue);
				}
				return fProcessQueueParentHelper;
			}
		}

		ProcessQueueParentHelper fProcessQueueParentHelper;

		#endregion

		#region ICPQAAttacheeHolder

		ICPQAHeaderAttachee[] ICPQAAttacheeHolder.Headers
		{
			get
			{
				var result = new ArrayList();
				result.AddRange(CustomsEntryHeaders);
				return (ICPQAHeaderAttachee[])result.ToArray(typeof(ICPQAHeaderAttachee));
			}
		}

		ICPQALineAttachee[] ICPQAAttacheeHolder.Lines
		{
			get
			{
				var result = new ArrayList();
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					result.AddRange(entryHeader.MergedLines);
				}
				return (ICPQALineAttachee[])result.ToArray(typeof(ICPQALineAttachee));
			}
		}

		#endregion

		#region ICMROtherMessageHeader Members

		ZString ICMROtherMessageHeader.CAN
		{
			get { return DeclarationNumber; }
		}

		ZString ICMROtherMessageHeader.DepotEstablishmentID
		{
			get { return DepotDocAddress.Address != null ? DepotDocAddress.Address.LocalControlledPremisesID : ZString.Empty; }
		}

		ZString ICMROtherMessageHeader.DestinationEstablishmentID
		{
			get { return ContainerTerminalOperatorDocAddress.Address != null ? ContainerTerminalOperatorDocAddress.Address.LocalControlledPremisesID : ZString.Empty; }
		}

		#endregion

		#region Autorating
		protected override BaseJobDeclarationInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new AUJobDeclarationInvoicingSupporter(this);
		}

		public class AUJobDeclarationInvoicingSupporter : BaseJobDeclarationInvoicingSupporter
		{
			public AUJobDeclarationInvoicingSupporter(JobDeclaration parent)
				: base(parent)
			{
			}

			public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default)
			{
				var result = base.GetReasonNotToAllowAutoRate();
				if (string.IsNullOrEmpty(result))
				{
					var stringBuilder = ((JobDeclaration)Parent).GetDeferredGSTStatusWarning();
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append("You should correct the Importer organization GST setting,");
						stringBuilder.Append("and then attempt the auto-rating again.");
						result = stringBuilder.ToStringWithNewLineBetweenAppends();
					}
				}
				return result;
			}

			public override string GetWarningForContinueAutoRate()
			{
				var result = base.GetWarningForContinueAutoRate();
				if (string.IsNullOrEmpty(result))
				{
					var stringBuilder = ((JobDeclaration)Parent).GetDeferredDutyStatusWarning();
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append("You should correct the Importer organization duty setting,");
						stringBuilder.Append("and then attempt the auto-rating again.");
						stringBuilder.Append("Would you like to continue?");
						result = stringBuilder.ToStringWithNewLineBetweenAppends();
					}
				}
				return result;
			}
		}

		public bool HasMixedPaymentModesBeenUsed
		{
			get
			{
				var result = false;
				var bankAccount = ZString.Empty;
				foreach (var entryHeader in CustomsEntryHeaders)
				{
					var cMRPAYRECMessages = entryHeader.CMRPAYRECMessages;
					foreach (var payrecMessage in cMRPAYRECMessages)
					{
						if (bankAccount == ZString.Empty)
						{
							bankAccount = payrecMessage.PAYRECInfoProvider.BankAccountNumber;
						}
						else if (bankAccount != payrecMessage.PAYRECInfoProvider.BankAccountNumber)
						{
							result = true;
							break;
						}
					}
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		#region IRatingSupporterWithAdapter

		protected override RatingAdaptersProvider GetRatingAdaptersProviderCore()
		{
			return new JobDeclarationRatingAdaptersProvider(this);
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new JobDeclarationRatingAdapter<JobDeclaration>(this);
		}

		#endregion

		#endregion

		#region Is Company Importer

		public ZBool IsEntryForAnImporter
		{
			get { return !CompanyABN.IsEmpty && ImporterABN == CompanyABN; }
		}

		public ZString CompanyABN
		{
			get { return GlbCompany.CurrentCompany.GC_BusinessRegNo.Replace(" ", "").Trim(); }
		}

		public ZString ImporterABN
		{
			get
			{
				var result = ZString.Empty;

				if (Importer != null)
				{
					var splitter = new ABNCACSplitter(Importer.LocalBusinessRegNo);
					result = splitter.ABN.Replace(" ", "").Trim();
				}

				return result;
			}
		}

		#endregion

		#region JobComInvoiceGroupHeaderSingleElementCollection

		public class JobComInvoiceGroupHeaderSingleElementCollection : BaseJobComInvoiceGroupHeaderSingleElementCollection
		{
			public JobComInvoiceGroupHeaderSingleElementCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new JobComInvoiceGroupHeader this[int index]
			{
				get { return (JobComInvoiceGroupHeader)Elements[index]; }
			}

			public new JobComInvoiceGroupHeader AddNew()
			{
				return (JobComInvoiceGroupHeader)base.AddNew();
			}
		}

		#endregion

		#region AQIS

		#region AQIS Collections

		[ChildEditable(false)]
		public AQISDocumentCollection AQISDocuments
		{
			get
			{
				if (fAQISDocuments == null)
				{
					fAQISDocuments = new AQISDocumentCollection(Factory, AddInfo);
					fAQISDocuments.SplitAndAddAQISElements(AddInfo.ZA_AQISDocuments_Hidden);
					RegisterEditableChildObject(fAQISDocuments);
				}

				return fAQISDocuments;
			}
		}
		AQISDocumentCollection fAQISDocuments;

		[ChildEditable(false)]
		public AQISPremisesIdAndProcessingTypeCollection AQISPremisesIdAndProcessingTypes
		{
			get
			{
				if (fAQISPremisesIdAndProcessingTypes == null)
				{
					fAQISPremisesIdAndProcessingTypes = new AQISPremisesIdAndProcessingTypeCollection(Factory, AddInfo);
					fAQISPremisesIdAndProcessingTypes.SplitAndAddAQISElements(AddInfo.ZA_AQISPremIdProcessType_Hidden);
					RegisterEditableChildObject(fAQISPremisesIdAndProcessingTypes);
				}

				return fAQISPremisesIdAndProcessingTypes;
			}
		}
		AQISPremisesIdAndProcessingTypeCollection fAQISPremisesIdAndProcessingTypes;

		#region AQIS Commodity Codes

		[ChildEditable(true)]
		public AQISCommodityCodeCollection AQISCommodityCodes
		{
			get
			{
				if (fAQISCommodityCodes == null)
				{
					fAQISCommodityCodes = new AQISCommodityCodeCollection(Factory);
					RegisterEditableChildObject(fAQISCommodityCodes);
				}

				return fAQISCommodityCodes;
			}
		}
		AQISCommodityCodeCollection fAQISCommodityCodes;

		#endregion

		#region AQIS Entity Ids

		[ChildEditable(true)]
		public AQISEntityIdCollection AQISEntityIds
		{
			get
			{
				if (fAQISEntityIds == null)
				{
					fAQISEntityIds = new AQISEntityIdCollection(Factory);
					RegisterEditableChildObject(fAQISEntityIds);
				}

				return fAQISEntityIds;
			}
		}
		AQISEntityIdCollection fAQISEntityIds;

		#endregion

		#region AQIS Permit Ids

		[ChildEditable(true)]
		public AQISPermitIdCollection AQISPermitIds
		{
			get
			{
				if (fAQISPermitIds == null)
				{
					fAQISPermitIds = new AQISPermitIdCollection(Factory);
					RegisterEditableChildObject(fAQISPermitIds);
				}

				return fAQISPermitIds;
			}
		}
		AQISPermitIdCollection fAQISPermitIds;

		#endregion

		#region AQIS Producer Code

		[ChildEditable(true)]
		public AQISProducerCodeCollection AQISProducerCodes
		{
			get
			{
				if (fAQISProducerCodes == null)
				{
					fAQISProducerCodes = new AQISProducerCodeCollection(Factory);
					RegisterEditableChildObject(fAQISProducerCodes);
				}

				return fAQISProducerCodes;
			}
		}
		AQISProducerCodeCollection fAQISProducerCodes;

		#endregion

		#endregion

		#region Is AQIS Certificate Request Message

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_IsAQISCertificateRequest_Hidden)]
		public ZBool IsAQISCertificateRequest
		{
			get { return AddInfo.ZA_IsAQISCertificateRequest_Hidden == "Y"; }
			set { AddInfo.ZA_IsAQISCertificateRequest_Hidden = value ? "Y" : ""; }
		}

		public ZPropertyInfo IsAQISCertificateRequestInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsAQISCertificateRequest), x => AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo); }
		}

		#endregion

		#endregion

		#region IMessageAttacheeParent Members

		IMessageAttachee[] IMessageAttacheeParent.MessageAttachees
		{
			get
			{
				var result = Array.Empty<IMessageAttachee>();
				if (IsImportCMR)
				{
					result = (IMessageAttachee[])new ArrayList(CustomsEntryHeaders).ToArray(typeof(IMessageAttachee));
				}
				return result;
			}
		}

		#endregion

		#region Importer DocAddresses

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobDocAddressValidationForCMR(addressToValidate, this);
		}

		#endregion

		#region IDocAddresses Overrides

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				return
					IsQuarantine
						? base.SupportedAddressTypesCore.Concat(new[] { DocAddressType.AQISProcessingEstablishment }).ToArray()
						: base.SupportedAddressTypesCore;
			}
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return
				addressType == DocAddressType.AQISProcessingEstablishment
					? AQISProcessingEstablishmentAddressRequirement
					: base.GetDocAddressRequirement(addressType);
		}

		JobDocAddressRequirement AQISProcessingEstablishmentAddressRequirement
		{
			get
			{
				if (fAQISProcessingEstablishmentAddressRequirement == null)
				{
					fAQISProcessingEstablishmentAddressRequirement = new JobDocAddressRequirement(DocAddressType.AQISProcessingEstablishment)
					{
						DefaultMax = 0,
					};
					DocAddressManager.AddRequirement(fAQISProcessingEstablishmentAddressRequirement);
				}
				return fAQISProcessingEstablishmentAddressRequirement;
			}
		}
		JobDocAddressRequirement fAQISProcessingEstablishmentAddressRequirement;

		#endregion // IDocAddresses Overrides

		#region PackingInformation

		protected override bool ShouldDefaultPackingInfoFromDeclarationToBillsCore
		{
			get
			{
				return CanDefaultPackingInfoFromDeclarationToBills &&
					(IsPackingInformationRelevant || IsPackingGroupRequiredForMessaging);
			}
		}

		public bool IsPackingGroupRequiredForMessaging => IsSAC;

		protected override bool IsPackingInformationRelevantCore
		{
			get { return IsImportCMR && !IsSAC && !IsExWarehouse && !IsTransportModeOther; }
		}

		protected override bool ShouldDefaultTotalPackTypeToBillCore
		{
			get { return false; } //should default JE_TotalNoOfPackPackType to Packing tab? Pack type is not exposed on the form
		}

		#endregion

		#region Quarantine Note Properties

		public ZString EXDOCNotifyText
		{
			get
			{
				var result = ZString.Empty;
				var notifyTextNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.EXDOCNotifyText.Description);
				if (notifyTextNotes.Length > 0)
				{
					result = notifyTextNotes[0].ST_NoteText.Replace("\r\n", "");
				}
				return result;
			}
		}

		public ZString EXDOCLetterOfCredit
		{
			get
			{
				var result = ZString.Empty;
				var letterOfCreditNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description);
				if (letterOfCreditNotes.Length > 0)
				{
					result = letterOfCreditNotes[0].ST_NoteText.Replace("\r\n", "");
				}
				return result;
			}
		}

		public ZString EXDOCAdditionalInformation
		{
			get
			{
				var result = ZString.Empty;
				var additionalInformationNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description);
				if (additionalInformationNotes.Length > 0)
				{
					result = additionalInformationNotes[0].ST_NoteText.Replace("\r\n", "");
				}
				return result;
			}
		}

		public ZString EXDOCAmendmentReason
		{
			get
			{
				var result = ZString.Empty;
				var eXDOCAmendmentReasonNotes = NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description);
				if (eXDOCAmendmentReasonNotes.Length > 0)
				{
					result = eXDOCAmendmentReasonNotes[0].ST_NoteText.Replace("\r\n", "");
				}
				return result;
			}
		}

		public ZString NEXDOCCancellationReason =>
			NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.NEXDOCCancellationReason.Description).FirstOrDefault()?.ST_NoteText.Replace("\r\n", string.Empty)
			?? ZString.Empty;

		#endregion

		#region Quarantine COLS

		public QuarantineColsHeader QuarantineCOLSHeader => Factory.GetValue(ref quarantineColsHeaderCached, delegate
		{
			if (EntryHeader != null && !EntryHeader.EntryNumber.IsEmpty)
			{
				var colsHeaderQuery = new ZQuery(QuarantineColsHeaderSchema.QCH_ClusterKey, JE_ClusterKey);
				colsHeaderQuery.AddToFilter(QuarantineColsHeaderSchema.QCH_CH_CusEntryHeader, EntryHeader.PK);
				var colsHeader = Factory.Load<QuarantineColsHeader>(colsHeaderQuery).FirstOrDefault();
				if (colsHeader != null)
				{
					RegisterEditableChildObject(colsHeader);
					return colsHeader;
				}
			}
			return null;
		});
		CachedProperty<QuarantineColsHeader> quarantineColsHeaderCached;

		public QuarantineColsHeader CreateCOLSHeaderIfRequired()
		{
			var quarantineColsHeader = QuarantineCOLSHeader;
			if (quarantineColsHeader == null)
			{
				if (EntryHeader != null && !EntryHeader.EntryNumber.IsEmpty)
				{
					quarantineColsHeader = Factory.New<QuarantineColsHeader>();
					quarantineColsHeader.QCH_CH_CusEntryHeader = EntryHeader.PK;
					quarantineColsHeader.QCH_ClusterKey = JE_ClusterKey;
				}
			}

			return quarantineColsHeader;
		}

		#endregion

		#region IMessageManageableBizObj Members

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return GetAmendmentWithdrawalReasonCore();
		}

		protected virtual AmendmentWithdrawalReason GetAmendmentWithdrawalReasonCore()
		{
			return new CMRAmendmentWithdrawalReason();
		}

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return GetMessageManagerForAmendmentDetectionCore();
		}

		protected virtual IMessageManager GetMessageManagerForAmendmentDetectionCore()
		{
			return new IMDMultiMessageManager(this, CMRMessageTypes.OriginalForAmendmentDetection);
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			var checkAmendment = !IsImportCMR || !MergeManager.RequiresMerge || DoMerge();
			if (checkAmendment)
			{
				var consolidatedEntryLogs =	EntryHeader?.ConsolidatedDeclaration?.Logs;
				if (consolidatedEntryLogs != null)
				{
					if (consolidatedEntryLogs.MostRecentLogByEventTime(AutoEvents.ConsolidatedEntryChanged) == null)
					{
						consolidatedEntryLogs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
					}

					Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);

					checkAmendment = false; // exit so we don't calculate the amendment messages or show the popup.
				}
			}
			return checkAmendment ? ContinueWithDetection.Yes : ContinueWithDetection.No;
		}

		/// <summary>
		/// Export checks amendment detection in its own way
		/// </summary>
		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return (IsImportCMR) && !JE_MessageStatus.IsEmpty && JE_MessageStatus != CustomsEntryStatus.NotSent.Code && JE_EntryStatus != CMRImportEntryAdvice.Withdrawn.Code; }
		}

		/// <summary>
		/// Export supports in its own way
		/// </summary>
		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected
		{
			get { return IsImportCMR; }
		}

		#endregion

		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new JobDeclarationCustomsCharges(this);
			}

			return base.GetService(serviceType);
		}

		#region IDataExportCSVFileNameProvider

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get
			{
				return JE_DeclarationReference.IsEmpty ? JE_HouseBill : JE_DeclarationReference;
			}
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region ICMRMessageRespondeeReference Members

		ZString ICMRMessageRespondeeReference.GetHTMLFormatDetailsIfNeeded(ZString details, bool isForHtml)
		{
			return isForHtml ? details.Replace(DeclarationReferenceDetail, "Declaration Reference: " + Enterprise.Customs.Business.MessageProcessors.EmailDefBuilder.GetJobLink(this, JE_DeclarationReference) + "\r\n") : details;
		}

		#endregion

		#region Invoicing Supporter

		internal ZStringBuilder GetDeferredGSTStatusWarning()
		{
			var result = new ZStringBuilder();
			if (Importer != null && HaveAllEntriesBeenLodged)
			{
				if (Importer.MiscServ.IsGSTVATDeferred)
				{
					if (HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTAmount))
					{
						result.Append("The consignee tab of the importer organization of this declaration has the GST Deferred box ticked,");
						result.Append("however GST information returned by Customs indicates that GST is not deferred.");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, "Importer GST flag set but Customs GST is not deferred");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
				else
				{
					if (HasAtLeastOneLineFeeOfType(CusEntryChargeTypeList.Codes.GSTDeferred))
					{
						result.Append("The consignee tab of the importer organization of this declaration has the GST Deferred box NOT ticked,");
						result.Append("however GST information returned by Customs indicates that GST is deferred.");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, "Importer GST flag NOT set but Customs GST is deferred");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
			}
			return result;
		}

		internal ZStringBuilder GetDeferredDutyStatusWarning()
		{
			var result = new ZStringBuilder();
			if (Importer != null && HaveAllEntriesBeenLodged)
			{
				var isConsigneeDutyDeferred = Importer.AUIsDutyDeferred;

				if (isConsigneeDutyDeferred && ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.TotalDeferredDutyFromCustoms.IsEmpty))
				{
					result.Append("The consignee tab of the importer organization of this declaration has the Duty Deferred box ticked,");
					result.Append("however duty information returned by Customs indicates that duty is not deferred.");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, "Importer duty flag set but Customs duty is not deferred");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (!isConsigneeDutyDeferred && ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => !x.TotalDeferredDutyFromCustoms.IsEmpty))
				{
					result.Append("The consignee tab of the importer organization of this declaration has the Duty Deferred box NOT ticked,");
					result.Append("however duty information returned by Customs indicates that duty is deferred.");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, "Importer duty flag NOT set but Customs duty is deferred");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			return result;
		}

		#endregion

		#region IApportionInvoiceHolder Members
		string IApportionInvoiceHolder.CountryContext
		{
			get { return IsImport && !IsImportCMR ? AUEdifice : (string)CountryCode; }
		}
		internal const string AUEdifice = "AUEdifice";
		#endregion

		#region Importer Diplomat

		public bool IsImporterDiplomat
		{
			get
			{
				return Importer != null && Importer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.AustraliaCodeTypes.Diplomat, Core.Constants.CountryCodes.Australia) != null;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public IEnumerable<IStorageDocsBaseCollection> EDocsForSelection
		{
			get
			{
				var eDocsStorageForSelection = new List<IStorageDocsBaseCollection>();
				eDocsStorageForSelection.Add(DocManagerInfo.AllEDocs);

				var shipmentDocs = Shipment?.DocManagerInfo.AllEDocs;
				if (shipmentDocs != null)
				{
					eDocsStorageForSelection.Add(shipmentDocs);
				}

				return eDocsStorageForSelection;
			}
		}

		protected override DeclarationDocManagerInfo GetNewDocManagerInfo()
		{
			return new AUDeclarationDocManagerInfo(this);
		}

		internal class AUDeclarationDocManagerInfo : DeclarationDocManagerInfo
		{
			public AUDeclarationDocManagerInfo(JobDeclaration parent)
				: base(parent)
			{
			}

			public override bool ReadOnly
			{
				get { return false; }
			}
		}

		#endregion

		#region IOnUniversalEventAddedHandler

		void IOnUniversalEventAddedHandler.OnUniversalEventAdded(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			CreateQuarantineCertificateNumberIfNeeded(eventAdded);
		}

		void CreateQuarantineCertificateNumberIfNeeded(UniversalEvent eventAdded)
		{
			if (eventAdded.IsNEXDOCSCertificatePrint()
				&& eventAdded.AttachedDocumentCollection != null
				&& QuarantineInvoice?.QuarantineExDocHeader is QuarantineExDocHeader quarantineHeader)
			{
				foreach (var doc in eventAdded.AttachedDocumentCollection)
				{
					if ((string)doc.Type?.Code == Enterprise.Core.Constants.RefDocTypes.QuarantineRemotePrint)
					{
						ZString certificateNumber = Path.GetFileNameWithoutExtension(doc.FileName.GetValueOrDefault());
						if (!certificateNumber.IsEmpty)
						{
							var entryNumber = quarantineHeader.CertificateNumbers.AddNew();
							entryNumber.CE_EntryNum = certificateNumber;

							// <EventTime>2021-07-05T04:26:23.2068568+10:00</EventTime> is retrieved as UTC.  Will be 14:26 when converted to local time.
							entryNumber.CE_IssueDate = eventAdded.EventTime.GetValueOrDefault(ZDateTimeOffset.UtcNow).ToZDateTime();
						}
					}
				}
			}
		}

		public CodeDescriptionPairList QuarantineCertificateNumbersForSelection
		{
			get
			{
				return Factory.GetCachedValue($"QuarantineCertificateNumbers|{JE_DeclarationReference}|{QuarantineInvoice?.JZ_InvoiceNumber ?? ZString.Empty}", () =>
				{
					var certificateNumbers = new CodeDescriptionPairList();

					var certificateEntryNumbers = QuarantineInvoice?.QuarantineExDocHeader.CertificateNumbers;
					if (certificateEntryNumbers != null)
					{
						var certificatesGroupedByNumber = certificateEntryNumbers.GroupBy(c => c.CE_EntryNum).OrderBy(g => g.Key);
						foreach (var certificateGroup in certificatesGroupedByNumber)
						{
							foreach (var certificate in certificateGroup.OrderBy(c => c.CE_IssueDate))
							{
								certificateNumbers.AddPair(certificate.CE_EntryNum, certificate.CE_IssueDate.ToLocalBranchTime().ToString());
							}
						}
					}

					return certificateNumbers;
				});
			}
		}

		#endregion

		bool Integration.Customs.AU.IJobDeclaration.DeclarationHasEntryHeader => CustomsEntryHeaders.Count > 0;

		#region IEDocDeliveryAuthorization

		bool IEDocDeliveryAuthorization.IsDocumentViewEnabled(IeDocBase doc)
		{
			var result = true;

			if (doc.DocType == Core.Constants.RefDocTypes.QuarantineRemotePrint)
			{
				result = !Enterprise.ZArchitecture.Environment.DataRegistry.Instance.AUCustoms.NEXDOCSDisableQRPView;
			}

			return result;
		}

		bool IEDocDeliveryAuthorization.IsDocumentDeliveryDisclaimerRequired(IeDocBase doc)
		{
			return doc.DocType == Core.Constants.RefDocTypes.QuarantineRemotePrint;
		}

		string IEDocDeliveryAuthorization.DeliveryDisclaimerMessage
		{
			get { return Res.GetString("506624AB-36F8-4942-BAFA-C7E4F45C8AD7", @"To be able to print export documents at my own site, I agree to the following:
• The print site is located in Australia
• I have a software package to communicate with NEXDOC that has been accredited by the Department
• I have a duplex laser printer which supports at least 600 dpi
• I will only print one set of export documents for each consignment
• I will not alter, vary or in any way change an export document generated in NEXDOC once it has been printed
• I understand that there are a number of criminal offenses under the Criminal Code Act 1995 (Cth) relating to the making and use of a false Commonwealth document. 
• The Department reserves the right to at any time suspend, cancel or amend the terms of this print agreement. Inability to demonstrate valid and legitimate use of the printed export document(s) may result in the Department revoking the option to print NEXDOC generated document(s) through the remote print function."); }
		}

		#endregion
	}
}
