using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public class SetterValueWithSuspender : IDisposable
	{
		public SetterValueWithSuspender(BusinessObject bizObj, IEnumerable<string> propertyNames)
		{
			this.supporter = bizObj as ISetterSuspenderSupporter;
			temporarilySuspendedFields = new List<IDisposable>();
			if (supporter != null)
			{
				var supportedFields = supporter.SupportedFields.ToHashSet();
				fieldsThatNeedToBeSuspended = propertyNames.Where(x => supportedFields.Contains(x)).ToHashSet();
				if (fieldsThatNeedToBeSuspended.Any())
				{
					setValue = SetValueWithSuspender;
				}
			}

			if (setValue == null)
			{
				setValue = SetValueDirectly;
			}
		}

		public void SetValue(BusinessObject bizObj, string propertyName, object value) => setValue(bizObj, propertyName, value);

		public void Dispose()
		{
			temporarilySuspendedFields.ForEach(x => x.Dispose());
		}

		delegate void SetValueDelegate(BusinessObject bizObj, string propertyName, object value);

		void SetValueDirectly(BusinessObject bizObj, string propertyName, object value)
		{
			if (bizObj[propertyName] == null || !bizObj[propertyName].Equals(value))
			{
				bizObj[propertyName] = value;
			}
		}

		void SetValueWithSuspender(BusinessObject bizObj, string propertyName, object value)
		{
			if (object.ReferenceEquals(supporter, bizObj) && fieldsThatNeedToBeSuspended.Contains(propertyName))
			{
				var setterSuspender = supporter.SetterSuspender;
				// Enable setter in case there is any previous suspended
				using (setterSuspender.ResumeSetting(propertyName))
				{
					SetValueDirectly(bizObj, propertyName, value);
				}
				temporarilySuspendedFields.Add(setterSuspender.SuspendSetting(propertyName));
			}
			else
			{
				SetValueDirectly(bizObj, propertyName, value);
			}
		}

		readonly ISetterSuspenderSupporter supporter;
		readonly List<IDisposable> temporarilySuspendedFields;
		readonly HashSet<string> fieldsThatNeedToBeSuspended;
		readonly SetValueDelegate setValue;
	}
}
