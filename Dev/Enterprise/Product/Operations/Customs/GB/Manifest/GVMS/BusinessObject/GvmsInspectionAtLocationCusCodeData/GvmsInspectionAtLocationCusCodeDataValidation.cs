using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsInspectionAtLocationCusCodeDataValidation : CusCodeDataValidation
	{
		public GvmsInspectionAtLocationCusCodeDataValidation(GvmsInspectionAtLocationCusCodeData parent)
			: base(parent) { }

		protected override void CheckCY_Code()
		{
			if (Parent.CY_Code.IsEmpty)
			{
				Parent.CY_CodeInfo.AddMessageError("CY_Code cannot be empty");
			}
			else if (!((GvmsInspectionAtLocationCusCodeData)Parent).Lookups.TypeList.GetAllCodes().Contains(Parent.CY_Code.ToString()))
			{
				Parent.CY_CodeInfo.AddMessageError("Invalid value for CY_Code");
			}
		}

		protected override void CheckCY_Data()
		{
			if (Parent.CY_Data.IsEmpty)
			{
				Parent.CY_DataInfo.AddMessageError("CY_Data cannot be empty");
			}
			else if (!((GvmsInspectionAtLocationCusCodeData)Parent).Lookups.InspectionLocationList.GetAllCodes().Contains(Parent.CY_Data.ToString()))
			{
				Parent.CY_DataInfo.AddMessageError("Invalid value for CY_Data");
			}
		}
	}
}
