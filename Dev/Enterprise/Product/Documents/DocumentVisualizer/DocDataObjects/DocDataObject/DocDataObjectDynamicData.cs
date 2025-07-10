using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	sealed class DocDataObjectDynamicData : DynamicData
	{
		public DocDataObjectDynamicData(object value, Type valueType, IDynamicData parent)
			: base(value, valueType, parent)
		{
			lazyZPropertyInfo = new Lazy<ZPropertyInfo>(GetZPropertyInfo);
		}

		public DocDataObjectDynamicData(object value, Type valueType, IDynamicDataManager manager)
			: base(value, valueType, manager)
		{
			lazyZPropertyInfo = new Lazy<ZPropertyInfo>(GetZPropertyInfo);
		}

		public ZPropertyInfo ZPropertyInfo => lazyZPropertyInfo.Value;

		ZPropertyInfo GetZPropertyInfo()
		{
			if (typeof(IZType).IsAssignableFrom(ValueType)
				&& Parent.Value is BusinessObject parentBizObj)
			{
				var propertyInfo = this.GetMetaData<PropertyInfo>(MetaDataType.DeclaringProperty);

				if (propertyInfo != null)
				{
					var zPropertyInfo = parentBizObj.ZPropertyInfoHash.GetPropertySafe(propertyInfo.Name);

					if (zPropertyInfo != null)
					{
						zPropertyInfo.ValueChanged += (sender, args) =>
						{
							if (!isSettingValueOnBizObj)
							{
								SetValue(lazyZPropertyInfo.Value.Value, ValueOrigin.Unspecified);
							}
						};
					}

					return zPropertyInfo;
				}
			}

			return null;
		}

		readonly Lazy<ZPropertyInfo> lazyZPropertyInfo;

		protected override object GetValueCore() => lazyZPropertyInfo.Value?.Value ?? base.GetValueCore();

		bool isSettingValueOnBizObj;

		protected override Try<object> SetValueCore(object value, ValueOrigin origin)
		{
			var result = base.SetValueCore(value, origin);

			if (!result.IsFaulted
				&& !isSettingValueOnBizObj
				&& (ZPropertyInfo?.HasSetter ?? false)
				&& ZDataType.IsConvertibleToZType(result.Value))
			{
				var zValue = ZDataType.ObjectToZType(result.Value);

				IDisposable GetOnValueChangedSuspender()
				{
					if (origin == ValueOrigin.Override
						|| origin == ValueOrigin.ResetOverride)
					{
						return ZPropertyInfo.SuspendOnValueChanged();
					}

					return null;
				}

				var suspender = GetOnValueChangedSuspender();

				try
				{
					isSettingValueOnBizObj = true;
					ZPropertyInfo.Value = zValue;
				}
				finally
				{
					isSettingValueOnBizObj = false;
					suspender?.Dispose();
				}
			}

			return result;
		}
	}
}
