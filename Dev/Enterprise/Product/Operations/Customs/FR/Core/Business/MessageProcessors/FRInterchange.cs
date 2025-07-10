using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRInterchange : EDIInterchange
	{
		public FRInterchange(BusinessObjectFactory factory, DataRow row)
			 : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			OldEI_BodyText = EI_BodyText;

			base.OnSaving();

			var interchangeNumberReplacementString = GetInterchangeNumberReplacementString(EI_InterchangeNum);
			EI_HeaderText = EI_HeaderText.Replace(InterchangeNumberPlaceHolderHtml, interchangeNumberReplacementString);
			EI_BodyText = EI_BodyText.Replace(InterchangeNumberPlaceHolderHtml, interchangeNumberReplacementString);
		}
	}
}
