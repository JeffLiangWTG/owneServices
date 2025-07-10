using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public sealed class SupportingDocumentLookups : CusSupportingInfoLookups
{
	public SupportingDocumentLookups(SupportingDocument parent) : base(parent) { }

	new SupportingDocument Parent => (SupportingDocument)base.Parent;

	public override ICollection CodeList => UniversalReferenceDataHelper.GetSupportingDocumentTypeCollection(Factory, DateOfValuationProviderParent?.DateOfValuation);

	public OrgHeaderCollection OrganizationList => new OrganisationsFindBoxCollection(Factory);

	public CodeDescriptionPairList RegistrationCodeList
	{
		get
		{
			var docAddress = Parent.OrganizationAddress;
			var countryCode = docAddress.E2_RN_NKCountryCode;
			return Factory.GetCachedValue($"Customs.IN.SupportingDocument.{docAddress.E2_OA_Address}", () =>
			{
				var result = new CodeDescriptionPairList();
				docAddress.Organisation?.CustomsCodes
					.GetAllOrgCusCodesForCountryAndCodes(countryCode)
					.Where(x => x.OK_OA_PremisesAddress.IsEmpty || x.OK_OA_PremisesAddress == docAddress.E2_OA_Address)
					.OrderBy(x => x.OK_CodeType)
					.ForEach(x => result.AddPair(x.OK_CodeType, x.OK_CustomsRegNo));
				return result;
			}, CacheStalenessPolicy.StaleOnFactorySave);
		}
	}

	IDateOfValuationProvider DateOfValuationProviderParent => Parent.Parent as IDateOfValuationProvider;
}
