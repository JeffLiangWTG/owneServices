using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public sealed class NonPersistentFee : Enterprise.DocumentWrappers.Customs.EU.NonPersistentFee, IESDocSADHLineTaxBoxSupporter
	{
		public NonPersistentFee(CusEntryLineFee fee, BusinessObjectFactory factory) : base(fee, factory)
		{
			DestinationStateIsCanaryIsland = new ZBool(((JobDeclaration)fee.EntryLine?.Declaration)?.DestinationStateIsCanaryIsland);
			EntryLineMethodOfPayment = fee.EntryLine.RandomLine.ZG_MethodOfPayment;
			EntryLineMethodOfPayment2 = ((CusEntryLine)fee.EntryLine).RandomLine.ZG_MethodOfPayment2;
		}

		public ZBool DestinationStateIsCanaryIsland { get; }

		public ZString EntryLineMethodOfPayment { get; }

		public ZString EntryLineMethodOfPayment2 { get; }
	}
}
