using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class SpecialBusinessIdentifierCollection : CusCodeDataCollection<SpecialBusinessIdentifier>, ICodeDescriptionOptionStorage
	{
		public SpecialBusinessIdentifierCollection(CusEntryInstruction parent) : base(parent, Constants.CusCodeDataTypes.Codes.SpecialBusinessIdentifier)
		{
		}

		public new CusEntryInstruction Master => base.Master as CusEntryInstruction;

		public IValidationModeProvider ValidationModeProvider => Master?.JobDeclaration;

		BusinessObject ICodeDescriptionOptionStorage.FindByCode(ZString code) => GetFirstElementHaving(code);

		void ICodeDescriptionOptionStorage.AddNew(ZString code) => AddNew(code);

		public IEnumerable<ZString> AllCodes => this.Cast<SpecialBusinessIdentifier>().Select(codeData => codeData.CY_Code);

		ICodeDescriptionPairList ICodeDescriptionOptionStorage.GetAllOptions() => Factory.GetCachedValue<SpecialBusinessList>();

		ZPropertyInfo ICodeDescriptionOptionStorage.SelectedOptionsAsStringPropertyInfo => Master.SpecialBusinessIdentifiersAsStringInfo;

		void ICodeDescriptionOptionStorage.ValidateSeletedOption(ZString code, bool selected, ZPropertyInfo propertyInfo, IEnumerable<ZString> selectedCodes)
		{
			if (selected && Master.WillGenerateExitingEntry && code == SpecialBusinessList.Codes.C03)
			{
				propertyInfo.AddNotification(Res.GetString("29cb3e46-6de2-403b-bb1b-5ef3bc00baaa", "Export job does not support {0}", SpecialBusinessList.Descriptions.C03), ValidationModeProvider);
			}
		}
	}
}
