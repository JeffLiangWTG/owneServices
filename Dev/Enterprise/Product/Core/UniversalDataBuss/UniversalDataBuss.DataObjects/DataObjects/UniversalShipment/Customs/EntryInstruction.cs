using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class EntryInstruction : IDataObject,
		IOrganizationAddressCollectionParent,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ICustomsSupportingInformationCollectionParent
	{
		public EntryInstruction()
		{
		}

		public EntryInstruction(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZInt? Link { get; set; }
		[MaxLength(7)]
		public ZString? Style { get; set; }
		public CodeDescriptionPair SubStyle { get; set; }
		[MaxLength(50)]
		public ZString? Description { get; set; }
		public CodeDescriptionPair MergeBy { get; set; }

		public List<AddInfo> AddInfoCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public CodeDescriptionPair5Char CustomsOffice { get; set; }
		public ZDateTime? DateAtCustomsOffice { get; set; }
		public UNLOCO FirstArrival { get; set; }
		public CodeDescriptionPair35Char LocationAtClearance { get; set; }
		public ZDateTime? DateOfValuation { get; set; }
		public SealInfo SealInfo { get; set; }
		public List<SealNumber> SealNumberCollection { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; private set; }
		public List<LocationOfGoods> LocationOfGoodsCollection { get; private set; }
		[MaxLength(5)]
		public ZString? Procedure { get; set; }
	}
}
