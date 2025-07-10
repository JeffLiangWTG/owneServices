using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSCusContainerValidation : CusContainerValidation
	{
		public EMCSCusContainerValidation(EMCSCusContainer parent)
			: base(parent)
		{
		}

		protected new EMCSCusContainer Parent => (EMCSCusContainer)base.Parent;

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateSealDetails();
				ValidateComment();
			}
		}

		#endregion

		#region Identity

		protected virtual void CheckCO_ContainerNumber_Mandatory()
		{
			if (Parent.ZG_UnitCode != EMCSTransportUnitCodeList.Codes.FixedTransportInstallations)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_ContainerNumberInfo);
			}
		}

		protected override void CheckCO_ContainerNumber()
		{
			CheckCO_ContainerNumber_Mandatory();
			if (!Parent.CO_ContainerNumber.IsEmpty)
			{
				if (Parent.ZG_UnitCode == EMCSTransportUnitCodeList.Codes.Container)
				{
					ContainerNumberValidation.WarnIfInvalid(Parent.CO_ContainerNumberInfo);
				}
				CheckCusContainerIsUnique(Parent.CO_ContainerNumberInfo);
			}
		}

		#endregion

		protected override void CheckCO_RC()
		{
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
		}

		protected override void CheckCO_Seal()
		{
		}

		#region SealDetails

		public void ValidateSealDetails()
		{
			ValidateCalculatedProperty(Parent.SealDetailsInfo);
		}

		#endregion

		#region Comment

		public void ValidateComment()
		{
			ValidateCalculatedProperty(Parent.CommentInfo);
		}

		#endregion

		#region Implementation

		void CheckCusContainerIsUnique(ZPropertyInfo info)
		{
			var thisDoc = Parent;

			if (thisDoc.CO_ContainerNumber.IsEmpty)
			{
				return;
			}

			var declaration = thisDoc.Declaration;
			var allOtherDocs = declaration?.CusContainers.Cast<EMCSCusContainer>().Where(c => c != thisDoc);
			if (allOtherDocs != null)
			{
				var otherDocsInError = new List<EMCSCusContainer>();

				var identityMustBeUnique = Res.GetString("{13E8FBA0-4B3F-46F1-99AF-554A80F7AF08}", "Identity must be unique.");
				foreach (var doc in allOtherDocs)
				{
					if (doc.CO_ContainerNumber == thisDoc.CO_ContainerNumber)
					{
						info.AddMessageError(identityMustBeUnique);
						break;
					}

					if (doc.CO_ContainerNumberInfo.HasMessageError(identityMustBeUnique))
					{
						otherDocsInError.Add(doc);
					}
				}

				if (!info.HasMessageErrors())
				{
					// clear error message on other party (if exists)
					foreach (var doc in otherDocsInError)
					{
						doc.Validation.ValidateCO_ContainerNumber();
					}
				}
			}
		}

		#endregion
	}
}
