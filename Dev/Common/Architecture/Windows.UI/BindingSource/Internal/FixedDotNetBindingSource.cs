using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common.Collections;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A System.Windows.Forms.BindingSource with the following fixes / enhancements:
	/// - GetItemProperties works when DataSource is a System.Type object.
	/// - Added protected virtual void NewRelatedBindingSource(string dataMember).
	/// </summary>
	internal class FixedDotNetBindingSource : BindingSource
	{
		public FixedDotNetBindingSource()
		{
		}

		public FixedDotNetBindingSource(object dataSource, string dataMember)
			: base(dataSource, dataMember)
		{
		}

		public override CurrencyManager GetRelatedCurrencyManager(string dataMember)
		{
			CurrencyManager result;
			if (string.IsNullOrEmpty(dataMember) || dataMember.Contains("."))
			{
				result = base.GetRelatedCurrencyManager(dataMember);
			}
			else
			{
				BindingSource relatedSource = GetRelatedBindingSource(dataMember);
				result = relatedSource.CurrencyManager;
			}
			return result;
		}

		public override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			PropertyDescriptorCollection result;
			if (listAccessors == null && DataSource is Type)
			{
				Type type = (Type)DataSource;
				Type elementType = ListUtil.GetListElementType(type) ?? type;
				result = TypeDescriptor.GetProperties(elementType);
			}
			else
			{
				result = base.GetItemProperties(listAccessors);
			}
			return result;
		}

		protected virtual BindingSource NewRelatedBindingSource(string dataMember)
		{ return new FixedDotNetBindingSource(this, dataMember); }

		#region Implementation

		Dictionary<string, BindingSource> relatedBindingSources;

		BindingSource GetRelatedBindingSource(string dataMember)
		{
			if (relatedBindingSources == null)
			{
				relatedBindingSources = new Dictionary<string, BindingSource>();
			}
			foreach (string key in relatedBindingSources.Keys)
			{
				if (string.Equals(key, dataMember, StringComparison.OrdinalIgnoreCase))
				{
					return relatedBindingSources[key];
				}
			}
			BindingSource bindingSource = NewRelatedBindingSource(dataMember);
			relatedBindingSources[dataMember] = bindingSource;
			return bindingSource;
		}

		#endregion
	}
}
