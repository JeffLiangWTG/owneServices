using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportValidation : ExitControlBase.Business.CusExitReportValidation
	{
		public CusExitReportValidation(AutoCusExitReport parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateGoodsLocationDescription();
		}

		protected new CusExitReport Parent => (CusExitReport)base.Parent;

		protected override void CheckCER_Type()
		{
			base.CheckCER_Type();
			if (Parent.CER_Type.IsEmpty)
			{
				var targetInfo = Parent.CER_TypeInfo;
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.CER_TypeInfo);
			}
		}

		protected override void CheckCER_TransportMode()
		{
			base.CheckCER_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.CER_TransportModeInfo);
		}

		protected override void CheckCER_TransportType()
		{
			base.CheckCER_TransportType();
			ListValidation.ErrorIfInvalidCode(Parent.CER_TransportTypeInfo);
		}

		protected override void CheckCER_TransportID()
		{
			base.CheckCER_TransportID();

			var parent = Parent;
			var transportType = parent.CER_TransportType;
			var targetInfo = parent.CER_TransportIDInfo;
			if (!transportType.IsEmpty)
			{
				if (SupportValidationWhenCER_TransportTypeNotEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}

				if (!transportType.In(new ZString[] { CusExitReportTransportTypeList.Codes._11, CusExitReportTransportTypeList.Codes._81 }) && parent.CER_TransportID.ToString().Any(char.IsLower))
				{
					targetInfo.AddMessageError(Res.GetString("EBE86DB1-A118-4A66-A107-CCB51E3077A2", "Must not contain lower case letters."));
				}
			}
		}

		protected override void CheckCER_RN_NKTransportNationality()
		{
			base.CheckCER_RN_NKTransportNationality();

			var parent = Parent;
			if (parent.CER_RN_NKTransportNationality.IsEmpty)
			{
				if (SupportValidationWhenCER_TransportTypeNotEmpty && !parent.CER_TransportType.IsEmpty)
				{
					var targetInfo = parent.CER_RN_NKTransportNationalityInfo;
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(parent.CER_RN_NKTransportNationalityInfo);
			}
		}

		protected virtual bool SupportValidationWhenCER_TransportTypeNotEmpty => true;

		protected override void CheckCER_CXC_Consignment()
		{
			if (Parent.CER_CXC_Consignment.IsEmpty)
			{
				Parent.CER_CXC_ConsignmentInfo.AddMessageError(Res.GetString("91FBBBFC-B604-45F6-96DA-9DFA672B910E", "An Exit Report must be linked to a Declaration/Entry."));
			}
			CheckCER_CXC_ConsignmentMrnIsAlreadyBeingUsed();
		}

		protected override void CheckCER_DateTime()
		{
			base.CheckCER_DateTime();

			var parent = Parent;
			if (parent.CER_DateTime.IsEmpty && !parent.CER_TransportType.IsEmpty && IsCER_DateTimeRequired())
			{
				var targetInfo = parent.CER_DateTimeInfo;
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
			}
		}

		protected virtual bool IsCER_DateTimeRequired() => Parent.CER_Type.EqualsIgnoringCase(ExitReportTypeList.Codes.ExitNotification);

		protected override void CheckCER_OfficeOfExit()
		{
			base.CheckCER_OfficeOfExit();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CER_OfficeOfExitInfo);
		}

		protected override void CheckCER_EnquiryInformationCode()
		{
			base.CheckCER_EnquiryInformationCode();
			if (Parent.IsCER_EnquiryInformationCodeRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_EnquiryInformationCodeInfo);
			}
		}

		protected void CheckCER_CXC_ConsignmentMrnIsAlreadyBeingUsed()
		{
			if (!IsUcc6RuleActive(Parent, x => x.ValidateCER_CXC_ConsignmentMrnIsAlreadyBeingUsed))
			{
				return;
			}

			var currentConsignment = Parent.CER_CXC_Consignment;
			var header = Parent.Header;
			if (header is null)
			{
				return;
			}

			var mrnAlreadyUsedInAnotherDeclaration = header.CusExitReports.Any(x => x.PK != Parent.PK && x.CER_CXC_Consignment == currentConsignment);
			if (mrnAlreadyUsedInAnotherDeclaration)
			{
				Parent.CER_CXC_ConsignmentInfo.AddMessageError(Res.GetString("0D2E5302-B2D3-44D2-9E88-3E568436E64E", "This MRN is already being used in another declaration."));
			}
		}

		protected override void CheckCER_Location()
		{
			base.CheckCER_Location();
			if (IsUcc6RuleActive(Parent, x => x.ValidateCER_LocationLookups))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_LocationInfo);
			}
		}

		bool IsUcc6RuleActive(CusExitReport report, Func<ICusExitReportUcc6ValidationDecider, bool> ruleCheck)
		{
			if (report.ValidationDecider is ICusExitReportUcc6ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected virtual void CheckGoodsLocationDescription()
		{
			var parent = Parent;
			if (parent.IsUCC6)
			{
				CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(parent);
			}
		}
	}
}
