using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class GuaranteeValidation : NctsGuaranteePhase5Validation
	{
		public GuaranteeValidation(Guarantee parent) : base(parent)
		{
		}

		protected new Guarantee Parent => (Guarantee)base.Parent;

		protected override bool CheckBondTypeIsEmptyIsActive => !Parent.NctsHeader.IsArrivalMovement;

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();

			var parent = Parent;
			var guarantees = parent.NctsHeader?.MovementHeader?.Guarantees.Cast<Guarantee>().ToArray() ?? Array.Empty<Guarantee>();
			if (guarantees.Length > 1)
			{
				if (guarantees.Any(x => x.PW_BondType.In(new ZString[] { NctsGuaranteeTypeList.Codes._8, NctsGuaranteeTypeList.Codes.B, NctsGuaranteeTypeList.Codes.R })))
				{
					parent.PW_BondTypeInfo.AddMessageError(Res.GetString("AA97FAAC-ED68-4CB8-A54D-8DE492BC6EAF", "Guarantee Types '8', 'B' or 'R' only allow one Guarantee to be entered per declaration."));
				}
			}
		}

		protected override void CheckPW_BondNumber()
		{
			base.CheckPW_BondNumber();

			var parent = Parent;
			var refNumber = parent.PW_BondNumber;
			var targetInfo = parent.PW_BondNumberInfo;
			if (parent.IsDropdownForReferenceNumberAndCode)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}

			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			if (parent.PW_BondType == NctsGuaranteeTypeList.Codes._4 && !refNumber.IsEmpty && refNumber.Length != 24)
			{
				targetInfo.AddMessageError(Res.GetString("9914dce8-dcae-456a-8853-16c973e59f39", "24 characters should be entered."));
			}
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();

			var targetInfo = Parent.PW_BondNumber2Info;
			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckPW_Password()
		{
			base.CheckPW_Password();

			var parent = Parent;
			if (parent.Lookups.AccessCodeList.Count > 1)
			{
				ListValidation.WarnIfInvalidCode(parent.PW_PasswordInfo, ResString.GetMultilingualString("AB801FF8-233D-453A-B062-444B5F22773E", "Guarantee Access Code is not included in master data of selected Guarantee."));
			}
		}
	}
}
