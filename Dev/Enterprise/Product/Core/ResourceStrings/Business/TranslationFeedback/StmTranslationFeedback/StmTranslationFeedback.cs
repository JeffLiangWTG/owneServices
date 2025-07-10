using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedback : AutoStmTranslationFeedback
	{
		public static StmTranslationFeedback New(BusinessObjectFactory factory, string language, string key, string matchType)
		{
			return New(factory, ResourceStringsFactory.Lookup(Res.DefaultLanguage, key), ResourceStringsFactory.LookupWithLanguageFallback(language, key), matchType);
		}

		public static StmTranslationFeedback New(BusinessObjectFactory factory, string language, string key, string level, string matchType)
		{
			return New(factory, ResourceStringsFactory.Lookup(Res.DefaultLanguage, key), ResourceStringsFactory.LookupWithLanguageFallback(language, key), level, matchType);
		}

		public static StmTranslationFeedback New(BusinessObjectFactory factory, HelpDataString sourceData, HelpDataString targetData, string matchType)
		{
			return New(factory, sourceData, targetData, sourceData.GetLevel(), matchType);
		}

		public static StmTranslationFeedback New(BusinessObjectFactory factory, HelpDataString sourceData, HelpDataString targetData, string level, string matchType)
		{
			StmTranslationFeedback newFeedback = null;
			if (targetData.HD_IsCheckedOut)
			{
				var queryForExisting = new ZDBOnlyQuery(typeof(StmTranslationFeedback));
				queryForExisting.AddToFilter(StmTranslationFeedbackSchema.XT_Status, TranslationFeedbackConfiguration.IsMasterDatabase ? TranslationFeedbackStatusList.Codes.Approved : TranslationFeedbackStatusList.Codes.New);
				queryForExisting.AddToFilter(StmTranslationFeedbackSchema.XT_Language, targetData.HD_Language);
				var resourceSubQuery = new ZDBOnlySubQuery(typeof(StmTranslationFeedbackResource), StmTranslationFeedbackResourceSchema.XQ_XT);
				resourceSubQuery.AddToFilter(StmTranslationFeedbackResourceSchema.XQ_ResourceStringKey, sourceData.HD_Code);
				resourceSubQuery.AddToFilter(StmTranslationFeedbackResourceSchema.XQ_ResourceStringLevel, level);
				queryForExisting.AddSubQuery(resourceSubQuery, JoinCondition.And);
				newFeedback = factory.LoadTop1<StmTranslationFeedback>(queryForExisting);
				if (newFeedback != null)
				{
					newFeedback.NewFromExisting();
				}
			}
			if (newFeedback == null)
			{
				newFeedback = factory.New<StmTranslationFeedback>();
				using (newFeedback.SuspendSettingHasChanges())
				{
					newFeedback.XT_Language = targetData.HD_Language;
					newFeedback.XT_Source = sourceData.GetCaptionAtLevel(level);
					newFeedback.XT_SuggestedTranslation = newFeedback.XT_OriginalTranslation = targetData.GetCaptionAtLevel(level);

					newFeedback.PrimaryContext = factory.New<StmTranslationFeedbackResource>();
					using (newFeedback.PrimaryContext.SuspendSettingHasChanges())
					{
						newFeedback.PrimaryContext.SourceData = sourceData;
						newFeedback.PrimaryContext.TargetData = targetData;
						newFeedback.PrimaryContext.XQ_ResourceStringKey = sourceData.HD_Code;
						newFeedback.PrimaryContext.XQ_ResourceStringLevel = level;
						newFeedback.PrimaryContext.XQ_MatchType = matchType;
						newFeedback.PrimaryContext.Parent = newFeedback;
						newFeedback.PrimaryContext.Update = true;
					}
				}
			}
			return newFeedback;
		}

		internal static StmTranslationFeedback New(BusinessObjectFactory factory, TranslationFeedbackEntry entry)
		{
			var feedback = factory.NewWithPrimaryKey<StmTranslationFeedback>(Guid.Parse(entry.PK));
			feedback.LoadFromTranslationFeedbackEntry(entry);
			return feedback;
		}

		public StmTranslationFeedback(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			XT_EnterpriseCode = registrationKey.EnterpriseCode;
			XT_CompanyCode = Env.CurrentCompany.Code;
			XT_DatabaseCode = registrationKey.ServerCode;
			XT_Status = TranslationFeedbackConfiguration.IsMasterDatabase ? TranslationFeedbackStatusList.Codes.Approved : TranslationFeedbackStatusList.Codes.New;
			XT_ClientStaffInitial = Env.CurrentUser.Initials;
		}

		public StmTranslationFeedbackResource PrimaryContext
		{
			get
			{
				if (primaryContext == null && IsInDatabase)
				{
					var query = new ZQuery(StmTranslationFeedbackResourceSchema.XQ_XT, this.PK);
					query.AddToFilter(StmTranslationFeedbackResourceSchema.XQ_MatchType, new string[] { TranslationFeedbackMatchTypes.Codes.Exact, TranslationFeedbackMatchTypes.Codes.FullText, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, TranslationFeedbackMatchTypes.Codes.RelatedLevel });
					primaryContext = Factory.LoadTop1<StmTranslationFeedbackResource>(query);
				}
				return primaryContext;
			}
			internal set
			{
				primaryContext = value;
			}
		}
		StmTranslationFeedbackResource primaryContext;

		public ZString ResourceStringKey
		{
			get { return PrimaryContext != null ? PrimaryContext.XQ_ResourceStringKey : ZString.Empty; }
		}

		[List("Lookups+ResourceStringLevels")]
		public ZString ResourceStringLevel
		{
			get { return PrimaryContext != null ? PrimaryContext.XQ_ResourceStringLevel : ZString.Empty; }
		}

		[List("Lookups+MatchTypes")]
		public ZString MatchType
		{
			get { return PrimaryContext != null ? PrimaryContext.XQ_MatchType : (ZString)TranslationFeedbackMatchTypes.Codes.None; }
		}

		[ReadOnly(true)]
		public ZString CompanyLicenceCode
		{
			get { return XT_EnterpriseCode + XT_CompanyCode + XT_DatabaseCode; }
			set
			{
				XT_EnterpriseCode = value.SubstringSafe(0, 3);
				XT_CompanyCode = value.SubstringSafe(3, 3);
				XT_DatabaseCode = value.SubstringSafe(6, 3);
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("6f4bd78e-b458-4700-b7ce-480ae8ddfb03", "Translation of {0}|{1}", ResourceStringKey, XT_OriginalTranslation);

		public ZBool CreatedOnMasterDatabase
		{
			get { return XT_EnterpriseCode == TranslationFeedbackMasterInfo.EnterpriseCode && XT_DatabaseCode == TranslationFeedbackMasterInfo.DatabaseCode; }
		}

		public StmTranslationFeedbackResourceCollection AllContexts
		{
			get
			{
				if (allContexts == null)
				{
					allContexts = new StmTranslationFeedbackResourceCollection(this);
					if (IsInDatabase)
					{
						allContexts.AddRange(SavedContexts);
					}
					else
					{
						if (PrimaryContext != null && PrimaryContext.XQ_MatchType != TranslationFeedbackMatchTypes.Codes.FullText)
						{
							allContexts.Add(PrimaryContext);
						}
					}
					foreach (var match in TranslationFeedbackFactory.GetExactMatches(Factory, XT_Language, XT_OriginalTranslation, string.Empty))
					{
						if (match.XT_Application == XT_Application && match.XT_Source == XT_Source && match.PrimaryContext != null)
						{
							var matchQuery = new ZQuery(StmTranslationFeedbackResourceSchema.XQ_ResourceStringKey, match.ResourceStringKey);
							matchQuery.AddToFilter(StmTranslationFeedbackResourceSchema.XQ_ResourceStringLevel, match.ResourceStringLevel);
							if (allContexts.Find(matchQuery).Length == 0)
							{
								using (match.PrimaryContext.SuspendSettingHasChanges())
								{
									if (PrimaryContext != null && (PrimaryContext.XQ_MatchType != TranslationFeedbackMatchTypes.Codes.FullText && PrimaryContext.XQ_MatchType != TranslationFeedbackMatchTypes.Codes.None))
									{
										match.PrimaryContext.Update = false;
									}
									match.PrimaryContext.Parent = this;
									match.PrimaryContext.XQ_MatchType = TranslationFeedbackMatchTypes.Codes.OtherContext;
								}
								allContexts.Add(match.PrimaryContext);
							}
						}
					}

					allContexts.Sort(new Comparison<StmTranslationFeedbackResource>(delegate(StmTranslationFeedbackResource item1, StmTranslationFeedbackResource item2)
						{
							if (item1.Update == item2.Update && item1.TargetData != null && item2.TargetData != null)
							{
								return item1.TargetData.HD_ContextClassName.CompareTo(item2.TargetData.HD_ContextClassName);
							}
							else
							{
								return item2.Update.CompareTo(item1.Update);
							}
						}));
				}
				return allContexts;
			}
		}
		StmTranslationFeedbackResourceCollection allContexts;

		public StmTranslationFeedbackResourceCollection SavedContexts
		{
			get
			{
				if (savedContexts == null)
				{
					savedContexts = new StmTranslationFeedbackResourceCollection(this);
					if (IsInDatabase)
					{
						savedContexts.AddRange(Factory.Load<StmTranslationFeedbackResource>(new ZQuery(StmTranslationFeedbackResourceSchema.XQ_XT, this.PK)));
					}
					foreach (StmTranslationFeedbackResource savedContext in savedContexts)
					{
						using (savedContext.SuspendSettingHasChanges())
						{
							savedContext.Update = true;
						}
					}
				}
				return savedContexts;
			}
		}
		StmTranslationFeedbackResourceCollection savedContexts;

		public TopLevelTranslationFeedbackCollection OtherTranslations
		{
			get
			{
				if (otherTranslations == null)
				{
					var factory = new BusinessObjectFactory();
					otherTranslations = new TopLevelTranslationFeedbackCollection(factory);
					ReloadOtherTranslations();
				}
				return otherTranslations;
			}
		}
		TopLevelTranslationFeedbackCollection otherTranslations;

		public void ReloadOtherTranslations()
		{
			if (otherTranslations != null)
			{
				otherTranslations.RemoveAll();
				foreach (var match in TranslationFeedbackFactory.CacheLookupBySource(otherTranslations.Factory, XT_Language, XT_Source, string.Empty))
				{
					if (match.XT_SuggestedTranslation != XT_SuggestedTranslation)
					{
						otherTranslations.Add(match);
					}
				}
			}
		}

		#region Property Overrides

		[List("Lookups+Languages")]
		[ReadOnly(true)]
		public override ZString XT_Language
		{
			get { return base.XT_Language; }
			set { base.XT_Language = value; }
		}

		[ReadOnly(true)]
		public override ZString XT_Source
		{
			get { return base.XT_Source; }
			set { base.XT_Source = value; }
		}

		[ReadOnly(true)]
		public override ZString XT_OriginalTranslation
		{
			get { return base.XT_OriginalTranslation; }
			set { base.XT_OriginalTranslation = value; }
		}

		public override ZString XT_SuggestedTranslation
		{
			get { return base.XT_SuggestedTranslation; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					base.XT_SuggestedTranslation = XT_Source;
				}
				else
				{
					base.XT_SuggestedTranslation = value.Trim();
				}
				translationDiff = null;
			}
		}

		protected bool XT_SuggestedTranslation_ReadOnly
		{
			get
			{
				return !CanEditSuggestedTranslation;
			}
		}

		protected int XT_SuggestedTranslation_MaxLength
		{
			get
			{
				if (!maxLength.HasValue)
				{
					if (this.IsSettingHasChangesSuspended)
					{
						return int.MaxValue;
					}
					maxLength = AllContexts.Count > 0 ? ((StmTranslationFeedbackResource)AllContexts.MinBy(context => ((StmTranslationFeedbackResource)context).MaxLength)).MaxLength : int.MaxValue;
				}
				return maxLength.Value;
			}
		}
		int? maxLength;

		bool CanEditSuggestedTranslation
		{
			get
			{
				return !IsInDatabase ||
					(TranslationFeedbackConfiguration.IsMasterDatabase && CreatedOnMasterDatabase && XT_Status == TranslationFeedbackStatusList.Codes.Approved) ||
					(XT_Status == TranslationFeedbackStatusList.Codes.New || XT_StatusInfo.HasChanges);
			}
		}

		[List("Lookups+Statuses")]
		public override ZString XT_Status
		{
			get { return base.XT_Status; }
			set
			{
				base.XT_Status = value;
				XT_StatusTime = ZDateTime.UtcNow;
			}
		}

		protected bool XT_Status_ReadOnly
		{
			get { return !TranslationFeedbackConfiguration.IsMasterDatabase; }
		}

		[ReadOnly(true)]
		public override ZString XT_ClientStaffInitial
		{
			get { return base.XT_ClientStaffInitial; }
			set { base.XT_ClientStaffInitial = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime XT_SystemCreateTimeUtc
		{
			get { return base.XT_SystemCreateTimeUtc; }
			set { base.XT_SystemCreateTimeUtc = value; }
		}

		public ZDateTime LocalStatusTime
		{
			get { return XT_StatusTime.ToDateTime().ToLocalTime(); }
		}

		public override void SetXT_ScreenshotSource(IStreamSource source)
		{
			screenshotSource = source;
			base.SetXT_ScreenshotSource(source);
		}
		IStreamSource screenshotSource;

		public override ZBlob XT_Screenshot
		{
			get
			{
				if (screenshotSource != null)
				{
					return ((MemoryStream)screenshotSource.GetStream()).ToArray();
				}
				return base.XT_Screenshot;
			}
			set { base.XT_Screenshot = value; }
		}

		protected bool XT_ReviewComment_ReadOnly
		{
			get { return !TranslationFeedbackConfiguration.IsMasterDatabase; }
		}

		#endregion

		public Diff.Info<string> TranslationDiff
		{
			get
			{
				if (translationDiff == null)
				{
					translationDiff = Diff.Simplify(Diff.CompareByTokens(XT_OriginalTranslation, XT_SuggestedTranslation));
				}
				return translationDiff.Value;
			}
		}
		Diff.Info<string>? translationDiff;

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			translationDiff = null;
			base.OnBeforeUpdatedByDataRefresh();
		}

		public override bool IsInDatabase
		{
			get
			{
				return base.IsInDatabase || isNewFromExisting;
			}
		}

		void NewFromExisting()
		{
			if (!base.IsInDatabase)
			{
				isNewFromExisting = true;
				Reload();
			}
		}
		bool isNewFromExisting;

		#region Actions

		public override bool IsSavedByFactory
		{
			get { return !IsNull && ((IBusiness)this).HasChangesNotIncludingChildren; }
		}

		public override void OnSaving()
		{
			CreateMessage();
			base.OnSaving();
		}

		void CreateMessage()
		{
			if (IsSavedByFactory)
			{
				if (XT_Status == TranslationFeedbackStatusList.Codes.New && !TranslationFeedbackConfiguration.IsMasterDatabase)
				{
					if (!IsInDatabase)
					{
						CreateTranslationFeedbackEntryMessage();
					}
					else if (XT_SuggestedTranslationInfo.HasChanges)
					{
						CreateTranslationFeedbackUpdateMessage();
					}
				}
				else if (XT_Status == TranslationFeedbackStatusList.Codes.Rejected && TranslationFeedbackConfiguration.IsMasterDatabase && !CreatedOnMasterDatabase)
				{
					CreateTranslationFeedbackUpdateMessage();
				}
				else if (XT_Status == TranslationFeedbackStatusList.Codes.Approved && TranslationFeedbackConfiguration.IsMasterDatabase && !CreatedOnMasterDatabase)
				{
					CreateTranslationFeedbackUpdateMessage();
				}
				else if (XT_Status == TranslationFeedbackStatusList.Codes.Canceled && !TranslationFeedbackConfiguration.IsMasterDatabase)
				{
					CreateTranslationFeedbackUpdateMessage();
				}
			}
		}

		void CreateTranslationFeedbackEntryMessage()
		{
			var entry = new TranslationFeedbackEntry();
			entry.PK = PK.ToString();
			entry.CompanyLicenceCode = CompanyLicenceCode;
			entry.ClientStaff = XT_ClientStaffInitial;
			entry.Language = XT_Language;
			entry.Source = XT_Source;
			entry.OriginalTranslation = XT_OriginalTranslation;
			entry.SuggestedTranslation = XT_SuggestedTranslation;
			entry.Comments = XT_Comments;
			entry.Screenshot = XT_Screenshot;
			entry.Application = XT_Application;
			entry.Resource = AllContexts
				.OfType<StmTranslationFeedbackResource>()
				.Where(context => context.Update)
				.Select(context => new TranslationFeedbackEntryResource
				{
					PK = context.PK.ToString(),
					ResourceStringKey = context.XQ_ResourceStringKey,
					ResourceStringLevel = context.XQ_ResourceStringLevel,
					MatchType = context.XQ_MatchType,
				})
				.ToArray();

			var xmlMessageBody = TranslationFeedbackMessagesSerializer.SerializeTranslationFeedbackEntry(entry);
			SystemMessage.CreateInterchange(this.Factory, xmlMessageBody, TranslationFeedbackMasterInfo.CompanyLicence);
		}

		void LoadFromTranslationFeedbackEntry(TranslationFeedbackEntry entry)
		{
			CompanyLicenceCode = entry.CompanyLicenceCode;
			XT_ClientStaffInitial = entry.ClientStaff;
			XT_Language = entry.Language;
			XT_Source = entry.Source;
			XT_OriginalTranslation = entry.OriginalTranslation;
			XT_SuggestedTranslation = entry.SuggestedTranslation;
			XT_Comments = entry.Comments;
			XT_Screenshot = entry.Screenshot;
			XT_Application = entry.Application;

			if (entry.Resource != null)
			{
				foreach (var resource in entry.Resource)
				{
					allContexts = new StmTranslationFeedbackResourceCollection(this);
					var context = Factory.NewWithPrimaryKey<StmTranslationFeedbackResource>(Guid.Parse(resource.PK));
					context.XQ_ResourceStringKey = resource.ResourceStringKey;
					context.XQ_ResourceStringLevel = resource.ResourceStringLevel;
					context.XQ_MatchType = resource.MatchType;
					context.Update = true;
					allContexts.Add(context);
				}
			}

			XT_Status = TranslationFeedbackStatusList.Codes.New;
		}

		void CreateTranslationFeedbackUpdateMessage()
		{
			var update = new TranslationFeedbackUpdate();
			update.PK = PK.ToString();
			update.Status = XT_Status;
			if (TranslationFeedbackConfiguration.IsMasterDatabase && !XT_ReviewComment.IsEmpty)
			{
				update.ReviewComment = XT_ReviewComment;
			}
			if (XT_SuggestedTranslationInfo.HasChanges)
			{
				update.SuggestedTranslation = XT_SuggestedTranslation;
			}

			var xmlMessageBody = TranslationFeedbackMessagesSerializer.SerializeTranslationFeedbackUpdate(update);
			SystemMessage.CreateInterchange(this.Factory, xmlMessageBody, TranslationFeedbackConfiguration.IsMasterDatabase ? (string)CompanyLicenceCode : TranslationFeedbackMasterInfo.CompanyLicence);
		}

		#endregion
	}
}
