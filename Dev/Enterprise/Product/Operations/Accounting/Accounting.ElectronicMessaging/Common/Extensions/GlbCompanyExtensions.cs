using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class GlbCompanyExtensions
	{
		public static IEDICommunicationsMode[] LoadGEICommuncationModes(this GlbCompany company)
		{
			var communicationModes = company.OrgProxy?
										.EDICommunicationsModes
										.FindByModuleAndFileFormat(EDICommunicationsMode.Modules.GlobalElectronicInvoicing, EDICommunicationsModeFileFormatList.Codes.XML);

			return (communicationModes != null && communicationModes.Any())
					? communicationModes.ToArray<IEDICommunicationsMode>()
					: System.Array.Empty<IEDICommunicationsMode>();
		}
	}
}
