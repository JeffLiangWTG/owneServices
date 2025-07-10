using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DataExporterResult
	{
		public DataExporterResult(ZString fileName, ZString content)
		{
			this.fContent = content;
			this.fFileName = fileName;
		}
		readonly ZString fFileName;
		readonly ZString fContent;

		public ZString FileName
		{
			get { return fFileName; }
		}

		public ZString Content
		{
			get { return fContent; }
		}
	}
}
