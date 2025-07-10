using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SetEntryStatusDetail : AutoSetEntryStatusDetail
	{
		public SetEntryStatusDetail(JobDeclaration declaration) : base(declaration.Factory)
		{
			JobDeclaration = declaration;

			using (SuspendSettingHasChanges())
			{
				if (JobDeclaration?.ActiveEntryHeaders.Count == 1)
				{
					var defaultEntry = JobDeclaration.ActiveEntryHeaders[0];
					CusEntryHeaderPK = defaultEntry.PK;
				}
				EventTime = ZDateTime.Now;
			}
		}

		public JobDeclaration JobDeclaration { get; }

		public CusEntryHeader EntryHeader => JobDeclaration?.ActiveEntryHeaders.FindByPK(CusEntryHeaderPK) as CusEntryHeader;

		public bool SetEntryStatus()
		{
			if (!HasErrors)
			{
				var entryHeader = EntryHeader;
				entryHeader.CH_EntryStatus = EntryStatus;
				if (entryHeader.ShouldLogEntryStatus && (!entryHeader.IsInDatabase || !ZString.Equals(EntryStatus, entryHeader.CH_EntryStatusInfo.OriginalValue)))
				{
					EntryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatus, EventTime.ToOffset());
				}
				return true;
			}
			return false;
		}

		public SetEntryStatusDetailLookups Lookups => GetNewLookups();
		protected virtual SetEntryStatusDetailLookups GetNewLookups()
		{
			return new SetEntryStatusDetailLookups(this);
		}

		[CargoWise.ComponentModel.List(nameof(Lookups) + "." + nameof(SetEntryStatusDetailLookups.ActiveEntryHeaders))]
		public override ZGuid CusEntryHeaderPK
		{
			get => base.CusEntryHeaderPK;
			set
			{
				base.CusEntryHeaderPK = value;
				using (GetValidationSuspender())
				{
					var entryHeader = EntryHeader;
					base.EntryStatus = entryHeader == null ? ZString.Empty : entryHeader.CH_EntryStatus;
				}
			}
		}

		[CargoWise.ComponentModel.List(nameof(Lookups) + "." + nameof(SetEntryStatusDetailLookups.EntryStatusList))]
		public override ZString EntryStatus { get => base.EntryStatus; set => base.EntryStatus = value; }
	}
}
