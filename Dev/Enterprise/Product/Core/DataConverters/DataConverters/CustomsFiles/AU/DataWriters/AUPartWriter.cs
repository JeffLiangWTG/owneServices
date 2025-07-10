using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters.CustomsFiles.AU
{
	public class PartWriter : CustomsFiles.PartWriter
	{
		public PartWriter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type GetPartType()
		{
			return typeof(OrgSupplierPart);
		}

		protected override void UpdateCountrySpecificData(BusinessObject bizO)
		{
		}

		protected override internal ZString CSVOutputLine
		{
			get
			{
				return PartNumber + "," +
						 RemoveComma(Description) + "," +
						 WeightUQ + "," +
						 RemoveComma(LookupCode) + "," +
						 RemoveComma(LookupCode) + "," +
						 ImporterCode + "," +
						 SupplierCode + "," +
						 ZString.Empty + "," +
						 DefaultStockUnit;
			}
		}
	}
}
