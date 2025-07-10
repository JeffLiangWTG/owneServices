using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionRequestMessageSendingObject : JobDeclarationMiscMessageSendingObjectCore, IImport5UASessionDetails
	{
		public PenaltyExemptionRequestMessageSendingObject(CusEntryHeader entry, AmendmentSessionalData sessionalData5FE, ZShort versionNumber) : base(entry, ElectronicDocumentTypeList.Codes._5UA)
		{
			this.versionNumber = versionNumber;
			SessionalData5FE = sessionalData5FE;
		}

		public new class Schema : JobDeclarationMiscMessageSendingObjectCore.Schema
		{
			public const int PenaltyExemptionReasonMaxLength = 500;
			public const int PenaltyExemptionReasonCodeMaxLength = 2;
		}

		public readonly AmendmentSessionalData SessionalData5FE;
		readonly ZShort versionNumber;

		public EDIMessage Message5FE => SessionalData5FE.Message5FE;

		[ResourceStringData("BCD480BB-3DE2-4927-8771-37BEC52EA85F", Caption = "Amendment Version No.")]
		public override ZShort AmendmentVersion => ZShort.ParseSafe(SessionalData5FE.AmendmentCW1VersionNo.ToString(), 1);

		public override ZString AmendmentType { get; }

		public override ZString AmendmentTypeDescription { get; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationMiscMessageSendingObjectCoreLookups.PenaltyExemption5UAOnlyCodeList))]
		[ResourceStringData("14580327-CB19-4E2B-8058-E761EA94AA77", Caption = "Penalty Exemption Reason Code")]
		[MaxLength(Schema.PenaltyExemptionReasonCodeMaxLength)]
		public ZString PenaltyExemptionReasonCode
		{
			get
			{
				return penaltyExemptionReasonCode;
			}
			set
			{
				CheckMaximumLength(PenaltyExemptionReasonCodeInfo, value);
				SetNonPersistentPropertyValue(PenaltyExemptionReasonCodeInfo, ref penaltyExemptionReasonCode, value);
			}
		}
		ZString penaltyExemptionReasonCode;

		public ZPropertyInfo PenaltyExemptionReasonCodeInfo => GetZPropertyInfo(nameof(PenaltyExemptionReasonCode));

		[ResourceStringData("4B6ECC33-39D1-48B1-8A2A-B803B59B4130", Caption = "Penalty Exemption Reason")]
		[MaxLength(Schema.PenaltyExemptionReasonMaxLength)]
		public ZString PenaltyExemptionReason
		{
			get { return penaltyExemptionReason; }
			set
			{
				CheckMaximumLength(PenaltyExemptionReasonInfo, value);
				SetNonPersistentPropertyValue(PenaltyExemptionReasonInfo, ref penaltyExemptionReason, value);
			}
		}
		ZString penaltyExemptionReason;

		public ZPropertyInfo PenaltyExemptionReasonInfo => GetZPropertyInfo(nameof(PenaltyExemptionReason));

		[ResourceStringData("FBE554CF-542B-499D-A27A-3D2C14BCB0CC", Caption = "Version No.")]
		public ZInt DutyPenaltyExemption5UASequenceNumber => versionNumber;

		[ResourceStringData("B6F40BA4-38BE-41E7-A397-03A03D137E8F", Caption = "Penalty Exemption Amount")]
		[DecimalPlaces(0)]
		public ZDecimal PenaltyExemptionAmount
		{
			get { return penaltyExemptionAmount; }
			set
			{
				SetNonPersistentPropertyValue(PenaltyExemptionAmountInfo, ref penaltyExemptionAmount, value);
			}
		}
		ZDecimal penaltyExemptionAmount;

		public ZPropertyInfo PenaltyExemptionAmountInfo => GetZPropertyInfo(nameof(PenaltyExemptionAmount));

		[ResourceStringData("B5A8A92A-FF7C-437B-B649-EB57F6355B4A", Caption = "Penalty Type")]
		public ZString PenaltyType => Factory.GetCachedValue<PenaltyExemptionCodeList>().GetDescriptionFromCode(PenaltyExemptionReasonCode.Left(1));

		[ResourceStringData("82164C1F-2DF3-4EDB-A09A-7ECC4203AF0E", Caption = "Amendment Declaration Date")]
		public ZDateTime AmendmentDeclarationDate => SessionalData5FE.Message5FE?.EM_MessageDateTime ?? ZDateTime.Empty;

		[ResourceStringData("1429D51D-7F8E-4400-B7EE-2886C2DEC8A5", Caption = "Penalty Amount (5FK)")]
		[DecimalPlaces(0)]
		public ZDecimal PenaltyAmountFrom5FK => SessionalData5FE.PenaltyAmountFrom5FK;

		ZString IImport5UASessionDetails.PenaltyExemptionIndicator => throw new NotImplementedException();

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new PenaltyExemptionRequestMessageSendingObjectValidation(this);
		}
	}
}
