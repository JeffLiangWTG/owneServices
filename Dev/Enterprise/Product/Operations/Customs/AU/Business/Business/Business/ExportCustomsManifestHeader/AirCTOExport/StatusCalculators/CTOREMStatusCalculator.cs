using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOREMStatusCalculator : CMRStatusCalculator<ExportCustomsManifestLines>
	{
		public CTOREMStatusCalculator(ExportCustomsManifestLines line)
			: base(line)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CTOREMStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CTOREM };
	}
}
