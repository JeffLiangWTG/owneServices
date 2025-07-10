using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE810HeaderProvider : HeaderProvider, IIE810Header
	{
		public IE810HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IEMCSCancellation cancellationOfEad) : base(emcsJobDeclaration)
		{
			this.cancellationOfEad = Argument.NotNull(cancellationOfEad, nameof(cancellationOfEad));
		}
		readonly IEMCSCancellation cancellationOfEad;

		public int CancellationReasonCode => ZInt.ParseSafe(cancellationOfEad.Reason, ZInt.Zero);

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(cancellationOfEad.Information);

		public DateTime? DateAndTimeOfValidationOfCancellation => this.IsValidationAttributeAllowed ? ZDateTime.Now.ToDateTime().ToUnspecifiedKindWithSecondsPrecision() : null;
	}
}
