using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RegistryWrapper : GenericWrapper
	{
		public RegistryWrapper(BusinessObjectFactory factory)
			: base(null, factory)
		{
		}

		public ZString CertificateOfOriginClause
		{
			get { return DocumentsDataRegistry.Instance.CertificateOfOriginStandardClause.Value; }
		}
	}
}
