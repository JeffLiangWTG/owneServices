using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASConsolExportAWBRateLineCollection : ConsolExportAWBRateLineCollection
	{
		public JASConsolExportAWBRateLineCollection(JASConsolExportAWBHeader master)
			: base(master)
		{
		}

		public new JASConsolExportAWBRateLine this[int index]
		{
			get { return (JASConsolExportAWBRateLine)(Elements[index]); }
		}

		public new JASConsolExportAWBRateLine AddNew()
		{
			return (JASConsolExportAWBRateLine)base.AddNew();
		}
	}
}
