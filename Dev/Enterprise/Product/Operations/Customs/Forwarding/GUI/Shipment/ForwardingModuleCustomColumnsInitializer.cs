using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Forwarding.GUI
{
	public class ForwardingZGridCustomColumnsInitializer : ZGridCustomColumnsInitializer
	{
		public ForwardingZGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, ICustomPropertyContainer propertyContainer)
			: base(grid, collection, groupName)
		{
			this.propertyContainer = propertyContainer;
		}

		readonly ICustomPropertyContainer propertyContainer;

		public void AddCustomColumns()
		{
			AddCustomColumns(propertyContainer.CustomProperties);
		}

		protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
		{
			return new ForwardingCustomPropertyDescriptor(propertyContainer, property);
		}

		#region ForwardingCustomPropertyDescriptor

		class ForwardingCustomPropertyDescriptor : ZCustomPropertyDescriptor
		{
			public ForwardingCustomPropertyDescriptor(ICustomPropertyContainer propertyContainer, ICustomProperty property)
				: base(property.Identifier, property.Info.Type)
			{
				this.propertyContainer = propertyContainer;
			}

			protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
			{
				return propertyContainer;
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}

			readonly ICustomPropertyContainer propertyContainer;
		}

		#endregion
	}
}
