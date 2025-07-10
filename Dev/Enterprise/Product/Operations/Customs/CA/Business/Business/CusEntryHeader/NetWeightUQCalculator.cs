namespace Enterprise.Customs.CA.Business
{
	public class NetWeightUQCalculator : Customs.Business.WeightUQCalculator
	{
		public NetWeightUQCalculator(CusEntryHeader entryHeader) : base(entryHeader) { }

		protected override CargoWise.Types.ZDecimal HeaderWeight(Customs.Business.BaseJobComInvoiceHeader header)
		{
			return ((JobComInvoiceHeader)header).JZ_NetWeight;
		}

		protected override CargoWise.Types.ZString HeaderWeightUQ(Customs.Business.BaseJobComInvoiceHeader header)
		{
			return ((JobComInvoiceHeader)header).JZ_NetWeightUQ;
		}

		protected override CargoWise.Types.ZDecimal DeclarationWeight(Customs.Business.BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).CA_NetWeight;
		}

		protected override CargoWise.Types.ZString DeclarationWeightUQ(Customs.Business.BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).CA_NetWeightUQ;
		}

		protected override int MaximumEntryHeaderCount
		{
			get { return 2; }
		}
	}
}
