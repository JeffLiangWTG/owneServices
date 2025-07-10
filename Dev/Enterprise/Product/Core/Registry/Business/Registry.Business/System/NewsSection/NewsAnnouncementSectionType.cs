using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.NewsAnnouncementSectionSectionCategory;

namespace Enterprise.Registry.Business
{
	[CodeProperty(Schema.Code), DescriptionProperty(Schema.Description)]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class NewsAnnouncementSectionType : RegistryBusinessObject, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string SectionCategory = "SectionCategory";
			public const string OrderItemsBy = "OrderItemsBy";
			public const string SystemDefined = "SystemDefined";
			public const string EnglishDescriptionToShow = "EnglishDescriptionToShow";
		}

		#endregion

		public NewsAnnouncementSectionType(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public NewsAnnouncementSectionType()
			: base()
		{
		}

		#region Properties

		[ResourceStringData("NewsAnnouncementSectionType|SectionCategory", Caption = "Section Category")]
		[ReadOnly(true)]
		public ZString SectionCategory
		{
			get
			{
				if (sectionCategory.IsEmpty)
				{
					if (SystemDefined && IsWiseTechGlobalCommunicationDefaultCode(Code))
					{
						sectionCategory = NewsAnnouncementSectionSectionCategoryTypeList.WiseTechGlobalCommunication;
					}
					else
					{
						sectionCategory = NewsAnnouncementSectionSectionCategoryTypeList.NewsAnnouncements;
					}
				}
				return sectionCategory;
			}
		}
		ZString sectionCategory;

		public ZPropertyInfo SectionCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.SectionCategory); }
		}

		public bool Code_ReadOnly
		{
			get { return SystemDefined; }
		}

		[MaxLength(nameof(MaxDescriptionLength))]
		[ReadOnlyMember(nameof(SystemDefined))]
		[ResourceStringData("NewsAnnouncementSectionType|EnglishDescriptionToShow", Caption = "Description")]
		public ZString EnglishDescriptionToShow
		{
			get
			{
				if (SystemDefined)
				{
					return EnglishDescription.Replace("{0}", NewsAnnouncementSectionListRetriever.GetCompanyName());
				}
				return EnglishDescription;
			}
			set
			{
				if (!SystemDefined)
				{
					EnglishDescription = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateEnglishDescriptionToShow();
					}
					EnglishDescriptionToShowInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EnglishDescriptionToShowInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescriptionToShow); }
		}

		[List(nameof(OrderItemsByList))]
		[ReadOnlyMember(nameof(SystemDefined))]
		[ResourceStringData("NewsAnnouncementSectionType|OrderItemsBy", Caption = "Sort Type")]
		public ZString OrderItemsBy
		{
			get
			{
				return orderItemsBy;
			}
			set
			{
				SetNonPersistentPropertyValue<ZString>(OrderItemsByInfo, ref orderItemsBy, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOrderBy();
				}
			}
		}
		ZString orderItemsBy;

		public ZPropertyInfo OrderItemsByInfo
		{
			get { return GetZPropertyInfo(Schema.OrderItemsBy); }
		}

		public bool SystemDefined { get; set; }

		#endregion

		#region Lookups

		public CodeDescriptionPairList OrderItemsByList
		{
			get { return orderItemsByList ?? (orderItemsByList = new NewsSectionSortTypeList()); }
		}
		CodeDescriptionPairList orderItemsByList;

		#endregion

		#region Validation

		public NewsAnnouncementSectionTypeValidation Validation
		{
			get { return new NewsAnnouncementSectionTypeValidation(this); }
		}

		#endregion

		#region Overrides
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.OrderItemsBy, OrderItemsBy);
			writer.WriteElementString(Schema.SystemDefined, XmlConvert.ToString(SystemDefined));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			OrderItemsBy = reader.ReadElementString(Schema.OrderItemsBy);
			SystemDefined = bool.Parse(reader.ReadElementString(Schema.SystemDefined));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var newsAnnouncementSectionType = new NewsAnnouncementSectionType();
			newsAnnouncementSectionType.Code = Code;
			newsAnnouncementSectionType.Description = Description;
			newsAnnouncementSectionType.OrderItemsBy = OrderItemsBy;
			newsAnnouncementSectionType.SystemDefined = SystemDefined;
			return newsAnnouncementSectionType;
		}

		bool ICanDelete.CanDelete
		{
			get { return !SystemDefined; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("bc2bd5a0-b96a-4456-8316-da8af68a91e0", "This is a system defined value and cannot be deleted."); }
		}

		#endregion
	}
}
