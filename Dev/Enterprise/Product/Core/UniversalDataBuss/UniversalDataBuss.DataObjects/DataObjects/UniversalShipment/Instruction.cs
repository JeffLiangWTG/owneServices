using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public partial class Instruction : IDataObject, ICustomizedFieldContainer
	{
		public Instruction()
		{
		}

		public Instruction(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public DropMode DropMode { get; set; }
		public CodeDescriptionPair Status { get; set; }
		public CodeDescriptionPair Type { get; set; }
		public CodeDescriptionPair TransportMode { get; set; }
		[CandidateKey]
		public ZInt? Sequence { get; set; }
		public OrganizationAddress Address { get; set; }
		[MaxLength(10), CodeMap(Constants.OrgPatternMatchOverrideRelationships.Equipment)]
		public ZString? Equipment { get; set; }
		[MaxLength(2147483646), AllowLineControlWhiteSpace]
		public ZString? ServiceInstruction { get; set; }
		public GreenhouseGasEmission GreenhouseGasEmission { get; set; }
		public ZBool? IsContainerRateable { get; set; }
		public ZBool? IsLooseRateable { get; set; }
		public ZBool? IsAuthorisedToLeave { get; set; }

		public List<InstructionContainerLink> InstructionContainerLinkCollection { get; private set; }
		public List<InstructionPackingLineLink> InstructionPackingLineLinkCollection { get; private set; }
		public List<CustomizedField> CustomizedFieldCollection { get; private set; }
		public DataObjectList<WeightData> WeightCollection { get; private set; }
	}
}
