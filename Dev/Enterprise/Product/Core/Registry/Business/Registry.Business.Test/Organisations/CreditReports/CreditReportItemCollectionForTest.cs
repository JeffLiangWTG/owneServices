using System.Xml.Serialization;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class CreditReportItemCollectionForTest : CreditReportItemCollection
	{
		public bool AllowSortForTest => base.AllowSort;

		public bool AllowNewCoreForTest => base.AllowNewCore;

		public bool AllowRemoveCoreForTest => base.AllowRemoveCore;
	}
}
