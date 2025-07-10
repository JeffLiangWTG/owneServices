using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RelatedBusinessObjectAttribute : Attribute, IFilteredAttributeForWrappingPropertyDescriptor
	{
		public RelatedBusinessObjectAttribute(string relatedBizObjName)
		{
			this.RelatedBizObjName = relatedBizObjName;
		}

		public string RelatedBizObjName { get; private set; }

		public static string GetRelatedBizObjName(PropertyDescriptor property)
		{
			RelatedBusinessObjectAttribute attr = (RelatedBusinessObjectAttribute)property.Attributes[typeof(RelatedBusinessObjectAttribute)];
			return attr == null ? null : attr.RelatedBizObjName;
		}

		#region GetCodeForGuid

		public static ZString GetCodeForGuid(ZPropertyInfo propertyInfo)
		{
			ZString result = "";

			BusinessObject relatedObject = BusinessObject.GetRelatedBizO(propertyInfo);
			if (relatedObject != null)
			{
				string codeProperty = CodePropertyAttribute.CodePropertyNameFromType(relatedObject.GetType());
				result = (ZString)relatedObject[codeProperty];
			}
			return result;
		}
		#endregion

		#region GetDescriptionForGuid

		public static ZString GetDescriptionForGuid(ZPropertyInfo propertyInfo)
		{
			var relatedObject = BusinessObject.GetRelatedBizO(propertyInfo);

			return relatedObject == null ? ZString.Empty : DescriptionPropertyAttribute.DescriptionFromBusinessObject(relatedObject);
		}
		#endregion

		#region IFilteredAttributeForWrappingPropertyDescriptor Members

		Attribute IFilteredAttributeForWrappingPropertyDescriptor.GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			return new RelatedBusinessObjectAttribute(wrappingProperty.Outer.Name + "+" + RelatedBizObjName);
		}

		#endregion
	}
}
