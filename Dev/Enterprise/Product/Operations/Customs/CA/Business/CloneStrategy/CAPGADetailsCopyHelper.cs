namespace Enterprise.Customs.CA.Business
{
	public static class CAPGADetailsCopyHelper
	{
		public static void CopyPGADetails(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			if (previousLine.CFIAPGAHeader != null)
			{
				((IPGAHeader)newLine.CFIAPGAHeader).CopyPersistentValuesFrom(previousLine.CFIAPGAHeader);
			}
			if (previousLine.CNSCPGAHeader != null)
			{
				((IPGAHeader)newLine.CNSCPGAHeader).CopyPersistentValuesFrom(previousLine.CNSCPGAHeader);
			}
			if (previousLine.DFOPGAHeader != null)
			{
				((IPGAHeader)newLine.DFOPGAHeader).CopyPersistentValuesFrom(previousLine.DFOPGAHeader);
			}
			if (previousLine.ECCCPGAHeader != null)
			{
				((IPGAHeader)newLine.ECCCPGAHeader).CopyPersistentValuesFrom(previousLine.ECCCPGAHeader);
			}
			if (previousLine.GACPGAHeader != null)
			{
				((IPGAHeader)newLine.GACPGAHeader).CopyPersistentValuesFrom(previousLine.GACPGAHeader);
			}
			if (previousLine.HCPGAHeader != null)
			{
				((IPGAHeader)newLine.HCPGAHeader).CopyPersistentValuesFrom(previousLine.HCPGAHeader);
			}
			if (previousLine.NRCanPGAHeader != null)
			{
				((IPGAHeader)newLine.NRCanPGAHeader).CopyPersistentValuesFrom(previousLine.NRCanPGAHeader);
			}
			if (previousLine.PHACPGAHeader != null)
			{
				((IPGAHeader)newLine.PHACPGAHeader).CopyPersistentValuesFrom(previousLine.PHACPGAHeader);
			}
			if (previousLine.TCPGAHeader != null)
			{
				((IPGAHeader)newLine.TCPGAHeader).CopyPersistentValuesFrom(previousLine.TCPGAHeader);
			}
		}
	}
}
