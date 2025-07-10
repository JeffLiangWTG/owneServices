using System.Xml.Serialization;
using Enterprise.Registry.Business;

namespace ZClientEDI.Business.Registry
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class TcaRimEnrollmentSchemeCollection : CodeDescriptionBoolCollection
	{
		public TcaRimEnrollmentSchemeCollection() : base(CodeLength)
		{
		}

		public static int CodeLength => 16;

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new TcaRimEnrollmentSchemeCollection();
		}
	}
}
