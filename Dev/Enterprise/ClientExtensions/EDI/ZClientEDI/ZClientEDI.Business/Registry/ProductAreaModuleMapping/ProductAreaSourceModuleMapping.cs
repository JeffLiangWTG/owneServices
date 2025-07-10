using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductAreaSourceModuleMapping : AutoProductAreaSourceModuleMapping, IProductAreaMapping
	{
		public ProductAreaSourceModuleMapping()
		{
		}

		#region Properties

		[List("Lookups.SourceModuleList")]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		public ZString Description
		{
			get
			{
				var sourceModule = GetSourceModule();
				return sourceModule != null ? sourceModule.Description : ZString.Empty;
			}
		}

		public ZString ModuleTreePath
		{
			get
			{
				var sourceModule = GetSourceModule();
				return sourceModule != null ? sourceModule.Path : ZString.Empty;
			}
		}

		[List("Lookups.ProductAreaList")]
		public override ZString ProductArea
		{
			get { return base.ProductArea; }
			set { base.ProductArea = value; }
		}

		SourceModule GetSourceModule()
		{
			return ProductAreaSourceModuleMappingHelper.SourceModules.Cast<SourceModule>().FirstOrDefault(s => s.Code.EqualsIgnoringCase(Code));
		}

		#endregion

		#region Validation

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
		}

		public override void ValidateProductArea()
		{
			base.ValidateProductArea();
			MandatoryValidation.CheckEntered(ProductAreaInfo);
			ListValidation.ErrorIfInvalidCode(ProductAreaInfo);
		}

		#endregion

		#region Implemenation

		public ProductAreaSourceModuleMappingLookups Lookups
		{
			get { return lookups ?? (lookups = new ProductAreaSourceModuleMappingLookups(this)); }
		}
		ProductAreaSourceModuleMappingLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ProductAreaSourceModuleMapping();
		}

		#endregion
	}
}

