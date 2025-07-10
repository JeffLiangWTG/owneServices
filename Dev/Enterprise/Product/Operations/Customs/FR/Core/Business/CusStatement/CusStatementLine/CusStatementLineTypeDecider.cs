using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementLineTypeDecider : TypeDecider, Integration.Customs.FR.ICusStatementLineTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var lineType = (row != null) ? row[CusStatementLine.Schema.B3_EntryType].ToString().Trim() : string.Empty;
			return GetTypeByLineType(lineType);
		}

		Type GetTypeByLineType(ZString lineType)
		{
			switch (lineType)
			{
				case StatementEntryTypeList.Codes.DCG:
					return typeof(CusStatementChargesDetail);
				case StatementEntryTypeList.Codes.Import:
				case StatementEntryTypeList.Codes.Export:
					return typeof(CusStatementEntry);
				default:
					ErrorReporter.ReportOnce(string.Join("|", "FR|CusStatementLine", lineType),
						string.Format(CultureInfo.InvariantCulture, "The CusStatementLineTypeDecider for FR could not load the object as the  B3_EntryType '{0}' is unknown. A shared BaseCusStatementLine was returned instead.", lineType));
					return typeof(Customs.Business.BaseCusStatementLine);
			}
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
