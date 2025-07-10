using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CustomFieldWrapper : GenericWrapper
	{
		public CustomFieldWrapper(IAccessBusinessObject bizOWithCustomField, CustomLabelInfo customField, BusinessObjectFactory factory)
			: base(null, factory)
		{
			BizOWithCustomField = bizOWithCustomField;
			CustomField = customField;
		}

		readonly IAccessBusinessObject BizOWithCustomField;
		readonly CustomLabelInfo CustomField;

		public string CustomFieldName
		{
			get { return CustomField != null ? CustomField.LabelName : ""; }
		}

		public MultilingualString Caption
		{
			get { return CustomField != null ? CustomField.Caption : (NoResString)""; }
		}

		public ZString Value
		{
			get { return BizOWithCustomField != null && CustomField != null ? BizOWithCustomField[CustomField.PropertyName].ToString() : ""; }
		}
	}
}
