using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business
{
	public class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
	{
		public CusAuthorizationUsageValidation(AutoCusAuthorizationUsage parent) : base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		protected override void CheckAGC_Code()
		{
			base.CheckAGC_Code();
			var parent = Parent;
			var code = parent.AGC_Code;
			var targetInfo = parent.AGC_CodeInfo;
			var instruction = parent.Instruction;

			if (instruction != null)
			{
				if (code == CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords && instruction.CEI_SubStyle != Declaration.EntrySubStyleList.Codes.Z)
				{
					targetInfo.AddWarning(Res.GetString("62769755-A650-422E-ABA2-DB6EAD216477", "EIR authorization must be used only for Sub Style Z."));
				}
			}
		}

		protected override void AddReferenceNumberAndOwnerCombinationWarning(ZPropertyInfo propertyInfo, ZString code, ZString number)
		{
			var parent = Parent;
			if (code == ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport)
			{
				if (!parent.Lookups.NumberList.Any(x => x.CPH_Number == number))
				{
					propertyInfo.AddWarning(Res.GetString("9D8E81D9-673D-49FC-A7AA-9FB1F1F9ED10", "Authorization number: {0} doesn't exist for Code LAME, Owner: {1}", number, parent.Owner?.OH_Code));
				}
			}
			else
			{
				base.AddReferenceNumberAndOwnerCombinationWarning(propertyInfo, code, number);
			}
		}
	}
}
