using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.MessageSending
{
	public class TP5MessageSendingObjectValidation : EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation
	{
		public TP5MessageSendingObjectValidation(TP5MessageSendingObject parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJustificationCode();
			ValidateJustification();
			ValidateQueryInformation();
			ValidateQueryIdentifier();
			ValidatePeriodFrom();
			ValidatePeriodTo();
			ValidateRequesterRole();
		}

		public void ValidateJustificationCode()
		{
			ValidateCalculatedProperty(Parent.JustificationCodeInfo);
		}

		protected override void CheckJustification()
		{
			base.CheckJustification();
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC014C && Parent.Justification.IsEmpty)
			{
				Parent.JustificationInfo.AddError(Res.GetString("8063A84A-2F6F-48F2-9C5A-3989BB6A24F0", "Justification is mandatory"));
			}
		}

		protected void CheckJustificationCode()
		{
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC014C && Parent.JustificationCode.IsEmpty)
			{
				Parent.JustificationCodeInfo.AddError(Res.GetString("EB62F5B5-3C25-4EAF-90C6-C2304180D39B", "Regular Justification Code is mandatory"));
			}
		}

		protected override void CheckQueryInformation()
		{
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC141C && Parent.QueryInformation.IsEmpty)
			{
				Parent.QueryInformationInfo.AddError(Res.GetString("C7C20937-1EFE-4971-A0FF-5B7349206DEA", "Query Information is mandatory"));
			}
		}

		public void ValidateQueryIdentifier()
		{
			ValidateCalculatedProperty(Parent.QueryIdentifierInfo);
		}

		protected void CheckQueryIdentifier()
		{
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC034C)
			{
				if(Parent.QueryIdentifier.IsEmpty)
				{
					Parent.QueryIdentifierInfo.AddMessageError(Res.GetString("B98A29EB-14E1-4AE0-8CAF-526295F7787C", "Query Identifier is a mandatory value"));
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.QueryIdentifierInfo);
			}
		}

		public void ValidatePeriodFrom()
		{
			ValidateCalculatedProperty(Parent.PeriodFromInfo);
		}

		protected void CheckPeriodFrom()
		{
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC034C)
			{
				CheckPeriodTimePropertyIsValid(Parent.PeriodFromInfo);
				CheckPeriodTimeRange(Parent.PeriodFromInfo, Parent.PeriodFrom, Parent.PeriodTo);
			}
		}

		public void ValidatePeriodTo()
		{
			ValidateCalculatedProperty(Parent.PeriodToInfo);
		}

		protected void CheckPeriodTo()
		{
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC034C)
			{
				CheckPeriodTimePropertyIsValid(Parent.PeriodToInfo);
				CheckPeriodTimeRange(Parent.PeriodToInfo, Parent.PeriodFrom, Parent.PeriodTo);
			}
		}

		void CheckPeriodTimePropertyIsValid(ZPropertyInfo ptyInfo)
		{
			ZDateTime ptyValue = (ZDateTime)ptyInfo.Value;

			if (!ptyValue.IsEmpty && !ptyValue.IsValid)
			{
				ptyInfo.AddError(Res.GetString("5AB97D62-267B-416C-A19D-910BFE6D78A6", "Please enter a valid date."));
			}
		}

		void CheckPeriodTimeRange(ZPropertyInfo ptyInfo, ZDateTime timeFrom, ZDateTime timeTo)
		{
			if (timeFrom.IsValid && timeTo.IsValid && timeFrom > timeTo)
			{
				ptyInfo.AddError(Res.GetString("B0E97830-1DB5-47BA-8641-D7FB3AE5BBCB", "Period From must be older than Period To."));
			}
		}

		public void ValidateRequesterRole()
		{
			ValidateCalculatedProperty(Parent.RequesterRoleInfo);
		}

		protected void CheckRequesterRole()
		{
			if (Parent.MessageType == TP5MessageTypeList.Codes.CC034C)
			{
				if (Parent.RequesterRole.IsEmpty)
				{
					Parent.RequesterRoleInfo.AddMessageError(Res.GetString("34EF3AC7-1BB0-49B9-8F57-B64A00AA8AAF", "Role is a mandatory value"));
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.RequesterRoleInfo);
			}
		}

		protected new TP5MessageSendingObject Parent => (TP5MessageSendingObject)base.Parent;
	}
}
