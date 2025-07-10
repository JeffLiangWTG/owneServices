using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeAddressValueObjectHelperForTesting : SysMergeAddressValueObjectHelper
	{
		public SysMergeAddressValueObjectHelperForTesting()
			: base("")
		{
		}

		public OrgAddress CreateFromValueObject_Exposed(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrgAddress addressValue, IValueObjectImportContext context)
		{
			return CreateFromValueObject(organisation, addressValue, context);
		}
	}
}
