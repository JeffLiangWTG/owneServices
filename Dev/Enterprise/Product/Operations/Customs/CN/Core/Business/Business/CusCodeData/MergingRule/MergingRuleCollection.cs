using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRuleCollection : CusCodeDataCollection<MergingRule>, ICodeDescriptionOptionStorage
	{
		public MergingRuleCollection(JobDeclaration parent) : base(parent, Constants.CusCodeDataTypes.Codes.MergingRule)
		{
		}

		BusinessObject ICodeDescriptionOptionStorage.FindByCode(ZString code)
		{
			return GetFirstElementHaving(code);
		}

		void ICodeDescriptionOptionStorage.AddNew(ZString code)
		{
			AddNew(code);
		}

		IEnumerable<ZString> ICodeDescriptionOptionStorage.AllCodes => this.Cast<MergingRule>().Select(codeData => codeData.CY_Code);

		ICodeDescriptionPairList ICodeDescriptionOptionStorage.GetAllOptions() => MergingRulesProvider.GetPairList(Factory);

		void ICodeDescriptionOptionStorage.ValidateSeletedOption(ZString code, bool selected, ZPropertyInfo propertyInfo, IEnumerable<ZString> selectedCodes) { }

		ZPropertyInfo ICodeDescriptionOptionStorage.SelectedOptionsAsStringPropertyInfo => null;

		public IValidationModeProvider ValidationModeProvider => Master as JobDeclaration;
	}
}
