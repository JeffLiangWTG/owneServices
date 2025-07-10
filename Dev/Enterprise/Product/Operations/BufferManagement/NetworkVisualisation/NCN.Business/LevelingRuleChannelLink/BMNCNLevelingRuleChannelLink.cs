using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("Rule: {Rule.BNR_Name}, {Channel.BNL_Name}")]
	public class BMNCNLevelingRuleChannelLink : AutoBMNCNLevelingRuleChannelLink
	{
		public BMNCNLevelingRuleChannelLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(Rule))]
		public override ZGuid BNK_BNR_Rule
		{
			get => base.BNK_BNR_Rule;
			set => base.BNK_BNR_Rule = value;
		}

		[List("Lookups.Channels")]
		[RelatedBusinessObject(nameof(Channel))]
		public override ZGuid BNK_BNL_Channel
		{
			get => base.BNK_BNL_Channel;
			set => base.BNK_BNL_Channel = value;
		}

		#endregion

		#region Related Business Objects

		public BMNCNLevelingRule Rule => Factory.Load<BMNCNLevelingRule>(BNK_BNR_Rule);

		public BMNCNChannel Channel => Factory.Load<BMNCNChannel>(BNK_BNL_Channel);

		#endregion
	}
}
