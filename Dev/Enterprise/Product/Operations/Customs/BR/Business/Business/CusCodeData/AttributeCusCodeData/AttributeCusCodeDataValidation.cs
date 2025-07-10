using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class AttributeCusCodeDataValidation : Customs.Business.CusCodeDataValidation
	{
		public AttributeCusCodeDataValidation(AttributeCusCodeData parent)
			: base(parent)
		{
		}

		public new AttributeCusCodeData Parent => (AttributeCusCodeData)base.Parent;

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			var targetInfo = Parent.CY_DataInfo;

			if (!Parent.ContentInfo.ReadOnly)
			{
				if (Parent.IsMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}

				if (Parent.MaxSize > 0 && Parent.CY_Data.Length > Parent.MaxSize)
				{
					targetInfo.AddMessageError(Res.GetString("ed69fce0-1e67-4c16-a23c-c89f845962c7", "Content exceeded the max size {0}", Parent.MaxSize));
				}
			}

			if (Parent.IsEffectiveInFuture)
			{
				targetInfo.AddWarning(Res.GetString("9643A13D-2D60-4B32-9B8E-6C4B940980E3", "This attribute has its effective date set to Start Date as determined by Customs and will remain unsent in messages until its effective date."));
			}
		}
	}
}
