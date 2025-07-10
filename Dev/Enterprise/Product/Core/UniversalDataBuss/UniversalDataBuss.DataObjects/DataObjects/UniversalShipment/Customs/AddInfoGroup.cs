using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class AddInfoGroup : IDataObject,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		IOrganizationAddressCollectionParent
	{
		public AddInfoGroup()
		{
		}

		public AddInfoGroup(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[Mandatory]
		public CodeDescriptionPair Type { get; set; }

		public List<AddInfo> AddInfoCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }
	}
}

