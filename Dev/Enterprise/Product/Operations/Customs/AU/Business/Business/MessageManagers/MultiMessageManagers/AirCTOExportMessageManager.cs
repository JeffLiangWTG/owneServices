using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOExportMessageManager : MultiMessageManager
	{
		public AirCTOExportMessageManager(AirCTOExportCustomsManifestHeader exportCustomsManifestHeader)
			: base()
		{
			this.exportCustomsManifestHeader = exportCustomsManifestHeader;
		}
		readonly AirCTOExportCustomsManifestHeader exportCustomsManifestHeader;

		protected override bool SendWheneverPossibleOnceMessagingActive => false;

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage => exportCustomsManifestHeader;

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<Customs.Business.SingleMessageManager>();

			foreach (ExportCustomsManifestLines line in exportCustomsManifestHeader.Lines)
			{
				result.Add(new CTORemovalMessageManager(line));
				result.Add(new CTOReceivalMessageManager(line));
			}

			return result.ToArray();
		}
	}
}
