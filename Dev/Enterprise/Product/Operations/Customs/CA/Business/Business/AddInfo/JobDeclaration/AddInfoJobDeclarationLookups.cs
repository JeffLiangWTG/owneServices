using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobDeclarationLookups : CAAddInfoLookups
	{
		public AddInfoJobDeclarationLookups(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		public new AddInfoJobDeclaration Parent => (AddInfoJobDeclaration)base.Parent;

		public CodeDescriptionPairList PriorityIndicatorList => Factory.GetCachedValue<PriorityIndicators>();

		public ZZRefCarrierCombinedCollection CarrierCodes => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, Parent.Parent?.JE_TransportMode ?? ZString.Empty);

		public ACROSSServiceOptions ServiceOptions => new ACROSSServiceOptions();

		public AssessmentOptions AssessmentOptions => Factory.GetCachedValue<AssessmentOptions>();

		public B3MergeByList CAMergeByList => Factory.GetCachedValue<B3MergeByList>();

		public BondTypeList BondTypeList => Factory.GetCachedValue<BondTypeList>();

		public CodeDescriptionPairList B2TypeList
		{
			get
			{
				return Factory.GetCachedValue("AddInfoJobDeclarationLookups|B2TypeList_ " + Parent.Parent.JE_MessageType, () =>
				{
					var result = new B2TypeList();
					if (Parent.Parent.IsB3X)
					{
						result.RemoveCode(Business.B2TypeList.Codes.Blanket);
					}
					return result;
				});
			}
		}

		public OrgHeaderCollection MailToOrganisations => new OrgHeaderCollection(Factory);

		public JobDeclarationCollection LodgedB3Declarations
		{
			get
			{
				var result = new OriginalDeclarationCollection(Factory, Parent.Parent.Company.PK, Parent.Parent.JE_MessageType);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction #", "Property", Parent.Parent.CA_OriginalTransactionNo));
				return result;
			}
		}
	}

	class OriginalDeclarationCollection : JobDeclarationCollection
	{
		public OriginalDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn, ZString messageType)
			: base(factory, companyPkToFilterOn)
		{
			this.messageType = messageType;
		}

		readonly ZString messageType;

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = base.CreateAdditionalFilter();
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, new List<ZString>() { messageType, JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments, JobMessageTypeList.Codes.XTypeEntry }.ToArray());
			return query;
		}
	}
}
