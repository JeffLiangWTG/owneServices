
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class REFRLine : MessageLine
	{
		public enum ReferenceFrom
		{
			Shipper,
			Consignee
		}

		public REFRLine(ZString reference, ReferenceFrom referenceFrom)
		{
			this.Reference = reference;
			this.From = referenceFrom;
		}

		#region Implementation

		protected override int FieldCount
		{
			get { return JXCConstants.REFRFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.REFR; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.REFRFieldPositions.Reference, Reference, JXCConstants.REFRFieldBoundaries.ReferenceMaxLength);
			dataRow.SetField(JXCConstants.REFRFieldPositions.FromShipperOrConsignee, From.ToString(), 1);
		}

		readonly ZString Reference;
		readonly ReferenceFrom From;

		#endregion
	}
}

#region Implementation
#endregion
