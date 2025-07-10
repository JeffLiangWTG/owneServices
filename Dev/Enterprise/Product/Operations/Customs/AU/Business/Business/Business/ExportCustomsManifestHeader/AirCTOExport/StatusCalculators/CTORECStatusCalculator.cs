using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTORECStatusCalculator : CMRStatusCalculator<ExportCustomsManifestLines>
	{
		public CTORECStatusCalculator(ExportCustomsManifestLines line)
			: base(line)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CTORECStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CTOREC };
	}
}
