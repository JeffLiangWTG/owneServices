using System.Xml.Serialization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed public class PackageTypePairForTest : PackageTypePair
	{
		protected override PackageTypePair GetInstanceForClone()
		{
			return new PackageTypePairForTest();
		}

		public override CodeDescriptionPairList CustomsPackageTypesList
		{
			get
			{
				if (packageTypesList == null)
				{
					packageTypesList = new CodeDescriptionPairList();
					packageTypesList.AddPair(SampleCode, SampleDescription);
				}
				return packageTypesList;
			}
		}
		CodeDescriptionPairList packageTypesList;

		internal const string SampleCode = "AAA";
		internal const string SampleDescription = "This is the sample";
	}
}
