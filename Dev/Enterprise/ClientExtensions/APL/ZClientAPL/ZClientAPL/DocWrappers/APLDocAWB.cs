using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.APL
{
	public class APLDocAWB : DocAWB
	{
		protected APLDocAWB(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap) : base(exportAWBHeader, factoryToWrap)
		{
		}

		public new static DocAWB New(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
		{
			return (exportAWBHeader == null) ? null : new APLDocAWB(exportAWBHeader, factoryToWrap);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}
	}
}
