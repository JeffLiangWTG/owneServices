using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ForwardingConsolDataExportCSVFileNameProvider : IDataExportCSVFileNameProvider
	{
		public ForwardingConsolDataExportCSVFileNameProvider(ForwardingConsol consol)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get
			{
				var builder = new ZStringBuilder();
				if (consol != null)
				{
					builder.AppendIfNotEmpty(consol.JK_UniqueConsignRef);
					builder.AppendIfNotEmpty(consol.JK_MasterBillNum);
				}
				return builder.ToStringWithDelimiterBetweenAppends("_");
			}
		}
	}
}
