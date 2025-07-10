using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class OrgSupBuyLinkTrnModeAddInfoBizObjValidation : ZValidation
	{
		public OrgSupBuyLinkTrnModeAddInfoBizObjValidation(OrgSupBuyLinkTrnModeAddInfoBizObj parent) : base(parent)
		{
			Parent = parent;
		}

		public override Type AutoValidationType => typeof(OrgSupBuyLinkTrnModeAddInfoBizObj);

		OrgSupBuyLinkTrnModeAddInfoBizObj Parent { get; }

		public override void ValidateAll()
		{
			ValidateZO_CustomsOffice();
			ValidateZO_OfficeOfEntryExit();
			ValidateZO_CIQOfficeOfEntryExit();
		}

		public void ValidateZO_CustomsOffice()
		{
			ValidateCalculatedProperty(Parent.ZO_CustomsOfficeInfo);
		}

		protected void CheckZO_CustomsOffice()
		{
			var targetInfo = Parent.ZO_CustomsOfficeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		public void ValidateZO_OfficeOfEntryExit()
		{
			ValidateCalculatedProperty(Parent.ZO_OfficeOfEntryExitInfo);
		}

		protected void CheckZO_OfficeOfEntryExit()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_OfficeOfEntryExitInfo);
		}

		public void ValidateZO_CIQOfficeOfEntryExit()
		{
			ValidateCalculatedProperty(Parent.ZO_CIQOfficeOfEntryExitInfo);
		}

		protected void CheckZO_CIQOfficeOfEntryExit()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_CIQOfficeOfEntryExitInfo);
		}
	}
}
