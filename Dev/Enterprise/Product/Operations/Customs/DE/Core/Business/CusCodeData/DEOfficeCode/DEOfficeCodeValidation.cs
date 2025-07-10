using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DEOfficeCodeValidation : EuOfficeCodeValidation
	{
		public DEOfficeCodeValidation(DEOfficeCode parent) : base(parent)
		{
		}

		protected new DEOfficeCode Parent => (DEOfficeCode)base.Parent;

		protected JobDeclaration Declaration => (JobDeclaration)Parent.Parent;

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			if (Declaration?.IsImport ?? false)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CY_CodeInfo);
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var declaration = Declaration;
			if (declaration != null)
			{
				var parent = Parent;
				var code = parent.CY_Code;
				var data = parent.CY_Data;
				var targetInfo = parent.CY_DataInfo;
				if (!data.IsEmpty)
				{
					if (code == EuOfficeCodesTypes.Codes.OfficeOfExit)
					{
						if (data != declaration.JE_CustomsOffice && declaration.CustomsEntryInstructions.Any(x => x.Style4thDigitIs9()))
						{
							targetInfo.AddMessageError(Res.GetString("88CA44EE-0C08-4370-AFD5-9B0256A9AE8E", "The Office of Exit must match the Office of Export."));
						}
					}
					else if (code == EuOfficeCodesTypes.Codes.OfficeOfPresentation)
					{
						var goodsOrigin = declaration.JE_GoodsOrigin;
						if (goodsOrigin.Length == 2 && !data.StartsWith(goodsOrigin, StringComparison.OrdinalIgnoreCase))
						{
							targetInfo.AddMessageError(Res.GetString("C7E0568E-3C23-47EF-BC47-846F65DFFD35", "The first two digits of the customs office with purpose 'PRE' must match country/region of [15] Origin."));
						}
					}
				}
			}
		}
	}
}
