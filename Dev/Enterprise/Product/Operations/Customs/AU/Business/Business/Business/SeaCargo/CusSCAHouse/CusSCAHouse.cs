using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[UserDefinedValues]
	[CodeProperty(CusSCAHouse.Schema.CA_HouseBill), DescriptionProperty(CusSCAHouse.Schema.CA_HouseBill)]
	public class CusSCAHouse :
		BaseCusSCAHouse,
		ISynchroniserDeletableBusinessObject,
		ISeaCargoShipmentInfo,
		Integration.Customs.AU.ICusSCAHouse,
		IWorkflowProvider,
		IWorkflowProviderCore,
		IWorkflowTriggerEventSource,
		ICustomFieldProvider,
		ICMRMessageRespondee,
		IAUCusUnderbondUnionCollectionParent,
		IStatusNeedsRecalculationProvider,
		IMessageManageableBizObj,
		IScanHouseBillProvider,
		IDocManagerSupport,
		IDocManagerSupportIncudingRelatedObjects
	{
		#region Schema

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new abstract class Schema : BaseCusSCAHouse.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string ShipmentStatus = "ShipmentStatus";
			public const string CA_HouseBillReadOnly = "CA_HouseBillReadOnly";
			public const string MessageStatus = "MessageStatus";
		}

		#endregion

		public CusSCAHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CA_OA_UnderbondFrom_ZAddress.DefaultAddressType = AddressType.DLV;
			CA_OA_UnderbondTo_ZAddress.DefaultAddressType = AddressType.DLV;
			MessageStatusCalculator = new CusSCAHouseMessageStatusCalculator(this);
		}

		public static CusSCAHouse New(BusinessObjectFactory factory)
		{
			return factory.New<CusSCAHouse>();
		}

		public CusSCAHouseMessageStatusCalculator MessageStatusCalculator;

		public static CusSCAHouse Load(BusinessObjectFactory factory, ZString bGMReference)
		{
			return (CusSCAHouse)new Loader(factory).LoadFromBGMReferenceAndApplicationCode(bGMReference, CusSCAOceanBill.ApplicationCodes);
		}

		public static CusSCAHouse LoadFromShipment(ForwardingShipment shipment)
		{
			return (CusSCAHouse)new Loader(shipment.Factory).LoadFromShipmentAndApplicationCode(shipment.PK, CusSCAOceanBill.ApplicationCodes);
		}

		public static CusSCAHouse Load(BusinessObjectFactory factory, ICusSCAHouseInfoProvider info)
		{
			ZString lloydsNumber = info.LloydsNumber;
			if (!lloydsNumber.IsEmpty)
			{
				ZString voyageNumber = info.VoyageNumber;
				if (!voyageNumber.IsEmpty)
				{
					ZString oceanBillNumber = info.OceanBillNumber;
					if (!oceanBillNumber.IsEmpty)
					{
						ZString houseBillNumber = info.HouseBillNumber;
						if (!houseBillNumber.IsEmpty)
						{
							var vessel = factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
							if (vessel != null)
							{
								ZQuery oceanBillFilter = new ZQuery();
								oceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_VesselName, vessel.RV_Code);
								oceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, oceanBillNumber);
								oceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
								foreach (CusSCAOceanBill oceanBill in factory.Load(typeof(CusSCAOceanBill), oceanBillFilter))
								{
									if (oceanBill.CB_Voyage.TrimStart(' ', '0').ToUpper() == voyageNumber.TrimStart(' ', '0').ToUpper())
									{
										foreach (CusSCAHouse house in oceanBill.HouseBills)
										{
											if (house.CA_HouseBill == houseBillNumber)
											{
												return house;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return null;
		}

		public static new readonly TypeDecider TypeDecider = new CusSCAHouseTypeDecider();

		public override ZGuid CA_CB
		{
			get => base.CA_CB;
			set
			{
				var oldValue = CA_CB;
				base.CA_CB = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CA_CB)
				{
					Pivot.MarkAsNeedingValidation();
				}

				if (!IsInDatabase && oldValue != CA_CB)
				{
					MessageStatusCalculator.DeriveStatusNow();
				}
			}
		}

		public override ZGuid CA_JS
		{
			get => base.CA_JS;
			set
			{
				var oldValue = CA_JS;
				base.CA_JS = value;
				if (!IsCopying && !IsInDatabase && value != ZGuid.Empty && oldValue != value)
				{
					stackTrace = System.Environment.StackTrace;
				}
			}
		}

		protected string stackTrace;

		[ReadOnlyMember(nameof(UsingFreightDefaults))]
		public override ZString CA_HouseBill
		{
			get => base.CA_HouseBill;
			set
			{
				var oldValue = CA_HouseBill;
				base.CA_HouseBill = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CA_HouseBill)
				{
					Pivot.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool CA_IsMasterHouse
		{
			get => base.CA_IsMasterHouse;
			set
			{
				var oldValue = CA_IsMasterHouse;
				base.CA_IsMasterHouse = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CA_IsMasterHouse)
				{
					Pivot.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CA_BGMReference
		{
			get
			{
				return base.CA_BGMReference;
			}
			set
			{
				if (IsInDatabase && !base.CA_BGMReference.IsEmpty)
				{
					ErrorReporter.ReportOnce("Changing BGM Reference in an invalid manner", string.Format("Can not change BGM reference after it has been saved, but still changing it from {0} to {1}", base.CA_BGMReference, value)); // Column name used in error message, not key
				}
				base.CA_BGMReference = value;
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (!IsDeleted && MessageInProgress); }
			set
			{
				base.ReadOnly = value;
				if (OceanBill != null)
				{
					this.OceanBill.UpdateReadOnly();
				}
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region New Properties

		static class Constants
		{
			public const string UserFriendlyStatuses = "UserFriendlyStatuses";
		}

		public ZString UserFriendlyStatuses
		{
			get
			{
				var result = ZString.Empty;
				if (Pivot.Count == 1)
				{
					result = Pivot[0].StatusCalculator.UserFriendlyStatusText;
				}
				return result;
			}
		}

		public ZPropertyInfo UserFriendlyStatusesInfo
		{
			get { return GetZPropertyInfo(Constants.UserFriendlyStatuses); }
		}

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && CMRStatusHelper.CanDelete(CA_MessageStatus) && PivotsCanBeDeleted;
			}
		}

		protected bool PivotsCanBeDeleted
		{
			get
			{
				foreach (CusSCAPivot pack in Pivot)
				{
					if (!pack.CanDelete)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;

				if (!CanDelete)
				{
					string tempString = !CMRStatusHelper.CanDelete(CA_MessageStatus) ? "sent messages" : "";

					if (!PivotsCanBeDeleted)
					{
						if (!string.IsNullOrEmpty(tempString))
						{
							tempString += " and ";
						}

						tempString += "attached Pack Lines that cannot be deleted";
					}

					result = (NoResString)("You cannot delete this HouseBill as there are " + tempString);
				}

				return result;
			}
		}

		#endregion

		#region Overriden Properties

		internal void RecalculateCA_ShipmentStatus()
		{
			if (Pivot.Count == 1)
			{
				CA_ShipmentStatus = Pivot[0].CV_CargoStatus;
			}
			else if (Pivot.Count > 1)
			{
				CA_ShipmentStatus = Pivot.Cast<CusSCAPivot>().All(x => x.CV_CargoStatus == Pivot[0].CV_CargoStatus) ? Pivot[0].CV_CargoStatus : (ZString)CMRConsolidatedCargoStatuses.Codes.SeePackingDetails;
			}
		}

		public override ZString CA_MessageStatus
		{
			get { return base.CA_MessageStatus; }
			set
			{
				var oldValue = CA_MessageStatus;
				base.CA_MessageStatus = value;
				if (oldValue != CA_MessageStatus)
				{
					if (!IsMarkingAsNeedingValidationSuspended && !IsCopying)
					{
						Pivot.MarkAsNeedingValidation();
					}

					if (OceanBill != null)
					{
						OceanBill.RefreshShouldStopKeyFieldsChangeCalculation();
					}
					AddShipmentMessagingEventIfRequired();
				}
			}
		}

		void AddShipmentMessagingEventIfRequired()
		{
			if (Shipment?.JS_ShipmentType == (ZString?)Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy)
			{
				switch (CA_MessageStatus)
				{
					case CMRBaseStatuses.Codes.AwaitingResponseToOriginal:
					case CMRBaseStatuses.Codes.AwaitingResponseToAmendment:
						new LogsForNominatedEvent(Shipment.GetLogs(), Events.HVLVReady).AddNew(ZString.Format("{0}|RES=Cargo Reporting", Shipment.JS_HouseBill));
						break;
				}
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			PopulateCA_BGMReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				CA_BGMReference = ZString.Empty;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (CA_MessageStatusInfo.HasChanges)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CA_MessageStatus), ConcurrencyPolicy.Strict);
			}
			ReportDuplicatedSeaCargoRecord();
		}

		void ReportDuplicatedSeaCargoRecord()
		{
			var existingHouseBill = FindDuplicate();
			if (existingHouseBill != null)
			{
				var shipment = Shipment;
				var oceanBill = OceanBill;
				ErrorReporter.ReportOnce("DE666B38-21AD-4275-90F0-CD719579522F", string.Format(CultureInfo.InvariantCulture, "Shipment with multiple CusSCAHouse children is saving, HouseBillCreateTime = {0}," +
					"ShipmentNumber={1},OceanBillNumber={2}." + System.Environment.NewLine + "HouseBillCreatedStackTrace: {3}", CA_SystemCreateTimeUtc, shipment.JS_UniqueConsignRef, oceanBill.CB_OceanBill, stackTrace));
			}
		}

		public virtual void PopulateCA_BGMReferenceIfNeeded()
		{
			if (CA_BGMReference.IsEmpty)
			{
				OceanBill?.BulkAllocateReferenceNumbersForChildBills();
			}

			if (CA_BGMReference.IsEmpty)
			{
				CommonShipment shipment = this.Shipment;
				if (shipment != null)
				{
					shipment.PopulateBillAndShipmentNumberIfNeeded();

					//CommonShipment.JS_UniqueConsignRef.MaxLength is 15 for AU.
					CA_BGMReference = shipment.JS_UniqueConsignRef.Left(CusSCAHouse.Schema.CA_BGMReferenceMaxLength);
				}
				else
				{
					CA_BGMReference = Env.NumberFountains.CusSCAHouseNumber.GetNextFormatted(Factory);
				}
			}
		}

		public override void CopyConsigneeDetails(IAddress consigneeAddress, bool copyOrganisationPK = true)
		{
			base.CopyConsigneeDetails(consigneeAddress, copyOrganisationPK);

			var cid = CargoHelper.GetIdentifier(consigneeAddress?.Organisation, consigneeAddress?.OrgAddress);
			var businessNumber = CargoHelper.GetConsigneeBusinessNumber(consigneeAddress?.Organisation);
			CA_ConsigneeBusinessNumber = !businessNumber.IsEmpty && !cid.IsEmpty ? ZString.Empty : businessNumber;
			CA_ConsigneeIdentifier = cid;
		}

		protected override void CopyConsigneeFK(IAddress consigneeAddress)
		{
			CA_OA_ConsigneeAddress = consigneeAddress.OrgAddress?.PK ?? ZGuid.Empty;
		}

		public override void CopyConsignorDetails(IAddress consignorAddress, bool copyOrganisationPK = true)
		{
			base.CopyConsignorDetails(consignorAddress, copyOrganisationPK);

			CA_ConsignorIdentifier = CargoHelper.GetIdentifier(consignorAddress?.Organisation, consignorAddress?.OrgAddress);
			CA_VendorIdentifier = CargoHelper.GetConsignorVendor(consignorAddress?.Organisation);
		}

		protected override void CopyConsignorFK(IAddress consignorAddress)
		{
			CA_OA_ConsignorAddress = consignorAddress.OrgAddress?.PK ?? ZGuid.Empty;
		}

		protected virtual bool ConsigneeAddressLockedUnlessUnMatched
		{
			get { return (Consignee != null && Consignee.PK != OrgHeader.UnmatchedOrganisationPK) && !MessageAcknowledged; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLockedUnlessUnMatched))]
		public override ZString CA_ConsigneeBusinessNumber
		{
			get { return base.CA_ConsigneeBusinessNumber; }
			set { base.CA_ConsigneeBusinessNumber = value; }
		}

		[ReadOnlyMember(nameof(ConsigneeAddressLockedUnlessUnMatched))]
		public override ZString CA_ConsigneeIdentifier
		{
			get { return base.CA_ConsigneeIdentifier; }
			set { base.CA_ConsigneeIdentifier = value; }
		}

		protected virtual bool ConsignorAddressLockedUnlessUnMatched
		{
			get { return (Consignor != null && Consignor.PK != OrgHeader.UnmatchedOrganisationPK) && !MessageAcknowledged; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLockedUnlessUnMatched))]
		public override ZString CA_ConsignorIdentifier
		{
			get { return base.CA_ConsignorIdentifier; }
			set { base.CA_ConsignorIdentifier = value; }
		}

		[ReadOnlyMember(nameof(ConsignorAddressLockedUnlessUnMatched))]
		public override ZString CA_VendorIdentifier
		{
			get { return base.CA_VendorIdentifier; }
			set
			{
				base.CA_VendorIdentifier = value.Replace(" ", "").Replace("/", "");
				if (value != base.CA_VendorIdentifier)
				{
					CA_VendorIdentifierInfo.RefreshBinding();
				}
			}
		}

		#region Constants

		public const string DuplicateHouseBillNumber = "This House Bill is already in use on this Ocean Bill: ";

		#endregion

		#region Properties

		public override OrgHeader Consignee => ConsigneeAddress?.Header;

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.Consignee_List))]
		public ZGuid ConsigneeOrgPK
		{
			get => CA_OA_ConsigneeAddress_ZAddress.OrgPK;
			set => CA_OA_ConsigneeAddress_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo ConsigneeOrgPKInfo => GetWrappedZPropertyInfo(nameof(ConsigneeOrgPK), x => CA_OA_ConsigneeAddress_ZAddress.OrgPKInfo);

		[List(nameof(CA_OA_ConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid CA_OA_ConsigneeAddress
		{
			get => base.CA_OA_ConsigneeAddress;
			set => base.CA_OA_ConsigneeAddress = value;
		}

		protected override ZAddress GetNewCA_OA_ConsigneeAddress_ZAddress()
		{
			var address = base.GetNewCA_OA_ConsigneeAddress_ZAddress();
			address.GetDefaultAddress = header => GetDefaultConsigneeAddress(header);
			return address;
		}

		ZGuid GetDefaultConsigneeAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				var addressDecider = new PortBasedOrgAddressDecider(organisation, ConsigneeAddressType, delegate()
				{ return CA_RL_NK_PortOfDestination; });
				result = addressDecider.AddressWithFallback.PK;
			}
			return result;
		}

		public override OrgHeader Consignor => ConsignorAddress?.Header;

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.Consignor_List))]
		public ZGuid ConsignorOrgPK
		{
			get => CA_OA_ConsignorAddress_ZAddress.OrgPK;
			set => CA_OA_ConsignorAddress_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo ConsignorOrgPKInfo => GetWrappedZPropertyInfo(nameof(ConsignorOrgPK), x => CA_OA_ConsignorAddress_ZAddress.OrgPKInfo);

		[List(nameof(CA_OA_ConsignorAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid CA_OA_ConsignorAddress
		{
			get => base.CA_OA_ConsignorAddress;
			set => base.CA_OA_ConsignorAddress = value;
		}

		protected override ZAddress GetNewCA_OA_ConsignorAddress_ZAddress()
		{
			var address = base.GetNewCA_OA_ConsignorAddress_ZAddress();
			address.GetDefaultAddress = header => GetDefaultConsignorAddress(header);
			return address;
		}

		ZGuid GetDefaultConsignorAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				var addressDecider = new PortBasedOrgAddressDecider(organisation, ConsignorAddressType, delegate()
				{ return CA_RL_NK_PortOfOrigin; });
				result = addressDecider.AddressWithFallback.PK;
			}
			return result;
		}

		protected override void CopyConsigneeDetails(OrgAddress orgAddress)
		{
			base.CopyConsigneeDetails(orgAddress);

			var cid = CargoHelper.GetIdentifier(orgAddress?.Header, orgAddress);
			var businessNumber = CargoHelper.GetConsigneeBusinessNumber(orgAddress?.Header);
			CA_ConsigneeBusinessNumber = !businessNumber.IsEmpty && !cid.IsEmpty ? ZString.Empty : businessNumber;
			CA_ConsigneeIdentifier = cid;
		}

		protected override void CopyConsignorDetails(OrgAddress orgAddress)
		{
			base.CopyConsignorDetails(orgAddress);

			CA_ConsignorIdentifier = CargoHelper.GetIdentifier(orgAddress?.Header, orgAddress);
			CA_VendorIdentifier = CargoHelper.GetConsignorVendor(orgAddress?.Header);
		}

		#region AggregatedMasterHouseBill

		public ZString AggregatedMasterHouseBill
		{
			get
			{
				return !CA_MasterHouseBill.IsEmpty || OceanBill == null ? CA_MasterHouseBill : OceanBill.CB_MasterHouseBill;
			}
		}

		#endregion

		#region ShipmentStatus

		public ZString ShipmentStatus
		{
			get
			{
				return Factory.GetCachedValue<CMRShipmentStatuses>().GetDescriptionFromCode(CA_ShipmentStatus);
			}
		}

		public ZPropertyInfo ShipmentStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ShipmentStatus); }
		}

		public ZString MessageStatus
		{
			get
			{
				return Factory.GetCachedValue<CMRStatuses>().GetDescriptionFromCode(CA_MessageStatus);
			}
		}

		public ZPropertyInfo MessageStatusInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.MessageStatus);
			}
		}

		public event EventHandler ManualStatusAcceptedAsAcknowledged;
		public void AcceptCurrentAsAcknowledged()
		{
			AcceptCurrentAsAcknowledged("Forced current status to be accepted as Acknowledge");
		}

		public void AcceptCurrentAsAcknowledged(string eventReference)
		{
			OnAcceptCurrentAsAcknowledged(eventReference);
		}

		/// <summary>
		/// This AcceptCurrentAsAcknowledged functionality is probably deprecated. Will remove it in the future.
		/// </summary>
		/// <param name="eventReference"></param>
		protected void OnAcceptCurrentAsAcknowledged(string eventReference)
		{
			CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			foreach (CusSCAPivot pivot in Pivot)
			{
				pivot.CV_CargoStatus = CMRBaseStatuses.Codes.NotSent;
			}

			if (ManualStatusAcceptedAsAcknowledged != null)
			{
				ManualStatusAcceptedAsAcknowledged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Underbond Movement Addresses

		public override ZGuid CA_OA_UnderbondFrom
		{
			get
			{
				return base.CA_OA_UnderbondFrom;
			}
			set
			{
				base.CA_OA_UnderbondFrom = value;
				CA_MoveUnderbondFrom_ReadOnly = value.IsValid;
			}
		}

		protected bool CA_MoveUnderbondFrom_ReadOnly { get; set; }
		public override ZString CA_MoveUnderbondFrom
		{
			get
			{
				ZString result = "";
				if (CA_OA_UnderbondFrom.IsValid)
				{
					result = UnderbondFrom.LocalControlledPremisesID;
				}
				else
				{
					result = base.CA_MoveUnderbondFrom;
				}
				return result;
			}
			set
			{
				base.CA_MoveUnderbondFrom = value;
				ZString currentCode = "";
				if (UnderbondFrom != null)
				{
					currentCode = UnderbondFrom.LocalControlledPremisesID;
				}
				if (currentCode.IsEmpty || currentCode != value)
				{
					OrgAddress newPremisesIDAddress = FindAddressForCusCode(value);
					if (newPremisesIDAddress != null)
					{
						base.CA_OA_UnderbondFrom = newPremisesIDAddress.PK;
					}
					else
					{
						base.CA_OA_UnderbondFrom = ZGuid.Empty;
					}
				}
			}
		}

		public override ZGuid CA_OA_UnderbondTo
		{
			get
			{
				return base.CA_OA_UnderbondTo;
			}
			set
			{
				base.CA_OA_UnderbondTo = value;
				CA_MoveUnderbondTo_ReadOnly = value.IsValid;
			}
		}

		protected bool CA_MoveUnderbondTo_ReadOnly { get; set; }

		public override ZString CA_MoveUnderbondTo
		{
			get
			{
				ZString result = "";
				if (CA_OA_UnderbondTo.IsValid)
				{
					result = UnderbondTo.LocalControlledPremisesID;
				}
				else
				{
					result = base.CA_MoveUnderbondTo;
				}
				return result;
			}
			set
			{
				base.CA_MoveUnderbondTo = value;
				ZString currentCode = "";
				if (UnderbondTo != null)
				{
					currentCode = UnderbondTo.LocalControlledPremisesID;
				}
				if (currentCode.IsEmpty || currentCode != value)
				{
					OrgAddress newPremisesIDAddress = FindAddressForCusCode(value);
					if (newPremisesIDAddress != null)
					{
						base.CA_OA_UnderbondTo = newPremisesIDAddress.PK;
					}
					else
					{
						base.CA_OA_UnderbondTo = ZGuid.Empty;
					}
				}
			}
		}

		#endregion

		#region ConsigneeAddressAsASingleLine

		public string ConsigneeAddressAsASingleLine
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(GetStringWithPaddingSpace(CA_ConsigneeAddress1));
				builder.Append(GetStringWithPaddingSpace(CA_ConsigneeAddress2));
				builder.Append(GetStringWithPaddingSpace(CA_ConsigneeSuburb));
				builder.Append(GetStringWithPaddingSpace(CA_ConsigneeState));
				builder.Append(CA_ConsigneePostcode);

				return builder.ToString();
			}
		}

		string GetStringWithPaddingSpace(string inputString)
		{
			return !string.IsNullOrEmpty(inputString) ? inputString + " " : inputString;
		}

		#endregion

		[MaxLength(50)]
		public ZString CA_HouseBillReadOnly
		{
			get { return CA_HouseBill; }
		}

		public ZPropertyInfo CA_HouseBillReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.CA_HouseBillReadOnly); }
		}

		public override ZString CA_ResponsiblePartyID
		{
			get
			{
				return base.CA_ResponsiblePartyID;
			}
			set
			{
				base.CA_ResponsiblePartyID = value.Replace(" ", "");
			}
		}

		[ReadOnlyMember(nameof(UsingFreightDefaults))]
		public override ZString CA_RL_NK_PortOfDestination
		{
			get => base.CA_RL_NK_PortOfDestination;
			set
			{
				var oldValue = base.CA_RL_NK_PortOfDestination;
				base.CA_RL_NK_PortOfDestination = value;
				var newValue = CA_RL_NK_PortOfDestination;
				if (!IsCopying && oldValue != newValue)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		[ReadOnlyMember(nameof(UsingFreightDefaults))]
		public override ZString CA_RL_NK_PortOfOrigin
		{
			get => base.CA_RL_NK_PortOfOrigin;
			set
			{
				var oldValue = base.CA_RL_NK_PortOfOrigin;
				base.CA_RL_NK_PortOfOrigin = value;
				var newValue = CA_RL_NK_PortOfOrigin;
				if (!IsCopying && oldValue != newValue)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		#endregion

		public const string CusSCAHouseUpdaterOverflowExceptionCausedByKey = "CusSCAHouseUpdaterOverflowExceptionCausedByKey";
		public const string CusSCAHouseUpdaterOverflowExceptionMaxRangeKey = "CusSCAHouseUpdaterOverflowExceptionMaxRangeKey";

		#region BusinessObjects

		protected override IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection()
		{
			return Pivot.Cast<BaseCusSCAPivot>();
		}

		[ChildEditable]
		public CusSCAPivotCollectionForHouseBill Pivot
		{
			get
			{
				if (fPivot == null)
				{
					fPivot = new CusSCAPivotCollectionForHouseBill(this);
					RegisterEditableChildObject(fPivot);
					fPivot.Load();
					// JPG - This is a hack to retain the same behaviour as we had previously. Should be reworked in a separate WI.
					if (fPivot.Take(2).Count() == 1)
					{
						var firstPivot = fPivot.Cast<CusSCAPivot>().First();
						firstPivot.CV_AssociatedContainer_ReadOnly = firstPivot.CV_CN.IsValid
							&& (firstPivot.CV_AssociatedContainer == CusSCAPivot.BreakBulk || firstPivot.CV_AssociatedContainer == CusSCAPivot.Bulk || firstPivot.CV_AssociatedContainer == CusSCAPivot.Liquid);
					}
				}
				return fPivot;
			}
		}

		public bool PivotCollectionHasBeenLoaded
		{
			get
			{
				return fPivot != null;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CA_JS = ZGuid.Empty;
			}
			Pivot.DeleteAll();
			base.Delete();
			OnDeleted(EventArgs.Empty);
		}

		public new CusSCAOceanBill OceanBill
		{
			get { return (CusSCAOceanBill)base.OceanBill; }
		}

		public void MakeOceanBillAnEditableChild()
		{
			var oceanBill = OceanBill;
			if (oceanBill != null && !oceanBill.IsDeleted && !IsRegisteredEditableChildObject(oceanBill))
			{
				oceanBill.UnregisterHouseBillsFromEditableChildren();
				RegisterEditableChildObject(oceanBill);
			}
		}

		#region UsingFreightDefaults

		protected bool UsingFreightDefaults
		{
			get
			{
				var oceanBill = OceanBill;
				return oceanBill != null && !oceanBill.OverrideFreightDefaults;
			}
		}

		#endregion

		#region Logs

		public LogsForNominatedEvent UnderbondCustomsResponseLogs
		{
			get
			{
				if (fUnderbondCustomsResponseLogs == null)
				{
					fUnderbondCustomsResponseLogs = new LogsForNominatedEvent(Logs, Events.UnderbondCustomsApproval);
				}
				return fUnderbondCustomsResponseLogs;
			}
		}

		public LogsForNominatedEvent CargoStatusAdviceResponseLogs
		{
			get
			{
				if (fCargoStatusAdviceResponseLogs == null)
				{
					fCargoStatusAdviceResponseLogs = new LogsForNominatedEvent(Logs, Events.CustomsManifestStatus);
				}
				return fCargoStatusAdviceResponseLogs;
			}
		}

		protected LogsForNominatedEvent fUnderbondCustomsResponseLogs;
		protected LogsForNominatedEvent fCargoStatusAdviceResponseLogs;

		#endregion

		#endregion

		#region Code Lists

		public RefCountryCollection CountryOfOriginList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public RefUNLOCOCollection PortOfOriginList
		{
			get { return new RefUNLOCOCollection(Factory, new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, Enterprise.Core.Constants.CountryCodes.Australia)); }
		}

		public RefUNLOCOCollection PortOfDestinationList
		{
			get { return new RefUNLOCOCollection(Factory, new ZQuery()); }
		}

		public OrgHeaderCollection CA_OH_Consignor_List
		{
			get
			{
				if (!CA_IsMasterHouse)
				{
					ConsignorCollection result = new ConsignorCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", CA_RL_NK_PortOfOrigin));
					return result;
				}
				else
				{
					ForwarderCollection result = new ForwarderCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", CA_RL_NK_PortOfOrigin));
					return result;
				}
			}
		}

		public OrgHeaderCollection CA_OH_Consignee_List
		{
			get
			{
				if (!CA_IsMasterHouse)
				{
					ConsigneeCollection result = new ConsigneeCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", CA_RL_NK_PortOfDestination));
					return result;
				}
				else
				{
					ForwarderCollection result = new ForwarderCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", CA_RL_NK_PortOfDestination));
					return result;
				}
			}
		}

		public OrgHeaderCollection CA_OH_Notify_List
		{
			get
			{
				OrgHeaderCollection result = new OrgHeaderCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Main UNLOCO", "Property", CA_RL_NK_PortOfDestination));
				return result;
			}
		}

		public OrgHeaderCollection UnderbondFromCollection
		{
			get
			{
				OrgHeaderCollection result = null;
				ZString portOfDischarge = (OceanBill != null) ? OceanBill.CB_RL_NKPortOfDischarge : ZString.Empty;
				if (!portOfDischarge.IsEmpty)
				{
					result = new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, portOfDischarge));
				}
				else
				{
					result = new OrgHeaderCollection(Factory);
				}
				return result;
			}
		}

		public OrgHeaderCollection UnderbondToCollection
		{
			get
			{
				OrgHeaderCollection result = null;
				if (CA_RL_NK_PortOfDestination.IsEmpty)
				{
					result = new OrgHeaderCollection(Factory);
				}
				else
				{
					result = new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, CA_RL_NK_PortOfDestination));
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected internal CusSCAPivotCollectionForHouseBill fPivot;

		public event EventHandler Deleted;
		protected virtual void OnDeleted(EventArgs e)
		{
			Deleted?.Invoke(this, e);
		}

		protected override void OnDeletedByDataRefresh()
		{
			base.OnDeletedByDataRefresh();
			Deleted?.Invoke(this, EventArgs.Empty);
		}

		public void SetHousebillReadOnly(bool readOnly)
		{
			this.ReadOnly = readOnly;
		}

		protected OrgAddress FindAddressForCusCode(ZString cusCode)
		{
			OrgAddress result = null;

			ZQuery cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, cusCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
			BusinessObject[] orgCusCodes = Factory.Load(typeof(OrgCusCode), cusCodeFilter);
			if (orgCusCodes.Length >= 1)
			{
				OrgCusCode firstResult = (OrgCusCode)orgCusCodes[0];
				if (!firstResult.OK_OA_PremisesAddress.IsEmpty)
				{
					result = Factory.Load<OrgAddress>(firstResult.OK_OA_PremisesAddress);
				}
				else if (firstResult.Lookups.PremisesAddresses.Count > 0)
				{
					result = firstResult.Lookups.PremisesAddresses[0];
				}
			}
			return result;
		}

		protected ZString GetContainerString(int containers)
		{
			return containers == 1 ? " Container" : " Containers";
		}

		public CusSCAHouse FindDuplicate()
		{
			if (!IsInDatabase && !CA_CB.IsEmpty && !CA_JS.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusSCAHouse));
				query.AddToFilter(CusSCAHouseSchema.CA_CB, CA_CB);
				query.AddToFilter(CusSCAHouseSchema.CA_JS, CA_JS);
				query.ReLoadExistingRows = true;

				return Factory.LoadTop1<CusSCAHouse>(query);
			}

			return null;
		}

		public new CusSCAHouseLookups Lookups
		{
			get { return (CusSCAHouseLookups)base.Lookups; }
		}

		protected override Customs.Business.CusSCAHouseLookups GetNewLookups()
		{
			return new CusSCAHouseLookups(this);
		}

		public new CusSCAHouseValidation Validation
		{
			get { return (CusSCAHouseValidation)base.Validation; }
		}

		protected override Customs.Business.CusSCAHouseValidation GetNewValidation()
		{
			return new CusSCAHouseValidation(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusSCAHouseFetchStrategy(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (Env.Registry.ConsolPaymentTerm == Enterprise.Core.Constants.PaymentType.Prepaid)
			{
				CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
			else if (Env.Registry.ConsolPaymentTerm == Enterprise.Core.Constants.PaymentType.Collect)
			{
				CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.Collect;
			}
		}

		protected override bool MessageInProgress
		{
			get { return new CusSCAHouseSEACRManager(this).IsWaitingForResponse; }
		}

		protected override bool MessageAcknowledged
		{
			get { return new CusSCAHouseSEACRManager(this).CanSendWithdrawal; }
		}

		public override void DefaultFromShipment()
		{
			var shipment = Shipment;
			var oceanbill = OceanBill;
			var consol = oceanbill?.Consol;
			if (shipment != null && consol != null)
			{
				var synchroniser = new CMRSeaCargoSynchroniser(consol, oceanbill);
				synchroniser.DefaultSeaCargoHouseFromShipment(this, shipment);
				if (CMRStatusHelper.CanDelete(CA_MessageStatus))
				{
					using (var houseSynchroniser = new CMRHouseBillSynchroniser(this, shipment))
					{
						houseSynchroniser.SetEnabled(true, houseSynchroniser.DetectEnabled);
						houseSynchroniser.Synchronise(new SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Force));
					}
				}
			}
		}

		public NonDependentEDIMessageCollection MessagesForBinding
		{
			get
			{
				if (fMessagesForBinding == null)
				{
					fMessagesForBinding = new NonDependentEDIMessageCollection(Factory);
					fMessagesForBinding.AddRange(Messages.ToArray());
					foreach (CusSCAPivot pivot in Pivot)
					{
						fMessagesForBinding.AddRange(pivot.Messages.ToArray());
					}
					Messages.CountChanged += Messages_CountChanged;
				}
				return fMessagesForBinding;
			}
		}

		NonDependentEDIMessageCollection fMessagesForBinding;

		void Messages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				MessagesForBinding.Add(e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				MessagesForBinding.Remove(e.BizObject);
			}
		}
		#endregion

		#region ISeaCargoShipmentInfo

		public ZGlobalMutex Mutex
		{
			get { return null; }
		}

		public void UnlockMutexIfNeeded() { }
		public void SynchroniseIfWeCan() { }

		public CusSCAHouse HouseBill
		{
			get { return this; }
		}

		public CusSCAHouse GetHouseBill
		{
			get { return this; }
		}

		public IManifestProvider ManifestProvider
		{
			get { return this.OceanBill; }
		}

		public SeaCargoSynchroniser SeaCargoSynchroniser
		{
			get { return null; }
		}

		public bool IsVisible
		{
			get { return true; }
		}

		public bool RegisterTopLevelBusinessObjectAsEditable
		{
			get { return false; }
		}

		BusinessObject ISeaCargoShipmentInfo.TopLevelObject
		{
			get { return this; }
		}

		void ISeaCargoShipmentInfo.StartSynchronising()
		{
		}

		bool ISeaCargoShipmentInfo.CanSynchronise
		{
			get { return false; }
		}

		bool ISeaCargoShipmentInfo.NoSynchronisingWillOccur
		{
			get { return false; }
		}

		ZString ISeaCargoShipmentInfo.SynchroniseFailureMessage
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ICusSCAHouse

		ZString Integration.Customs.AU.ICusSCAHouse.CA_HouseBill => CA_HouseBill;

		ZString Integration.Customs.AU.ICusSCAHouse.CA_ShipmentStatus
		{
			get { return CA_ShipmentStatus; }
			set { CA_ShipmentStatus = value; }
		}

		ICodeDescriptionPairList Integration.Customs.AU.ICusSCAHouse.ShipmentStatusesList => Factory.GetCachedValue<CMRShipmentStatuses>();

		#endregion

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, CA_RL_NK_PortOfDestination, CA_RL_NK_PortOfDestination.Left(2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, CA_RL_NK_PortOfOrigin, CA_RL_NK_PortOfOrigin.Left(2), ZString.Empty);
			return result;
		}

		protected override bool SupportsWorkflowCore => true;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (SupportsWorkflow && HasChanges && Shipment == null && OceanBill != null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		protected override ProcessTaskCollection GetNewCusSCAHouseProcessTaskCollection()
		{
			return new CusSCAHouseProcessTaskCollection(this);
		}

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				IGlbCompany result = null;
				var bill = OceanBill;
				if (bill != null)
				{
					var branch = bill.Branch;
					result = branch?.Company;
				}
				else
				{
					var shipment = Shipment;
					if (shipment != null)
					{
						var job = shipment.Job;
						result = job?.Company;
					}
				}
				return result ?? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var shipment = Shipment;
				if (shipment != null)
				{
					list.Add(shipment);
				}
				return list;
			}
		}

		#endregion

		#region ICMRMessageRespondee

		EDIMessageCollection ICMRMessageRespondee.Messages
		{
			get { return Messages; }
		}

		public ZString Details
		{
			get
			{
				StringBuilder builder = new StringBuilder();

				if (OceanBill != null)
				{
					builder.Append(OceanBill.Details);
					builder.Append("\r\n");
				}

				builder.Append("HOUSE BILL DETAILS:\r\n");

				if (!CA_BGMReference.IsEmpty)
				{
					builder.Append("Message Reference: " + CA_BGMReference + "\r\n");
				}

				if (!CA_HouseBill.IsEmpty)
				{
					builder.Append("House Bill: " + CA_HouseBill + "\r\n");
				}

				if (Consignor != null)
				{
					builder.Append("Consignor: " + Consignor.OH_FullNameTruncated + "\r\n");
				}

				if (Consignee != null)
				{
					builder.Append("Consignee: " + Consignee.OH_FullNameTruncated + "\r\n");
				}

				if (_PortOfOrigin != null)
				{
					builder.Append("Origin: " + _PortOfOrigin.Code + "\r\n");
				}

				if (_PortOfDestination != null)
				{
					builder.Append("Destination: " + _PortOfDestination.Code + "\r\n");
				}

				return builder.ToString();
			}
		}

		public ZString ShortDescription
		{
			get { return OceanBill != null ? OceanBill.ShortDescription : ZString.Empty; }
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		public bool CanSendWithoutDelay
		{
			get
			{
				var messageFunctions = new CMREdiMessageFunctions();
				return messageFunctions.DoMessagesContainAnyCARSTs(Messages);
			}
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		[ChildEditable]
		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollectionForSeaCargo(this);
					RegisterEditableChildObject(allUnderbonds);
					allUnderbonds.Load();
				}
				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		ICusUnderbondDependentCollectionParent[] ICusUnderbondUnionCollectionParent.GetAllPossibleCollectionProviders()
		{
			var result = new ArrayList();
			result.AddRange(Pivot);
			result.AddRange(Pivot.Cast<CusSCAPivot>().Select(pivot => pivot.Container).WhereNotNull().ToArray());
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		EDIMessageCollection IStatusNeedsRecalculationProvider.Messages
		{
			get { return Messages; }
		}

		public bool StatusNeedsRecalculation => fMessages?.HasChanges ?? false;

		#endregion

		#region IMessageManageableBizObj Members

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusSCAHouseMessageManager(this);
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return ContinueWithDetection.Yes;
		}

		#endregion

		#region IScanHouseBillProvider

		ZString IScanHouseBillProvider.ShipmentType
		{
			get { return Shipment != null ? Shipment.JS_ShipmentType : ZString.Empty; }
		}

		ZString IScanHouseBillProvider.HouseBill
		{
			get { return CA_HouseBill; }
		}

		ZString IScanHouseBillProvider.ConsigneeName
		{
			get { return CA_ConsigneeName; }
		}

		ZString IScanHouseBillProvider.ConsignorName
		{
			get { return CA_ConsignorName; }
		}

		ZString IScanHouseBillProvider.GoodsDescription
		{
			get { return Pivot.Count > 0 ? Pivot[0].GoodsDescription : ZString.Empty; }
		}

		IManifestInfo IScanHouseBillProvider.GetManifestInformation(CusUnderbond underbond)
		{
			return GetManifestInformation(null);
		}

		public CusSCAPivot GetManifestInformation(CusUnderbond underbond)
		{
			return underbond != null ? Pivot.Cast<CusSCAPivot>().FirstOrDefault(x => x.CN_ContainerNumber == underbond.ContainerNumber) : Pivot.Cast<CusSCAPivot>().FirstOrDefault();
		}

		ZBool IScanHouseBillProvider.ShouldScan(CusUnderbond underbond)
		{
			var containerNo = underbond.ContainerNumber;
			return Pivot.Cast<CusSCAPivot>().Select(pivot => pivot.Container).Cast<CusSCAContainer>().Any(x => x.CN_ContainerNumber == containerNo) && !underbond.IsSeaOutturned(CA_HouseBill);
		}

		void IScanHouseBillProvider.LogReadyForLocalDeliveryIfIsCargoStatusClear()
		{
			var manifest = GetManifestInformation(null);
			if (manifest != null)
			{
				manifest.LogReadyForLocalDeliveryIfIsCargoStatusClear();
			}
		}

		void IScanHouseBillProvider.ResetCargoReceivedAtDepotLogs(string reference)
		{
			var manifest = GetManifestInformation(null);
			if (manifest != null)
			{
				manifest.ResetCargoReceivedAtDepotLogs(reference);
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerIncludingRelatedObjectsInfo(this, Core.Constants.DocManagerCodes.SCAHouseBill));
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocManagerSupportIncudingRelatedObjects Members

		BusinessObject IDocManagerSupportIncudingRelatedObjects.SelfReference => this;

		IEnumerable<BusinessObject> IDocManagerSupportIncudingRelatedObjects.GetRelatedBusinessObjects()
		{
			foreach (var underbond in AllUnderbonds)
			{
				yield return underbond;
			}
		}

		#endregion

		#region ICustomFieldProvider
		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		protected void ResetCustomBusinessObject()
		{
			customBusinessObject = null;
			OnResetCustomBusinessObject?.Invoke();
		}

		public delegate void OnResetCustomBusinessObjecDelegate();
		public OnResetCustomBusinessObjecDelegate OnResetCustomBusinessObject;
		#endregion

		#region eTail2 status mapping

		protected override ZString GetReason()
		{
			return CargoHelper.GetACSAQISStatusReason(UserFriendlyStatuses);
		}

		protected override ZString GetHVLVStatusMapping(ZString status)
		{
			var result = ZString.Empty;

			if (eTailConsignmentHoldStatusCodes.Contains(status))
			{
				result = CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Held;
			}
			else if (eTailConsignmentClearStatusCodes.Contains(status))
			{
				result = CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Cleared;
			}
			else if (eTailConsignmentGovernmentAgencyRequirementsStatusCodes.Contains(status))
			{
				result = CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.GovernmentAgencyRequirements;
			}
			else if (eTailConsignmentTransshipmentStatusCodes.Contains(status))
			{
				result = CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Transshipment;
			}

			return result;
		}

		IEnumerable<ZString> eTailConsignmentHoldStatusCodes
		{
			get
			{
				yield return CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
				yield return CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
				yield return CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
				yield return CMRConsolidatedCargoStatuses.Codes.SeePackingDetails;
				yield return CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
				yield return CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction;
				yield return CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo;
			}
		}

		IEnumerable<ZString> eTailConsignmentClearStatusCodes
		{
			get
			{
				yield return CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
				yield return CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			}
		}

		IEnumerable<ZString> eTailConsignmentGovernmentAgencyRequirementsStatusCodes
		{
			get
			{
				yield return CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
				yield return CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			}
		}

		IEnumerable<ZString> eTailConsignmentTransshipmentStatusCodes
		{
			get
			{
				yield return CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
				yield return CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			}
		}
		#endregion

		#region Update Consignee/Consignor/Notify from Shipment

		public void UpdateConsigneeFromShipment(CommonShipment shipment)
		{
			if (shipment != null)
			{
				var defaultAddress = CargoHelper.GetDefaultConsigneeAddress(shipment.ConsigneeDocumentaryAddress, shipment.ConsigneeDeliveryAddress, false);
				if (defaultAddress != null)
				{
					CopyConsigneeDetails(JobDocAddressWrapper.New(defaultAddress));
				}
			}
		}

		public void UpdateConsignorFromShipment(CommonShipment shipment)
		{
			if (shipment != null)
			{
				var shipmentConsignorAddress = shipment.ConsignorDocumentaryAddress;
				if (shipmentConsignorAddress != null)
				{
					CopyConsignorDetails(JobDocAddressWrapper.New(shipmentConsignorAddress));
				}
			}
		}

		public void UpdateNotifyFromShipment(CommonShipment shipment)
		{
			var docAddress = shipment?.NotifyPartyDocumentaryAddress;
			if (docAddress != null)
			{
				CopyNotifyDetails(JobDocAddressWrapper.New(docAddress));
			}
		}

		#endregion
	}
}
