using System;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ProductXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public ProductXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			if ((++objectCounter) % 20 == 0)
			{
				if (!context.NotificationsHasErrors)
				{
					context.FactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
				}

				if (!context.Factory.IsValidationSuspended)
				{
					context.Factory.SuspendValidation();
				}
				context.Factory.RefreshEnabled = false;
			}

			return dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
		}

		int objectCounter;
	}
}
