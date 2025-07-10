using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocumentWrapperValidation : ZValidation
	{
		public SupportingDocumentWrapperValidation(SupportingDocumentWrapper parent) : base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
			Parent = parent;
		}

		SupportingDocumentWrapper Parent { get; }

		public void ValidateIsSelected()
		{
			ValidateCalculatedProperty(Parent.IsSelectedInfo);
		}

		protected void CheckIsSelected()
		{
			if (Parent.IsSelected)
			{
				if (Parent.SupportingDocument?.EDoc == ZGuid.Empty)
				{
					Parent.IsSelectedInfo.AddError(ValidationCaptions.SupportingDocumentWrapper.IsSelectedEDocMissing);
				}
			}
		}

		public override Type AutoValidationType => typeof(SupportingDocumentWrapperValidation);

		public override void ValidateAll()
		{
			ValidateIsSelected();
		}
	}
}
