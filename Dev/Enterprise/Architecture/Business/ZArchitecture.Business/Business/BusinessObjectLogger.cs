using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class BusinessObjectLoggerOptions
	{
		public BusinessObjectLoggerOptions()
			: this("", true, Events.RecordAudited)
		{
		}

		/// <param name="prefixToReference">StmALog.SL_Reference will be prefixed with this value</param>
		/// <param name="lastLogPropertySetPrefix">
		/// If not null, xxxLastLogDate/xxxLastLogUser/xxxLastLogReference on the main BusinessObject will be updated,
		/// where xxx is the provided prefix.
		/// </param>
		public BusinessObjectLoggerOptions(string prefixToReference, string lastLogPropertySetPrefix, Event logEvent)
			: this(prefixToReference, false, logEvent)
		{
			this.LastLogPropertySetPrefix = lastLogPropertySetPrefix;
		}

		/// <param name="prefixToReference">StmALog.SL_Reference will be prefixed with this value</param>
		/// <param name="changeMainAuditFieldsOnBusinessObject">If true, XX_LastAuditedDate/XX_LastAuditedUser on the main BusinessObject will be updated</param>
		public BusinessObjectLoggerOptions(string prefixToReference, bool changeMainAuditFieldsOnBusinessObject, Event logEvent)
		{
			this.PrefixToReference = prefixToReference;
			this.ChangeMainAuditFieldsOnBusinessObject = changeMainAuditFieldsOnBusinessObject;
			this.LogEvent = logEvent;
		}

		public string PrefixToReference
		{
			get;
			private set;
		}

		public Event LogEvent
		{
			get;
			private set;
		}

		public bool ChangeMainAuditFieldsOnBusinessObject
		{
			get;
			private set;
		}

		public string LastLogPropertySetPrefix
		{
			get;
			private set;
		}

		public bool DisallowDuplicateWriteToLog
		{
			get;
			private set;
		}
	}

	public class BusinessObjectLogger : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string Reference = "Reference";
		}

		#endregion

		public BusinessObjectLogger(IStmALogParent revisedBusinessObject)
			: this(revisedBusinessObject, (BusinessObject)revisedBusinessObject)
		{
		}

		public BusinessObjectLogger(IStmALogParent topLevelBusinessObject, BusinessObject revisedBusinessObject)
			: this(topLevelBusinessObject, revisedBusinessObject, new BusinessObjectLoggerOptions())
		{
		}

		public BusinessObjectLogger(IStmALogParent topLevelBusinessObject, BusinessObject revisedBusinessObject, BusinessObjectLoggerOptions loggerOptions)
			: base(((BusinessObject)topLevelBusinessObject).Factory)
		{
			this.topLevelBusinessObject = topLevelBusinessObject;
			this.revisedBusinessObject = revisedBusinessObject;

			this.loggerOptions = loggerOptions;

			if (loggerOptions.PrefixToReference.Length > 3)
			{
				ErrorReporter.ReportOnce("PrefixToReference is too long", "PrefixToReference is too long");
			}
		}

		public bool WriteToLog()
		{
			return WriteToLogCore();
		}

		protected virtual bool WriteToLogCore()
		{
			if (TopLevelBusinessObjectHasChanges)
			{
				throw new InvalidOperationException("The topLevelBusinessObject has changes. Writing to Log aborted.");
			}

			if (RevisedBusinessObjectHasChanges)
			{
				throw new InvalidOperationException("The revisedBusinessObject has changes. Writing to Log aborted.");
			}

			bool result = true;
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EnterpriseBusinessObject cleanRevisedBusinessObject = (EnterpriseBusinessObject)newFactory.ImportFromAnotherFactory(revisedBusinessObject);

			using (cleanRevisedBusinessObject.Logs.SuspendCreateAutoAdminLog())
			{
				string formattedPrefix = string.IsNullOrEmpty(loggerOptions.PrefixToReference) ? "" : PrefixIndicator + loggerOptions.PrefixToReference;

				if (formattedPrefix.Length > 0 && Reference.Length > 0)
				{
					formattedPrefix += ":";
				}

				cleanRevisedBusinessObject.Logs.AddNew(loggerOptions.LogEvent, formattedPrefix + Reference);

				ChangeMainAuditFieldsOnBusinessObjectIfNecessary(cleanRevisedBusinessObject);
				ChangeLastLogPropertiesOnBusinessObjectIfApplicable(cleanRevisedBusinessObject, Reference);

				try
				{
					newFactory.Save();
				}
				catch (ZSaveConcurrencyException)
				{
					result = false;
				}
			}

			if (result && topLevelBusinessObject == revisedBusinessObject)
			{
				((ILogsInternals)topLevelBusinessObject.Logs).ReloadFromDB();
			}
			return result;
		}

		public const string PrefixIndicator = "~";

		void ChangeMainAuditFieldsOnBusinessObjectIfNecessary(EnterpriseBusinessObject cleanRevisedBusinessObject)
		{
			if (loggerOptions.ChangeMainAuditFieldsOnBusinessObject)
			{
				ZString tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(revisedBusinessObject.TableName);

				if (!tablePrefix.IsEmpty)
				{
					string lastAuditedDatePropertyName = tablePrefix + "_LastAuditedDate";
					cleanRevisedBusinessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(
						lastAuditedDatePropertyName,
						ZDateTime.Now,
						false);

					string lastAuditedUserPropertyName = tablePrefix + "_LastAuditedUser";
					cleanRevisedBusinessObject.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(
						lastAuditedUserPropertyName,
						StaticCurrentFetcher.Instance.CurrentUserCode,
						false);
				}
			}
		}

		void ChangeLastLogPropertiesOnBusinessObjectIfApplicable(EnterpriseBusinessObject cleanRevisedBizObj, ZString reference)
		{
			if (loggerOptions.LastLogPropertySetPrefix != null)
			{
				cleanRevisedBizObj.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(
					loggerOptions.LastLogPropertySetPrefix + LastLogDatePropertySuffix,
					ZDateTime.Now,
					false);

				cleanRevisedBizObj.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(
					loggerOptions.LastLogPropertySetPrefix + LastLogUserPropertySuffix,
					StaticCurrentFetcher.Instance.CurrentUserCode,
					false);

				cleanRevisedBizObj.SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(
					loggerOptions.LastLogPropertySetPrefix + LastLogReferencePropertySuffix,
					reference,
					false);
			}
		}

		public const string LastLogDatePropertySuffix = "LastLogDate";
		const string LastLogUserPropertySuffix = "LastLogUser";
		const string LastLogReferencePropertySuffix = "LastLogReference";

		public bool TopLevelBusinessObjectHasChanges
		{
			get { return BusinessObjectHasChanges((BusinessObject)topLevelBusinessObject); }
		}

		public bool RevisedBusinessObjectHasChanges
		{
			get { return BusinessObjectHasChanges(revisedBusinessObject); }
		}

		bool BusinessObjectHasChanges(BusinessObject businessObject)
		{
			return (businessObject.HasChanges || !businessObject.IsInDatabase);
		}

		readonly IStmALogParent topLevelBusinessObject;
		readonly BusinessObject revisedBusinessObject;
		readonly BusinessObjectLoggerOptions loggerOptions;

		#region Reference

		public ZString Reference
		{
			get { return reference; }
			set
			{
				CheckMaximumLength(ReferenceInfo, value);
				SetNonPersistentPropertyValue(ReferenceInfo, ref reference, value);
				ValidateReference();
			}
		}

		public ZPropertyInfo ReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.Reference); }
		}

		ZString reference;

		public int Reference_MaxLength
		{
			get
			{
				if (!referenceMaxLength.HasValue)
				{
					referenceMaxLength = StmALog.Schema.SL_ReferenceMaxLength - 5;
					if (loggerOptions.LogEvent == Events.RecordAudited && loggerOptions.LastLogPropertySetPrefix == null && revisedBusinessObject is IAuditColumnProviderForBOLogger auditColumnProviderForBOLogger)
					{
						referenceMaxLength = auditColumnProviderForBOLogger.AuditReferenceMaxLength;
					}
					else
					{
						var info = revisedBusinessObject?.ZPropertyInfoHash?.GetPropertySafe(loggerOptions.LastLogPropertySetPrefix + LastLogReferencePropertySuffix);
						if (info != null)
						{
							var maxLength = info.MaxLength;
							if (maxLength > 0 && maxLength < referenceMaxLength.Value)
							{
								referenceMaxLength = maxLength;
							}
						}
					}
				}
				return referenceMaxLength.Value;
			}
		}
		int? referenceMaxLength;

		void ValidateReference()
		{
			ReferenceInfo.ClearAllNotifications();

			if (Reference.StartsWith(PrefixIndicator))
			{
				ReferenceInfo.AddError(EnteredReferenceStartingWithSystemFormat);
			}
		}

		public static string EnteredReferenceStartingWithSystemFormat
		{
			get { return Res.GetString("82032037-ecf3-4ca4-9a50-a08639eb8359", "You cannot enter a reference starting with a tilde(~) as it is an indicative format to system. Please remove it."); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReference();
		}

		#endregion
	}
}
