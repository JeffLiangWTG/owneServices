using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderMessageSendingObject : AutoNctsHeaderMessageSendingObject
	{
		public new class Schema : AutoNctsHeaderMessageSendingObject.Schema
		{
			public const string ShouldSend = nameof(NctsHeaderMessageSendingObject.ShouldSend);
		}

		public NctsHeaderMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader?.Factory)
		{
			NctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));

			SetDefaultData();
		}
		public NctsHeader NctsHeader { get; }

		public NctsHeaderMessageSendingObjectLookups Lookups => lookups ?? (lookups = GetNewLookups());
		NctsHeaderMessageSendingObjectLookups lookups;

		protected virtual NctsHeaderMessageSendingObjectLookups GetNewLookups() => new NctsHeaderMessageSendingObjectLookups(this);

		protected override bool ShouldSend_ReadOnly => ZBool.True;

		public override ZString LRN => NctsHeader.MovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty;

		public override ZString MRN => NctsHeader.MovementReferenceNumber;

		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.DestinationCustomsOfficeCodeList))]
		public override ZString DestinationCustomsOfficeCode
		{
			get => base.DestinationCustomsOfficeCode;
			set => base.DestinationCustomsOfficeCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.MessageTypeList))]
		public override ZString MessageType
		{
			get => base.MessageType;
			set
			{
				var oldValue = MessageType;
				base.MessageType = value.ToUpperInvariant();
				if (!IsCopying && oldValue != MessageType)
				{
					ClearReleaseRequestIfNeeded();
					ClearJustificationIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.ReleaseRequestedFlags))]
		public override ZString ReleaseRequest
		{
			get => base.ReleaseRequest;
			set => base.ReleaseRequest = value;
		}

		protected override bool ReleaseRequest_ReadOnly => !MessageType.EqualsIgnoringCase(ReleaseRequestCode);

		internal string ReleaseRequestCode => NctsHeader.Configuration.MessageSendingConfiguration.ReleaseRequestCode;

		void ClearReleaseRequestIfNeeded()
		{
			if (!ReleaseRequest.IsEmpty && ReleaseRequest_ReadOnly)
			{
				ReleaseRequest = ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.DestinationCustomsOfficeCodeList))]
		public override ZString ActualOfficeOfDestination
		{
			get => base.ActualOfficeOfDestination;
			set
			{
				base.ActualOfficeOfDestination = value;
				if (!IsValidationSuspended)
				{
					((NctsActualConsigneeJobDocAddressValidation)ActualConsignee.AdditionalValidation).ValidateOrganisationPK();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.DepartureOfficeOfEnquiryCodeList))]
		public override ZString DepartureOfficeOfEnquiry
		{
			get => base.DepartureOfficeOfEnquiry;
			set => base.DepartureOfficeOfEnquiry = value;
		}

		protected override bool DepartureOfficeOfEnquiry_ReadOnly
		{
			get
			{
				var result = false;
				var header = NctsHeader;
				if (header.IsPhase5)
				{
					if (header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
					{
						result = departureMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry) && x.CY_Type.EqualsIgnoringCase(EU.Business.CusCodeDataTypeList.Codes.OfficeCode));
					}
					else if (header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
					{
						result = arrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry) && x.CY_Type.EqualsIgnoringCase(EU.Business.CusCodeDataTypeList.Codes.OfficeCode));
					}
				}
				else
				{
					result = header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry) && x.CY_Type.EqualsIgnoringCase(EU.Business.CusCodeDataTypeList.Codes.OfficeCode));
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.NCTS.Business.MessageSendingAction|ActualConsignee", Caption = "Actual Consignee")]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.Consignees))]
		public JobDocAddress ActualConsignee
		{
			get
			{
				if (actualConsigneeJobDocAddress == null || actualConsigneeJobDocAddress.IsDeleted)
				{
					actualConsigneeJobDocAddress = Factory.New<JobDocAddress>();
					actualConsigneeJobDocAddress.MakeNonPersistent();
					actualConsigneeJobDocAddress.AdditionalValidation = GetActualConsigneeJobDocAddressAdditionalValidation(actualConsigneeJobDocAddress);
					actualConsigneeJobDocAddress.DocAddressChanged += ActualConsigneeDocAddressChanged;
					RegisterEditableChildObject(actualConsigneeJobDocAddress);
				}
				return actualConsigneeJobDocAddress;
			}
		}
		JobDocAddress actualConsigneeJobDocAddress;

		void ActualConsigneeDocAddressChanged(object sender, EventArgs e)
		{
			((NctsActualConsigneeJobDocAddressValidation)ActualConsignee.AdditionalValidation).ValidateOrganisationPK();
			Validation.ValidateActualOfficeOfDestination();
		}

		protected virtual ZValidation GetActualConsigneeJobDocAddressAdditionalValidation(JobDocAddress actualConsigneeJobDocAddress) => new NctsActualConsigneeJobDocAddressValidation(actualConsigneeJobDocAddress, this);

		protected override NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderMessageSendingObjectValidation(this);
		protected override bool Consignee_ReadOnly => true;
		protected override bool DestinationCustomsOfficeCode_ReadOnly => true;

		protected override bool Justification_ReadOnly => NctsHeader.IsPhase5Departure && !MessageType.EqualsIgnoringCase(NCTS5DeparturePhaseList.Codes.Cancellation);

		public override ZString MessageStatus => NctsHeader.EffectiveMessageStatus;

		void ClearJustificationIfNeeded()
		{
			if (!Justification.IsEmpty && Justification_ReadOnly)
			{
				Justification = ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.Consignees))]
		public override ZString Consignee
		{
			get => base.Consignee;
			set => base.Consignee = value;
		}

		#region Default Values

		/// <summary>
		/// Sets the default values inferred from the NctsHeader instance.
		///
		/// Since the NctsHeader instance is not yet initialized in SetDefaultValues, it has been marked as sealed, enforcing the use of SetDefaultDataCore instead.
		/// </summary>
		void SetDefaultData()
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				var sendingConfiguration = NctsHeader.Configuration.MessageSendingConfiguration;
				sendingConfiguration.SetDefaultMessageType(this);

				ShouldSend = sendingConfiguration.GetShouldSendDefault(this);

				if (NctsHeader.IsPhase5)
				{
					if (NctsHeader.MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
					{
						ActualOfficeOfDestination = departureMovementHeader.DestinationCustomsOfficeCodeForDeparture;
						DepartureOfficeOfEnquiry = departureMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry) && x.CY_Type.EqualsIgnoringCase(EU.Business.CusCodeDataTypeList.Codes.OfficeCode))?.CY_Data ?? ZString.Empty;
					}
					else if (NctsHeader.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
					{
						ActualOfficeOfDestination = arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival;
						DepartureOfficeOfEnquiry = arrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry) && x.CY_Type.EqualsIgnoringCase(EU.Business.CusCodeDataTypeList.Codes.OfficeCode))?.CY_Data ?? ZString.Empty;
					}
				}
				else
				{
					ActualOfficeOfDestination = NctsHeader.DestinationCustomsOfficeCodeForDeparture;
					DepartureOfficeOfEnquiry = NctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry) && x.CY_Type.EqualsIgnoringCase(EU.Business.CusCodeDataTypeList.Codes.OfficeCode))?.CY_Data ?? ZString.Empty;
				}

				SetDefaultDataCore();
			}
		}

		protected virtual void SetDefaultDataCore()
		{
		}

		protected sealed override void SetDefaultValues() => base.SetDefaultValues();

		#endregion

		#region ValidationDecider

		internal INctsHeaderMessageSendingObjectValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsHeaderMessageSendingObjectValidationDecider> validationDeciderCached;

		protected virtual INctsHeaderMessageSendingObjectValidationDecider GetValidationDecider() => NctsHeader.Configuration.MessageSendingConfiguration.GetValidationDecider();

		#endregion
	}
}
