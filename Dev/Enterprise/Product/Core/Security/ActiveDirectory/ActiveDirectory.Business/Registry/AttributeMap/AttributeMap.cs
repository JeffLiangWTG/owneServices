using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	[XmlSerializerAssembly("Enterprise.Security.ActiveDirectory.XmlSerializers")]
	public class AttributeMap : RegistryBusinessObjectTemplate
	{
		public AttributeMap()
			: base(RegistryFactory.Instance)
		{
		}

		public string GetActiveDirectoryAttributeFromSchema(SchemaColumn schema)
		{
			Argument.NotNull(schema, nameof(schema));

			var foundItem = MapItems.Cast<AttributeMapItem>().FirstOrDefault(item => item.Matches(schema));
			string result = foundItem != null ? foundItem.ActiveDirectoryAttributeName : null;
			if (string.IsNullOrEmpty(result))
			{
				throw new NotSupportedException("Could not get an AD property for schema " + schema.Name);
			}
			return result;
		}

		public AttributeMapItemCollection MapItems => mapItems ?? (mapItems = new AttributeMapItemCollection());
		AttributeMapItemCollection mapItems;

		public bool IsSynced(string columnName)
		{
			var mapItem = MapItems.Cast<AttributeMapItem>().FirstOrDefault(i => i.EnterpriseColumnName == columnName);
			return mapItem != null && mapItem.IsSynced;
		}

		public bool IsSynced(SchemaColumn schemaColumn)
		{
			var mapItem = MapItems.Cast<AttributeMapItem>().FirstOrDefault(i => i.Matches(schemaColumn));
			return mapItem != null && mapItem.IsSynced;
		}

		#region Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteStartElement(Schema.MapItems);
			foreach (AttributeMapItem item in MapItems)
			{
				writer.WriteStartElement(Schema.MapItem);
				writer.WriteElementString(Schema.EnterpriseColumnName, item.EnterpriseColumnName);
				writer.WriteElementString(Schema.ActiveDirectoryPropertyName, item.ActiveDirectoryAttributeName);
				writer.WriteElementString(Schema.IsSynced, item.IsSynced.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var xmlReader = reader.Reader;
			while (xmlReader.Read() && xmlReader.Name == Schema.MapItem)
			{
				xmlReader.Read();
				var enterpriseColumnName = xmlReader.ReadElementContentAsString();
				var activeDirectoryPropertyName = xmlReader.ReadElementContentAsString();
				var isSynced = new ZBool(xmlReader.ReadElementContentAsString());
				if (!string.IsNullOrEmpty(enterpriseColumnName) && !string.IsNullOrEmpty(activeDirectoryPropertyName))
				{
					MapItems.Add(new AttributeMapItem(enterpriseColumnName, activeDirectoryPropertyName, isSynced));
				}
			}

			AddElementsFromDefault();
		}

		public void AddElementsFromDefault()
		{
			//add in new rows from DefaultMap that weren't in old value
			foreach (AttributeMapItem item in AttributeMap.DefaultMap.MapItems)
			{
				if (!MapItems.Cast<AttributeMapItem>().Any(x => x.EnterpriseColumnName == item.EnterpriseColumnName))
				{
					MapItems.Add(item);
				}
			}
		}

		public new AttributeMap Clone() => (AttributeMap)GetClone(null, null);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new AttributeMap();
			foreach (AttributeMapItem item in MapItems)
			{
				result.MapItems.Add(item.Clone());
			}

			return result;
		}

		public string ToStringForSerialisation()
		{
			var builder = new StringBuilder();
			foreach (AttributeMapItem item in MapItems)
			{
				builder.AppendFormat("{0}|{1}|{2},", item.EnterpriseColumnName, item.ActiveDirectoryAttributeName, item.IsSynced.ToString());
			}

			return builder.ToString();
		}

		static class Schema
		{
			internal const string MapItems = "MapItems";
			internal const string MapItem = "MapItem";
			internal const string PropertyMapItem = "PropertyMapItem";
			internal const string EnterpriseColumnName = "EnterpriseColumnName";
			internal const string ActiveDirectoryPropertyName = "ActiveDirectoryPropertyName";
			internal const string IsSynced = "IsSynced";
		}

		#endregion

		public override bool Equals(object obj)
		{
			var other = obj as AttributeMap;
			return other != null && MapItems.SequenceEqual(other.MapItems);
		}

		public override int GetHashCode() => MapItems.Aggregate(0, (accumulator, mapItem) => accumulator ^= mapItem.GetHashCode());

		public static AttributeMap Current => ActiveDirectoryRegistry.Instance.AttributeMapping.Value;

		public static AttributeMap DefaultMap
		{
			get
			{
				var defaultPropertyMap = new AttributeMap();
				defaultPropertyMap.MapItems.AddRange(new[]
				{
					new AttributeMapItem(GlbStaffSchema.GS_City, ADAttributes.City, true),
					new AttributeMapItem(GlbStaffSchema.GS_EmailAddress, ADAttributes.Email, true),
					new AttributeMapItem(GlbStaffSchema.GS_FaxNum, ADAttributes.PhoneFaxOther, true),
					new AttributeMapItem(GlbStaffSchema.GS_FullName, ADAttributes.DisplayName, true),
					new AttributeMapItem(GlbStaffSchema.GS_HomePhone, ADAttributes.PhoneHomePrimary, true),
					new AttributeMapItem(GlbStaffSchema.GS_IsActive, ADAttributes.UserAccountControl, true),
					new AttributeMapItem(GlbStaffSchema.GS_LoginName, ADAttributes.UserPrincipalName, true),
					new AttributeMapItem(GlbStaffSchema.GS_MobilePhone, ADAttributes.PhoneMobilePrimary, true),
					new AttributeMapItem(GlbStaffSchema.GS_Pager, ADAttributes.PhonePagerPrimary, true),
					new AttributeMapItem(GlbStaffSchema.GS_ProfilePhoto, ADAttributes.ThumbnailPhoto, true),
					new AttributeMapItem(GlbStaffSchema.GS_Postcode, ADAttributes.PostalCode, true),
					new AttributeMapItem(GlbStaffSchema.GS_State, ADAttributes.State, true),
					new AttributeMapItem(GlbStaffSchema.GS_Title, ADAttributes.Title, true),
					new AttributeMapItem(GlbStaffSchema.GS_UserAddress1, ADAttributes.Address, true),
					new AttributeMapItem(GlbStaffSchema.GS_WorkExtension, ADAttributes.PhoneOfficeOther, true),
					new AttributeMapItem(GlbStaffSchema.GS_WorkPhone, ADAttributes.TelephoneNumber, true),
					new AttributeMapItem(GlbGroupSchema.GG_Desc, ADAttributes.Name, true),
					new AttributeMapItem(GlbStaffSchema.GS_WorkingLanguage, ADAttributes.PreferredLanguage, false),
				});

				if (ClientHookLoader.Instance.Client == Clients.EDI)
				{
					defaultPropertyMap.MapItems.AddRange(new[]
					{
						new AttributeMapItem(GlbStaff.Schema.CurrentDRMManager, (NoResString)"manager", true),
					});
				}
				return defaultPropertyMap;
			}
		}
	}
}
