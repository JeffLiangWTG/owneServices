using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyModuleTimeFilter : ModuleTimeFilter
	{
		public DummyModuleTimeFilter()
			: base("DummyTimeFilter", DummyBizoSchema.Z0_SmallDateTime)
		{
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			string sqlDate2 = Property2.IsValid ? Property2.SqlFormat : ZString.Empty;
			writer.WriteElementString("Property2", sqlDate2);
		}
	}
}
