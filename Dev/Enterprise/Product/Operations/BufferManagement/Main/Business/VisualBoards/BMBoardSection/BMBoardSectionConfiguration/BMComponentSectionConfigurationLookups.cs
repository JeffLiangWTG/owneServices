using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentSectionConfigurationLookups : ZLookups
	{
		public BMComponentSectionConfigurationLookups(BMComponentSectionConfiguration parent)
			: base(parent)
		{
		}

		BMComponentSectionConfiguration SectionConfiguration
		{
			get { return (BMComponentSectionConfiguration)Parent; }
		}

		#region ColorList

		public CodeDescriptionPairList ColorList
		{
			get { return Factory.GetCachedValue("Enterprise.ZArchitecture.Core.ColorList", () => new ColorList()); }
		}

		#endregion

		#region ChannelByList

		public CodeDescriptionPairList ChannelByList
		{
			get
			{
				var channelTypes = new ChannelTypeList();
				var list = new CodeDescriptionPairList();

				foreach (var code in DefaultChannelsProvider.SupportedChannelTypes)
				{
					var description = channelTypes.GetDescriptionFromCode(code);
					list.Add(new CodeDescriptionPair(code, description));
				}

				return list;
			}
		}

		public CodeDescriptionPairList ChannelSecondaryByList
		{
			get
			{
				var list = ChannelByList;
				list.AddPair(BMConstants.ChannelByTimeCode, BMConstants.ChannelByTimeDescription);
				return list;
			}
		}

		#endregion

		#region Section layout

		public CodeDescriptionPairList FlowDirectionList
		{
			get { return Factory.GetCachedValue<FlowDirectionList>(); }
		}

		public CodeDescriptionPairList LastCellList
		{
			get
			{
				return Factory.GetCachedValue("BMBoardSectionLookups.LastCellList." + SectionConfiguration.Orientation, () =>
				{
					var list = new LastCellList();
					var flowDirection = SectionConfiguration.FlowDirection.ToString();
					switch (flowDirection)
					{
						case BufferManagement.Business.FlowDirectionList.Codes.Up:
						case BufferManagement.Business.FlowDirectionList.Codes.Down:
							list.RemoveCode(BufferManagement.Business.LastCellList.Codes.Top);
							list.RemoveCode(BufferManagement.Business.LastCellList.Codes.Bottom);
							break;

						case BufferManagement.Business.FlowDirectionList.Codes.Left:
						case BufferManagement.Business.FlowDirectionList.Codes.Right:
							list.RemoveCode(BufferManagement.Business.LastCellList.Codes.Left);
							list.RemoveCode(BufferManagement.Business.LastCellList.Codes.Right);
							break;
					}
					return list;
				});
			}
		}

		public CodeDescriptionPairList PanelLayoutStyleList
		{
			get { return Factory.GetCachedValue<PanelLayoutTypeList>(); }
		}

		public CodeDescriptionPairList ChannelTypeList
		{
			get { return Factory.GetCachedValue<ChannelTypeList>(); }
		}

		public CodeDescriptionPairList TimeProgressionModeList
		{
			get { return Factory.GetCachedValue<TimeProgressionModeList>(); }
		}

		public CodeDescriptionPairList TimeFieldList
		{
			get
			{
				var list = new TimeProgressionFieldList();
				if (SectionConfiguration.IsBuffer)
				{
					list.Clear();
					list.Add(new CodeDescriptionPair(TimeProgressionFieldList.Codes.TransferTime, TimeProgressionFieldList.Descriptions.TransferTime));
					list.Add(new CodeDescriptionPair(TimeProgressionFieldList.Codes.WorkingTimeSinceStartable, TimeProgressionFieldList.Descriptions.WorkingTimeSinceStartable));
				}

				return list;
			}
		}

		public CodeDescriptionPairList CardTypeList
		{
			get { return Factory.GetCachedValue<CardTypeList>(); }
		}

		#endregion

		public ActiveBusinessObjectCollection<GlbGroup> SystemReleaseGroups => new GlbGroupActiveBusinessObjectCollection(Factory, staffGroupOnly: true);

		#region Customised Cards

		public BMControlCustomisationCollection DetailedCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.DetailedCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.DetailedCard)); }
		}

		public BMControlCustomisationCollection SummaryCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.SummaryCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.TaskCard)); }
		}

		public BMControlCustomisationCollection WorkflowDetailedCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.WorkflowDetailedCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard)); }
		}

		public BMControlCustomisationCollection WorkflowSummaryCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.WorkflowSummaryCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard)); }
		}

		#endregion
	}
}
