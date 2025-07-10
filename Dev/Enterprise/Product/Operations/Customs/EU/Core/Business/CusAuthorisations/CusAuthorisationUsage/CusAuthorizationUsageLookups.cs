//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusAuthorizationUsageLookups
//
//    This class should be used for overriding collections in AutoCusAuthorizationUsageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusAuthorizationUsageLookups : AutoCusAuthorizationUsageLookups
	{
		public CusAuthorizationUsageLookups(AutoCusAuthorizationUsage parent) : base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		public virtual ICollection CodeList => JobDeclaration is JobDeclaration declaration ? (declaration.IsUCC6 ? Provider.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory) : Provider.GetAuthorisationTypeList(Factory)) : new CodeDescriptionPairList();

		public CusAuthorisationHeaderCollection NumberList
		{
			get
			{
				var parent = Parent;
				var numbers = new CusAuthorisationHeaderCollectionFiltered(Factory, parent.AGC_Code, parent.AGC_OH_Owner);
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", parent.AGC_Code, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber, "Property", parent.EffectiveReferenceNumber));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", parent.AGC_OH_Owner, false));
				return numbers;
			}
		}

		public JobDeclaration JobDeclaration => declaration ?? (declaration = Instruction?.JobDeclaration ?? Parent.InvoiceLine?.Declaration);
		JobDeclaration declaration;

		protected Declaration.CusEntryInstruction Instruction => Parent.Instruction;

		CusAuthorisationHeaderProvider Provider
		{
			get
			{
				var countryCode = JobDeclaration.CountryCode;
				return (CusAuthorisationHeaderProvider)Factory.GetCachedValue("CusAuthorizationUsageLookups.Provider_" + countryCode, () =>
				{
					return Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(countryCode);
				});
			}
		}
	}
}

