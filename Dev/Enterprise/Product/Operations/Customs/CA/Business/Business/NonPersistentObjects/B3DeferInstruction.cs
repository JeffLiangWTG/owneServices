using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using EnvProxy = Enterprise.ZArchitecture.Environment.EnvProxy;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class B3DeferInstruction : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Const

		const string OttawaUNLOCO = "CAOTT";

		#endregion

		#region Schema

		public static class Schema
		{
			public const string DeferActionCode = "DeferActionCode";
			public const string HeldUntilDateTime = "HeldUntilDateTime";
			public const string ScheduleTimeZone = "ScheduleTimeZone";
		}

		#endregion

		public B3DeferInstruction(JobDeclaration declaration, ZDate k84CutOffDate)
			: base(declaration.Factory)
		{
			this.parent = declaration;
			this.deferActionCode = GetDefaultDeferredB3SendAction();

			if (deferActionCode == DeferredB3SendActionList.Codes.Defer)
			{
				this.scheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.EST;
				this.heldUntilDateTimeUTC = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(OttawaUNLOCO, GetDefaultOTWTimeForNextAccountingPeriod().ToDateTime());
			}

			this.MessageForNotificationLabel = GenerateMessageForNotificationLabel(k84CutOffDate);
			this.ExistingHeldUntilDateTime = declaration.ScheduledB3MessageTime;
		}

		public B3DeferInstruction(JobDeclaration declaration, ZDateTime heldUntilDateTime)
			: base(declaration.Factory)
		{
			this.parent = declaration;
			this.deferActionCode = DeferredB3SendActionList.Codes.Defer;
			this.scheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.EST;
			this.HeldUntilDateTime = heldUntilDateTime;
		}

		public B3DeferInstruction(BusinessObjectFactory factory)
			: base(factory)
		{
			this.deferActionCode = DeferredB3SendActionList.Codes.Now;
		}

		readonly JobDeclaration parent;

		#region Properties

		public readonly ResourceStringData MessageForNotificationLabel;

		#region DeferActionCode

		[BusinessObjectTestExclude]
		[List(nameof(DeferActionCodeList))]
		[MaxLength(3)]
		public ZString DeferActionCode
		{
			get { return deferActionCode; }
			set
			{
				if (value != DeferActionCode)
				{
					SetNonPersistentPropertyValue(DeferActionCodeInfo, ref deferActionCode, value);
					if (value != DeferredB3SendActionList.Codes.Defer)
					{
						ScheduleTimeZone = ZString.Empty;
						HeldUntilDateTime = ZDateTime.Empty;
					}
					else
					{
						ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.EST;
						HeldUntilDateTime = GetDefaultOTWTimeForNextAccountingPeriod();
					}
					if (DeferActionCodeChanged != null)
					{
						DeferActionCodeChanged(value);
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateDeferInstructionCode();
				}
			}
		}
		ZString deferActionCode;

		public ZPropertyInfo DeferActionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DeferActionCode); }
		}

		public event Action<ZString> DeferActionCodeChanged;

		#endregion

		#region HeldUntilDateTime

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(HeldUntilDateTime_ReadOnly))]
		public ZDateTime HeldUntilDateTime
		{
			get { return GetScheduledTimeFromUTC(heldUntilDateTimeUTC); }
			set
			{
				SetNonPersistentPropertyValue(HeldUntilDateTimeInfo, ref heldUntilDateTimeUTC, ConvertScheduledTimeToUTC(value));
				if (!IsValidationSuspended)
				{
					ValidateHeldUntilDateTime();
				}
			}
		}
		internal ZDateTime heldUntilDateTimeUTC;

		public ZPropertyInfo HeldUntilDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.HeldUntilDateTime); }
		}

		bool HeldUntilDateTime_ReadOnly
		{
			get { return this.DeferActionCode != DeferredB3SendActionList.Codes.Defer; }
		}

		#endregion

		#region ScheduleTimeZone

		[BusinessObjectTestExclude]
		[List(nameof(ScheduleTimeZoneList))]
		[ReadOnlyMember(nameof(HeldUntilDateTime_ReadOnly))]
		public ZString ScheduleTimeZone
		{
			get { return scheduleTimeZone; }
			set
			{
				var oldValue = ScheduleTimeZone;
				SetNonPersistentPropertyValue(ScheduleTimeZoneInfo, ref scheduleTimeZone, value);
				this.HeldUntilDateTimeInfo.RefreshBinding();
				if (oldValue != value)
				{
					var test = this.HeldUntilDateTime;
				}
				if (!IsValidationSuspended)
				{
					ValidateScheduleTimeZone();
				}
			}
		}
		ZString scheduleTimeZone;
		public ZPropertyInfo ScheduleTimeZoneInfo
		{
			get { return GetZPropertyInfo(Schema.ScheduleTimeZone); }
		}

		#endregion

		#region ExistingHeldUntilDateTime

		public ZDateTime ExistingHeldUntilDateTime { get; private set; }

		#endregion

		internal string ScheduleDescription
		{
			get
			{
				var result = string.Empty;
				if (this.DeferActionCode == DeferredB3SendActionList.Codes.Now)
				{
					result = DeferredB3SendActionList.Descriptions.Now;
				}
				else
				{
					result = string.Format("{0} {1} {2}", DeferredB3SendActionList.Descriptions.Defer, this.HeldUntilDateTime.ToString(), this.ScheduleTimeZone);
				}

				return result;
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDeferInstructionCode();
			ValidateHeldUntilDateTime();
			ValidateScheduleTimeZone();
		}

		#region ValidateDeferInstructionCode

		public void ValidateDeferInstructionCode()
		{
			if (!IsValidationSuspended)
			{
				DeferActionCodeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(DeferActionCodeInfo);
				ListValidation.ErrorIfInvalidCode(DeferActionCodeInfo);
			}
		}

		#endregion

		#region ValidateHeldUntilDateTime

		public void ValidateHeldUntilDateTime()
		{
			if (!IsValidationSuspended)
			{
				HeldUntilDateTimeInfo.ClearAllNotifications();
				var targetInfo = HeldUntilDateTimeInfo;
				TypeValidation.CheckValidSmallDateTime(targetInfo);
				if (!HeldUntilDateTime_ReadOnly)
				{
					MandatoryValidation.CheckEntered(targetInfo);
					if (heldUntilDateTimeUTC <= ZDateTime.UtcNow)
					{
						targetInfo.AddError(ResString.GetMultilingualString("CCE8BA93-6EE9-43DC-8323-FDAD21072BA2", "The past date is not allowed for scheduling, please enter future date."));
					}
				}
			}
		}

		#endregion

		#region ValidateScheduleTimeZone

		public void ValidateScheduleTimeZone()
		{
			if (!IsValidationSuspended)
			{
				ScheduleTimeZoneInfo.ClearAllNotifications();
				if (!HeldUntilDateTime_ReadOnly)
				{
					MandatoryValidation.CheckEntered(ScheduleTimeZoneInfo);
					ListValidation.ErrorIfInvalidCode(ScheduleTimeZoneInfo);
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		public ICodeDescriptionPairList DeferActionCodeList
		{
			get
			{
				if (parent.HasScheduledB3Message)
				{
					return Factory.GetCachedValue<DeferredB3SendActionListWithCancel>();
				}
				else
				{
					return Factory.GetCachedValue<DeferredB3SendActionList>();
				}
			}
		}

		public ICodeDescriptionPairList ScheduleTimeZoneList
		{
			get { return Factory.GetCachedValue<B3ScheduleTimeZoneQualifierList>(); }
		}

		#endregion

		#region Implementation

		ZDateTime GetDefaultOTWTimeForNextAccountingPeriod()
		{
			var cutOffDate = parent.K84CutOffDate;
			return cutOffDate.AddDays(25 - cutOffDate.Day).AddHours(4);
		}

		ResourceStringData GenerateMessageForNotificationLabel(ZDate k84CutOffDate)
		{
			ResourceStringData resultResData = null;
			var authDate = parent.JE_EntryAuthorisationDate;
			var dueDate = parent.EstimatedPaymentDueDate.ToShortDateString();
			var cutoffDate = k84CutOffDate.ToShortDateString();

			if (parent.IsLVS)
			{
				resultResData = Res.GetData("FE4591B2-3D60-42E4-92D7-C5C355228C62",
					@"The due date for accounting of this consolidated LVS job appears to fall after the Monthly Statement cut-off date.
Do you want to delay sending the Entry message?

LVS Period:             {0}
Entry Due Date:            {1}
Statement cut-off Date: {2}

If you choose to generate Entry message and schedule the transmission for a future date then the Entry message will be generated now, using the current status of the declaration, and scheduled for sending to Customs at the start of the next accounting period (you may override this scheduled date). Note that if subsequent changes are made in this declaration that would affect the data to be sent to Customs, then the new message will have to be generated prior to the submission date.")
					.Format(authDate.ToString("MMM-yy"), dueDate, cutoffDate);
			}
			else
			{
				if (parent.IsLowValueNormalReleaseJob)
				{
					resultResData = Res.GetData("2256B743-9985-4B98-A156-AB5B8BE65448",
@"The due date for accounting of this Low Value job appears to fall after the Monthly Statement cut-off date.
Do you want to delay sending the Entry message?

Release Date:           {0}
Entry Due Date:            {1}
Statement cut-off Date: {2}

If you choose to generate Entry message and schedule the transmission for a future date then the Entry message will be generated now, using the current status of the declaration, and scheduled for sending to Customs at the start of the next accounting period (you may override this scheduled date). Note that if subsequent changes are made in this declaration that would affect the data to be sent to Customs, then the new message will have to be generated prior to the submission date.")
					.Format(authDate.ToShortDateString(), dueDate, cutoffDate);
				}
				else
				{
					resultResData = Res.GetData("A441489E-BC00-4955-920D-7B1C30474850",
										@"The due date for accounting of this job appears to fall after the Monthly Statement cut-off date.
Do you want to delay sending the Entry message?

Release Date:           {0}
Entry Due Date:            {1}
Statement cut-off Date: {2}

If you choose to generate Entry message and schedule the transmission for a future date then the Entry message will be generated now, using the current status of the declaration, and scheduled for sending to Customs at the start of the next accounting period (you may override this scheduled date). Note that if subsequent changes are made in this declaration that would affect the data to be sent to Customs, then the new message will have to be generated prior to the submission date.")
						.Format(authDate.ToShortDateString(), dueDate, cutoffDate);
				}
			}

			return resultResData;
		}

		string GetDefaultDeferredB3SendAction()
		{
			string result = null;
			var isLowValue = parent.IsLVS;
			var importerAddInfo = parent.ImporterAddInfo;
			if (importerAddInfo != null)
			{
				result = isLowValue ? importerAddInfo.ZO_DeferredLowValueB3SendAction : importerAddInfo.ZO_DeferredNormalB3SendAction;
			}

			if (!DeferredB3SendActionList.IsValidCode(result))
			{
				result = isLowValue
					? CACustomsDataRegistry.Instance.DefaultDeferredLowValueEntrySendAction.GetFallBackValueAtAllLevels(parent.CompanyPK.ToGuid(), parent.JE_GB.ToGuid(), Guid.Empty)
					: CACustomsDataRegistry.Instance.DefaultDeferredNormalEntrySendAction.GetFallBackValueAtAllLevels(parent.CompanyPK.ToGuid(), parent.JE_GB.ToGuid(), Guid.Empty);
			}

			return DeferredB3SendActionList.IsValidCode(result) ? result : DeferredB3SendActionList.Codes.Defer;
		}

		#region TimeZoneCalculation

		ZDateTime ConvertScheduledTimeToUTC(ZDateTime value)
		{
			var result = value;
			if (value.IsValid)
			{
				var sourceDateTime = value.ToDateTime();
				switch (this.ScheduleTimeZone)
				{
					case B3ScheduleTimeZoneQualifierList.Codes.Local:
						result = EnvProxy.Instance.Time.GetUtcFromLocalTime(sourceDateTime);
						break;
					case B3ScheduleTimeZoneQualifierList.Codes.EST:
						result = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(OttawaUNLOCO, sourceDateTime);
						break;
				}
			}
			return result;
		}

		internal ZDateTime GetScheduledTimeFromUTC(ZDateTime valueUTC)
		{
			var result = valueUTC;
			if (valueUTC.IsValid && !valueUTC.IsEmpty)
			{
				var sourceDateTime = valueUTC.ToDateTime();
				switch (this.ScheduleTimeZone)
				{
					case B3ScheduleTimeZoneQualifierList.Codes.Local:
						result = EnvProxy.Instance.Time.GetLocalTimeFromUtc(sourceDateTime);
						break;
					case B3ScheduleTimeZoneQualifierList.Codes.EST:
						result = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc(OttawaUNLOCO, sourceDateTime);
						break;
				}
			}
			return result;
		}

		#endregion

		#endregion
	}
}
