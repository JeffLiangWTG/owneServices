namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CalcExportManifestLine : AutoCalcExportManifestLine
	{
		public CalcExportManifestLine(CalcExportManifestHeader header)
			: base(header.Factory)
		{
		}
	}
}
