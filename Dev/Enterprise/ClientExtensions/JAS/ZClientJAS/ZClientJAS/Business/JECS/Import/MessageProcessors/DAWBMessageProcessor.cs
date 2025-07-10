using System;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class DAWBMessageProcessor : MAWBMessageProcessor
	{
		public DAWBMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override Type FirstLineType
		{
			get { return typeof(DAWBRecord); }
		}
	}
}
