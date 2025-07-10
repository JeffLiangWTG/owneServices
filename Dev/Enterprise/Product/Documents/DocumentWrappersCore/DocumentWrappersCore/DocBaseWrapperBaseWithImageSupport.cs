using System.Collections.Generic;
using System.Drawing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappersCore
{
	[AllowNoStaticNew]
	public class DocBaseWrapperBaseWithImageSupport : DocumentWrapper
	{
		protected DocBaseWrapperBaseWithImageSupport(object objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		#region CompanyLogo

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Disposed String")]
		public Image CompanyLogo
		{
			get
			{
				var result = DocumentBrandingImage;

				if (result == null)
				{
					result = GetCompanyLogoFallback();
					SetTag(result, "CompanyLogo");
				}

				SetTag(result, string.Format("{0}(Disposed: {1})", result?.Tag, result?.IsDisposed()));
				return result;
			}
		}

		protected virtual Image GetCompanyLogoFallback()
		{
			return SystemDataRegistry.Instance.CompanyLogo.Value;
		}

		#endregion

		#region BrandName

		public ZString BrandName
		{
			get
			{
				var result = DocumentBrandingObject == null
					? DefaultBrandName
					: DocumentBrandingObject.BrandName;

				return (!Env.Registry.OrgAllowMixedCase) ? result.ToUpper() : result;
			}
		}

		protected virtual ZString DefaultBrandName
		{
			get { return GlbCompany.CurrentCompany != null ? GlbCompany.CurrentCompany.GC_Name : ZString.Empty; }
		}

		#endregion

		#region BrandEmailAddress

		public ZString BrandEmailAddress
		{
			get
			{
				return DocumentBrandingObject == null
					? GlbStaff.CurrentUser.GS_EmailAddress
					: GetEmailAddress(DocumentBrandingObject);
			}
		}

		#endregion

		#region Branding

		protected Image DocumentBrandingImage
		{
			get { return DocumentBrandingObject == null ? null : DocumentBrandingObject.Image; }
		}

		protected virtual ClientTariffAndLevel TariffAndLevelRegistry
		{
			get
			{
				ClientTariffAndLevel result = null;
				if (DocumentsDataRegistry.Instance.EnableClientBranding.Value)
				{
					ZString tariffLevelCode = "";

					if (BrandedOrganisation != null)
					{
						tariffLevelCode = BrandedOrganisation.CompanyData.RateTariffLevels.DefaultLevel.ToString();
					}
					else if (ContactOrganisation != null)
					{
						tariffLevelCode = ContactOrganisation.CompanyData.RateTariffLevels.DefaultLevel.ToString();
					}

					result = (ClientTariffAndLevel)DocumentsDataRegistry.Instance.ClientTariffAndLevels.Value.FindByCode(tariffLevelCode);
				}

				return result;
			}
		}

		protected virtual AgentDocumentBrand AgentBrand
		{
			get
			{
				AgentDocumentBrand result = null;
				if (DocumentsDataRegistry.Instance.EnableAgentBranding.Value)
				{
					if (BrandedOrganisation != null)
					{
						result = (AgentDocumentBrand)DocumentsDataRegistry.Instance.AgentDocumentBrand.Value.FindByCode(BrandedOrganisation.MiscServ.OM_FWAgentCategory);
					}
					else if (ContactOrganisation != null)
					{
						result = (AgentDocumentBrand)DocumentsDataRegistry.Instance.AgentDocumentBrand.Value.FindByCode(ContactOrganisation.MiscServ.OM_FWAgentCategory);
					}
				}
				return result;
			}
		}

		protected virtual ClientAndAgentBrandingBusinessObject AlternativeBranding
		{
			get { return null; }
		}

		protected ClientAndAgentBrandingBusinessObject DocumentBrandingObject
		{
			get
			{
				ClientAndAgentBrandingBusinessObject result = null;

				if (DocumentContactType != null)
				{
					switch (DocumentContactType.BrandingType)
					{
						case ContactBrandingType.Agent:
							result = SetTag(AgentBrand, "AgentBrand");
							break;

						case ContactBrandingType.Client:
							result = SetTag(TariffAndLevelRegistry, "TariffAndLevelRegistry");
							break;
					}
				}

				return result ?? SetTag(AlternativeBranding, "AlternativeBranding");
			}
		}

		ClientAndAgentBrandingBusinessObject SetTag(ClientAndAgentBrandingBusinessObject businessObject, string tag)
		{
			if (businessObject != null)
			{
				SetTag(businessObject.Image, tag);
			}
			return businessObject;
		}

		void SetTag(Image image, string tag)
		{
			if (image != null)
			{
				image.Tag = tag;
			}
		}

		ZString GetEmailAddress(ClientAndAgentBrandingBusinessObject brandingObject)
		{
			var canUseStaffAddress = GlbStaff.CurrentUser.GS_PublishEmailAddress && !GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty;

			if (!canUseStaffAddress || brandingObject.UseGeneric)
			{
				return brandingObject.BrandEmailAddress;
			}
			else if (!brandingObject.ReplaceDomainNames)
			{
				return GlbStaff.CurrentUser.GS_EmailAddress;
			}
			else
			{
				var userEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
				var brandDomainName = brandingObject.BrandEmailAddress.Substring(brandingObject.BrandEmailAddress.IndexOf('@'));
				var username = userEmailAddress.RemoveSafe(userEmailAddress.IndexOf('@'), userEmailAddress.Length);
				return string.IsNullOrEmpty(brandDomainName) ? (string)userEmailAddress : (username + brandDomainName);
			}
		}

		#endregion

		#region Document Constants

		Dictionary<string, object> reportConstants = new Dictionary<string, object>();
		bool hasDocWrapperContextBeenUpdatedWithReportConstants;

		protected T GetTemplateConstantValue<T>(string key)
		{
			bool isFound;
			return GetTemplateConstantValue<T>(key, out isFound);
		}

		protected T GetTemplateConstantValue<T>(string key, out bool isFound)
		{
			return DocWrapperContext.GetTemplateConstantValue<T>(key, out isFound);
		}

		protected T GetTemplateConstantValue<T>(string key, T defaultValue)
		{
			bool isFound;
			T result = GetTemplateConstantValue<T>(key, out isFound);
			return isFound ? result : defaultValue;
		}

		protected override sealed void SetDocWrapperContext(Dictionary<string, object> constants)
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(constants);
			OnDocWrappersContextSet();
			reportConstants = constants;
		}

		protected void OverrideDocumentDirectionAfterItsSetByTheReport_HACK_DoNotUse_ToBeRemoved(DocumentDirection direction)
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(new Dictionary<string, object> { { Constants.TemplateDefined.DocumentDirection, direction.ToString() } });
		}

		protected virtual void OnDocWrappersContextSet() { }

		IDocWrapperContext DocWrapperContext
		{
			get
			{
				DocWrapperContextManager docWrapperContextmanager = Factory.GetDocWrapperContextManager();
				if (!hasDocWrapperContextBeenUpdatedWithReportConstants)
				{
					docWrapperContextmanager.UpdateDocWrapperContextFromReportConstants(reportConstants);
					hasDocWrapperContextBeenUpdatedWithReportConstants = true;
				}
				return docWrapperContextmanager;
			}
		}

#if DEBUG
		public ZGuid DocWrapperContextContactOrganisationPK_Exposed()
		{
			return DocWrapperContext.ContactOrganisationPK;
		}
#endif

		public ContactType DocumentContactType
		{
			get { return ContactType.Find(DocWrapperContext.DocumentContactTypeCode); }
		}

		public OrgHeader BrandedOrganisation
		{
			get
			{
				return DocWrapperContext.BrandedOrganisationPK.IsValid && !DocWrapperContext.BrandedOrganisationPK.IsEmpty
						? Factory.Load<OrgHeader>(DocWrapperContext.BrandedOrganisationPK)
						: null;
			}
		}

		public OrgHeader ContactOrganisation
		{
			get
			{
				return DocWrapperContext.ContactOrganisationPK.IsValid && !DocWrapperContext.ContactOrganisationPK.IsEmpty
						? Factory.Load<OrgHeader>(DocWrapperContext.ContactOrganisationPK)
						: null;
			}
		}

		public ZString ReportName
		{
			get { return DocWrapperContext.ReportName; }
		}

		public ZString DocumentDirection
		{
			get { return DocWrapperContext.DocumentDirection; }
		}

		public ZString MenuTitle
		{
			get { return DocWrapperContext.MenuTitle; }
		}

		public ZString DocumentDeliveryMode
		{
			get { return DocWrapperContext.DocumentDeliveryMode; }
		}

		public ZGuid DocumentMenuItemPK
		{
			get { return DocWrapperContext.MenuItemPK; }
		}

		#endregion
	}
}
