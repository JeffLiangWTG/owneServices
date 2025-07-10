using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAcceptabilityBandViewModel : NonPersistentBusinessObject, IMENTAcceptabilityBandViewModel
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MENTAcceptabilityBandViewModel(BMComponentAcceptabilityBand band)
		{
			this.band = band;
			band.MENTAcceptabilityBandViewModel = this;
			FindAndRegisterPreviousQuery();
			band.RegisterEditableChildObject(this);
		}

		[ReadOnlyMember(nameof(MentEnabled_Readonly))]
		public ZBool MentEnabled
		{
			get { return mentEnabled; }
			set
			{
				if (value && !mentEnabled)
				{
					this.RegisterEditableChildObject(query);
					band.RefreshBindingIncludingChildren();
					query.HasChanges = true;

					SetQueryName(band, query);
					ChangeQueryAccessability(false, query);

					SetNonPersistentPropertyValue(MentEnabledInfo, ref mentEnabled, ZBool.True);
				}
				else if (!value && mentEnabled && query != null && !query.IsInDatabase)
				{
					ChangeQueryAccessability(true, query);
					SetNonPersistentPropertyValue(MentEnabledInfo, ref mentEnabled, ZBool.False);
				}
				else
				{
					// Do nothing, we should be readonly.
				}
			}
		}

		public ZPropertyInfo MentEnabledInfo
		{
			get { return GetZPropertyInfo(nameof(MentEnabled)); }
		}

		protected bool MentEnabled_Readonly
		{
			get { return query != null && query.IsInDatabase; }
		}

		ZBool mentEnabled;

		public MENTAgedScoreQuery Query
		{
			get { return query; }
		}

		MENTAgedScoreQuery query;

		public BMComponentAcceptabilityBand Band
		{
			get { return band; }
		}

		readonly BMComponentAcceptabilityBand band;

		void FindAndRegisterPreviousQuery()
		{
			var factory = band.Factory;
			var filter = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, band.PK);

			query = factory.LoadTop1<MENTAgedScoreQuery>(filter);

			if (query != null)
			{
				mentEnabled = true;

				this.RegisterEditableChildObject(query);
				band.RefreshBindingIncludingChildren();
			}
			else
			{
				mentEnabled = false;

				query = CreateQuery(band);
				ChangeQueryAccessability(true, query);
			}
		}

		static void ChangeQueryAccessability(bool inAccessible, MENTAgedScoreQuery query)
		{
			query.ShouldNotBeSavedByFactory = inAccessible;

			if (inAccessible)
			{
				query.Validation.ValidateAll();
				query.SuspendValidation();
			}
			else
			{
				query.ResumeValidation();
			}

			query.SetReadOnlyIncludingChildren(true);
		}

		static MENTAgedScoreQuery CreateQuery(BMComponentAcceptabilityBand band)
		{
			var factory = band.Factory;

			var query = factory.New<MENTAgedScoreQuery>();

			using (var suspender = query.GetValidationSuspender())
			{
				using (query.SuspendSettingHasChanges())
				{
					query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
					SetQueryName(band, query);
					CauseScheduleToBeCreated(query);
				}
			}

			return query;
		}

		static void SetQueryName(BMComponentAcceptabilityBand band, MENTAgedScoreQuery query)
		{
			var queryText = band.BAB_Name.KeepAlphanumericCharacters().SubstringSafe(0, MENTAgedScoreQuerySchema.MAQ_Code.MaxLength).ToString();
			query.MAQ_Code = queryText;
			query.MAQ_AttributeDescription = "FROM_AB:" + queryText;
			query.MAQ_QueryDescription = (NoResString)"Generated MENT code from Acceptability Band: " + band.BAB_Name;
		}

		static void CauseScheduleToBeCreated(MENTAgedScoreQuery query)
		{
			var schedule = query.QuerySchedule;
			using (schedule.SuspendSettingHasChanges())
			{
				schedule.S5_ScheduleDescription = query.MAQ_Code;
			}
		}

		public void UpdateMENTCode()
		{
			SetQueryName(band, query);
		}
	}
}
