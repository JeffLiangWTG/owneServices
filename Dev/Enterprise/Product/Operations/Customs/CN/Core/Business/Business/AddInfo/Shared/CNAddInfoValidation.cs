//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCNAddInfoValidation
//
//    This class should be used for overriding validation in AutoCNAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;

namespace Enterprise.Customs.CN.Business
{
	public class CNAddInfoValidation : AutoCNAddInfoValidation
	{
		public CNAddInfoValidation(AutoCNAddInfo parent)
			: base(parent)
		{
			if (!parent.GetType().IsSubclassOf(typeof(AddInfo)))
			{
				throw new ArgumentException("Parent is not a subclass of AddInfo");
			}
		}

		public new AddInfo Parent => (AddInfo)base.Parent;
	}
}
