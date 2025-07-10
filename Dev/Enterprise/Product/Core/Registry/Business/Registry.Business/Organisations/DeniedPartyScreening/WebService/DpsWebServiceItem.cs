using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DpsWebServiceItem : RegistryBusinessObjectTemplate
	{
		public DpsWebServiceItem()
		{
		}

		public DpsWebServiceItem(DpsWebServiceItemCollection itemCollection)
		{
			this.itemCollection = itemCollection;
		}

		public DpsWebServiceItemCollection ItemCollection
		{
			get
			{
				if (itemCollection == null)
				{
					itemCollection = new DpsWebServiceItemCollection();
					RegisterEditableChildObject(itemCollection);
				}

				return itemCollection;
			}
		}
		DpsWebServiceItemCollection itemCollection;

		#region copy/clone

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			if (ItemCollection != null)
			{
				((DpsWebServiceItem)clone).SetItemCollection((DpsWebServiceItemCollection)ItemCollection.Clone(null, null));
			}
		}

		public void SetItemCollection(DpsWebServiceItemCollection collection)
		{
			UnRegisterEditableChildObject(ItemCollection);
			itemCollection = collection;
			RegisterEditableChildObject(ItemCollection);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DpsWebServiceItem();
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string WebServiceUrl = "WebServiceUrl";
			public const string Role = "Role";
			public const int RoleMaxLength = 3;
		}

		#endregion

		#region Properties

		[MaxLength(4)]
		public ZString Code
		{
			get { return code; }
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);

				if (IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		[MaxLength(Schema.RoleMaxLength)]
		[List("RoleOptionList")]
		[ResourceStringData("DpsWebServiceItem|Role", Caption = "Role")]
		public ZString Role
		{
			get { return role; }
			set
			{
				CheckMaximumLength(RoleInfo, value);
				SetNonPersistentPropertyValue(RoleInfo, ref role, value);
			}
		}

		ZString role;

		public ZPropertyInfo RoleInfo => GetZPropertyInfo(Schema.Role);

		public CodeDescriptionPairList RoleOptionList
		{
			get
			{
				var applicableRoleList = new CodeDescriptionPairList();

				applicableRoleList.Add(RoleHelper.Production);
				applicableRoleList.Add(RoleHelper.ProductionFailover);
				applicableRoleList.Add(RoleHelper.Staging);

				return applicableRoleList;
			}
		}

		[MaxLength(150)]
		public ZString WebServiceUrl
		{
			get { return webServiceUrl; }
			set
			{
				SetNonPersistentPropertyValue(WebServiceUrlInfo, ref webServiceUrl, value);
			}
		}
		ZString webServiceUrl;

		public ZPropertyInfo WebServiceUrlInfo
		{
			get { return GetZPropertyInfo(Schema.WebServiceUrl); }
		}

		#endregion

		#region ValidationCore

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			ValidateWebServiceUrl();
			ValidateRoleOption();
			base.RunPreSaveValidationCore();
		}

		void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();

			if (Code.IsEmpty)
			{
				CodeInfo.AddError(Res.GetString("0EB60B05-A8C4-4E3D-9636-A21C4524A8EA", "Code should not be empty."));
			}
			else if (Code.Length != 4)
			{
				CodeInfo.AddError(Res.GetString("BCD464BB-8F79-425C-A441-D9B794C5FE05", "Code Length should be 4."));
			}
			else
			{
				var duplicateCodes = ParentCollections.FirstOrDefault()?.Cast<DpsWebServiceItem>().GroupBy(x => x.Code).Where(x => x.Count() > 1).Select(x => x.Key).Where(x => x == Code);

				if (duplicateCodes != null && duplicateCodes.Any())
				{
					CodeInfo.AddError(Res.GetString("335218B2-7B37-4354-A2A1-ACADAA8C5FB4", "There are duplicate Codes: {0}.", Code));
				}
			}
		}

		void ValidateWebServiceUrl()
		{
			WebServiceUrlInfo.ClearAllNotifications();

			if (WebServiceUrl.IsEmpty)
			{
				WebServiceUrlInfo.AddError(Res.GetString("038B093D-0A11-483C-A25F-04EE3A9C1A92", "Web Service URL should not be empty."));
			}
			else
			{
				bool isValid = Uri.TryCreate(WebServiceUrl, UriKind.Absolute, out var result) && (result?.Scheme == Uri.UriSchemeHttp || result?.Scheme == Uri.UriSchemeHttps);
				if (isValid)
				{
					var duplicateWebURLs = ParentCollections.FirstOrDefault()?.Cast<DpsWebServiceItem>().GroupBy(x => x.WebServiceUrl).Where(x => x.Count() > 1).Select(x => x.Key).Where(x => x == WebServiceUrl);

					if (duplicateWebURLs != null && duplicateWebURLs.Any())
					{
						WebServiceUrlInfo.AddError(Res.GetString("55A133FB-B16C-4A40-99FC-5E41F0A5F079", "There are duplicate Web Service URLs: {0}.", WebServiceUrl));
					}
				}
				else
				{
					WebServiceUrlInfo.AddError(Res.GetString("05E55E37-826D-409F-AD4F-67B781AA45AD", "Invalid Web Service URL: {0}.", WebServiceUrl));
				}
			}
		}

		public void ValidateRoleOption()
		{
			RoleInfo.ClearAllNotifications();

			if (ParentCollection != null && ParentCollection.Cast<DpsWebServiceItem>().Count(x => x.Role == RoleHelper.Code.Production) != 1)
			{
				RoleInfo.AddError(Res.GetString("A3639B2F-ED01-473E-A404-793419FEE774", "One production web service URL must be specified."));
			}
			if (ParentCollection != null && ParentCollection.Cast<DpsWebServiceItem>().Count(x => x.Role == RoleHelper.Code.Staging) != 1)
			{
				RoleInfo.AddError(Res.GetString("A3599B2F-ED01-473E-A404-793419FEE774", "One staging web service URL must be specified."));
			}

			MandatoryValidation.CheckEntered(RoleInfo);
			ListValidation.ErrorIfInvalidCode(RoleInfo, RoleOptionList);
		}

		#endregion

		DpsWebServiceItemCollection ParentCollection => (DpsWebServiceItemCollection)GetParentCollection(this, typeof(DpsWebServiceItemCollection));

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.WebServiceUrl, WebServiceUrl);
			writer.WriteElementString(Schema.Role, Role);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			WebServiceUrl = reader.ReadElementString(Schema.WebServiceUrl);
			Role = reader.ReadElementString(Schema.Role);
		}

		#endregion
	}
}
