namespace Enterprise.DocumentVisualizer.Business
{
	public static class DocumentInfoExtensions
	{
		public static bool CanDeliver(this IDocumentInfo documentInfo)
		{
			if (documentInfo?.Descriptor?.PrintInstructions?.DeliveryModes == null)
			{
				return false;
			}

			foreach (var deliveryMode in documentInfo.Descriptor.PrintInstructions.DeliveryModes)
			{
				if (documentInfo?.Descriptor.PrintInstructions.GetNumberOfCopies(deliveryMode) > 0)
				{
					return true;
				}
			}

			return false;
		}
	}
}
