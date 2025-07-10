using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.JP;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent) { }

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			var parent = Parent;
			var codeType = parent.OK_CodeType;
			var notificationType = warningCodeTypes.Contains(codeType) ? NotificationType.Warning : NotificationType.Error;
			CustomsRegistrationNumberValidation.ValidateCustomsCode(notificationType, codeType, parent.OK_CustomsRegNo, parent.OK_CustomsRegNoInfo);
		}

		readonly ZString[] warningCodeTypes = { OrgCusCode.CodeTypes.ControlledPremisesID, CodeTypes.CarrierCode };
	}
}
