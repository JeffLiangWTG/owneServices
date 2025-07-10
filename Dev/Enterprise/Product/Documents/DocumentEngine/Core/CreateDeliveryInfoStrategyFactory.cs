namespace Enterprise.DocumentEngine
{
	using CargoWise.Application;
	using Enterprise.DocumentEngine.DeliveryMethods;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;

	public interface ISupportCreateDeliveryInfoStrategyFactory
	{
		string AttachmentType { get; }
	}

	class CreateDeliveryInfoStrategyFactory
	{
		public ICreateDeliveryInfoStrategy Create(DeliveryMethod deliveryMethod, IStmMenuItem menuItem = null)
		{
			if (menuItem != null
				&& menuItem.SU_MenuType == Core.Constants.StmMenuItemTypes.Forms)
			{
				return ObjectFactory.Get<IFormDeliveryInfoStrategy>();
			}

			var supportedMethod = deliveryMethod as ISupportCreateDeliveryInfoStrategyFactory;
			if (supportedMethod != null)
			{
				switch (supportedMethod.AttachmentType)
				{
					case AttachmentTypeList.Codes.Csv:
						return new CsvCreateDeliveryInfoStrategy() { IncludeColumnHeadings = false };

					case AttachmentTypeList.Codes.CsvWithHeadings:
						return new CsvCreateDeliveryInfoStrategy() { IncludeColumnHeadings = true };

					case AttachmentTypeList.Codes.Xml:
						return new XmlCreateDeliveryInfoStrategy();

					case AttachmentTypeList.Codes.Txt_Comm:
					case AttachmentTypeList.Codes.Txt_Semi:
					case AttachmentTypeList.Codes.Txt_Pipe:
						return new TxtCreateDeliveryInfoStrategy(GetDelimiterByAttachmentType(supportedMethod.AttachmentType));
				}
			}

			return new DefaultCreateDeliveryInfoStrategy();
		}

		string GetDelimiterByAttachmentType(string attachmentType)
		{
			return attachmentType.Substring(attachmentType.Length - 1, 1);
		}
	}
}
