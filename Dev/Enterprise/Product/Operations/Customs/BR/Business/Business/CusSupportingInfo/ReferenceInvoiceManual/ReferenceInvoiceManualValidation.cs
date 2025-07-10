using System;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ReferenceInvoiceManualValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ReferenceInvoiceManualValidation(ReferenceInvoiceManual parent) : base(parent)
		{
		}

		public new ReferenceInvoiceManual Parent => (ReferenceInvoiceManual)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var number = Parent.CSI_ReferenceNumber;
			var targetInfo = Parent.CSI_ReferenceNumberInfo;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (!number.IsEmpty)
			{
				if (!CNPJValidator.ValidateCNPJ(number))
				{
					targetInfo.AddMessageError(Res.GetString("ba0ce95f-d6d3-4f62-903b-1b9d9c874905", "The entered CNPJ is not valid.", targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			base.CheckCSI_AdditionalDescription();
			var number = Parent.CSI_AdditionalDescription;
			var targetInfo = Parent.CSI_AdditionalDescriptionInfo;

			if (!number.IsEmpty)
			{
				if (!DateTime.TryParseExact(number, "yyyy/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
				{
					targetInfo.AddError(Res.GetString("1251c7c7-c0d7-42cb-835c-62cbf6742343", "Incorrect format YYYY/MM.", targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
		}

		protected void CheckCSI_State()
		{
			var targetInfo = Parent.CSI_StateInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		public void ValidateCSI_State()
		{
			((IValidationInternals)this).Validate(Parent.CSI_StateInfo, () => { CheckCSI_State(); });
		}
	}
}
