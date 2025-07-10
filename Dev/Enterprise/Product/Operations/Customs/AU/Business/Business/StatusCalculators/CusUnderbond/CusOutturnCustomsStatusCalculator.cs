
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnCustomsStatusCalculator : CMRStatusCalculator<CusOutturn>
	{
		public CusOutturnCustomsStatusCalculator(CusOutturn outturn) : base(outturn)
		{
		}

		protected override ZString GetDefaultStatus() => ZString.Empty;

		protected internal override ZPropertyInfo StatusInfo => Parent.C5_CustomsStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CARST };
	}
}
