using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Outer), RootElement("SoManyKeys")]
	public class UberCandidateKeyObject : IDataObject
	{
		[ReferenceProperty, MaxLength(25)]
		public ZString? ReferenceData { get; set; }

		[CandidateKey, Mandatory]
		public UberEnum? EnumCandidateKey { get; set; }
		[CandidateKey]
		public ZBool? BooleanCandidateKey { get; set; }
		[CandidateKey]
		public ZDateTime? DateCandidateKey { get; set; }
		[CandidateKey]
		public ZDecimal? DecimalCandidateKey { get; set; }
		[CandidateKey]
		public ZInt? IntCandidateKey { get; set; }
		[CandidateKey]
		public ZLong? LongCandidateKey { get; set; }
		[CandidateKey, Mandatory]
		public UberUNLOCO MandatoryUNLOCOCandidateKey { get; set; }
		[CandidateKey]
		public UberUNLOCO NonMandatoryUNLOCOCandidateKey { get; set; }
		[CandidateKey]
		public ZShort? ShortCandidateKey { get; set; }
		[CandidateKey, MaxLength(2048)]
		public ZString? StringCandidateKey { get; set; }
		[CandidateKey, MaxLength(1024)]
		public ZString? ZStringCandidateKey { get; set; }

		public List<UberCandidateKeyObject> CandidateKeyObjectCollection { get; set; }
	}

	public enum UberEnum
	{
		Candidate,
		Key,
		Type
	}
}
