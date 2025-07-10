using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TGE.Business
{
	internal abstract class CSSMapper
	{
		public FlatFileDataRowCollection Map(BusinessObject bizObj)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();
			result.Add(MapCore(bizObj));
			return result;
		}

		CSSDataRow MapCore(BusinessObject bizObj)
		{
			CSSDataRow result = new CSSDataRow();
			result.SequenceNumber = FileSequenceNumber;
			result.FileCreateDateTime = ZDateTime.UtcNow;
			result.Direction = Direction;
			MapCoreSpecific(result, bizObj);
			result.PopulateFields();
			return result;
		}

		internal ZString FileSequenceNumber { get; set; }

		protected abstract void MapCoreSpecific(CSSDataRow result, BusinessObject bizObj);

		protected abstract ZString Direction { get; }
	}
}
