using CargoWise.Common;
using CargoWise.Customs.CN.MessageContracts;

namespace Enterprise.Customs.CN.Business
{
	public class AcdAgrRequestOperInfoProvider : IAcdAgrRequestOperInfo
	{
		readonly CusEntryHeader entryHeader;

		public AcdAgrRequestOperInfoProvider(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		public string DeclarantCustomsCode => ((ICustomsEntryHeader)entryHeader).DeclarantCCD;

		public string Sign => string.Empty;

		public string OperationType => "1";
	}
}
