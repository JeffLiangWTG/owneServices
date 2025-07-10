using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public override CodeDescriptionPairList DeclarationLanguageList => LookupsHelper.GetBELanguageList(Factory);

	public override CodeDescriptionPairList PaymentPartyList => new PaymentMethodList();

	public override OrganisationsFindBoxCollection ConsigneeList => new ConsigneeCollection(Factory);

	public ZZRefCusCodeListCombinedCollection BELocationOfGoodsList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
		Factory,
		Core.Constants.CountryCodes.Belgium,
		new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities },
		ZDateTime.Today,
		new[] { new RefCusCodeListAttributeFilter(UniversalReferenceConstants.Type, SQLComparisonOperator.Equal, UniversalReferenceConstants.ValidLocationOfGoods(Parent.JE_TransportMode).Select(s => new ZString(s)).ToArray()) }
		);

	public virtual ZZRefCusCodeListCombinedCollection JE_CustomsOfficeList =>
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(
		Factory,
		Core.Constants.CountryCodes.Belgium,
		new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities },
		ZDateTime.Today,
		new[]
		{
			new RefCusCodeListAttributeFilter(UniversalReferenceConstants.Type, SQLComparisonOperator.Equal, UniversalReferenceConstants.DALocatie),
			new RefCusCodeListAttributeFilter(UniversalReferenceConstants.SubType, SQLComparisonOperator.Equal, UniversalReferenceConstants.Kantoor)
		});
}
