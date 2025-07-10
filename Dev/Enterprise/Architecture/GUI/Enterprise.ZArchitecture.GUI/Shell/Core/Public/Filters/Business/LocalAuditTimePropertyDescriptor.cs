using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class LocalAuditTimePropertyDescriptor : KPropertyDescriptor, IGuiOnlyPropertyDescriptor
	{
		public LocalAuditTimePropertyDescriptor(SchemaDateTimeColumn rawDateTimeColumn)
			: this(rawDateTimeColumn.Name)
		{
		}

		public LocalAuditTimePropertyDescriptor(string rawDateTimePropertyName)
			: base(null, GetLocalPropertyName(rawDateTimePropertyName), Array.Empty<Attribute>())
		{
			this.rawDateTimePropertyName = rawDateTimePropertyName;

			var splitterIndex = rawDateTimePropertyName.LastIndexOfAny(new[] { '+', '.' });
			if (splitterIndex >= 0)
			{
				sourcePropertyName = rawDateTimePropertyName.Substring(0, splitterIndex);
			}
			else
			{
				sourcePropertyName = string.Empty;
			}
		}

		static string GetLocalPropertyName(string rawDateTimePropertyName)
		{
			return rawDateTimePropertyName;
		}

		readonly string rawDateTimePropertyName;
		readonly string sourcePropertyName;

		public override Type PropertyType
		{
			get { return typeof(ZDateTime); }
		}

		protected override object GetValueCore(object component)
		{
			var componentAsBizObj = component as BusinessObject;
			if (componentAsBizObj == null)
			{
				return null;
			}

			var value = componentAsBizObj[rawDateTimePropertyName];
			if (value == null)
			{
				return null;
			}

			var rawDateTime = (ZDateTime)value;
			var needToConvertToLocal = rawDateTime.IsValid;
			if (needToConvertToLocal)
			{
				var sourceAutoAdminTarget = (String.IsNullOrWhiteSpace(sourcePropertyName))
					? component as IAutoAdminLogTarget
					: ((BusinessObject)component)[sourcePropertyName] as IAutoAdminLogTarget;
				needToConvertToLocal = (sourceAutoAdminTarget != null);
			}

			return (needToConvertToLocal)
				? TimeFactory.Instance.GetLocalTimeFromUtc(rawDateTime.ToDateTime())
				: rawDateTime;
		}

		protected override void SetValueCore(object component, object value)
		{
			var componentAsBizObj = component as BusinessObject;
			if (componentAsBizObj == null)
			{
				return;
			}

			var dateTimeValue = (ZDateTime)value;
			var needToConvertToUtc = dateTimeValue.IsValid;
			if (needToConvertToUtc)
			{
				var sourceAutoAdminTarget = (String.IsNullOrWhiteSpace(sourcePropertyName))
					? component as IAutoAdminLogTarget
					: ((BusinessObject)component)[sourcePropertyName] as IAutoAdminLogTarget;
				needToConvertToUtc = (sourceAutoAdminTarget != null);
			}

			componentAsBizObj[rawDateTimePropertyName] = (needToConvertToUtc)
				? TimeFactory.Instance.GetUtcFromLocalTime(dateTimeValue.ToDateTime())
				: dateTimeValue;
		}
	}
}
