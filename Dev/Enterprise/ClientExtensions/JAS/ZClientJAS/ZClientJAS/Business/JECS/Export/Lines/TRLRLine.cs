
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class TRLRLine : MessageLine
	{
		public TRLRLine()
		{
		}

		protected override int FieldCount
		{
			get { return 0; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.TRLR; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			// Do nothing as this line does not have a content
		}
	}
}
