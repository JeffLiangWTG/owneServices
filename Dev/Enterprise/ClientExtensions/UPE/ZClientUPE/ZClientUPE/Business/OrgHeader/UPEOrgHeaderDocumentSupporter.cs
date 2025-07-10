using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgHeaderDocumentSupporter : OrgHeaderDocumentSupporter
	{
		public UPEOrgHeaderDocumentSupporter(UPEOrgHeader orgHeader)
			: base(orgHeader)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			switch (dataContext)
			{
				case Core.Constants.DataContext.Organisation:
					result = new DocumentWrapper[] { UPEDocOrganisation.New(OrgHeader, OrgHeader.Factory) };
					break;
				default:
					result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
					break;
			}
			return result;
		}
	}
}
