using System.Collections.Immutable;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ImportEntryMessageSendingAction : MessageSendingAction, IObsoleteValidation
	{
		public ImportEntryMessageSendingAction(CusEntryHeader entry, MessageSendingActionParent actionParent) : base(entry, (x) => ((CusEntryHeader)x).EntryNumber, actionParent)
		{
			RegistrationNumber = Details;
			SetCusCon();
		}

		public JobComInvoiceHeader Invoice => (JobComInvoiceHeader)MessagingObject.RandomHeader;

		public new CusEntryHeader MessagingObject => (CusEntryHeader)base.MessagingObject;

		public CusEntryInstruction EntryInstruction => MessagingObject.EntryInstruction;

		public ZString DeclarationType => EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZString SubStyle => EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

		public ZString Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZString EntryStatus => MessagingObject.CH_EntryStatus;

		[ResourceStringData("3ba4674c-de35-487f-ba36-dbc50ca38d50", Caption = "[18] Transport ID (Inland)")]
		public ZString TransportIDInLand => MessagingObject.Declaration?.ZG_Box18TransportID ?? ZString.Empty;

		[ResourceStringData("32d4e910-558c-4560-a8b1-9429b7966696", Caption = "[30] Goods Location")]
		public ZString GoodsLocation => MessagingObject.Declaration?.JE_LocationOfGoods ?? ZString.Empty;

		[ReadOnlyMember(nameof(CusCon_ReadOnly))]
		public ZBool CusCon
		{
			get => cusCon;
			set => SetNonPersistentPropertyValue(CusConInfo, ref cusCon, value);
		}
		ZBool cusCon;

		public ZPropertyInfo CusConInfo => GetZPropertyInfo(nameof(CusCon));

		ZBool CusCon_ReadOnly => !RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber(Details) || !CusConEditableSubStyles.Contains(SubStyle) || !CusConEditableEntryStatus.Contains(EntryStatus);

		public ZBool CanSend => !RegistrationNumber.IsEmpty || Details.IsEmpty;

		[ReadOnlyMember(nameof(RegistrationNumber_ReadOnly))]
		public ZString RegistrationNumber
		{
			get => registrationNumber;

			set
			{
				SetNonPersistentPropertyValue(RegistrationNumberInfo, ref registrationNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateRegistrationNumber();
				}

				if (!registrationNumber.IsEmpty)
				{
					SetMrnCusEntryNumber(value);
				}

				SetCusCon();
			}
		}

		ZString registrationNumber;

		public ZPropertyInfo RegistrationNumberInfo => GetZPropertyInfo(nameof(RegistrationNumber));

		bool RegistrationNumber_ReadOnly => !(RegistrationNumberEditableSubStyles.Contains(SubStyle) && (EntryStatus.IsEmpty || EntryStatus == UniversalReferenceConstants.EntryStatus.REJ));

		public void ValidateRegistrationNumber()
		{
			var targetInfo = RegistrationNumberInfo;
			targetInfo.ClearAllNotifications();
			if (!RegistrationNumber.IsEmpty && RegistrationNumberEditableSubStyles.Contains(SubStyle) && EntryStatus.IsEmpty)
			{
				if (!RegistrationNumber.StartsWith(UniversalReferenceConstants.ATLASReferenceNumberIdentifier.ATA))
				{
					targetInfo.AddMessageError(Res.GetString("71f557bf-4d67-4df2-a175-a5c635995075", "Registration Number digit 1-3 must start with 'ATA'."));
				}

				if (RegistrationNumber.Length != RegistrationNumberValidationHelper.RegistrationNumberLength)
				{
					targetInfo.AddMessageError(Res.GetString("32596988-121b-4614-beb2-be70c111ad1e", "Registration Number must have 21 characters."));
				}
			}
		}

		protected override bool ShouldSend_ReadOnly => ShouldSendDisabledEntryStatus.Contains(EntryStatus) || MessagingObject.CH_Status == EDIMessage.Status.Sent;

		void SetCusCon()
		{
			CusCon = RegistrationNumber.StartsWith("ATA") && RegistrationNumberEditableSubStyles.Contains(SubStyle) &&
				(EntryStatus.IsEmpty || CusConAutoSetEntryStatus.Contains(EntryStatus));
		}

		static readonly ImmutableHashSet<string> ShouldSendDisabledEntryStatus = ImmutableHashSet.Create(
			UniversalReferenceConstants.EntryStatus.RC2,
			UniversalReferenceConstants.EntryStatus.RL5,
			UniversalReferenceConstants.EntryStatus.TX4,
			UniversalReferenceConstants.EntryStatus.TX5,
			UniversalReferenceConstants.EntryStatus.TX6,
			UniversalReferenceConstants.EntryStatus.TX7,
			UniversalReferenceConstants.EntryStatus.TX8,
			UniversalReferenceConstants.EntryStatus.TXF,
			UniversalReferenceConstants.EntryStatus.TXR,
			UniversalReferenceConstants.EntryStatus.TRA
		);

		static readonly ImmutableHashSet<string> CusConEditableEntryStatus = ImmutableHashSet.Create(
			UniversalReferenceConstants.EntryStatus.RC1,
			UniversalReferenceConstants.EntryStatus.RL2,
			UniversalReferenceConstants.EntryStatus.RL3,
			UniversalReferenceConstants.EntryStatus.RL5,
			UniversalReferenceConstants.EntryStatus.RLB
		);

		static readonly ImmutableHashSet<string> CusConEditableSubStyles = ImmutableHashSet.Create(
			EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
			EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB,
			EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC
		);

		static readonly ImmutableHashSet<string> RegistrationNumberEditableSubStyles = ImmutableHashSet.Create(
			ImportSubStyleList.Codes.D,
			ImportSubStyleList.Codes.E,
			ImportSubStyleList.Codes.F
		);

		static readonly ImmutableHashSet<string> CusConAutoSetEntryStatus = ImmutableHashSet.Create(
			UniversalReferenceConstants.EntryStatus.RC1,
			UniversalReferenceConstants.EntryStatus.RL2,
			UniversalReferenceConstants.EntryStatus.RL3,
			UniversalReferenceConstants.EntryStatus.RL5,
			UniversalReferenceConstants.EntryStatus.RLB,
			UniversalReferenceConstants.EntryStatus.REJ
		);
	}
}
