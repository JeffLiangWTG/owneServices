using System;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class AdditionalProcedureCodeValidation : CusCodeDataValidation
	{
		public AdditionalProcedureCodeValidation(AdditionalProcedureCode parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			var additionalCode = Parent.CY_Code;
			if (!additionalCode.IsEmpty)
			{
				var cpcParent = Parent.Parent;
				if (cpcParent != null)
				{
					var mainProcedureCode = cpcParent.MainProcedure;
					var mainProcedureCodePrefix = cpcParent.MainProcedurePrefix;
					if (!mainProcedureCodePrefix.IsEmpty && CY_CodeCheckFirst4Characters)
					{
						if (!additionalCode.StartsWith(mainProcedureCodePrefix, StringComparison.OrdinalIgnoreCase))
						{
							Parent.CY_CodeInfo.AddMessageError(Res.GetString("EDFA7224-E4B9-47F0-8A8E-D43680BF0D45", "The first 4 characters of additional procedure code should be same as the main procedure code's."));
						}
					}

					if (!mainProcedureCode.IsEmpty && additionalCode == mainProcedureCode || cpcParent.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => x.PK != Parent.PK && x.CY_Code == additionalCode))
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("4FFED856-797A-4B1A-B924-B916FBC56CC8", "Additional procedure code already specified. Additional procedure codes should not be duplicated."));
					}
					cpcParent.AdditionalProcedureCodesAsStringInfo.AddAllNotificationsFrom(Parent.CY_CodeInfo);
				}
			}
		}

		protected new AdditionalProcedureCode Parent => (AdditionalProcedureCode)base.Parent;

		protected virtual bool CY_CodeCheckFirst4Characters => true;
	}
}
