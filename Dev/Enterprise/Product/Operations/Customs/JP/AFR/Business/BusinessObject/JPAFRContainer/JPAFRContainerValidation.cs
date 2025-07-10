//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRContainerValidation
//
//    This class should be used for overriding validation in AutoJPAFRContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRContainerValidation : AutoJPAFRContainerValidation
	{
		public JPAFRContainerValidation(AutoJPAFRContainer parent1)
			: base(parent1)
		{
			var parent = parent1 as JPAFRContainer;
			this.isShippingLineEntry = parent != null && parent.IsBillShippingLineEntry;
		}

		readonly bool isShippingLineEntry;

		protected new JPAFRContainer Parent
		{
			get { return (JPAFRContainer)base.Parent; }
		}

		protected override void CheckJPC_ContainerNum()
		{
			base.CheckJPC_ContainerNum();
			if (Parent.JPC_ContainerNum.IsEmpty)
			{
				Parent.JPC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.ContainerNumberIsRequired);
			}
			else if (Parent.JPC_ContainerNum.Length > ValidationConstants.Constants.ContainerNumMaxLength)
			{
				Parent.JPC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.MaximumContainerNumberLengthExceeded);
			}
			else if (Parent.JPC_ContainerNum.KeepAlphanumericCharacters() != Parent.JPC_ContainerNum)
			{
				Parent.JPC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.ContainerNumberShouldContainOnlyAlphanumeric);
			}
			else
			{
				CheckContainerNumberIsNotDuplicated();
				ContainerNumberValidation.WarnIfInvalid(Parent.JPC_ContainerNumInfo);
			}
		}

		protected override void CheckJPC_RC_ContainerType()
		{
			base.CheckJPC_RC_ContainerType();
			var targetInfo = Parent.JPC_RC_ContainerTypeInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			var targetContainer = Parent.ContainerType;
			if (targetContainer != null)
			{
				targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetContainer.RC_Code);
			}
		}

		protected override void CheckJPC_OwnershipCode()
		{
			base.CheckJPC_OwnershipCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JPC_OwnershipCodeInfo);
		}

		protected override void CheckJPC_Seal1()
		{
			base.CheckJPC_Seal1();
			var targetValue = Parent.JPC_Seal1;
			var targetInfo = Parent.JPC_Seal1Info;
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetValue);
			if (targetValue.IsEmpty && Parent.JPC_Seal2.IsEmpty)
			{
				targetInfo.AddWarning(ValidationConstants.Container.NoSealRequiredWhenNoSealNumberPresents);
			}
		}

		protected override void CheckJPC_Seal2()
		{
			base.CheckJPC_Seal1();
			ValidateJPC_Seal1();
			Parent.JPC_Seal2Info.AddMessageErrorIfContainsInvalidNACCSCharacter(Parent.JPC_Seal2);
		}

		protected override void CheckJPC_TypeOfService()
		{
			base.CheckJPC_TypeOfService();
			if (isShippingLineEntry)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JPC_TypeOfServiceInfo);
			}
		}

		protected override void CheckJPC_VanningType()
		{
			base.CheckJPC_VanningType();
			if (isShippingLineEntry)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JPC_VanningTypeInfo);
			}
		}

		protected override void CheckJPC_CCCApplicationId()
		{
			base.CheckJPC_CCCApplicationId();
			if (isShippingLineEntry)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JPC_CCCApplicationIdInfo);
			}
		}

		#region Util

		void CheckContainerNumberIsNotDuplicated()
		{
			var bill = Parent.Bill;
			if (bill != null && bill.Containers != null)
			{
				if (bill.Containers.Any(x => x.JPC_ContainerNum == Parent.JPC_ContainerNum && x != Parent))
				{
					Parent.JPC_ContainerNumInfo.AddError(ValidationConstants.Container.ContainerNumberIsDuplicated);
				}
			}
		}

		#endregion
	}
}
