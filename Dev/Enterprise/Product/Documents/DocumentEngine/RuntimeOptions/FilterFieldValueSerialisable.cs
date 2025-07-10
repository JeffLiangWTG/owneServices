using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class FilterFieldValueSerialisable : FilterFieldWithUTSupport
	{
		protected FilterFieldValueSerialisable(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected FilterFieldValueSerialisable(BaseFieldJsonData data) : base(data)
		{
		}

		public string ValueAsStringForSerialisation
		{
			get { return ValueAsStringForSerialisationInternal; }
			set { ValueAsStringForSerialisationInternal = value; }
		}

		protected abstract string ValueAsStringForSerialisationInternal { get; set; }

		public void SetValueFromXML(string valueAsString)
		{
			using (SuspendSettingHasChanges())
			{
				ValueAsStringForSerialisation = valueAsString;
			}
			fHasSerialisableValueChanged = true;
		}

		public bool HasSerialisableValueChanged
		{
			get { return fHasSerialisableValueChanged; }
		}
		bool fHasSerialisableValueChanged;

		public void UpdateValueIfNotOverriddenByUser(Report reportContainingDataSource)
		{
			LoadValueIfNotOverriddenByUser(reportContainingDataSource);
		}

		internal IDisposable DoNotChangeValueAsStringForSerialisation()
			=> new DisposableAction(() => tempValueAsStringForSerialisation = ValueAsStringForSerialisation, () =>
			{
				using (SuspendSettingHasChanges())
				{
					ValueAsStringForSerialisation = tempValueAsStringForSerialisation;
				}
			});
		string tempValueAsStringForSerialisation;

		internal void EvaluateValueForDisableFixedValueCacheReport(Report reportContainingDataSource)
		{
			using (SuspendSettingHasChanges())
			{
				ValueAsStringForSerialisation = RegexProvider.InnermostMacrosRegex.Replace(DefaultExpression, reportContainingDataSource.ReplaceSingleMacroNotInTemplateBody);
				lastTriggeredByDisableFixedValueCacheReport = true;
			}
		}

		// One class overrrides to never Load a default value. BG: Don't know why. Investigate later.
		protected virtual void LoadValueIfNotOverriddenByUser(Report reportContainingDataSource)
		{
			if (!HasSerialisableValueChanged || lastTriggeredByDisableFixedValueCacheReport)
			{
				using (SuspendSettingHasChanges())
				using (ReplacingDefaultExpression())
				{
					ValueAsStringForSerialisation = RegexProvider.InnermostMacrosRegex.Replace(DefaultExpression, new MatchEvaluator(reportContainingDataSource.ReplaceSingleMacroNotInTemplateBody));
					fHasSerialisableValueChanged = false;
					lastTriggeredByDisableFixedValueCacheReport = false;
				}
			}
		}

		bool lastTriggeredByDisableFixedValueCacheReport;

		protected void SetHasChangesBecauseTheValueHasBeenSetByTheUser()
		{
			fHasSerialisableValueChanged = true;
			HasChanges = true;
		}

		bool isReplacingDefaultExpression;
		IDisposable ReplacingDefaultExpression()
		{
			isReplacingDefaultExpression = true;
			return new DisposableAction(() => isReplacingDefaultExpression = false);
		}

		protected override object GetReplacement(string macro, Report report)
		{
			var selfMacro = "<" + DisplayName + ">";
			if (isReplacingDefaultExpression && selfMacro.Equals(macro, StringComparison.OrdinalIgnoreCase))
			{
				return ValueAsObject;
			}

			return base.GetReplacement(macro, report);
		}

		#region Test stuff
#if DEBUG

		internal void ResetHasChangesForTesting()
		{
			fHasSerialisableValueChanged = false;
			HasChanges = false;
		}

#endif
		#endregion
	}
}
