
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class JXCWarningInfo
	{
		public JXCWarningInfo(ZPropertyInfo info, ZString warningMessage)
		{
			this.Info = info;
			this.WarningMessage = warningMessage;
		}

		public BusinessObject BizObj
		{
			get { return Info.BizObj; }
		}

		public readonly ZPropertyInfo Info;
		public readonly string WarningMessage;
	}
}
