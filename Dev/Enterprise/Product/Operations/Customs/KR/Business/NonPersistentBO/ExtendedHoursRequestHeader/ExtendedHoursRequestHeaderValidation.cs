using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestHeaderValidation : AutoExtendedHoursRequestHeaderValidation
	{
		public ExtendedHoursRequestHeaderValidation(AutoExtendedHoursRequestHeader parent) : base(parent)
		{
		}

		new ExtendedHoursRequestHeader Parent => (ExtendedHoursRequestHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMessageType();
		}

		protected override void CheckCustomsOffice()
		{
			base.CheckCustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.CustomsOfficeInfo);
			MandatoryValidation.CheckEntered(Parent.CustomsOfficeInfo);
		}

		protected override void CheckDepartment()
		{
			base.CheckDepartment();
			ListValidation.MessageErrorIfInvalidCode(Parent.DepartmentInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DepartmentInfo);
		}

		protected override void CheckStartDate()
		{
			base.CheckStartDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.StartDateInfo);
			if (Parent.StartDate.IsValid)
			{
				if (Parent.StartDate < ZDateTime.Today)
				{
					Parent.StartDateInfo.AddMessageError(Res.GetString("85C40743-E0A5-4561-9685-DD5D367D4611", "The 'Start Period' must be greater than or equal to today's date."));
				}
			}
		}

		protected override void CheckEndDate()
		{
			base.CheckEndDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.EndDateInfo);
			if (Parent.StartDate.IsValid && Parent.EndDate.IsValid)
			{
				if (Parent.EndDate <= Parent.StartDate)
				{
					Parent.EndDateInfo.AddMessageError(Res.GetString("D07E57BA-1816-4FC8-BCCA-48312B83066C", "The 'End Period' must be greater than the 'Start Period'."));
				}
			}
		}

		protected override void CheckBranchPK()
		{
			base.CheckBranchPK();
			MandatoryValidation.CheckEntered(Parent.BranchPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.BranchPKInfo);
		}

		public void ValidateMessageType()
		{
			ValidateCalculatedProperty(Parent.MessageTypeInfo);
		}

		protected void CheckMessageType()
		{
			ZString uniPassDeclarantID = KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithFallbackDefault(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)?.ToString() ?? ZString.Empty;
			if (uniPassDeclarantID.IsEmpty)
			{
				Parent.MessageTypeInfo.AddError(Res.GetString("99DCF7CB-D605-484A-96FE-F90466B4A06B", "Please enter a UNIPASS Declarant ID in the registry."));
			}
		}
	}
}
