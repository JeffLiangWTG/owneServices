using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class FreightWrapperFromOrgBO : FreightWrapper
	{
		#region Constructors
		public FreightWrapperFromOrgBO(OrgHeader orgHeaderBO, BusinessObjectFactory factory)
			: base(orgHeaderBO, factory)
		{
			Argument.NotNull(factory, "factory");
			OrgHeaderBO = orgHeaderBO;
		}

		FreightWrapperFromOrgBO(OrgHeader orgHeaderBO, DocOrgSupplierBuyerLink orgSupplierLink, DocOrgSupplierBuyerLink orgBuyerLink, BusinessObjectFactory factory)
			: base(orgHeaderBO, factory)
		{
			Argument.NotNull(factory, "factory");
			this.OrgHeaderBO = orgHeaderBO;
			this.orgSupplierLink = orgSupplierLink;
			this.orgBuyerLink = orgBuyerLink;
		}

		public static FreightWrapperFromOrgBO New(OrgHeader orgHeaderBO, BusinessObjectFactory factory)
		{
			return new FreightWrapperFromOrgBO(orgHeaderBO, factory);
		}

		public static FreightWrapperFromOrgBO New(OrgHeader orgHeaderBO, DocumentWrapper orgSupplierLink, DocumentWrapper orgBuyerLink, BusinessObjectFactory factory)
		{
			return new FreightWrapperFromOrgBO(orgHeaderBO, (DocOrgSupplierBuyerLink)orgSupplierLink, (DocOrgSupplierBuyerLink)orgBuyerLink, factory);
		}
		#endregion

		readonly OrgHeader OrgHeaderBO;
		readonly DocOrgSupplierBuyerLink orgSupplierLink;
		readonly DocOrgSupplierBuyerLink orgBuyerLink;

		protected override DocOrgSupplierBuyerLink GetOrgSupplierLink()
		{
			return orgSupplierLink;
		}

		protected override DocOrgSupplierBuyerLink GetOrgBuyerLink()
		{
			return orgBuyerLink;
		}

		protected override DocOrganisation GetOrgDocument()
		{
			return (DocOrganisation)DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Organisation, OrgHeaderBO);
		}

		#region Implementation

		protected override ZString GetJobNumber()
		{
			return OrgDocument.ToString();
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, OrgHeaderBO, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, OrgHeaderBO, ContactType.All, Factory);
		}

		#endregion
	}
}
