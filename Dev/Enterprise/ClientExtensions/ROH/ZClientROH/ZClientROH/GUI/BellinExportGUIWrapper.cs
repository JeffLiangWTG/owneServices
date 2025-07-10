using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Client.Rohlig.Bellin;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.Rohlig.GUI
{
	public class BellinExportGUIWrapper : FlatFileXmlExportGUIWrapper
	{
		public BellinExportGUIWrapper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override string FormCaption
		{
			get { return Constants.BellinCaption; }
		}

		protected override string DialogFilter
		{
			get { return "CSV Files | *" + FileExtention; }
		}

		protected override string FileExtention
		{
			get { return "." + nameof(FileExtensionType.Csv).ToLower(); }
		}

		protected override AccountingTransactionsDataExporter DataExporter
		{
			get
			{
				if (BellinDataExporter == null)
				{
					BellinDataExporter = new BellinDataExporter(Factory);
				}
				return BellinDataExporter;
			}
		}
		BellinDataExporter BellinDataExporter;

		protected override bool IsOKToExport()
		{
			bool result = false;
			if (DataExporter.FilterProvider.CurrentBatchNo != 0 || !(DateFrom.IsEmpty || DateTo.IsEmpty))
			{
				result = base.IsOKToExport();
			}
			else
			{
				Globals.Message.ShowError("Must Specify Date From and Date To", "Missing From/To Dates");
			}
			return result;
		}

		#region Properties

		#region APCreditorGroup

		public ZGuid APCreditorGroup
		{
			get { return DataExporter.FilterProvider.AccountGroup; }
			set
			{
				DataExporter.FilterProvider.AccountGroup = value;
				APCreditorGroupInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo APCreditorGroupInfo
		{
			get { return GetZPropertyInfo(nameof(APCreditorGroup)); }
		}

		#endregion

		#endregion

		#region Look up

		protected override OrganisationsFindBoxCollection OrgHeadersListCore
		{
			get
			{
				if (fOrgHeadersListCore == null)
				{
					fOrgHeadersListCore = new CreditorCollection(Factory);
				}

				return fOrgHeadersListCore;
			}
		}

		public OrgCreditorGroupCollection AccountCreditorGroup
		{
			get
			{
				if (fAccountCreditorGroup == null)
				{
					fAccountCreditorGroup = new OrgCreditorGroupCollection(Factory);
				}
				return fAccountCreditorGroup;
			}
		}
		OrgCreditorGroupCollection fAccountCreditorGroup;

		#endregion
	}
}

#region Implementation
#endregion
