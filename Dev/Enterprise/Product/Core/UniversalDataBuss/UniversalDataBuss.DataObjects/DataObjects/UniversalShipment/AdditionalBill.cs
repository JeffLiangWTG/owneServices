using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public partial class AdditionalBill : IDataObject,
		IOrganizationAddressCollectionParent,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ICustomsSupportingInformationCollectionParent
	{
		public AdditionalBill()
		{
		}

		public AdditionalBill(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(35)]
		public ZString? BillNumber { get; set; }
		public WayBillType BillType { get; set; }
		public ZDateTime? IssueDate { get; set; }
		[MaxLength(35)]
		public ZString? ParentBillNumber { get; set; }
		public ZInt? Link { get; set; }
		public CodeDescriptionPair MessageStatus { get; set; }
		public ZDecimal? NoOfPacks { get; set; }
		public PackageType PackType { get; set; }
		public CodeDescriptionPair LinePriceCurrency { get; set; }

		public List<AddInfo> AddInfoCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; set; }
	}
}
