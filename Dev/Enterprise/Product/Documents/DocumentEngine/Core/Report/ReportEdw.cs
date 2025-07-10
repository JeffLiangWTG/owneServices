using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine
{
	public partial class ReportEdw : Report
	{
		#region Constructors

		public ReportEdw(DocumentPack pack, ExcelTemplate template, Guid mainPK, DataContext dataContext)
			: base(pack, template, mainPK, dataContext)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, string reportName, ContactType typeOfContact, bool isPasswordProtectedForModifying)
			: base(pack, template, reportName, typeOfContact, isPasswordProtectedForModifying)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, string reportName, ContactType typeOfContact, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening)
			: base(pack, template, reportName, typeOfContact, isPasswordProtectedForModifying, isPasswordProtectedForOpening)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, Guid mainPK, DataContext dataContext, string reportName)
			: base(pack, template, mainPK, dataContext, reportName)
		{
			this.Name = reportName;
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, ZGuid menuTemplatePivotPK)
			: base(pack, template, docDataProvider, reportName, typeOfContact, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, menuTemplatePivotPK)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: base(pack, template, docDataProvider, reportName, typeOfContact, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, ZGuid menuTemplatePivotPK)
			: base(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, menuTemplatePivotPK)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: base(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, IBODocDataProvider docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: base(pack, template, docDataProvider, reportName, typeOfContact, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public ReportEdw(DocumentPack pack, ExcelTemplate template, IBODocDataProvider docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: base(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public ReportEdw(DocumentPack documentPack, ExcelTemplate excelTemplate)
			: base(documentPack, excelTemplate)
		{
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI005:WeakReferenceTargetRaceConditionRule", Justification = "Parent object reset")]
		public override DbConnection RunningConnection
		{
			get
			{
				DbConnection connection = runningConnection;
				if (connection == null)
				{
					runningConnection = Db.NewAdminConnection(Db.EdwDatabaseName);
					connection = runningConnection;
					connection.EnsureIsOpen();
				}
				return connection;
			}
		}
	}
}
