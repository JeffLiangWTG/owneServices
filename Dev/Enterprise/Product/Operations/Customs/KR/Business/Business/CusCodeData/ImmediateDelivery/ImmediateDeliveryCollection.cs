using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImmediateDeliveryCollection : CusCodeDataCollection<ImmediateDelivery>
	{
		public ImmediateDeliveryCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.ImmediateDelivery)
		{
		}

		public ImmediateDeliveryCollection(CusEntryLine entryLine)
			: base(entryLine, CusCodeDataTypeList.Codes.ImmediateDelivery)
		{
		}

		public void AddNewIfRequired(ZString immediateDeliveryNumber)
		{
			var immediateDelivery = this.Cast<ImmediateDelivery>().FirstOrDefault(x => x.CY_Data == immediateDeliveryNumber);
			if (immediateDelivery == null)
			{
				immediateDelivery = AddNew();
				immediateDelivery.CY_Data = immediateDeliveryNumber;
			}
		}
	}
}
