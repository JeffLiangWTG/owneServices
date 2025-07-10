using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusInBondMoveHeaderLightValidationTester : LightValidationTester
	{
		public CusInBondMoveHeaderLightValidationTester(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			var propertyName = info.Name;
			return propertyName != "CE_ExpiryDate" && propertyName != "CE_Category" && propertyName != "CE_EntryNum" && propertyName != "CE_EntryType" && propertyName != "CE_IssueDate" && propertyName != "CE_ParentID" && propertyName != "CE_ParentTable" && propertyName != "CE_RN_NKCountryCode" && base.ShouldTestProperty(info);
		}
	}
}
