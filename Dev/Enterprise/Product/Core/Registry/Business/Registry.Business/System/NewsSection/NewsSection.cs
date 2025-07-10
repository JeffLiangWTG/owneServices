using System.ComponentModel;
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
	public class NewsSection : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string LayoutPanelID = "LayoutPanelID";
			public const string SectionID = "SectionID";
			public const string HideReadItems = "HideReadItems";
			public const string MandatoryToRead = "MandatoryToRead";
		}

		#endregion

		public NewsSection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		public NewsSection()
			: base()
		{
		}

		#region Properties

		[List("LayoutPanelList")]
		[ReadOnly(true)]
		[ResourceStringData("NewsSection|LayoutPanelID", Caption = "Panel Location")]
		[MaxLength(15)]
		public ZString LayoutPanelID
		{
			get { return layoutPanelID; }
			set { SetNonPersistentPropertyValue<ZString>(LayoutPanelIDInfo, ref layoutPanelID, value); }
		}

		ZString layoutPanelID;

		public ZPropertyInfo LayoutPanelIDInfo
		{
			get { return GetZPropertyInfo(Schema.LayoutPanelID); }
		}

		[List("SectionList")]
		[ReadOnlyMemberAttribute(nameof(SectionID_ReadOnly))]
		[ResourceStringData("NewsSection|SectionID", Caption = "News Section")]
		[MaxLength(3)]
		public ZString SectionID
		{
			get { return sectionID; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(SectionIDInfo, ref sectionID, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSectionID();
				}
			}
		}

		ZString sectionID;

		public ZPropertyInfo SectionIDInfo
		{
			get { return GetZPropertyInfo(Schema.SectionID); }
		}

		protected virtual bool SectionID_ReadOnly
		{
			get { return LayoutPanelID.StartsWith("Bottom-"); }
		}

		[ResourceStringData("NewsSection|SectionName", Caption = "News Section Name")]
		public ZString SectionName
		{
			get { return SectionList.GetDescriptionFromCode(SectionID); }
		}

		[ReadOnlyMemberAttribute(nameof(HideReadItems_ReadOnly))]
		[ResourceStringData("NewsSection|HideReadItems", Caption = "Hide Read Items")]
		public ZBool HideReadItems
		{
			get { return hideReadItems; }
			set
			{
				SetNonPersistentPropertyValue(HideReadItemsInfo, ref hideReadItems, value);
			}
		}
		ZBool hideReadItems = true;

		public ZPropertyInfo HideReadItemsInfo
		{
			get { return GetZPropertyInfo(Schema.HideReadItems); }
		}

		protected virtual bool HideReadItems_ReadOnly
		{
			get { return false; }
		}

		[ReadOnlyMemberAttribute(nameof(MandatoryToRead_ReadOnly))]
		[ResourceStringData("NewsSection|MandatoryToRead", Caption = "Mandatory To Read")]
		public ZBool MandatoryToRead
		{
			get { return mandatoryToRead; }
			set
			{
				SetNonPersistentPropertyValue(MandatoryToReadInfo, ref mandatoryToRead, value);
			}
		}
		ZBool mandatoryToRead = false;

		public ZPropertyInfo MandatoryToReadInfo
		{
			get { return GetZPropertyInfo(Schema.MandatoryToRead); }
		}

		protected virtual bool MandatoryToRead_ReadOnly
		{
			get { return false; }
		}

		public bool IsWiseTechSection
		{
			get
			{
				return SectionID == NewsSectionTypeList.Codes.WiseLearningUpdates ||
					SectionID == NewsSectionTypeList.Codes.WiseNews ||
					SectionID == NewsSectionTypeList.Codes.ProductUpdates ||
					SectionID == NewsSectionTypeList.Codes.TechnicalAdvisoryNotes;
			}
		}

		#endregion

		#region Lookups

		public ReadOnlyCodeDescriptionPairList SectionList
		{
			get { return sectionList ?? (sectionList = NewsAnnouncementSectionListRetriever.GetList()); }
		}
		ReadOnlyCodeDescriptionPairList sectionList;

		public CodeDescriptionPairList LayoutPanelList
		{
			get
			{
				if (layoutPanelList == null)
				{
					layoutPanelList = new CodeDescriptionPairList();
					layoutPanelList.AddPair("Top-Left");
					layoutPanelList.AddPair("Top-Middle");
					layoutPanelList.AddPair("Top-Right");
					layoutPanelList.AddPair("Bottom-Left");
					layoutPanelList.AddPair("Bottom-Middle");
					layoutPanelList.AddPair("Bottom-Right");
				}

				return layoutPanelList;
			}
		}

		CodeDescriptionPairList layoutPanelList;

		#endregion

		#region Validation

		public NewsSectionValidation Validation
		{
			get { return new NewsSectionValidation(this); }
		}

		#endregion

		#region Overrides

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LayoutPanelID, LayoutPanelID);
			writer.WriteElementString(Schema.SectionID, SectionID);
			writer.WriteElementString(Schema.HideReadItems, HideReadItems.ToString());
			writer.WriteElementString(Schema.MandatoryToRead, MandatoryToRead.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LayoutPanelID = reader.ReadElementString(Schema.LayoutPanelID);
			SectionID = reader.ReadElementString(Schema.SectionID);
			HideReadItems = reader.ReadElementStringAsZBool(Schema.HideReadItems);
			MandatoryToRead = reader.ReadElementStringAsZBool(Schema.MandatoryToRead);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NewsSection();
		}

		#endregion
	}
}
