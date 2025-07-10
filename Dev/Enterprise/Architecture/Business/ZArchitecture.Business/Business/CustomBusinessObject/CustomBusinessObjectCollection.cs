using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class CustomBusinessObjectCollection : NonPersistentBusinessObjectCollection<CustomBusinessObject>, IDynamicBusinessObjectCollection
	{
		public CustomBusinessObjectCollection(BusinessObjectFactory factory, ICustomPropertyCollection properties)
			: base(factory)
		{
			this.properties = properties;
		}

		public CustomBusinessObjectCollection(ICustomPropertyCollection properties)
			: this(null, properties)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomBusinessObject(Factory, null, properties);
		}

		#region IDynamicBusinessObjectCollection Members

		IDynamicBusinessObject IDynamicBusinessObjectCollection.Template
		{
			get
			{
				if (template == null)
				{
					template = (CustomBusinessObject)CreateNonPersistentBusinessObject();
				}

				return template;
			}
		}
		CustomBusinessObject template;

		#endregion

		readonly ICustomPropertyCollection properties;
	}
}
