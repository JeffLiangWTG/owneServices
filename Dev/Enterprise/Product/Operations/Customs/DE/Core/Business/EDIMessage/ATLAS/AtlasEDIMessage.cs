using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class AtlasEDIMessage : DEEDIMessage
	{
		public AtlasEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber() => DEEDIMessageSharedHelpers.GetDEMessageReferenceNumber(Factory);

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			DEEDIMessageSharedHelpers.GetInterchangeControlReferenceAndFillInPlaceHolders(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
		}
	}
}
