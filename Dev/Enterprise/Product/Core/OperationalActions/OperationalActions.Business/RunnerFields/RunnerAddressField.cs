using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerAddressField : RunnerField<OperationalActionAddressFieldSupporter, ZGuid>
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : RunnerField<OperationalActionAddressFieldSupporter, ZGuid>.Schema
		{
			public const string Organisation_List = "Organisation_List";
		}

		#endregion

		public RunnerAddressField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionAddressFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		public ZAddress Property_ZAddress
		{
			get { return property_ZAddress ?? (property_ZAddress = NewProperty_ZAddress()); }
		}
		ZAddress property_ZAddress;

		ZAddress NewProperty_ZAddress()
		{
			ZAddress address = new ZAddress(PropertyInfo);
			address.DefaultAddressType = FieldSupporter.DefaultAddressType;
			return address;
		}

		public IBusinessObjectCollection Organisation_List
		{
			get { return organisation_List ?? (organisation_List = FieldSupporter.ConstructCollection(Factory)); }
		}
		IBusinessObjectCollection organisation_List;
	}
}
