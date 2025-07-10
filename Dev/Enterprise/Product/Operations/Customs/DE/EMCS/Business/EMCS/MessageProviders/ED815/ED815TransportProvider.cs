using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED815TransportProvider : TransportProvider, IEMCSTransport
	{
		public ED815TransportProvider(EMCSCusContainer container, EMCSJobDeclaration emcsJobDeclaration) : base(container)
		{
			this.emcsJobDeclaration = emcsJobDeclaration;
		}
		readonly EMCSJobDeclaration emcsJobDeclaration;

		string IEMCSTransport.IdentityOfUnit
		{
			get
			{
				var result = IdentityOfUnit;
				if (result.IsNullOrEmpty() && emcsJobDeclaration.ZG_DeferredSubmission == EmcsDeferredSubmissionList.Codes.JaZusammengefasstesEVd)
				{
					result = Constants.ConsolidatedDocumentDefaultString;
				}
				return result;
			}
		}
	}
}
