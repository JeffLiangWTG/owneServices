using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	abstract class BaseShapeTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override void SetPropertyInfoValue_ForXmlColumnPropertyAttributeTest(ZPropertyInfo propertyInfo, IZType value)
		{
			if (propertyInfo.Name == nameof(BMNCNShape.IsPinned))
			{
				var shape = (BMNCNShape)propertyInfo.BizObj;

				using (shape.AllowChangingPinnedStatus_ForTest())
				{
					base.SetPropertyInfoValue_ForXmlColumnPropertyAttributeTest(propertyInfo, value);
				}
			}
			else
			{
				base.SetPropertyInfoValue_ForXmlColumnPropertyAttributeTest(propertyInfo, value);
			}
		}
	}
}
