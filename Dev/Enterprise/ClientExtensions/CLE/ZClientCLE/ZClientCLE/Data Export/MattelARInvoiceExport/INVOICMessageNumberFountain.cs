using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class INVOICMessageNumberFountain
	{
		internal protected INVOICMessageNumberFountain()
		{
		}

		public static INVOICMessageNumberFountain Instance
		{
			get { return new INVOICMessageNumberFountain(); }
		}

		#region InterchangeNumber

		public INumberFountainProxy InterchangeNumber
		{
			get { return new FormattedNumberFountainFactory("INVOICInterchangeNumber", formatDigits: 7).New(); }
		}

#endregion
	}
}
