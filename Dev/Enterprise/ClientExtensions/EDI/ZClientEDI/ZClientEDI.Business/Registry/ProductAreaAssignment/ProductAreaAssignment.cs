using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductAreaAssignment : AutoProductAreaAssignment
	{
		public ProductAreaAssignment() { }

		public ProductAreaAssignment(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public ProductAreaAssignmentLookups Lookups
		{
			get { return lookups ?? (lookups = new ProductAreaAssignmentLookups(this)); }
		}
		ProductAreaAssignmentLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ProductAreaAssignment(fallbackLevel, factory);
		}

		#region Properties

		[List("Lookups.ProductAreaList")]
		public override ZString ProductArea
		{
			get { return base.ProductArea; }
			set { base.ProductArea = value; }
		}

		[List("Lookups.StaffList")]
		public override ZString Staff
		{
			get { return base.Staff; }
			set { base.Staff = value; }
		}

		#endregion

		#region Validation

		public override void ValidateProductArea()
		{
			base.ValidateProductArea();
			MandatoryValidation.CheckEntered(ProductAreaInfo);
			ListValidation.ErrorIfInvalidCode(ProductAreaInfo);
		}

		public override void ValidateStaff()
		{
			base.ValidateStaff();
			MandatoryValidation.CheckEntered(StaffInfo);
			ListValidation.ErrorIfInvalidCode(StaffInfo);
		}

		#endregion
	}
}

