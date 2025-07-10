using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CIQProductQualificationValidation : CusSupportingInfoValidation
	{
		public CIQProductQualificationValidation(CIQProductQualification parent) : base(parent)
		{
		}

		protected new CIQProductQualification Parent => (CIQProductQualification)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			var targetInfo = Parent.CSI_CodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			targetInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

			var invoiceLine = Parent.Parent;
			var productQualificationCode = Parent.CSI_Code;
			if (invoiceLine != null)
			{
				var declaration = invoiceLine.Declaration;
				var allPQs = invoiceLine.CIQProductQualifications.Cast<CIQProductQualification>().ToArray();
				if (allPQs.Any(pq => pq.CSI_Code == productQualificationCode && pq != Parent))
				{
					targetInfo.AddWarning(Res.GetString("C1AF37CE-E0BD-47E8-968A-97CBA5614966", "The Product Qualification Type has been duplicated."));
				}
				else
				{
					if (declaration != null && declaration.IsImport && Parent.SupportsVIN)
					{
						if (allPQs.Any(pq => pq.SupportsVIN && pq != Parent))
						{
							targetInfo.AddNotification(Res.GetString("F30CC507-B39A-42FE-AC45-51CCBE9750DE", "Only one of 408/409/603 should be entered."), ValidationModeProvider);
						}
						if (!invoiceLine.VINDataCollection.Any())
						{
							targetInfo.AddNotification(Res.GetString("B03F0FC9-5C0B-474B-9C28-038AF3156CC9", "VIN data is required for the selected Product Qualification Type."), ValidationModeProvider);
						}
					}

					if (invoiceLine.CIQRequires)
					{
						var requiredCargoAttributes = GetRequiredCargoAttributes(productQualificationCode);
						if ((requiredCargoAttributes?.Any() ?? false) && !invoiceLine.CargoAttributes.IsProvidedAny(requiredCargoAttributes))
						{
							targetInfo.AddWarning(Res.GetString("2D0110B9-BE1C-4D6F-8E48-F49FE8A5AC0D", "Cargo Attribute {0} needs to be selected for this Product Qualification.", string.Join(",", requiredCargoAttributes)));
						}
					}
				}

				if (declaration != null && declaration.IsTwoStepDeclaration && invoiceLine.CIQRequires)
				{
					if (!ProductQualificationCodeList.IsProductQualificationCodeSupportTSD(productQualificationCode))
					{
						targetInfo.AddNotification(Res.GetString("B0D901C0-5AD1-47AC-AF2C-7250ACC49641", "This Product Qualification Type is not supported in two-step declaration clearance mode."), ValidationModeProvider);
					}
				}
			}
		}

		static string[] GetRequiredCargoAttributes(string productQualificationCode)
		{
			string[] cargoAttributes = null;

			switch (productQualificationCode)
			{
				case ProductQualificationCodeList.Codes._422:
					cargoAttributes = new[] { CargoAttributeList.Codes._20 };
					break;
				case ProductQualificationCodeList.Codes._402:
				case ProductQualificationCodeList.Codes._423:
					cargoAttributes = new[] { CargoAttributeList.Codes._21 };
					break;
			}

			return cargoAttributes;
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			if (Parent.CSI_Quantity > 0 && Parent.CSI_UnitOfQuantity.IsEmpty)
			{
				Parent.CSI_UnitOfQuantityInfo.AddNotification(Res.GetString("3DB49288-2556-49D3-986E-51E45BBBF04D", "Unit is required when quantity is greater than 0"), ValidationModeProvider);
			}
			CheckCSI_UnitOfQuantityIsAvalidCode();
		}

		protected void CheckCSI_UnitOfQuantityIsAvalidCode()
		{
			Parent.CSI_UnitOfQuantityInfo.AddNotificationIfInvalidCode(Parent.Lookups.UnitOfMeasurementList, ValidationModeProvider);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			Parent.CSI_ReferenceNumberInfo.AddNotificationIfNotEntered(ValidationModeProvider);
		}
	}
}
