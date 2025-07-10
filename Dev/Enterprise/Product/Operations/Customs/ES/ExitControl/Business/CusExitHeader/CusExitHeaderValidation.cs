using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitHeaderValidation : EU.ExitControl.Business.CusExitHeaderValidation
	{
		public CusExitHeaderValidation(CusExitHeader parent) : base(parent)
		{
		}

		protected new CusExitHeader Parent => (CusExitHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDiscrepancies();
		}

		public void ValidateDiscrepancies()
		{
			var warningMessage = Res.GetString("2FEA87A6-4F99-46F3-8AE8-1A549DBB91EC", "Data entered in this tab will not be sent to Spanish Customs when Discrepancies flag is not ticked.");
			foreach (var report in Parent.CusExitReports)
			{
				report.Consignment?.ClearRowNotificationsContaining(warningMessage);
				report.Consignment?.CusExitConsignmentItems.ForEach(item =>
				{
					item.ClearRowNotificationsContaining(warningMessage);
					item.CusExitConsignmentPackagePivots.ForEach(package =>
					{
						package.ClearRowNotificationsContaining(warningMessage);
						package.Container?.ClearRowNotificationsContaining(warningMessage);
					});
				});

				if (!report.CER_Calc_Discrepancies && (report.Consignment?.CusExitConsignmentItems.Any() ?? false))
				{
					report.Consignment?.AddRowWarning(warningMessage);
					report.Consignment?.CusExitConsignmentItems.ForEach(item =>
					{
						item.AddRowWarning(warningMessage);
						item.CusExitConsignmentPackagePivots.ForEach(package =>
						{
							package.AddRowWarning(warningMessage);
							package.Container?.AddRowWarning(warningMessage);
						});
					});
				}
			}
		}

		protected override void CheckCXH_CustomsProfile()
		{
			base.CheckCXH_CustomsProfile();

			CertificateHelper.CheckCustomsProfile(Parent.CXH_CustomsProfileInfo, Parent.CXH_CustomsProfile, Parent.CustomsAgent);
		}
	}
}
