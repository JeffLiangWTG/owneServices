using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusMAWB : CusMAWB
	{
		public UPECusMAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldFieldsBeReadonly
		{
			get { return (IsInDatabase && !GlbStaff.CurrentUser.GS_IsController) || base.ShouldFieldsBeReadonly; }
		}

		protected override void SetOutturnResultType(CusHAWB hAWB, CusOutturn outturn)
		{
			if (hAWB.CS_IsSurplus)
			{
				outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			}
			else if (hAWB.CS_PiecesManifested < hAWB.CS_PiecesLanded)
			{
				outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
			}
			else if (hAWB.CS_PiecesManifested > hAWB.CS_PiecesLanded)
			{
				outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			}
			else
			{
				outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			}
		}
	}
}
