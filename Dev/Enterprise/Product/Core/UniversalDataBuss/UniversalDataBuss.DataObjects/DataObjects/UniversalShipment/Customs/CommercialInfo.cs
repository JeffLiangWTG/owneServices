using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class CommercialInfo : IDataObject,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ITransportLogisticsCostCollectionParent
	{
		[MaxLength(35), CandidateKey]
		public ZString? Name { get; set; }
		public ZDateTime? DateOfLandedCostProcessing { get; set; }

		public DataObjectList<CommercialInvoiceHeader> CommercialInvoiceCollection { get; set; }
		public List<CommercialCharge> CommercialChargeCollection { get; set; }
		public List<AddInfo> AddInfoCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<CommercialInfo> SubGroupCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<TransportLogisticsCost> TransportLogisticsCostCollection { get; set; }
	}
}

