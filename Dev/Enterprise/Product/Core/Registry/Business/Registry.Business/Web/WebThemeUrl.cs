using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebThemeUrl : RegistryBusinessObjectTemplate
	{
		public WebThemeUrl()
			: base()
		{ }

		#region Schema

		public abstract class Schema
		{
			public const string Url = "Url";
			public const string CompanyCode = "CompanyCode";
			public const string ThemeName = "ThemeName";
		}

		#endregion

		#region Url

		public ZString Url
		{
			get { return url; }
			set
			{
				CheckMaximumLength(UrlInfo, value);
				url = value;
				UrlInfo.RefreshBinding();
			}
		}
		ZString url;

		public ZPropertyInfo UrlInfo
		{
			get { return GetZPropertyInfo(nameof(Url)); }
		}

		#endregion

		#region Company

		[List("CompanyCodes")]
		public ZString CompanyCode
		{
			get { return companyCode; }
			set { SetNonPersistentPropertyValue<ZString>(CompanyCodeInfo, ref companyCode, value); }
		}
		ZString companyCode;

		public ZPropertyInfo CompanyCodeInfo => GetZPropertyInfo(Schema.CompanyCode);

		#endregion

		#region ThemeName

		[List("Themes")]
		public ZString ThemeName
		{
			get { return themeName; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ThemeNameInfo, ref this.themeName, value);
				if (!IsValidationSuspended)
				{
					ValidateThemeName();
				}
			}
		}
		public ZString themeName;

		public ZPropertyInfo ThemeNameInfo
		{
			get { return GetZPropertyInfo(nameof(ThemeName)); }
		}

		#endregion

		#region Validation

		public void ValidateThemeName()
		{
			ThemeNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ThemeNameInfo);
			ListValidation.ErrorIfInvalidCode(ThemeNameInfo);
		}

		public void ValidateCompanyCode()
		{
			CompanyCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CompanyCodeInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateThemeName();
			ValidateCompanyCode();
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebThemeUrl();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var clonedUrlTheme = (WebThemeUrl)clone;
			using (clonedUrlTheme.GetValidationSuspender())
			{
				clonedUrlTheme.Url = Url;
				clonedUrlTheme.ThemeName = ThemeName;
				clonedUrlTheme.CompanyCode = CompanyCode;
			}
			clonedUrlTheme.ResumeValidation();
		}

		#endregion

		#region Read / Write Elements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Url = reader.ReadElementString(Schema.Url);
			ThemeName = reader.ReadElementString(Schema.ThemeName);
			CompanyCode = reader.ReadElementString(Schema.CompanyCode);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Url, Url);
			writer.WriteElementString(Schema.ThemeName, ThemeName);
			writer.WriteElementString(Schema.CompanyCode, CompanyCode);
		}

		#endregion

		#region Lookup

		public CodeDescriptionPairList Themes
		{
			get
			{
				CodeDescriptionPairList themes = new CodeDescriptionPairList();
				themes.AddRange(WebDataRegistry.Instance.WebCampaignCustomTheme.Value);
				return themes;
			}
		}

		public CodeDescriptionPairList CompanyCodes
		{
			get
			{
				return CurrentFactory.GetCachedValue<CodeDescriptionPairList>("WebThemeUrl.CompanyCodes",
					() =>
					{
						var companyCodes = new CodeDescriptionPairList();
						var companies = CurrentFactory.Load<IGlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));
						foreach (IGlbCompany company in companies)
						{
							companyCodes.AddPairIfNotExist(company.GC_Code, company.GC_Name);
						}
						return companyCodes;
					});
			}
		}

		#endregion
	}
}
