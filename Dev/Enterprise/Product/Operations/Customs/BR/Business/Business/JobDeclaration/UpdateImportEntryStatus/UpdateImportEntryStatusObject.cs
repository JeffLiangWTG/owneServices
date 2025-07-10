using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class UpdateImportEntryStatusObject : NonPersistentBusinessObject
	{
		public UpdateImportEntryStatusObject(JobDeclaration declaration) : base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public JobDeclaration Declaration
		{
			get => declaration;
			set
			{
				declaration = value;
				EntryHeader = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
				fEntryStatus = EntryHeader?.CH_EntryStatus ?? ZString.Empty;
				fRiskChannel = EntryHeader?.CH_RiskChannel ?? ZString.Empty;
			}
		}

		JobDeclaration declaration;
		public CusEntryHeader EntryHeader { get; private set; }

		protected override ZString HumanReadableNameCore => Res.GetString("79B20975-5F7A-4C4C-ADFA-578AD90DB211", "Update Entry Status");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EventDate = ZDateTime.Now;
		}

		#region EntryStatus

		[List(nameof(Lookups) + "." + nameof(UpdateImportEntryStatusObjectLookups.EntryStatusList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.UpdateImportEntryStatusObject|EntryStatus", Caption = "Entry Status", FullDescription = "The Entry Status.")]
		[MaxLength(3)]
		public ZString EntryStatus
		{
			get => fEntryStatus;

			set
			{
				SetNonPersistentPropertyValue(EntryStatusInfo, ref fEntryStatus, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryStatus();
				}
			}
		}

		ZString fEntryStatus;

		public ZPropertyInfo EntryStatusInfo => GetZPropertyInfo(nameof(EntryStatus));

		#endregion

		#region EventDate

		[ResourceStringData("Enterprise.Customs.BR.Business.UpdateImportEntryStatusObject|EventDate", Caption = "Event Date", FullDescription = "The Event Date.")]
		public ZDateTime EventDate
		{
			get => fEventDate;
			set
			{
				SetNonPersistentPropertyValue(EventDateInfo, ref fEventDate, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEventDate();
				}
			}
		}

		ZDateTime fEventDate;

		public ZPropertyInfo EventDateInfo => GetZPropertyInfo(nameof(EventDate));

		#endregion

		#region RiskChannel

		[List(nameof(Lookups) + "." + nameof(UpdateImportEntryStatusObjectLookups.RiskChannelList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.UpdateImportEntryStatusObject|RiskChannel", Caption = "Risk Channel", FullDescription = "The Risk Channel.")]
		[MaxLength(1)]
		public ZString RiskChannel
		{
			get => fRiskChannel;
			set
			{
				SetNonPersistentPropertyValue(RiskChannelInfo, ref fRiskChannel, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRiskChannel();
				}
			}
		}

		ZString fRiskChannel;

		public ZPropertyInfo RiskChannelInfo => GetZPropertyInfo(nameof(RiskChannel));

		#endregion

		public void UpdateEntryStatus()
		{
			if (EntryHeader != null)
			{
				if (EntryHeader.CH_EntryStatus != EntryStatus)
				{
					EntryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, EntryStatus, EventDate.ToOffset());

					if (ShouldUpdateReleaseDate(EntryStatus))
					{
						EntryHeader.CH_EntryReleaseDate = EventDate;
					}
					else if (EntryStatus.IsEmpty && ShouldUpdateReleaseDate(EntryHeader.CH_EntryStatus))
					{
						EntryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
					}

					EntryHeader.CH_EntryStatus = EntryStatus;
				}

				if (EntryHeader.CH_RiskChannel != RiskChannel)
				{
					EntryHeader.CH_RiskChannel = RiskChannel;
				}

				if (EntryHeader.HasChanges)
				{
					try
					{
						Declaration.Factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		bool ShouldUpdateReleaseDate(ZString entryStatus)
		{
			return CustomsStatusAttributeHelper.ShouldUpdateReleaseDate(Declaration.Factory, entryStatus, Core.Constants.CountryCodes.Brazil, ZDateTime.Today);
		}

		#region Validation

		public UpdateImportEntryStatusObjectValidation Validation
		{
			get { return new UpdateImportEntryStatusObjectValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		#endregion

		#region Lookups

		public UpdateImportEntryStatusObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new UpdateImportEntryStatusObjectLookups(this);
				}
				return fLookups;
			}
		}

		UpdateImportEntryStatusObjectLookups fLookups;

		#endregion
	}
}
