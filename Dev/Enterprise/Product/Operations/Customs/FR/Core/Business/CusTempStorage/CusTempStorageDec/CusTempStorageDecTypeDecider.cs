using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageDecTypeDecider : TypeDecider, Integration.Customs.FR.ICusTempStorageDecTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = (row != null) ? row[CusTempStorageDec.Schema.STH_DeclarationType].ToString().Trim() : string.Empty;
			return GetTypeByApplicationCode(applicationCode);
		}

		Type GetTypeByApplicationCode(ZString applicationCode)
		{
			switch (applicationCode)
			{
				case FRConstants.TemporaryStorage.AppCodeFRC:
					return typeof(FRCCusTempStorageDec);
				case FRConstants.TemporaryStorage.AppCodeIST:
					return typeof(ISTCusTempStorageDec);
				case FRConstants.TemporaryStorage.AppCodeLAD:
					return typeof(LADTCusTempStorageDec);
				default:
					ErrorReporter.ReportOnce(string.Join("|", "FR|CusTempStorageDec", applicationCode),
						string.Format(CultureInfo.InvariantCulture, "The CusTempStorageDecTypeDecider for FR could not load the object as the STH_DeclarationType '{0}' is unknown. A base EU.CusTempStorageDec was returned instead.", applicationCode));
					return typeof(EU.Business.CusTempStorage.CusTempStorageDec);
			}
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
