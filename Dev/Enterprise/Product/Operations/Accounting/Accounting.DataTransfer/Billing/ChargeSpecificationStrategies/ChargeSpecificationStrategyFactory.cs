using System;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	internal static class ChargeSpecificationStrategyFactory
	{
		public static IChargeSpecificationStrategy NewStrategy(IValueObjectImportContext context, Xsd.ChargesSpecified chargesSpecified, Xsd.PostedChargeHandling postedChargeHandling)
		{
			switch (chargesSpecified)
			{
				case Xsd.ChargesSpecified.AllCharges: return new AllChargesStrategy(context, postedChargeHandling == Xsd.PostedChargeHandling.Ignore);
				case Xsd.ChargesSpecified.NewCharges: return new NewChargesStrategy();
				case Xsd.ChargesSpecified.UpdatedCharges: return new UpdatedChargesStrategy(context, postedChargeHandling == Xsd.PostedChargeHandling.Ignore);
				default: throw new ArgumentOutOfRangeException(nameof(chargesSpecified), chargesSpecified, "unrecognised value");
			}
		}
	}
}
