using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class InfoValuePair
	{
		public InfoValuePair(PropertyInfo info, IZType value)
		{
			this.info = info;
			this.value = value;
		}

		public InfoValuePair(string field, string fieldName, IZType value)
		{
			this.field = field;
			this.fieldName = fieldName;
			this.value = value;
		}

		public PropertyInfo Info
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return info; }
		}

		public string Field
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return field; }
		}

		public string FieldName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fieldName; }
		}

		public IZType Value
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return value; }
		}

		public void ApplyToBusinessObject(BusinessObject target)
		{
			using ((target as IRegisterStatusChangeMode)?.TemporarilySetStatusChangeModeToChangedByOperationalAction())
			{
				var propertyInfo = target.FindPropertyInfo(Info.Name);
				var result = Value;

				if (propertyInfo != null)
				{
					var propertyListDataSource = MetaData.GetListDataSource(propertyInfo.BizObj, propertyInfo.PropertyDescriptor);

					// If lookup is a code description pair, is not null, and contains the value to be set
					if (propertyListDataSource is ReadOnlyCodeDescriptionPairList list)
					{
						var index = list.IndexOfCode(Value);
						if (index != -1)
						{
							result = new ZString(list[index].Code);
						}
					}
				}

				Info.SetValue(target, result, null);
			}
		}

		readonly PropertyInfo info;
		readonly string field;
		readonly string fieldName;
		readonly IZType value;
	}
}
