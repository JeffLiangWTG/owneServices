
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class DAWBRecord : MAWBRecord
	{
		public DAWBRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		protected override ZString AgentType
		{
			get { return Core.Constants.AgentType.Direct; }
		}
	}
}
