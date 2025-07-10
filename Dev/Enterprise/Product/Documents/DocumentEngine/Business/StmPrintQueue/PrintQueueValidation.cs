using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine
{
	public static class PrintQueueValidation
	{
		public static void ValidatePrintQueue(ZPropertyInfo printQueuePKInfo)
		{
			StmPrintQueue printQueue = printQueuePKInfo.BizObj.Factory.Load<StmPrintQueue>((ZGuid)printQueuePKInfo.Value);
			if (printQueue != null)
			{
				if (!printQueue.IsOnline)
				{
					printQueuePKInfo.AddError(Res.GetString("5f9d3873-f164-4b3c-924e-cb0cb5c9acbd", "The printer you have selected is not currently installed. Please see your system administrator."));
				}
				else if (!printQueue.IsPrintAllowed)
				{
					printQueuePKInfo.AddError(Res.GetString("a57267d9-da8b-4ac7-bfaf-d12a481453b9", "You do not have the security rights to print to the selected printer."));
				}
			}
		}
	}
}
