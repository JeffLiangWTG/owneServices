using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using CodeList = Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.KR.Business
{
	public class FTAMessageSendingObjectLookups : ZLookups
	{
		public FTAMessageSendingObjectLookups(FTAMessageSendingObject parent) : base(parent)
		{
		}
		public CodeDescriptionPairList FTARelationArticleCodeList => Factory.GetCachedValue<FTALawCodeList>();
		public CodeDescriptionPairList YNCodeList => Factory.GetCachedValue<CodeList.YesNoList>();
		public RefUNLOCOCollection PortOfLoadings => new RefUNLOCOCollection(Factory);

		public RefCountryCollection Countries => new RefCountryCollection(Factory);

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public ConsigneeCollection ConsigneeList => new ConsigneeCollection(Factory);

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public ConsignorCollection ConsignorList => new ConsignorCollection(Factory);
	}
}
