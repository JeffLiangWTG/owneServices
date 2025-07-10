using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class UndgValidation : UNDGDataItemValidation
	{
		public UndgValidation(AutoUNDGDataItem parent)
			: base(parent)
		{ }

		protected override void CheckDI_DG_NKSubs()
		{
			base.CheckDI_DG_NKSubs();
			if (Parent.UNDGSubstance?.DG_FlashPoint.IsEmpty ?? true)
			{
				Parent.DI_DG_NKSubsInfo.AddMessageError(ResString.GetMultilingualString("3ACCFFBE-FD20-4563-B9BB-897E5C1BE78E", "should have a flash point entered."));
			}
		}
	}
}
