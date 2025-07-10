using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNRootDiagramShape : BMNCNShape
	{
		public BMNCNRootDiagramShape(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[BusinessObjectTestExclude]
		public override ZGuid BNS_BNS_RootShape
		{
			get => base.BNS_BNS_RootShape;
			set
			{
				if (!value.IsEmpty)
				{
					ErrorReporter.ReportOnce("Attempted to set a root diagram on another root diagram shape. This makes no sense. SAD!");
				}

				base.BNS_BNS_RootShape = value;
			}
		}

		[XmlColumnProperty]
		public ZBool ShouldShowNonScheduledSection
		{
			get => GetXmlColumnPropertyValue<ZBool>(ShouldShowNonScheduledSectionInfo);
			set
			{
				if (value && !IsScaled)
				{
					ErrorReporter.ReportOnce("Tried to enable the non-scheduled section on a shape that isn't a scaled diagram. SAD!");
				}

				SetXmlColumnPropertyValue(ShouldShowNonScheduledSectionInfo, value);
			}
		}

		public new ZDecimal Height
		{
			get { return 0d; }
		}

		protected override void SetHeight(ZDecimal value)
		{
		}

		public ZPropertyInfo ShouldShowNonScheduledSectionInfo => GetZPropertyInfo(nameof(ShouldShowNonScheduledSection));

		protected bool ShouldShowNonScheduledSection_ReadOnly => !IsScaled;

		[XmlColumnProperty]
		public ZDecimal NonScheduledSectionWidth
		{
			get => GetXmlColumnPropertyValue<ZDecimal>(NonScheduledSectionWidthInfo);
			set => SetXmlColumnPropertyValue(NonScheduledSectionWidthInfo, value);
		}

		public ZPropertyInfo NonScheduledSectionWidthInfo => GetZPropertyInfo(nameof(NonScheduledSectionWidth));

		#endregion

		#region Related Business Objects

		#region Shapes

		public ICollection<BMNCNShape> AllNestedShapes => Factory.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_BNS_RootShape, PK));

		#endregion

		#region Channels

		[ChildEditable]
		public BMNCNChannelCollection Channels
		{
			get
			{
				if (channels == null)
				{
					channels = SupportsChanneling ? new BMNCNChannelCollection(this) : BMNCNChannelCollection.Empty(Factory);
					RegisterEditableChildObject(channels);
				}

				return channels;
			}
		}

		BMNCNChannelCollection channels;

		protected override bool SupportsChannelingCore()
		{
			return IsScaled;
		}

		#endregion

		#region Leveling Rules

		[ChildEditable]
		public BMNCNLevelingRuleCollection LevelingRules
		{
			get
			{
				if (levelingRules == null)
				{
					levelingRules = SupportsChanneling ? new BMNCNLevelingRuleCollection(this) : BMNCNLevelingRuleCollection.Empty(Factory);
					RegisterEditableChildObject(levelingRules);
				}

				return levelingRules;
			}
		}

		BMNCNLevelingRuleCollection levelingRules;

		#endregion

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			NonScheduledSectionWidth = CCPMConstants.DefaultNonScheduledSectionWidth;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedShape = base.CloneInternal(args);
			var clonedDiagram = clonedShape as BMNCNRootDiagramShape;

			if (clonedDiagram != null && SupportsChanneling && channels != null)
			{
				foreach (var channel in Channels)
				{
					var clonedChannel = (BMNCNChannel)channel.Clone();
					clonedDiagram.Channels.Add(clonedChannel);
				}
			}

			return clonedShape;
		}

		public override void Delete()
		{
			Channels.DeleteAll();
			LevelingRules.DeleteAll();

			base.Delete();
		}

		#endregion
	}
}
