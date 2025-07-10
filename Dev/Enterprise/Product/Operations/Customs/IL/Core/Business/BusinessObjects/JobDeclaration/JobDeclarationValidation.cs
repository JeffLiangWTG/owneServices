using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class JobDeclarationValidation : AutoILJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageTypeInfo, Parent.Lookups.MessageTypeList);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MessageTypeInfo);
		}

		protected override void CheckJE_MessageSubType()
		{
			var parent = (JobDeclaration)Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.JE_MessageSubTypeInfo, parent.Lookups.MessageSubTypeList);
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_MessageSubTypeInfo);
		}

		protected override void CheckJE_TransportMeans()
		{
			base.CheckJE_TransportMeans();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportMeansInfo);
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_DeclarantAddressInfo);
		}

		protected override void CheckJE_ManifestNumber()
		{
			base.CheckJE_ManifestNumber();

			var parent = Parent;

			if (parent.TransportMode == Core.Constants.TransportModes.Sea)
			{
				if (!parent.JE_ManifestNumber.IsNumbersOnlyOrEmpty)
				{
					parent.JE_ManifestNumberInfo.AddMessageError(Constants.JobDeclaration.ManifestNumberSeaDigitsOnly);
				}

				if (parent.JE_ManifestNumber.Length > JobDeclaration.Schema.JE_ManifestNumberMaxLengthSea)
				{
					parent.JE_ManifestNumberInfo.AddMessageError(Constants.JobDeclaration.ManifestNumberSeaMaxLength);
				}
			}
			else if (parent.TransportMode == Core.Constants.TransportModes.Road)
			{
				if (parent.JE_ManifestNumber.Length > JobDeclaration.Schema.JE_ManifestNumberMaxLengthRoa)
				{
					parent.JE_ManifestNumberInfo.AddMessageError(Constants.JobDeclaration.ManifestNumberRoaMaxLength);
				}
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo);
		}
	}
}
