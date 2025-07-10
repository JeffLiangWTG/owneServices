using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CombinedConfigurationManager : ColumnConfigurationManager
	{
		public CombinedConfigurationManager(ColumnConfigurationsManager headingManager, string settingName)
			: base(headingManager)
		{
			this.settingName = settingName;
			this.fUniqueDescription = settingName;
		}

		public CombinedConfigurationManager(ColumnConfigurationsManager headingManager, ZGuid filterPk, ZString linkCode, ZString linkDescription, ZString uniqueDescription)
			: this(headingManager, filterPk, linkCode, linkDescription, uniqueDescription, ZString.Empty)
		{
		}

		public CombinedConfigurationManager(ColumnConfigurationsManager headingManager, ZGuid filterPk, ZString linkCode, ZString linkDescription, ZString uniqueDescription, ZString companyCode)
			: base(headingManager)
		{
			this.fLinkPk = filterPk;
			this.fUniqueDescription = uniqueDescription;
			this.fLinkCode = linkCode;
			this.fCompanyCode = companyCode;

			this.settingName = companyCode.IsEmpty ?
				string.Format((NoResString)"{0} (as {1}) - {2}", linkCode, headingManager.SaveToFilterField, uniqueDescription) :
				string.Format("{0}({1}) - {2}", CompanyName, companyCode, uniqueDescription);
		}

		ZGuid fLinkPk;
		ZString fUniqueDescription;
		ZString fLinkCode;
		readonly ZString fCompanyCode;

		public string SettingName
		{
			get { return settingName; }
		}
		readonly string settingName;

		protected override void DeleteCore()
		{
			DocumentsDataRegistry.Instance.ReportColumnSettings.Delete(UniqueDescription, ReportID, LinkPK);
		}

		protected override void SaveCore(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string selectedOrientation, string selectedLanguage)
		{
			DocumentsDataRegistry.Instance.ReportColumnSettings.Set(UniqueDescription, ReportID, LinkPK, CreateXml(filters, selectedGroupByName, selectedSortOrderName, selectedOrientation, selectedLanguage));
		}

		protected override string GetXml()
		{
			string xml;

			if (LinkPK.IsValid)
			{
				xml = Globals.IsWeb ?
					DocumentsDataRegistry.Instance.ReportColumnSettings.Get(UniqueDescription, HeadingManager.ReportID, LinkPK, CompanyCode) :
					DocumentsDataRegistry.Instance.ReportColumnSettings.Get(UniqueDescription, HeadingManager.ReportID, LinkPK);
			}
			else
			{
				xml = DocumentsDataRegistry.Instance.ReportColumnSettings.Get(SettingName, ReportID);
			}
			return xml;
		}

		public override string ToString()
		{
			return string.Format("{0}", this.Description);
		}

		public override ZGuid LinkPK
		{
			get { return fLinkPk; }
		}

		public override ZString Description
		{
			get { return SettingName; }
		}

		public override ZString UniqueDescription
		{
			get { return fUniqueDescription; }
		}

		public override ZString LinkCode
		{
			get { return fLinkCode; }
		}

		public ZString CompanyCode
		{
			get { return fCompanyCode; }
		}

		public ZString CompanyName
		{
			get { return Company != null ? Company.GC_Name : ZString.Empty; }
		}

		public GlbCompany Company
		{
			get { return company ?? (company = CompanyCode.IsEmpty ? null : GlbCompany.CurrentCompany.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, CompanyCode)); }
		}
		GlbCompany company;

		internal void SetValuesForTransformOnly(ZGuid pk, ZString description, ZString linkCode)
		{
			fLinkPk = pk;
			if (!description.IsEmpty)
			{
				fUniqueDescription = description;
			}
			fLinkCode = linkCode;
		}
	}
}
