using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ExceptionKeyStacktraceDepthCollection : RegistryBusinessObjectCollectionTemplate<ExceptionKeyStacktraceDepth>
	{
		public ExceptionKeyStacktraceDepthCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public ExceptionKeyStacktraceDepthCollection()
			: base(null, null)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExceptionKeyStacktraceDepthCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExceptionKeyStacktraceDepth(CurrentFallbackLevel, CurrentFactory);
		}

		public ExceptionKeyStacktraceDepth this[string exceptionType]
		{
			get { return this.Cast<ExceptionKeyStacktraceDepth>().FirstOrDefault(x => x.ExceptionType == exceptionType); }
		}
	}
}

