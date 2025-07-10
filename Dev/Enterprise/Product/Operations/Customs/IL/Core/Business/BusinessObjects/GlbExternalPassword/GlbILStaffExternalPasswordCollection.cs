using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public class GlbILStaffExternalPasswordCollection : DependentBusinessObjectCollection<GlbILStaffExternalPassword, GlbStaff>
	{
		public GlbILStaffExternalPasswordCollection(GlbStaff master)
			: base(master, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ILS))
		{
			this.EnableMaxCountValidation(maxCount: 1, warnAtHalfway: false, CargoWise.ComponentModel.NotificationType.Error, ValidationCaptions.GlbILExternalPasswordCollection.OnlyOneSignatureEntryIsAllowed);
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (Count > 1)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"{GetType().FullName} error"), ValidationCaptions.GlbILExternalPasswordCollection.OnlyOneSignatureEntryIsAllowed);
			}
		}
	}
}
