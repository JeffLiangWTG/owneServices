using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageDecTypeDecider : TypeDecider, Integration.Customs.DE.ICusTempStorageDecTypeDecider
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
				case TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader:
					return typeof(CHGOFFCusTempStorageDec);
				case TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm:
					return typeof(CHGSPOCusTempStorageDec);
				case TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation:
					return typeof(CHGTSTCusTempStorageDec);
				case TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo:
					return typeof(CUSPCSCusTempStorageDec);
				case TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger:
					return typeof(CUSPRLCusTempStorageDec);
				case TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation:
					return typeof(PRLCONCusTempStorageDec);
				case TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch:
					return typeof(REXDISCusTempStorageDec);
				default:
					ErrorReporter.ReportOnce(string.Join("|", "DE|CusTempStorageDec", applicationCode),
						string.Format(CultureInfo.InvariantCulture, "The CusTempStorageDecTypeDecider for DE could not load the object as the STH_DeclarationType '{0}' is unknown. A base EU.CusTempStorageDec was returned instead.", applicationCode));
					return typeof(EU.Business.CusTempStorage.CusTempStorageDec);
			}
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
