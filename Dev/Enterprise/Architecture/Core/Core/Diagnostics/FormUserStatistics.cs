using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class FormUserStatistics
	{
		#region Key Presses / Mouse Clicks / Control Changes

		public int KeyPresses
		{
			get { return keyPresses; }
			set
			{
				UpdateInactiveDurationOnActivedForm();
				keyPresses = value;
			}
		}
		int keyPresses;

		public int MouseClicks
		{
			get { return mouseClicks; }
			set
			{
				UpdateInactiveDurationOnActivedForm();
				mouseClicks = value;
			}
		}
		int mouseClicks;

		public int ControlFocusChanges
		{
			get { return controlFocusChanges; }
			set
			{
				UpdateInactiveDurationOnActivedForm();
				controlFocusChanges = value;
			}
		}
		int controlFocusChanges;

		#endregion

		#region Open / Close Time

		[SuppressMessage("Microsoft.Maintainability", "CA1500", Justification = "Parameters are used to set local variables")]
		public void NotifyFormShownUtc(string formCaption, string moduleName, DateTime shownDateTimeUtc)
		{
			this.shownDateTimeUtc = shownDateTimeUtc;
			this.formCaption = formCaption;
			this.moduleName = moduleName;
		}

		public void NotifyFormClosed(Guid pK, string tableCode)
		{
			closeDateTimeUtc = EnvProxy.Instance.Time.CurrentUtcDateTime;
			businessObjectPK = pK;
			businessObjectTableCode = tableCode;
			CalculateFormInactiveTime();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1500", Justification = "Parameters are used to set local variables")]
		public void NotifyFormClosedUtc(DateTime closeDateTimeUtc, TimeSpan inactiveDuration, int controlFocusChanges, int mouseClicks, int keyPresses)
		{
			this.closeDateTimeUtc = closeDateTimeUtc;
			this.inactiveDuration = inactiveDuration;
			this.controlFocusChanges = controlFocusChanges;
			this.mouseClicks = mouseClicks;
			this.keyPresses = keyPresses;
			CalculateFormInactiveTime();
		}

		public DateTime ShownDateTimeUtc
		{
			get { return shownDateTimeUtc; }
		}
		DateTime shownDateTimeUtc;

		public bool IsClosed
		{
			get { return CloseDateTimeUtc != DateTime.MinValue; }
		}

		public DateTime CloseDateTimeUtc
		{
			get { return closeDateTimeUtc; }
		}
		DateTime closeDateTimeUtc;

		#endregion

		#region Active / Inactive Duration

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		readonly int minutesForInactive = 1;
		DateTime lastActivityTimeOnActivedForm;
		DateTime lastTimeFormWasActive = DateTime.MinValue;

#if DEBUG
		public virtual
#endif
		DateTime GetCurrentUtcTime()
		{
			return DateTime.UtcNow;
		}

		void UpdateInactiveDurationOnActivedForm()
		{
			var currentTime = GetCurrentUtcTime();
			if (lastActivityTimeOnActivedForm != DateTime.MinValue && lastActivityTimeOnActivedForm < currentTime.AddMinutes(-minutesForInactive)) // last activity time less than a minute before now
			{
				var thisInactiveTime = currentTime - lastActivityTimeOnActivedForm;
				inactiveDuration = inactiveDuration.Add(thisInactiveTime);
			}
			lastActivityTimeOnActivedForm = currentTime;
		}

		public void NotifyFormDeactivate()
		{
			lastActivityTimeOnActivedForm = DateTime.MinValue;
			lastTimeFormWasActive = GetCurrentUtcTime();
		}

		public void NotifyFormActivate()
		{
			CalculateFormInactiveTime();
		}

		void CalculateFormInactiveTime()
		{
			if (lastTimeFormWasActive != DateTime.MinValue)
			{
				var thisInactiveTime = GetCurrentUtcTime() - lastTimeFormWasActive;
				inactiveDuration = inactiveDuration.Add(thisInactiveTime);
				lastTimeFormWasActive = DateTime.MinValue;
			}
		}

		public TimeSpan ActiveDuration
		{
			get
			{
				var result = CloseDateTimeUtc - ShownDateTimeUtc;
				result = result.Subtract(inactiveDuration);
				return new TimeSpan(Math.Max(result.Ticks, 0));
			}
		}

		public TimeSpan InactiveDuration
		{
			get { return inactiveDuration; }
		}
		TimeSpan inactiveDuration;

		#endregion

		#region Form and Business Object Details

		public string ModuleName
		{
			get { return moduleName; }
		}
		string moduleName;

		public string FormCaption
		{
			get { return formCaption; }
		}
		string formCaption;

		public Guid BusinessObjectPK
		{
			get { return businessObjectPK; }
		}
		Guid businessObjectPK;

		public string BusinessObjectTableCode
		{
			get { return businessObjectTableCode; }
		}
		string businessObjectTableCode;

		public bool IsExternalProcess
		{
			get { return isExternalProcess; }
			set { isExternalProcess = value; }
		}
		bool isExternalProcess;

		#endregion
	}
}
