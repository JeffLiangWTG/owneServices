using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNLevelingRule : AutoBMNCNLevelingRule
	{
		public BMNCNLevelingRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(Diagram))]
		public override ZGuid BNR_BNS_Diagram
		{
			get => base.BNR_BNS_Diagram;
			set => base.BNR_BNS_Diagram = value;
		}

		public override ZString BNR_Name
		{
			get => base.BNR_Name;
			set
			{
				base.BNR_Name = value;

				if (!IsValidationSuspended && Diagram != null)
				{
					foreach (var rule in Diagram.LevelingRules)
					{
						rule.Validation.ValidateBNR_Name();
					}
				}
			}
		}

		[List("Lookups.RuleTypes")]
		public override ZString BNR_Type
		{
			get => base.BNR_Type;
			set => base.BNR_Type = value;
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("936f97f5-e05a-4888-a927-cebab2978c61", "Leveling Rule");

		#region New Properties

		[ResourceStringData("BMNCNLevelingRule.RuleEffect", Caption = "Rule Effect", FullDescription = "The outcome on scheduling this rule creates.")]
		public ZString RuleEffect
		{
			get
			{
				if (!BNR_RuleValueInfo.HasErrors())
				{
					var isSingular = BNR_RuleValue == 1;

					switch (BNR_Type)
					{
						case LevelingRuleTypeList.Codes.MaximumConcurrentEntities:
							return isSingular
								? Res.GetString("1145ae39-467e-49c0-91e3-6b1ba393ba0b", "No more than 1 entity may be scheduled to run concurrently.")
								: Res.GetString("4ef59cb4-4a17-4cdf-9183-b85d9efa3215", "No more than {0} entities may be scheduled to run concurrently.", BNR_RuleValue);

						case LevelingRuleTypeList.Codes.MinimumEntityStartGapSize:
							return isSingular
								? Res.GetString("08e8b746-6565-4d5d-a4f4-2531396f1feb", "Entities may not be scheduled to start closer than 1 scale unit from the start of the preceding entity.")
								: Res.GetString("fd60b1ad-821b-4933-81c9-d5edb3f66437", "Entities may not be scheduled to start closer than {0} scale units from the start of the preceding entity.", BNR_RuleValue);

						case LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities:
							return isSingular
								? Res.GetString("4fc57d2c-ea0e-4956-8cfa-79ce6f3026e8", "Entities may not be scheduled to start closer than 1 scale unit from the end of the preceding entity.")
								: Res.GetString("7b68f198-13dd-4d2e-9bd3-66eb12ae4edc", "Entities may not be scheduled to start closer than {0} scale units from the end of the preceding entity.", BNR_RuleValue);
					}
				}

				return string.Empty;
			}
		}

		[List("Lookups.ColorList")]
		[ResourceStringData("BMNCNLevelingRule.ColorName", Caption = "Color", FullDescription = "The color with which to render shapes and time slots that violate this Leveling Rule.")]
		public ZString ColorName
		{
			get
			{
				var col = ColorHelper.GetColorName(BNR_Color, Lookups.ColorList);
				if (!string.IsNullOrEmpty(col))
				{
					colorName = col;
				}
				return colorName;
			}
			set
			{
				BNR_Color = ColorHelper.SetColorName(Lookups.ColorList, value, out colorName);

				ColorNameInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateColorName();
				}
			}
		}
		ZString colorName;

		public ZPropertyInfo ColorNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ColorName)); }
		}

		#endregion

		#region Related Business Objects

		public BMNCNRootDiagramShape Diagram => Factory.Load<BMNCNRootDiagramShape>(BNR_BNS_Diagram);

		[ChildEditable]
		public BMNCNLevelingRuleChannelLinkCollection ChannelLinks
		{
			get
			{
				if (channelLinks == null)
				{
					channelLinks = new BMNCNLevelingRuleChannelLinkCollection(this);
					RegisterEditableChildObject(channelLinks);
				}

				return channelLinks;
			}
		}

		BMNCNLevelingRuleChannelLinkCollection channelLinks;

		#endregion

		#region BusinessObject Overrides

		public override void OnSaving()
		{
			base.OnSaving();

			if (Diagram != null && !Diagram.BNS_BNS_RootShape.IsEmpty)
			{
				throw new NotSupportedException("Somehow we created a leveling rule for a non-diagram. This isn't supported. Sad!");
			}
		}

		public override void Delete()
		{
			ChannelLinks.DeleteAll();

			base.Delete();
		}

		#endregion
	}
}
