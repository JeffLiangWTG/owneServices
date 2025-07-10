using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CustomsOfficeValidation : CusCodeDataValidation
	{
		public CustomsOfficeValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected new CustomsOffice Parent => (CustomsOffice)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.Declaration;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			if (Parent.Declaration?.CIQRequires ?? false)
			{
				Parent.CY_DataInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}

			CheckCustomsOfficeIsValid(Parent.CY_DataInfo, Parent.Declaration?.DateOfValuation ?? ZDateTime.Today, ValidationModeProvider);
		}

		public static void CheckCustomsOfficeIsValid(ZPropertyInfo propertyInfo, ZDateTime date, IValidationModeProvider validationModeProvider)
		{
			var customsOffice = (ZString)propertyInfo.Value;
			if (!customsOffice.IsEmpty)
			{
				if (CNRefCusCodeListLoader.GetCustomsOffice(propertyInfo.BizObj.Factory, customsOffice, date) == null)
				{
					propertyInfo.AddNotification(ListValidation.InvalidCodeMessageError.ToString(), validationModeProvider);
				}
				else if (customsOffice.EndsWith("00", StringComparison.OrdinalIgnoreCase))
				{
					propertyInfo.AddNotification(Res.GetString("c252b7bd-7f8c-4634-b84b-d5955aa03e98", "The Customs Office should not be a directly competent Customs (ends with 00)"), validationModeProvider);
				}
			}
		}
	}
}
