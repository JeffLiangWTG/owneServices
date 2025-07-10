using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class EntryLine : IDataObject,
		IAddInfoCollectionParent,
		ICustomsReferenceCollectionParent
	{
		[CandidateKey, Mandatory]
		public ZShort? LineNumber { get; set; }
		public ZDecimal? CustomsValue { get; set; }
		public ZDecimal? DutyRatePercent { get; set; }
		public ZDecimal? DutyRateFlatAmount { get; set; }
		public CodeDescriptionPair CustomsStatus { get; set; }
		public CodeDescriptionPair DutyRateFlatAmountUnit { get; set; }
		[MaxLength(512), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }
		[MaxLength(15)]
		public ZString? HarmonisedCode { get; set; }

		public List<EntryLineCharge> EntryLineChargeCollection { get; set; }
		public List<AddInfo> AddInfoCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; set; }
	}
}

