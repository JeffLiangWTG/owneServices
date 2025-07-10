using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class EnterpriseQualificationLookups : CusCodeDataLookups
	{
		public EnterpriseQualificationLookups(EnterpriseQualification parent)
			: base(parent)
		{
		}

		CusEntryInstruction EntryInstruction => (CusEntryInstruction)((CusCodeData)base.Parent).Parent;

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				var declaration = EntryInstruction?.JobDeclaration;
				return declaration == null ? new CodeDescriptionPairList() :
					EnterpriseQualificationListHelper.GetCachedEnterpriseQualificationList(Factory, declaration.IsImport);
			}
		}

		public override CodeDescriptionPairList CY_DataList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var declaration = EntryInstruction?.JobDeclaration;
				if (declaration != null)
				{
					var enterpriseQualificationList = CY_CodeList;
					var code = ((EnterpriseQualification)Parent).CY_Code;

					if (declaration.IsImport)
					{
						AddCustomsCodes(declaration.Importer, enterpriseQualificationList, code, result);
						AddCustomsCodes(declaration.Buyer, enterpriseQualificationList, code, result);
					}

					if (declaration.IsExport)
					{
						AddCustomsCodes(declaration.Supplier, enterpriseQualificationList, code, result);
						AddCustomsCodes(declaration.Manufacturer, enterpriseQualificationList, code, result);
					}
				}
				return result;
			}
		}

		static void AddCustomsCodes(OrgHeader orgHeader, CodeDescriptionPairList enterpriseQualificationList, ZString code, CodeDescriptionPairList result)
		{
			if (orgHeader != null)
			{
				var customsCodes = orgHeader.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.China && (code.IsEmpty && enterpriseQualificationList.ContainsCode(x.OK_CodeType) || x.OK_CodeType == code));
				if (customsCodes.Any())
				{
					result.AddRange(customsCodes.Select(x => new CodeDescriptionPair(x.OK_CustomsRegNo.ToString(), FormattableString.Invariant($"({orgHeader.OH_Code}){enterpriseQualificationList.GetDescriptionFromCode(x.OK_CodeType)}"))).ToArray());
				}
			}
		}
	}
}
