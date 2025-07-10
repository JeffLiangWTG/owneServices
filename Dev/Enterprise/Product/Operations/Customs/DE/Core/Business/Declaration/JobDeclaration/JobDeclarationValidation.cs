using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclarationValidation : AutoDEJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override void CheckJE_OwnerRef()
		{
			base.CheckJE_OwnerRef();
			MandatoryValidation.WarnIfNotEntered(Parent.JE_OwnerRefInfo);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			var parent = Parent;
			if (!parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_RL_NKOrigin();

				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RL_NKOriginInfo);
			}
		}

		protected override void CheckJE_IATALoadPort()
		{
		}

		protected override void CheckJE_CustomsOffice()
		{
			var parent = Parent;
			if (!parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_CustomsOffice();
				ListValidation.MessageErrorIfInvalidCode(parent.JE_CustomsOfficeInfo);
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			var parent = Parent;
			if (!parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_RL_NKFinalDestination();
				if (!parent.IsAir)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RL_NKFinalDestinationInfo);
				}
			}
		}

		protected override string MessageErrorDestinationPortCodeInvalid => Res.GetString("2a4aa4df-9601-49b0-b898-9797938b8509", "This Destination is invalid for this Entry Style.");

		protected override void CheckJE_PaymentMethodLogicForEU()
		{
		}

		protected override void ValidatePowerOfAttorney()
		{
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();

			if (Parent.JE_OA_Representative.IsEmpty)
			{
				CheckJE_OA_RepresentativeIsNotEmpty();
			}
		}

		protected virtual void CheckJE_OA_RepresentativeIsNotEmpty()
		{
			if (DeclarantTypeIsDIR)
			{
				Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("76A8248C-7005-4FD4-849B-343D249198EF", "Please enter a Representative for Rep. Type 'DIR'."));
			}
		}

		protected ZBool DeclarantTypeIsDIR => Parent.JE_DeclarantType == RepresentationTypeList.Codes._2Direct;

		protected void CheckBrokerHasWorkPhone()
		{
			var parent = Parent;

			if (parent.CusAgent is GlbStaff cusAgent && cusAgent.GS_WorkPhone.IsEmpty)
			{
				parent.JE_GS_NKCusAgentInfo.AddMessageError(Res.GetString("54E5E834-597D-40DA-8D63-7DCC78A089EC",
					"Please add the 'Work Phone' to the Broker's user profile. This is mandatory information for customs messages."));
			}
		}

		protected void CheckBrokerHasJobTitle()
		{
			var parent = Parent;

			if (parent.CusAgent is GlbStaff cusAgent && cusAgent.GS_Title.IsEmpty)
			{
				parent.JE_GS_NKCusAgentInfo.AddMessageError(Res.GetString("28155BE6-2D91-496F-8F41-768227865A0F",
					"Please add the 'Job Title' to the Broker's user profile. This is mandatory information for customs messages."));
			}
		}

		protected override void CheckJE_DeclarantType()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_DeclarantType();
			}
		}

		protected override void CheckJE_TransportMode()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_TransportMode();
			}
		}

		protected override void CheckJE_ContainerMode()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_ContainerMode();
			}
		}

		protected override void CheckJE_UCR()
		{
			if (!Parent.IsInwardProcessingAVABR)
			{
				base.CheckJE_UCR();
			}
		}
	}
}
