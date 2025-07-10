using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
	{
		public CusAuthorizationUsageValidation(CusAuthorizationUsage parent) : base(parent)
		{
		}

		new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		protected override void CheckAGC_OH_Owner()
		{
			base.CheckAGC_OH_Owner();
			var parent = Parent;
			var declaration = parent.Instruction?.JobDeclaration as JobDeclaration;
			if (declaration != null)
			{
				var holders = declaration.ActualClientAndDeclarant;
				if (holders == null || !holders.Any(holder => holder.PK == parent.AGC_OH_Owner))
				{
					var temp = declaration.IsImport ? Res.GetString("23c83acb-9667-4e42-9394-1e482c5213d2", "Consignee") : Res.GetString("a0a3ccbe-8180-43a9-a283-180457c8104c", "Consignor");
					var errorMessage = Res.GetString("77EBAC6C-A66A-452C-A5C6-31C54DE74ACE",
						"The owner should be either {0} or Declarant.", temp);
					parent.AGC_OH_OwnerInfo.AddMessageError(errorMessage);
				}
			}
		}

		protected override void CheckAGC_Code()
		{
			base.CheckAGC_Code();

			var parent = Parent;
			if (!parent.AGC_CPH_Authorization.IsEmpty)
			{
				var code = parent.AGC_Code;
				if (parent.AuthorisationHeader is CusAuthorisationHeader authorisationHeader && authorisationHeader.CPH_IsAdHoc
					&& code != Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse
					&& code != Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing
					&& code != Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing
					&& code != Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission)
				{
					parent.AGC_CodeInfo.AddMessageError(Res.GetString("683D4F79-0258-418E-91B3-9230B741BA8A", "You can't request an Ad Hoc Authorization for authorization types other than IPO, EUS, OPO, or TEA."));
				}
			}
		}

		protected override void CheckAGC_CPH_Authorization()
		{
			base.CheckAGC_CPH_Authorization();

			var parent = Parent;
			var entryHeader = parent.Instruction?.EntryHeader;
			if (parent.AuthorisationHeader is CusAuthorisationHeader auth
				&& !auth.CPH_IsActive
				&& (entryHeader == null || entryHeader.CH_EntryStatus < EntryStatusDescriptionCodeList.Codes.ES100))
			{
				parent.AGC_CPH_AuthorizationInfo.AddMessageError(Res.GetString("60055752-9D24-4D7D-9BAF-7522D8A55207", "The authorization you chose is inactive, please choose another one"));
			}

			var authorisation = parent.RelatedAuthorisationHeader;
			if (authorisation != null)
			{
				authorisation.RunPreSaveValidation();
				foreach (var notification in authorisation.NotificationsIncludingChildren)
				{
					parent.AGC_CPH_AuthorizationInfo.AddNotification(notification.Type, notification.Message);
				}
			}
		}

		protected override bool UseEffectiveReferenceNumberValidation => false;
	}
}
