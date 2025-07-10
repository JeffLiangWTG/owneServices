using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("Channel: {BNL_Name}")]
	[CodeProperty(nameof(BNL_Name)), DescriptionProperty(nameof(BNL_Name))]
	public class BMNCNChannel : AutoBMNCNChannel, IBMNCNChannel, IDiagramChannel
	{
		public BMNCNChannel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ResourceStringData("24e66593-4b3a-4324-8b2d-3e7691a7f351", Caption = "Sequence")]
		public override ZInt BNL_Sequence
		{
			get => base.BNL_Sequence;
			set
			{
				var oldValue = BNL_Sequence;
				base.BNL_Sequence = value;

				if (oldValue != BNL_Sequence && !IsValidationSuspended)
				{
					OtherChannelsInDiagram?.ForEach(x => x.Validation.ValidateBNL_Sequence());
					ResetShapes();
				}
			}
		}

		[ResourceStringData("53246325-f411-490d-81db-51a158ef2a27", Caption = "Name")]
		public override ZString BNL_Name
		{
			get => base.BNL_Name;
			set
			{
				var oldValue = BNL_Name;
				base.BNL_Name = value;

				if (oldValue != BNL_Name && !IsValidationSuspended)
				{
					OtherChannelsInDiagram?.ForEach(x => x.Validation.ValidateBNL_Name());
				}
			}
		}

		[ResourceStringData("a6ea4aa7-b072-460e-8af6-d34f999294cf", Caption = "Height", FullDescription = "The height, in pixels, of this channel on the diagram surface.")]
		public override ZInt BNL_Height
		{
			get => base.BNL_Height;
			set
			{
				var oldValue = BNL_Height;
				base.BNL_Height = value;

				if (BNL_Height != oldValue)
				{
					ResetShapes();
				}
			}
		}

		#endregion

		#region New Properties

		[List(nameof(ColorList))]
		[ResourceStringData("3c80b937-ba71-4073-8191-958fc738a097", Caption = "Color", FullDescription = "The color to add to the background of shapes within this channel.")]
		[BusinessObjectTestExclude]
		public ZString Color
		{
			get
			{
				var col = ColorHelper.GetColorName(BNL_Color, ColorList);
				if (!string.IsNullOrEmpty(col))
				{
					color = col;
				}
				return color;
			}
			set
			{
				var oldValue = BNL_Color;
				BNL_Color = ColorHelper.SetColorName(ColorList, value, out color);

				if (BNL_Color != oldValue)
				{
					RefreshBinding();
				}
			}
		}
		ZString color;

		public ZPropertyInfo ColorInfo => GetZPropertyInfo(nameof(Color));

		#endregion

		#region Related Business Objects

		public BMNCNRootDiagramShape Diagram => Factory.Load<BMNCNRootDiagramShape>(BNL_ParentId);

		IEnumerable<BMNCNChannel> OtherChannelsInDiagram => Diagram?.Channels.Where(x => x != this);

		IEnumerable<BMNCNShape> GetShapes()
		{
			int channelTop = 0;
			int channelBottom = 0;
			var allChannels = Diagram?.Channels;

			if (allChannels != null)
			{
				channelTop = allChannels.OrderBy(c => c.BNL_Sequence).TakeWhile(c => c.BNL_Sequence != BNL_Sequence).Sum(c => c.BNL_Height);
				channelBottom = channelTop + BNL_Height;
			}

			return Diagram?.ChildShapes.Where(s => s.Top + s.Height > channelTop && s.Top < channelBottom);
		}

		void ResetShapes()
		{
			Shapes.Clear();

			if (OtherChannelsInDiagram != null)
			{
				foreach (var channel in OtherChannelsInDiagram)
				{
					channel.Shapes.Clear();
				}
			}
		}

		public List<BMNCNShape> Shapes
		{
			get
			{
				if (shapes == null)
				{
					shapes = new List<BMNCNShape>();
				}

				if (Diagram != null && shapes.IsNullOrEmpty())
				{
					shapes.AddRange(GetShapes());
				}

				return shapes;
			}
		}
		List<BMNCNShape> shapes;

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BNL_Sequence = 1;
		}

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return base.GetPropertiesToExcludeFromCloning().Concat(new[] { BMNCNChannelSchema.Constants.BNL_ParentId });
		}

		public override void Delete()
		{
			var ruleLinks = Factory.Load<BMNCNLevelingRuleChannelLink>(new ZQuery(BMNCNLevelingRuleChannelLinkSchema.BNK_BNL_Channel, PK));

			foreach (var link in ruleLinks)
			{
				link.Delete();
			}

			base.Delete();
		}

		#endregion

		#region Lookups

		public ColorList ColorList => Factory.GetCachedValue<ColorList>();

		#endregion

		#region IDiagramChannel Members

		string IDiagramChannel.Name => BNL_Name;

		int IDiagramChannel.Height => BNL_Height;

		Color IDiagramChannel.Color => CargoWise.NetworkVisualisation.Business.ColorFromNameConverter.ColorFromName(Color);

		#endregion

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			Color = System.Drawing.Color.LightSeaGreen.ToString();
		}

#endif
		#endregion
	}
}
