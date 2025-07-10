using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class PropertyValueSetter : ValueSetter
	{
		public PropertyValueSetter(BusinessObject bo, ZString propertyName, Func<object> getValue, IXmlImportLogger logger) : base(getValue, logger)
		{
			this.businessObject = Argument.NotNull(bo, "businessObject");
			this.propertyName = Argument.NotNullOrEmpty(propertyName, "propertyName");
		}
		readonly BusinessObject businessObject;
		readonly ZString propertyName;

		public static ZString GetKey(ZGuid pk, ZString propertyName)
		{
			return pk.ToStringKey() + propertyName;
		}

		protected override ZString MatchingKeyCore
		{
			get { return GetKey(businessObject.PK, propertyName); }
		}

		protected override void SetValueCore()
		{
			if (value != null)
			{
				businessObject[propertyName] = value;
			}
		}
	}
}
