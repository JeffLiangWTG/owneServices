
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class HEADLine : MessageLine
	{
		public HEADLine(IJXCExportHeader headerData)
		{
			this.HeaderData = headerData;
		}

		protected override int FieldCount
		{
			get { return JXCConstants.HEADFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.HEAD; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			if (HeaderData.ReceivingForwarder != null)
			{
				dataRow.SetField(JXCConstants.HEADFieldPositions.DestOfficeCode, HeaderData.ReceivingForwarder.OfficeCode);
				dataRow.SetField(JXCConstants.HEADFieldPositions.DestNettingCode, HeaderData.ReceivingForwarder.NettingCode);
			}
			else
			{
				dataRow.SetField(JXCConstants.HEADFieldPositions.DestOfficeCode, JXCConstants.NoNettingCode);
				dataRow.SetField(JXCConstants.HEADFieldPositions.DestNettingCode, JXCConstants.NoNettingCode);
			}

			if (HeaderData.SendingForwarder != null)
			{
				dataRow.SetField(JXCConstants.HEADFieldPositions.SendingOfficeCode, HeaderData.SendingForwarder.OfficeCode);
				dataRow.SetField(JXCConstants.HEADFieldPositions.SendingNettingCode, HeaderData.SendingForwarder.NettingCode);
			}
			else
			{
				dataRow.SetField(JXCConstants.HEADFieldPositions.SendingOfficeCode, JXCConstants.NoNettingCode);
				dataRow.SetField(JXCConstants.HEADFieldPositions.SendingNettingCode, JXCConstants.NoNettingCode);
			}

			dataRow.SetField(JXCConstants.HEADFieldPositions.FreightDest, HeaderData.FreightDest);
		}

		readonly IJXCExportHeader HeaderData;
	}
}

#region Implementation
#endregion
