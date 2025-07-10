using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public class T2LClearanceEmailObject : IT2LClearanceEmailProvider
	{
		public T2LClearanceEmailObject(ZString csvClearance)
		{
			CSVClearance = csvClearance;
		}

		public ZString CSVClearance { get; }
	}
}
