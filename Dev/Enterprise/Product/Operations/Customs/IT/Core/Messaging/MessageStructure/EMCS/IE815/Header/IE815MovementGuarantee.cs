using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815MovementGuarantee
{
	public IE815MovementGuarantee(IMovementGuarantee movementGuarantee)
	{
		this.movementGuarantee = Argument.NotNull(movementGuarantee, "movementGuarantee");
	}
	readonly IMovementGuarantee movementGuarantee;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(4, false)]
	[MessageFieldRules("R")]
	public ZInt GuarantorTypeCode => movementGuarantee.GuarantorTypeCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldRules("C", "C070", "R081", "R085")]
	public ZString ConsignorCodeGuarantee => movementGuarantee.ConsignorCodeGuarantee;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldRules("C", "C070", "R082", "R085")]
	public ZString ConsignorTypeGuarantee => movementGuarantee.ConsignorTypeGuarantee;

	[MessageLayout(Order = 3)]
	[MessageFieldDecimalRepresentation(12, 2, false)]
	[MessageFieldRules("C", "C070", "R083", "R085")]
	public ZDecimal ConsignorDepositAmountCommitted => movementGuarantee.ConsignorDepositAmountCommitted;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldRules("C", "C071", "R081", "R086")]
	public ZString ConsigneeCodeGuarantee => movementGuarantee.ConsigneeCodeGuarantee;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldRules("C", "C071", "R082", "R086")]
	public ZString ConsigneeTypeGuarantee => movementGuarantee.ConsigneeTypeGuarantee;

	[MessageLayout(Order = 6)]
	[MessageFieldDecimalRepresentation(12, 2, false)]
	[MessageFieldRules("C", "C071", "R083", "R086")]
	public ZDecimal ConsigneeDepositAmountCommitted => movementGuarantee.ConsigneeDepositAmountCommitted;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldRules("C", "C072", "R081", "R087")]
	public ZString TransporterCodeGuarantee => movementGuarantee.TransporterCodeGuarantee;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldRules("C", "C072", "R082", "R087")]
	public ZString TransporterTypeGuarantee => movementGuarantee.TransporterTypeGuarantee;

	[MessageLayout(Order = 9)]
	[MessageFieldDecimalRepresentation(12, 2, false)]
	[MessageFieldRules("C", "C072", "R083", "R087")]
	public ZDecimal TransporterDepositAmountCommitted => movementGuarantee.TransporterDepositAmountCommitted;

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldRules("C", "C073", "R081", "R088")]
	public ZString OwnerCodeGuarantee => movementGuarantee.OwnerCodeGuarantee;

	[MessageLayout(Order = 11)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldRules("C", "C073", "R082", "R088")]
	public ZString OwnerTypeGuarantee => movementGuarantee.OwnerTypeGuarantee;

	[MessageLayout(Order = 12)]
	[MessageFieldDecimalRepresentation(12, 2, false)]
	[MessageFieldRules("C", "C073", "R083", "R088")]
	public ZDecimal OwnerDepositAmountCommitted => movementGuarantee.OwnerDepositAmountCommitted;

	[MessageLayout(Order = 13)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R", "R036")]
	public ZInt TotalGuarantorsIterations => movementGuarantee.Guarantors.Count();

	[MessageLayout(Order = 14)]
	public IEnumerable<IE815GuarantorTrader> Guarantors
	{
		get
		{
			int i = 0;
			foreach (var guarantor in movementGuarantee.Guarantors)
			{
				yield return new IE815GuarantorTrader(guarantor, ++i);
			}
		}
	}
}
