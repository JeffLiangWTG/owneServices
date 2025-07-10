using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class ProcessProvider : IProcess
	{
		ProcessProvider(ProcessRelatedNumber processRelated)
		{
			this.processRelated = Argument.NotNull(processRelated, nameof(processRelated));
		}
		readonly ProcessRelatedNumber processRelated;

		public static ProcessProvider New(ProcessRelatedNumber processRelated) => processRelated == null ? null : new ProcessProvider(processRelated);

		public string Type => "ADMINISTRATIVO";

		public string Identification => processRelated.CE_EntryNum;
	}
}

