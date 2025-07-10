//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBROrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoBROrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BROrgImpAddInfoValidation : AutoBROrgImpAddInfoValidation
	{
		public BROrgImpAddInfoValidation(AutoBROrgImpAddInfo parent) : base(parent)
		{
		}

		new BROrgImpAddInfo Parent => base.Parent as BROrgImpAddInfo;

		protected override void CheckZO_BSBNumber()
		{
			base.CheckZO_BSBNumber();
			if (!Parent.ZO_BSBNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.ZO_BSBNumberInfo.AddMessageError(Res.GetString("5BD330FC-CF19-46B4-B18B-C250F027971A", "BSB number must be numeric"));
			}
		}

		protected override void CheckZO_BrokerCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ZO_BrokerCodeInfo);
			if (Parent.Broker != null)
			{
				GlbExternalPasswordValidation_CCT.CheckValidCertificate(Parent.BrokerCertificate, Parent.ZO_BrokerCodeInfo);
			}
		}
	}
}
