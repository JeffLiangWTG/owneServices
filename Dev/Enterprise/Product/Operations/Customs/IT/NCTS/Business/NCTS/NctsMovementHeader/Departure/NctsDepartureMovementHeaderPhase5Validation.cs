using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsDepartureMovementHeaderPhase5Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation
{
	public NctsDepartureMovementHeaderPhase5Validation(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	protected override void CheckBM_PlaceOfUnloadingMandatory()
	{
		// Intentionally kept blank
	}

	protected override void CheckBM_ForeignDestPortKCodeMandatory()
	{
		// Intentionally kept blank
	}

	protected override void CheckTirCarnetExpiryDateMandatory()
	{
		// Intentionally kept blank
	}

	protected override void CheckBM_MethodOfPayment()
	{
		base.CheckBM_MethodOfPayment();

		var parent = Parent;
		var header = parent.Header;
		if (header.IsInPhase5TransitionPeriod)
		{
			var validator = new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => parent?.BM_MethodOfPayment ?? ZString.Empty,
			lineValuesProvider: () => header.GetGoodsItems().Select(x => x.BY_TransportChargesMethodOfPayment))
			{
				IsEmptyFunc = x => x.IsEmpty
			};
			validator.ValidateHeader(parent.BM_MethodOfPaymentInfo);
		}
		else if (!parent.BM_MethodOfPaymentInfo.ReadOnly)
		{
			new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => parent?.BM_MethodOfPayment ?? ZString.Empty,
			lineValuesProvider: () => header.Bills.Select(x => x.B0_TransportPaymentMethod))
			{
				IsEmptyFunc = x => x.IsEmpty,
				IgnoredHeaderSameLinesMessageProvider = () => ValidationCaptions.NctsDepartureMovementHeader.HeaderLevelWillBeIgnoredHousesHaveSameValue,
				IgnoredHeaderWithLinesValuesMessageProvider = () => ValidationCaptions.NctsDepartureMovementHeader.HeaderLevelWillBeIgnoredHousesHaveDifferentValue
			}.ValidateHeader(parent.BM_MethodOfPaymentInfo);
		}
	}

	protected override bool ShouldValidateInlandTransportList() => false;
}
