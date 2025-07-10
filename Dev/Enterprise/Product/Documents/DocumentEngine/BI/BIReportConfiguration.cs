using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Reporting.WinForms;

namespace Enterprise.DocumentEngine
{
	public class BiReportConfiguration : IDisposable
	{
		string ContextName;
		protected string MenuName;
		public BiReport Report;
		protected List<ColumnHeading> ReportColumns;

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public BiReportConfiguration(string context, string menuName, IEnumerable<ReportParameterInfo> initialReportParameterInfoCollection)
		{
			this.initialReportParameterInfoCollection = initialReportParameterInfoCollection;
			Init(context, menuName);
		}

		void Init(string context, string menuName)
		{
			ContextName = context;
			MenuName = menuName;

			CreateReportData();

			ReportCommandCollection collection = new ReportCommandCollection(new BusinessObjectFactory(), ContextName);
			collection.SetReadOnlyIncludingChildren(true);
			collection.LoadWithMoreFiltering(
				new ZQuery(new ZQuery(StmMenuItemSchema.SU_BusinessContext, ContextName),
				new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName))
				);

			DocumentPack documentPack = new DocumentPack(collection[0]);

			ExcelTemplateForBi excelTemplate = new ExcelTemplateForBi("BIReportTemplates.xls");

			Report = new BiReport(documentPack, excelTemplate);

			PopulateReportTemplate();
		}

		public List<ColumnHeading> GetSortedHeadings()
		{
			List<ColumnHeading> result = new List<ColumnHeading>();
			ColumnHeadingCollection columnHeadingCollection = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings.CloneVisible();
			foreach (ColumnHeading columnHeading in columnHeadingCollection)
			{
				result.Add(columnHeading);
			}
			result.Sort((x, y) => x.CurrentPosition.CompareTo(y.CurrentPosition));
			return result;
		}

		public List<ColumnHeading> GetRemovedHeadings()
		{
			var result = new List<ColumnHeading>();
			result.AddRange(Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings.Clone().ToArray().Where(x => x.Hidden));
			return result;
		}

		void CreateReportData()
		{
			var scalarCount = (int)Db.Connection.ExecuteScalar(CheckMenu);
			if (scalarCount > 0)
			{
				return;
			}
			Db.Connection.ExecuteScalar(SetupMenu);
		}

		string SetupMenu
		{
			get
			{
				return String.Format(CultureInfo.InvariantCulture, @"
					insert into dbo.StmMenuItem 
					(
					SU_PK, 
					SU_AddressCategory,
					SU_AllowRawView,
					SU_BusinessContext,
					SU_ContactType, 
					SU_DeliveryRestrictionType,
					SU_DocumentDirection, 
					SU_DownloadOnly, 
					SU_DraftOption, 
					SU_FlexCelLineSpacing, 
					SU_Hint, 
					SU_IsClientSpecific, 
					SU_IsLocalDocument, 
					SU_IsModifiable, 
					SU_IsPublished,
					SU_IsSystemDefined, 
					SU_IsZippedDocPack, 
					SU_LicenceLevel, 
					SU_MenuIndex,
					SU_MenuName, 
					SU_MenuShortcut, 
					SU_MenuType, 
					SU_MustRunOnline,
					SU_PreventAutoDelivery, 
					SU_ShowDocToSendTab, 
					SU_SupportsVisualisation
					)
					values (
					/*SU_PK*/ NEWID(), 
					/*SU_AddressCategory*/'OFF',
					/*SU_AllowRawView*/0,
					/*SU_BusinessContext*/'{0}',
					/*SU_ContactType*/'NCT', 
					/*SU_DeliveryRestrictionType*/'NON',
					/*SU_DocumentDirection*/'ANY', 
					/*SU_DownloadOnly*/0, 
					/*SU_DraftOption*/'BTH', 
					/*SU_FlexCelLineSpacing*/1.000, 
					/*SU_Hint*/'{1}', 
					/*SU_IsClientSpecific*/0, 
					/*SU_IsLocalDocument*/0, 
					/*SU_IsModifiable*/0, 
					/*SU_IsPublished*/1,
					/*SU_IsSystemDefined*/1, 
					/*SU_IsZippedDocPack*/0, 
					/*SU_LicenceLevel*/'STD', 
					/*SU_MenuIndex*/0,
					/*SU_MenuName*/'{1}', 
					/*SU_MenuShortcut*/'None', 
					/*SU_MenuType*/'DOC', 
					/*SU_MustRunOnline*/0,
					/*SU_PreventAutoDelivery*/1, 
					/*SU_ShowDocToSendTab*/1, 
					/*SU_SupportsVisualisation*/1
					)",
							/*0*/ContextName,
					/*1*/MenuName
				);
			}
		}

		string CheckMenu
		{
			get
			{
				return String.Format(CultureInfo.InvariantCulture, @"
					select count(*) MenuExists from dbo.StmMenuItem 
					where
					SU_BusinessContext = '{0}' and
					SU_MenuName = '{1}'
					",
						 /*0*/ContextName,
					/*1*/MenuName
				);
			}
		}

		void PopulateReportTemplate()
		{
			FetchReportColumns();

			if (ReportColumns == null)
			{
				return;
			}

			for (int index = 0; index < ReportColumns.Count; ++index)
			{
				Report.ColumnHeadingManager.DefaultTemplateConfigurationManager.AddHeading(MenuName, ReportColumns[index]);
			}

			Report.ColumnHeadingManager.DefaultTemplateConfigurationManager.SetTitle(MenuName, MenuName);
			Report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load(Report.FilterCollection, Report.GroupByCollection, Report.SortOrderCollection, Report.OrientationManager, Report.Parent);
		}

		public virtual void FetchReportColumns()
		{
			ReportColumns = new List<ColumnHeading>();
			ReportColumns = GetReportColumns();
		}

		public List<ColumnHeading> GetReportColumns()
		{
			if (initialReportParameterInfoCollection != null)
			{
				foreach (var reportParameterInfo in initialReportParameterInfoCollection)
				{
					if (reportParameterInfo.Name.Equals("Column_Layout"))
					{
						return ParseReportColumns(reportParameterInfo.Values[0]);
					}
				}
			}
			return null;
		}

		readonly IEnumerable<ReportParameterInfo> initialReportParameterInfoCollection;

		public ReportViewer rptViewer;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		List<ColumnHeading> ParseReportColumns(string columnLayoutXML)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(columnLayoutXML);

			XmlNodeList order = xmlDoc.GetElementsByTagName((NoResString)"Order");
			XmlNodeList name = xmlDoc.GetElementsByTagName((NoResString)"Name");
			XmlNodeList label = xmlDoc.GetElementsByTagName("Label");

			List<ColumnHeading> reportColumns = new List<ColumnHeading>();
			for (int index = 0; index < order.Count; ++index)
			{
				reportColumns.Add(new ColumnHeading());
			}

			for (int index = 0; index < order.Count; ++index)
			{
				reportColumns[int.Parse(order[index].InnerText, CultureInfo.InvariantCulture) - 1].CurrentPosition = index;
				reportColumns[int.Parse(order[index].InnerText, CultureInfo.InvariantCulture) - 1].DisplayLabel = label[index].InnerText;
				reportColumns[int.Parse(order[index].InnerText, CultureInfo.InvariantCulture) - 1].Description = label[index].InnerText;
				reportColumns[int.Parse(order[index].InnerText, CultureInfo.InvariantCulture) - 1].HeadingText = name[index].InnerText;
				reportColumns[int.Parse(order[index].InnerText, CultureInfo.InvariantCulture) - 1].Hidden = false;
			}

			return reportColumns;
		}

		public string UpdateReportColumnsConfig()
		{
			String updatedReportColumns = "<ColumnLayout> ";
			List<ColumnHeading> sortedHeadings = GetSortedHeadings();
			foreach (ColumnHeading currentColumn in sortedHeadings)
			{
				updatedReportColumns += (NoResString)"<Column> ";

				updatedReportColumns += (NoResString)"<Order>";
				updatedReportColumns += (currentColumn.CurrentPosition + 1).ToString(CultureInfo.InvariantCulture);
				updatedReportColumns += (NoResString)"</Order> ";

				updatedReportColumns += (NoResString)"<Name>";
				updatedReportColumns += currentColumn.HeadingText;
				updatedReportColumns += (NoResString)"</Name> ";

				updatedReportColumns += (NoResString)"<Label>";
				updatedReportColumns += currentColumn.DisplayLabel;
				updatedReportColumns += (NoResString)"</Label> ";

				updatedReportColumns += (NoResString)"</Column> ";
			}
			updatedReportColumns += " </ColumnLayout>";
			return updatedReportColumns;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				Report.Dispose();
			}
		}
	}
}
