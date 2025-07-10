using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgSupplierBuyerLinkAddInfoValidation : ZValidation
	{
		public CNOrgSupplierBuyerLinkAddInfoValidation(CNOrgSupplierBuyerLinkAddInfo parent) : base(parent)
		{
			Parent = parent;
		}

		protected CNOrgSupplierBuyerLinkAddInfo Parent { get; }

		public override Type AutoValidationType => typeof(CNOrgSupplierBuyerLinkAddInfoValidation);

		public override void ValidateAll()
		{
			ValidateZO_ProcedureCode();
			ValidateZO_LevyType();
			ValidateZO_ManualNo();
		}

		public void ValidateZO_ProcedureCode()
		{
			ValidateCalculatedProperty(Parent.ZO_ProcedureCodeInfo);
		}

		protected void CheckZO_ProcedureCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.ZO_ProcedureCodeInfo, Parent.Lookups.ProcedureCodeList);
		}

		internal void ValidateZO_LevyType()
		{
			ValidateCalculatedProperty(Parent.ZO_LevyTypeInfo);
		}

		protected void CheckZO_LevyType()
		{
			ListValidation.WarnIfInvalidCode(Parent.ZO_LevyTypeInfo, Parent.Lookups.LevyTypeList);
		}

		internal void ValidateZO_ManualNo()
		{
			ValidateCalculatedProperty(Parent.ZO_ManualNoInfo);
		}

		protected void CheckZO_ManualNo()
		{
			var value = Parent.ZO_ManualNo;
			var targetInfo = Parent.ZO_ManualNoInfo;

			if (!value.IsEmpty && !Regex.IsMatch(value, CusEntryInstructionValidation.ManualNumberPattern))
			{
				targetInfo.AddWarning(CusEntryInstructionValidation.ManualNumberFormatErrorMessage);
			}

			var levyType = Parent.ZO_LevyType;
			if (!levyType.IsEmpty)
			{
				var supportedTypes = LevyTypeList.GetSupportedManualTypesByLevyType(levyType);
				if (supportedTypes.Any() && !value.IsEmpty && !supportedTypes.Contains(value[0].ToString().ToUpper(CultureInfo.InvariantCulture)))
				{
					targetInfo.AddWarning(Res.GetString("909D36F7-8AA0-4813-B830-0D2C9167C8F1", "When Levy Type is {0}, Manual Number should start with {1} .", levyType, string.Join(",", supportedTypes)));
				}
			}
		}
	}
}
