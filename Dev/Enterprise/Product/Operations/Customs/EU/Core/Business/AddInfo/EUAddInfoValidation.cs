//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUAddInfoValidation
//
//    This class should be used for overriding validation in AutoEUAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class EUAddInfoValidation : AutoEUAddInfoValidation
	{
		public EUAddInfoValidation(AutoEUAddInfo parent)
			: base(parent)
		{
			if (!parent.GetType().IsSubclassOf(typeof(AddInfo)))
			{
				throw new ArgumentException("Parent is not a subclass of AddInfo");
			}
		}

		public new AddInfo Parent => (AddInfo)base.Parent;

		protected override void CheckZG_CustomsAuthorisationReferenceForExportFallback()
		{
			base.CheckZG_CustomsAuthorisationReferenceForExportFallback();
			if (!Parent.ZG_CustomsAuthorisationReferenceForExportFallback.IsEmpty)
			{
				var jobDeclaration = (Parent.Parent as JobComInvoiceHeader)?.JobDeclaration;
				if (jobDeclaration != null)
				{
					CusEntryNumValidation.ValidateCustomsAuthorisationReferenceNumberType_Format(jobDeclaration.CountryCode, Parent.ZG_CustomsAuthorisationReferenceForExportFallbackInfo, jobDeclaration);
				}
			}
		}
	}
}
