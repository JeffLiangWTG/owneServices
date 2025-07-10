using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class ExitPresentationHeaderProvider : ExitHeaderProvider, IExitPresentationHeader
	{
		public ExitPresentationHeaderProvider(CusExitReport report) : base(report)
		{
		}

		public IAESPartyContactPerson ExitCarrierContactPerson => PartyContactPersonGlbStaffProvider.NewOrNull(GlbStaff.CurrentUser);

		public IReadOnlyCollection<string> AdditionalInformationCodes
		{
			get
			{
				if (additionalInformationCodes == null)
				{
					additionalInformationCodes = report.Consignment.AdditionalInfos.Where(x => !x.CSI_Code.IsEmpty)
						.Select(x => (string)x.CSI_Code).ToList();
				}
				return additionalInformationCodes;
			}
		}
		List<string> additionalInformationCodes;
	}
}
