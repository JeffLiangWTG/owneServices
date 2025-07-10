using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccPrintingUtilitySubClassForTesting : AccPrintingUtility
	{
		public AccPrintingUtilitySubClassForTesting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccPrintingUtilitySubClassForTesting(BusinessObjectFactory factory, Constants.DataContext context)
			: base(factory, context)
		{
		}

		protected override DeliveryInstructionDestination RunPrintSet(DocumentCommand command, AllowedDeliveryOptions options, ZGuid printQueuePK)
		{
			if (!printQueuePK.IsValid)
			{
				PrintTask task = new PrintTask(command);
				DeliveryInstructions instruction = new DeliveryInstructions();
				instruction.Destination = DeliveryInstructionDestination.None;
				task.Run(instruction);
				return DeliveryInstructionDestination.None;
			}
			else
			{
				return base.RunPrintSet(command, options, printQueuePK);
			}
		}

		protected override DeliveryInstructionDestination RunPrintTask(DocumentPack docPack, DocumentCommand docCommand, DeliveryInstructions instructions, AllowedDeliveryOptions options)
		{
			PrintTask task = new PrintTask();
			task.Add(docPack);
			DeliveryInstructions instruction = new DeliveryInstructions();
			instruction.Destination = DeliveryInstructionDestination.None;
			task.Run(instruction);
			return DeliveryInstructionDestination.None;
		}
	}
}
