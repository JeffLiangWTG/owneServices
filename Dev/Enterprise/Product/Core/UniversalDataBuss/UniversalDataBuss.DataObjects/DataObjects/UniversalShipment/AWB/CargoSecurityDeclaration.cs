using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public sealed partial class CargoSecurityDeclaration : IDataObject
	{
		public CargoSecurityDeclaration()
		{
		}

		public CargoSecurityDeclaration(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public CodeDescriptionPair AgentApprovalCategory { get; set; }

		[MaxLength(15)]
		public ZString? AgentApprovalNumber { get; set; }

		public ZDateTime? AgentApprovalExpiryDate { get; set; }

		public Country AgentApprovalCountry { get; set; }

		public CodeDescriptionPair SecurityStatus { get; set; }

		public ZDateTime? SecurityStatusIssueDate { get; set; }

		[MaxLength(50)]
		public ZString? SecurityStatusIssuedBy { get; set; }

		[MaxLength(256)]
		public ZString? AdditionalScreeningMethods { get; set; }

		[MaxLength(512), AllowLineControlWhiteSpace]
		public ZString? TSASecurityStatement { get; set; }

		[MaxLength(512), AllowLineControlWhiteSpace]
		public ZString? AdditionalSecurityInformation { get; set; }

		[MaxLength(100)]
		public ZString? AdditionalSecurityInformationStatement { get; set; }

		public List<ReceivedFromShipper> ReceivedFromShipperCollection { get; private set; }
		public List<CodeDescriptionPair> ScreeningMethodCollection { get; private set; }
		public List<CodeDescriptionPair4Char> GroundsForExemptionCollection { get; private set; }
	}
}
