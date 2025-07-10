using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override ICodeDescriptionPairList CustomsOfficeList
			=> RefCusCodeListTypes.GetCachedList(Factory, Parent.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, includeParentDataGrouping: false);

		public override CodeDescriptionPairList PaymentPartyList
			=> RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, ZDateTime.Today, includeParentDataGrouping: false);

		public OrganisationsFindBoxCollection RepresentativeList => new BrokerCollection(Factory);

		protected override ZString EntryStatusListCodeType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UserDefinedEntryStatus;
	}
}
