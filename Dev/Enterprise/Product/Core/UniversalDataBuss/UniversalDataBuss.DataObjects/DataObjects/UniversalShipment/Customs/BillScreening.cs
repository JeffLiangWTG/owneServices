using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class BillScreening : IDataObject, ICustomsSupportingInformationCollectionParent
	{
		public BillScreening()
		{
		}

		public BillScreening(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(3)]
		public ZString? Result { get; set; }
		[MaxLength(70)]
		public ZString? AuthorisedPerson { get; set; }
		[MaxLength(1)]
		public ZString? PersonType { get; set; }
		[MaxLength(17)]
		public ZString? PersonIdentifier { get; set; }
		public OrganizationAddress FacilityPlace { get; set; }
		[MaxLength(35)]
		public ZString? FacilityPlaceSubDivision { get; set; }
		[MaxLength(35)]
		public ZString? FacilityPlaceNumber { get; set; }
		[MaxLength(70)]
		public ZString? FacilityPlacePOBox { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; private set; }
	}
}
