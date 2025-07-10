using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface ICanBeImportOrExport
	{
		ZBool IsImport { get; }
		ZBool IsExport { get; }
		string Level { get; }
		string TrueCountryCode { get; }
		void ValidatePreviousDocuments();
		string DataGroupingCode { get; }
	}

	public abstract class ImportExportAwareAddInfo : Customs.Business.BaseAddInfo
	{
		protected ImportExportAwareAddInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected void SetupParentAndEventsAndLoadValues(ZPropertyInfo addInfoProperty)
		{
			SetupEventsAndLoadValues(addInfoProperty);
			CheckForGoodParentage();
		}
		public ICanBeImportOrExport ImportExportParent
		{
			get;
			private set;
		}

		public bool IsCopyingExposed
		{
			get { return base.IsCopying; }
		}

		public abstract ZString KeyToDeterimeUniqueness { get; }

		void CheckForGoodParentage()
		{
			if (Parent == null)
			{
				ErrorReporter.ReportOnce("BEU-ImportExportAwareAddInfo-ParentIsNull", "Parent was null.");
			}
			else
			{
				var parent = Parent as CusAddInfo;
				if (parent == null)
				{
					ErrorReporter.ReportOnce("BEU-ImportExportAwareAddInfo-ParentIsNotCusAddInfo", "Parent was not CusAddInfo. It was " + Parent.GetType().FullName);
				}
				else
				{
					var grandparent = parent.Parent;
					if (grandparent == null)
					{
						var parentString = FormatCaiForErrorReport(parent);
						ErrorReporter.ReportOnce("BEU-ImportExportAwareAddInfo-GrandparentIsNull", "GrandParent was null. Parent=" + parentString);
					}
					else
					{
						ImportExportParent = grandparent as ICanBeImportOrExport;
						if (ImportExportParent == null)
						{
							var parentString = FormatCaiForErrorReport(parent);
							ErrorReporter.ReportOnce("BEU-ImportExportAwareAddInfo-ParentNotICanBeImportOrExport",
								string.Format("Grandparent for a Multi Line AddInfo must implement ICanBeImportOrExport. Exact type={0}; Parent={1}", grandparent.GetType().FullName, parentString));
						}
					}
				}
			}
		}

		string FormatCaiForErrorReport(CusAddInfo cai)
		{
			var parentString = cai != null ? string.Format("{0}={1};{2}={3};{4}={5}", CusAddInfo.Schema.B7_AddInfoData, cai.B7_AddInfoData, CusAddInfo.Schema.B7_ParentID, cai.B7_ParentID, CusAddInfo.Schema.B7_ParentTableCode, cai.B7_ParentTableCode) : (NoResString)"(null)";
			return parentString;
		}
	}
}
