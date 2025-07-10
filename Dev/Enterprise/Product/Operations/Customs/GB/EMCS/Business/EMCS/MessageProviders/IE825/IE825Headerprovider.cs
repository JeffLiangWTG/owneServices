using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE825HeaderProvider : HeaderProvider, IIE825Header
	{
		public IE825HeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration) { }

		public string UpstreamArc => emcsJobDeclaration.EADNumber;

		public IReadOnlyCollection<IIE825SplitDetail> SplitDetails => splitDetails ?? (splitDetails = emcsJobDeclaration.Invoices.Cast<EU.EMCS.Business.EMCSJobComInvoiceHeader>().Select(x => new IE825SplitDetailProvider(x)).ToArray());
		IReadOnlyCollection<IIE825SplitDetail> splitDetails;

		public string MemberStateCode => emcsJobDeclaration.ZG_CCTMSA;
	}
}
