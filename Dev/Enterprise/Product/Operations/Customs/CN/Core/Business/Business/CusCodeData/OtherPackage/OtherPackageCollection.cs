using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class OtherPackageCollection : CusCodeDataCollection<OtherPackage>, ICodeDescriptionOptionStorage
	{
		public OtherPackageCollection(CusEntryInstruction parent) : base(parent, Constants.CusCodeDataTypes.Codes.Package)
		{
		}

		public new CusEntryInstruction Master => base.Master as CusEntryInstruction;

		public IEnumerable<ZString> AllCodes => this.Cast<OtherPackage>().Select(codeData => codeData.CY_Code);

		public ZPropertyInfo SelectedOptionsAsStringPropertyInfo => Master.OtherPackagesAsStringInfo;

		public IValidationModeProvider ValidationModeProvider => Master?.JobDeclaration;

		public BusinessObject FindByCode(ZString code) => GetFirstElementHaving(code);

		public ICodeDescriptionPairList GetAllOptions() => Factory.GetCachedValue<PackageType>();

		public void ValidateSeletedOption(ZString code, bool selected, ZPropertyInfo propertyInfo, IEnumerable<ZString> selectedCodes)
		{
		}

		void ICodeDescriptionOptionStorage.AddNew(ZString code) => AddNew(code);
	}
}
