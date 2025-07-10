using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class CusExitDetail : EU.Business.CusExitDetail, Integration.Customs.FR.ICusExitDetail
	{
		public CusExitDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString SiretNumber => AgentOrDeclarant?.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.Siret, Core.Constants.CountryCodes.France) ?? ZString.Empty;

		public ZString EORINumber => AgentOrDeclarant?.GetUnprefixedEORI() ?? ZString.Empty;

		public OrgHeader AgentOrDeclarant => GetAgentOrDeclarant();

		OrgHeader GetAgentOrDeclarant()
		{
			OrgHeader result = null;
			var exitHeader = Factory.Load<CusExitControlHeader>(CED_CEH);
			if (exitHeader != null)
			{
				result = (
					exitHeader.Agent
					?? Factory.Load<JobDeclaration>(exitHeader.CEH_ParentID)?.DeclarantOrgAddress
				)?.Header
				?? Factory.Load<ForwardingShipment>(exitHeader.CEH_ParentID)?.ExportBroker;
			}

			return result;
		}
	}
}
