using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CompanyDefaultConfigurationManager : ColumnConfigurationManager
	{
		public CompanyDefaultConfigurationManager(ColumnConfigurationsManager headingManager)
			: base(headingManager)
		{
		}

		protected override void DeleteCore()
		{
			DocumentsDataRegistry.Instance.ReportColumnSettings.Delete("", ReportID, GlbCompany.CurrentCompany.PK);
		}

		protected override void LoadCore()
		{
			if (String.IsNullOrEmpty(GetXml()))
			{
				RefreshHeadingManager(HeadingManager.DefaultTemplateConfigurationManager.GetCopyOfHeadings());
			}
		}

		protected override void SaveCore(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string selectedOrientation, string selectedLanguage)
		{
			DocumentsDataRegistry.Instance.ReportColumnSettings.Set("", ReportID, GlbCompany.CurrentCompany.PK, CreateXml(filters, selectedGroupByName, selectedSortOrderName, selectedOrientation, selectedLanguage));
		}

		public override string ToString()
		{
			return Res.GetString("343781bc-3024-4a4e-ac10-70e5696a8424", "{0}({1}) - Login Company Default Configuration", this.Description, this.Code);
		}

		public override ZGuid LinkPK
		{
			get { return GlbCompany.CurrentCompany.PK; }
		}

		public ZString Code
		{
			get { return GlbCompany.CurrentCompany.GC_Code; }
		}

		public override ZString Description
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		public override ZString UniqueDescription
		{
			get { return ZString.Empty; }
		}

		public override ZString LinkCode
		{
			get { return Code; }
		}

		protected override string GetXml()
		{
			return DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", ReportID, GlbCompany.CurrentCompany.PK);
		}
	}
}
