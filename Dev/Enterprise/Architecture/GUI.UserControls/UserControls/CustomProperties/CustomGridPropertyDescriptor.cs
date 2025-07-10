using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	internal class CustomGridPropertyDescriptor : KPropertyDescriptor, IZPropertyInfoRetriever
	{
		#region Constructor

		public CustomGridPropertyDescriptor(Func<BusinessObject, CustomBusinessObject> customBizObjGetter, string customPropertyName, Type propertyType)
			: base(null, customPropertyName, Array.Empty<Attribute>())
		{
			this.customBizObjGetter = customBizObjGetter;
			this.customPropertyName = customPropertyName;
			this.propertyType = propertyType;
		}

		readonly Func<BusinessObject, CustomBusinessObject> customBizObjGetter;
		readonly string customPropertyName;

		#endregion

		#region PropertyType

		public override Type PropertyType
		{
			get { return propertyType; }
		}
		readonly Type propertyType;

		#endregion

		#region Set/Get

		protected override object GetValueCore(object component)
		{
			var bizObj = (BusinessObject)component;
			var customBizObj = customBizObjGetter(bizObj);
			return customBizObj[customPropertyName];
		}

		protected override void SetValueCore(object component, object value)
		{
			var bizObj = (BusinessObject)component;
			var customizObj = customBizObjGetter(bizObj);
			customizObj[customPropertyName] = value;
		}

		#endregion

		#region Converter

		public override TypeConverter Converter
		{
			get { return typeConverter ?? (typeConverter = GetNewConverter()); }
		}
		TypeConverter typeConverter;

		TypeConverter GetNewConverter()
		{
			var converterType = propertyType.Assembly.GetType(propertyType.FullName + "TypeConverter", false);
			if (converterType != null)
			{
				return Activator.CreateInstance(converterType) as TypeConverter;
			}

			return null;
		}

		#endregion

		#region IZPropertyInfoRetriever Members

		public ZPropertyInfo GetZPropertyInfo(BusinessObject businessObject)
		{
			var customBizObj = customBizObjGetter(businessObject);
			return customBizObj.ZPropertyInfoHash[customPropertyName];
		}

		#endregion
	}
}
