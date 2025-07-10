using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public abstract class BMBoardSectionConfigurationWithOverridableSectionName<TValidation> : NonPersistentBusinessObject<TValidation>,
		IBoardSectionNameOverridable,
		IBoardSectionConfigurationBizo
		where TValidation : BMBoardSectionConfigurationWithOverridableSectionNameValidation
	{
		protected BMBoardSectionConfigurationWithOverridableSectionName(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IBoardSectionConfigurationBizo Members

		[MaxLength(200)]
		public ZString SectionName
		{
			get
			{
				return SectionNameIsOverridden && !string.IsNullOrWhiteSpace(SectionNameOverride) ? SectionNameOverride.Trim() : DefaultSectionName.Trim();
			}
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		public abstract void CopyConfigurationPropertiesToNewSection(IBMBoardSection section);

		#endregion

		[XmlColumnProperty]
		public ZBool SectionNameIsOverridden
		{
			get
			{
				return GetXmlColumnPropertyValue<ZBool>(SectionNameIsOverriddenInfo);
			}
			set
			{
				SetXmlColumnPropertyValue(SectionNameIsOverriddenInfo, value);
				SectionNameIsOverriddenInfo.RefreshBinding();
				SectionNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SectionNameIsOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(SectionNameIsOverridden)); }
		}

		public abstract ZString DefaultSectionName { get; }

		public ZPropertyInfo DefaultSectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(DefaultSectionName)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfigurationBase.SectionNameOverride", Caption = "Custom Section Name", FullDescription = "The name entered here will be displayed on the section header. Untick to use the automatically generated name.")]
		[MaxLength(200)]
		public virtual ZString SectionNameOverride
		{
			get
			{
				return GetXmlColumnPropertyValue<ZString>(SectionNameOverrideInfo);
			}
			set
			{
				SetXmlColumnPropertyValue(SectionNameOverrideInfo, value.Trim());
				SectionNameOverrideInfo.RefreshBinding();
				if (SectionNameIsOverridden)
				{
					SectionNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SectionNameOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(SectionNameOverride)); }
		}
	}
}
