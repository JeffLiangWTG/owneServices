//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGbCusAddInfoValidation
//
//    This class should be used for overriding validation in AutoGbCusAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.GB.Business.Declaration
{
	using System;
	using CargoWise.EntityFramework;

	public class GBCusAddInfoValidation : AutoGBCusAddInfoValidation
	{
		public GBCusAddInfoValidation(AutoGBCusAddInfo parent) : base(parent)
		{
		}

		public new GBCusAddInfo Parent
		{
			get { return (GBCusAddInfo)base.Parent; }
		}

		protected override void CheckG9_Nch1Priority()
		{
			base.CheckG9_Nch1Priority();
			ListValidation.MessageErrorIfInvalidCode(Parent.G9_Nch1PriorityInfo, Parent.GbNch1PriorityList);
		}

		protected override void CheckG9_Nch1RequestType()
		{
			base.CheckG9_Nch1RequestType();
			ListValidation.MessageErrorIfInvalidCode(Parent.G9_Nch1RequestTypeInfo, Parent.GbNch1RequestTypeList);
			if (Parent.G9_Nch1RequestType.StartsWith("C16", StringComparison.OrdinalIgnoreCase) && !Parent.Declaration.IsExport)
			{
				Parent.G9_Nch1RequestTypeInfo.AddMessageError("This option is only for exports");
			}
		}
	}
}
