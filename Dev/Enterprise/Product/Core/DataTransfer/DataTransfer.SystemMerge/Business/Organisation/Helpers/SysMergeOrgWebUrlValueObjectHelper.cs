using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeOrgWebUrlValueObjectHelper
	{
		public SysMergeOrgWebUrlValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgWebUrlCollection webUrlValueCollection, OrgHeaderForDataTransfer organisation, IValueObjectImportContext context)
		{
			if (webUrlValueCollection.IsSpecified)
			{
				for (int i = 0; i < webUrlValueCollection.Count; i++)
				{
					Xsd.SysMergeOrgWebUrl webUrlValue = webUrlValueCollection[i];
					OrgWebURL newWebUrl = ImportFromValueObject(organisation, webUrlValue, context);
				}
			}
		}

		OrgWebURL ImportFromValueObject(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrgWebUrl xsdWebUrl, IValueObjectImportContext context)
		{
			OrgWebURL newWebUrl = organisation.Factory.NewWithPrimaryKey<OrgWebURL>(new Guid(xsdWebUrl.PK));
			newWebUrl.PU_OH = organisation.PK;

			context.SetPropertyInfoValueIfValueNotEmpty(newWebUrl.PU_URLInfo, xsdWebUrl.URL);
			SetURLDetails(newWebUrl, xsdWebUrl, context);

			return newWebUrl;
		}

		void SetURLDetails(OrgWebURL webUrlToUpdate, Xsd.SysMergeOrgWebUrl webUrlValue, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(webUrlToUpdate.PU_DescriptionInfo, webUrlValue.Description);
			context.SetPropertyInfoValueIfValueNotEmpty(webUrlToUpdate.PU_IsPrimaryInfo, webUrlValue.IsPrimary.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(webUrlToUpdate.PU_TypeInfo, webUrlValue.Type);
			context.SetPropertyInfoValueIfValueNotEmpty(webUrlToUpdate.PU_URLInfo, webUrlValue.URL);

			if (!webUrlValue.OrgHeaderPK.IsEmpty)
			{
				webUrlToUpdate.PU_OH = new ZGuid(webUrlValue.OrgHeaderPK);
			}
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgWebUrlCollection webUrlValueCollection, INotifications notifications)
		{
			ZQuery query = new ZQuery(OrgWebURLSchema.PU_OH, org.PK);
			OrgWebURL[] webUrls = org.Factory.Load<OrgWebURL>(query);

			for (int i = 0; i < webUrls.Length; i++)
			{
				OrgWebURL webUrl = webUrls[i];
				Xsd.SysMergeOrgWebUrl xsdWebUrl = exportToValueObject(webUrl, notifications);
				webUrlValueCollection.Add(xsdWebUrl);
			}
		}

		public Xsd.SysMergeOrgWebUrl exportToValueObject(OrgWebURL webUrl, INotifications notifications)
		{
			Xsd.SysMergeOrgWebUrl result = new Xsd.SysMergeOrgWebUrl();

			result.PK = webUrl.PK.ToString();
			result.URL = webUrl.PU_URL;
			result.Type = webUrl.PU_Type;
			result.OrgHeaderPK = webUrl.PU_OH.ToString();
			if (webUrl.PU_IsPrimary)
			{
				result.IsPrimary = webUrl.PU_IsPrimary;
				result.IsPrimarySpecified = true;
			}
			result.Description = webUrl.PU_Description;

			return result;
		}

		#endregion
	}
}
