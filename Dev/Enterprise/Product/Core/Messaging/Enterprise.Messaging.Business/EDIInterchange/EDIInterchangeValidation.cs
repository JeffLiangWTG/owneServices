using CargoWise.EntityFramework;
using CargoWise.Types;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIInterchangeValidation
//
//    This class should be used for overriding validation in AutoEDIInterchangeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Messaging.Business
{
	public class EDIInterchangeValidation : AutoEDIInterchangeValidation
	{
		public EDIInterchangeValidation(AutoEDIInterchange parent) : base(parent)
		{
		}

		protected override void CheckEI_HeaderTextIsWesternEuropean()
		{
			if (ShouldCheckText)
			{
				CheckText(Parent.EI_HeaderTextInfo);
			}
		}

		protected override void CheckEI_BodyTextIsWesternEuropean()
		{
			if (ShouldCheckText)
			{
				CheckText(Parent.EI_BodyTextInfo);
			}
		}

		protected override void CheckEI_FooterTextIsWesternEuropean()
		{
			if (ShouldCheckText)
			{
				CheckText(Parent.EI_FooterTextInfo);
			}
		}

		void CheckText(ZPropertyInfo info)
		{
			if (!((ZString)info.Value.ToString()).IsWindows1252OrEmpty)
			{
				info.AddError(EnglishCharactersValidation.GetNotificationMessage(info));
			}
		}

		protected override void CheckEI_BodyDataIsValidZBlobSize()
		{
			//do nothing.
		}

		bool ShouldCheckText
		{
			get
			{
				var interchange = Parent as EDIInterchange;
				return interchange == null || !(interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
			}
		}
	}
}
