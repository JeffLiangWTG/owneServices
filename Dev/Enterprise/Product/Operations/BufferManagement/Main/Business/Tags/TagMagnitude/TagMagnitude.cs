using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(Schema.TGM_Code), DescriptionProperty("Description")]
	[ModuleID(ModuleId.BMTagMagnitude)]
	public class TagMagnitude : AutoTagMagnitude, ITagMagnitude, IDataVersionLoggingSupported, IAuditParent
	{
		public TagMagnitude(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider

		class TagMagnitudeTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(TagMagnitude);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				var workQueuesGroup = TagProvider.GetWorkQueuesTagGroup(factory);

				if ((Guid)row[TagMagnitudeSchema.TGM_TGD_Tag.Name] == workQueuesGroup.PK)
				{
					return typeof(WorkQueue);
				}
				else
				{
					return typeof(TagMagnitude);
				}
			}

			public override Type GetTypeForNew()
			{
				return typeof(TagMagnitude);
			}
		}

		public static readonly TypeDecider TypeDecider = new TagMagnitudeTypeDecider();

		#endregion

		#region Xml Properties

		#region Background Color

		[XmlColumnProperty]
		public ZBool ApplyColorToBackground
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ApplyColorToBackgroundInfo); }
			set
			{
				SetXmlColumnPropertyValue(ApplyColorToBackgroundInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateColor();
				}
			}
		}

		public ZPropertyInfo ApplyColorToBackgroundInfo
		{
			get { return GetZPropertyInfo(nameof(ApplyColorToBackground)); }
		}

		#endregion

		#region Border Color

		[XmlColumnProperty]
		public ZBool ApplyColorToBorder
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ApplyColorToBorderInfo); }
			set
			{
				SetXmlColumnPropertyValue(ApplyColorToBorderInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateColor();
				}
			}
		}

		public ZPropertyInfo ApplyColorToBorderInfo
		{
			get { return GetZPropertyInfo(nameof(ApplyColorToBorder)); }
		}

		#endregion

		#region Border Style

		[XmlColumnProperty]
		[List("BorderStyles")]
		[MaxLength(20)]
		public ZString BorderStyle
		{
			get { return GetXmlColumnPropertyValue<ZString>(BorderStyleInfo); }
			set
			{
				SetXmlColumnPropertyValue(BorderStyleInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateBorderStyle();
				}
			}
		}

		public ZPropertyInfo BorderStyleInfo
		{
			get { return GetZPropertyInfo(nameof(BorderStyle)); }
		}

		public CodeDescriptionPairList BorderStyles
		{
			get { return Factory.GetCachedValue<BorderStyleList>(); }
		}

		#endregion

		#region Color

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[MaxLength(40)]
		public ZString Color
		{
			get { return GetXmlColumnPropertyValue<ZString>(ColorInfo); }
			set
			{
				SetXmlColumnPropertyValue(ColorInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateColor();
				}
			}
		}

		public ZPropertyInfo ColorInfo
		{
			get { return GetZPropertyInfo(nameof(Color)); }
		}

		#endregion

		#region Color Priority

		[XmlColumnProperty]
		public ZInt VisualStylePriority
		{
			get { return GetXmlColumnPropertyValue<ZInt>(VisualStylePriorityInfo); }
			set
			{
				SetXmlColumnPropertyValue(VisualStylePriorityInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateVisualStylePriority();
				}
			}
		}

		public ZPropertyInfo VisualStylePriorityInfo
		{
			get { return GetZPropertyInfo(nameof(VisualStylePriority)); }
		}

		#endregion

		#endregion

		#region Properties

		[ReadOnlyMember(nameof(IsReadOnlyForSystemDefinedTagGroup))]
		public override ZString TGM_Code
		{
			get { return base.TGM_Code; }
			set { base.TGM_Code = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnlyForSystemDefinedTagGroup))]
		public override ZString TGM_Description
		{
			get { return base.TGM_Description; }
			set { base.TGM_Description = value; }
		}

		[RelatedBusinessObject("OwnerGroup")]
		[List("Lookups.OwnerGroups")]
		[ReadOnlyMember(nameof(IsReadOnlyForSystemDefinedTagGroup))]
		public override ZGuid TGM_GG_OwnerGroup
		{
			get { return base.TGM_GG_OwnerGroup; }
			set { base.TGM_GG_OwnerGroup = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnlyForSystemDefinedTagGroup))]
		public override ZBool TGM_IsActive
		{
			get { return base.TGM_IsActive; }
			set { base.TGM_IsActive = value; }
		}

		[ReadOnlyMember(nameof(IsReadOnlyForSystemDefinedTagGroup))]
		public override ZInt TGM_RuleRunSequence
		{
			get { return base.TGM_RuleRunSequence; }
			set { base.TGM_RuleRunSequence = value; }
		}

		protected virtual bool IsReadOnlyForSystemDefinedTagGroup
		{
			get { return Definition != null && Definition.TGD_IsSystem; }
		}

		#endregion

		#region New Properties

		public string DisplayText
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", TGM_Code, TGM_Description); }
		}

		public ZString Description
		{
			get
			{
				var tagGroup = Definition;

				return tagGroup != null ? string.Format(CultureInfo.CurrentCulture, "{0} ({1})", TGM_Description, tagGroup.TGD_DescriptionMultilingual) : TGM_Code.ToString();
			}
		}

		#endregion

		#region Related Business Objects

		[List("Lookups.Definitions")]
		[RelatedBusinessObject("Definition")]
		public override ZGuid TGM_TGD_Tag
		{
			get { return base.TGM_TGD_Tag; }
			set { base.TGM_TGD_Tag = value; }
		}

		public TagDefinition Definition
		{
			get { return Factory.Load<TagDefinition>(TGM_TGD_Tag); }
		}

		[ChildEditable]
		public TagLinkCollection Tags
		{
			get
			{
				if (tags == null)
				{
					tags = new TagLinkCollection(this);
					RegisterEditableChildObject(tags);
				}
				return tags;
			}
		}
		TagLinkCollection tags;

		public Color GetColor()
		{
			return string.IsNullOrEmpty(Color) ? System.Drawing.Color.Empty : ColorList.ColorFromName(Color);
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			if (ShouldPreventDeletingTagsOnSystemGroups && !IsDeleted && Definition != null && !Definition.IsDeleted && Definition.TGD_IsSystem)
			{
				throw new CannotDeleteException(Res.GetString("fa0e1f76-28e0-4e57-864d-0ef3467a7ccb", "Cannot delete system tags."));
			}

			Tags.DeleteAll();
			base.Delete();
		}

		protected virtual bool ShouldPreventDeletingTagsOnSystemGroups
		{
			get { return true; }
		}

		protected new TagMagnitudeValidation Validation
		{
			get { return base.Validation; }
		}

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(TagMagnitudeSchema.TGM_VisualizationData); }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return TGM_Description; }
		}

		#endregion

		#region Logging

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region For Test
#if DEBUG

		public override string ToString()
		{
			return DisplayText;
		}

#endif
		#endregion

		#region ITagMagnitude

		public ITagDefinition TagDefinition
		{
			get { return Definition; }
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(TagLinkSchema.TGL_TGM_Magnitude, null);
			}
		}

		#endregion
	}
}
