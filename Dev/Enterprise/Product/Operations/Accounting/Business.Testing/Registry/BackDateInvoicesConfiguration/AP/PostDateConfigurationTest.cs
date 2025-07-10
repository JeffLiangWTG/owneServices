using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PostDateConfiguration))]
	public class PostDateConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			PostDateConfiguration result = new PostDateConfiguration();

			result.JobType = "SHP";
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Enterprise.Core.Constants.TransportModes.Air;
			result.SignificantDateCode = PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate;
			result.ReversalRule = PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules;

			return result;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new PostDateConfiguration BizObj
		{
			get
			{
				return (PostDateConfiguration)base.BizObj;
			}
		}
	}
}
