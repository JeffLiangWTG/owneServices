using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ServiceTypeModuleMapping : AutoProductAreaSourceModuleMapping, IProductAreaMapping
	{
		public ServiceTypeModuleMapping()
		{
		}

		#region Properties

		[List("Lookups.ServiceTypeList")]
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
				return sourceModule != null ? (ZString)sourceModule.Description : ZString.Empty;
			}
		}

		CodeDescriptionPair GetSourceModule() =>
				EDIDataRegistry.Instance.ServiceTypes.Value.Cast<CodeDescriptionPair>().FirstOrDefault(s => s.Code.Equals(Code, System.StringComparison.InvariantCultureIgnoreCase));

		#endregion

		#region Validation

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
		}

		#endregion

		#region Implemenation

		public ServiceTypeModuleMappingLookups Lookups => lookups ?? (lookups = new ServiceTypeModuleMappingLookups(this));
		ServiceTypeModuleMappingLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceTypeModuleMapping();
		}

		#endregion
	}
}

