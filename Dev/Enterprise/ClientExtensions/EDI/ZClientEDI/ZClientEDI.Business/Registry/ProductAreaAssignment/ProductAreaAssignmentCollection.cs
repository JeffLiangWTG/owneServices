using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductAreaAssignmentCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ProductAreaAssignmentCollection()
			: base() { }

		public ProductAreaAssignmentCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new ProductAreaAssignment this[int index]
		{
			get { return (ProductAreaAssignment)Elements[index]; }
		}

		public new ProductAreaAssignment AddNew()
		{
			return (ProductAreaAssignment)base.AddNew();
		}

		public ProductAreaAssignment AddNew(ZString productArea, ZString staff)
		{
			ProductAreaAssignment result = AddNew();
			result.ProductArea = productArea;
			result.Staff = staff;
			return result;
		}

		public ZString GetAssignedStaffCodeByProductArea(string productAreaCode)
		{
			ZString result = ZString.Empty;
			string expectedProductArea = string.IsNullOrEmpty(productAreaCode) ? ProductAreaAssignmentLookups.BlankProductAreaCode : productAreaCode;
			foreach (ProductAreaAssignment assignment in this)
			{
				if (assignment.ProductArea == expectedProductArea)
				{
					result = assignment.Staff;
				}
			}
			return result;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ProductAreaAssignmentCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProductAreaAssignment(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion
	}
}

