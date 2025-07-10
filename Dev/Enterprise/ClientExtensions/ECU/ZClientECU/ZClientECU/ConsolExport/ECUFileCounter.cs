using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ECU.ConsolExport
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in ECUConsolExporter")]
	public class ECUFileCounter : FileNameNumberFountain
	{
		protected ECUFileCounter()
		{
		}

		public static FileNameNumberFountain NewDelegate()
		{
			return new ECUFileCounter();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected override long MaxValue
		{
			get { return 99999999; }
		}

		protected override ZString GenerateFilename(IDbConnected connected, bool progressNumber)
		{
			return GetGenerateFileID(connected, progressNumber);
		}

		public override ZString GetGenerateFileID(IDbConnected connected, bool progressNumber)
		{
			ZString nextFileID = progressNumber ? New().FileID.GetNextFormatted(connected) : New().FileID.PeekPreliminaryFormatted(connected);
			return nextFileID.PadLeft(8, '0');
		}

		protected override ZString FountainName
		{
			get { return "ECUNumberCounter"; }
		}
	}
}
