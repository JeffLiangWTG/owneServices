using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.Business
{
	public class OfficeCodeLookups : EuOfficeCodeLookups
	{
		public OfficeCodeLookups(EuOfficeCode officeCode) : base(officeCode)
		{
		}

		protected new OfficeCode Parent => (OfficeCode)base.Parent;

		public override CustomsOfficeCodeCollection OfficeCodeList
		{
			get
			{
				var rolesList = new List<ZString>();
				if (Parent.Declaration.IsUCC6AndIsExport && Parent.CY_RoleCodes.Count() == 1)
				{
					if (Parent.CY_RoleCodes.FirstOrDefault().Equals(EuOfficeCodesTypes.Codes.OfficeOfPresentation))
					{
						rolesList.Add(EuOfficeCodesTypes.Codes.OfficeOfExport);
					}
					else if (Parent.CY_RoleCodes.FirstOrDefault().Equals(EuOfficeCodesTypes.Codes.OfficeOfExit))
					{
						rolesList.Add(EuOfficeCodesTypes.Codes.OfficeOfExit);
						rolesList.Add(EuOfficeCodesTypes.Codes.OfficeOfExitInland);
					}
				}

				var result = rolesList.Any() ? GetOfficeCodeList(rolesList.ToArray()) : base.OfficeCodeList;

				if (Parent.Declaration.IsImport && Parent.CY_Code == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice && DeclarationConfiguration.HasImportUCC6Functionality())
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.ListType, "Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, false));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)Universal.RefCusCodeListAttributeTypes.Codes.ROLE, false));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.AttributeValue, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, false));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZZRefCusCodeListFilters.AttributeValue, "Property", (ZString)EuOfficeCodesTypes.Codes.SupervisingCustomsOffice, false));
				}

				return result;
			}
		}
	}
}
