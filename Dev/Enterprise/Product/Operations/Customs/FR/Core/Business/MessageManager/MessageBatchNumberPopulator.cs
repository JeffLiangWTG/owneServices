using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Environment;

namespace Enterprise.Customs.FR.Business
{
	public class MessageBatchNumberPopulator
	{
		public MessageBatchNumberPopulator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public void PopulateBatchNumber(FREDIMessage message)
		{
			if (batchNumber.IsEmpty)
			{
				batchNumber = Env.NumberFountains.FRMessageBatchNumber.GetNextFormatted(factory);
			}

			message.EM_ApplicationReference = batchNumber;
		}
		ZString batchNumber;
	}
}
