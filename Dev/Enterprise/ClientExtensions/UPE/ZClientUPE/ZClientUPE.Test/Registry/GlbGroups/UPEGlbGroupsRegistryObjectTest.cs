using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEGlbGroupsRegistryObject))]
	public class UPEGlbGroupsRegistryObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			UPEGlbGroupsRegistryObject result = new UPEGlbGroupsRegistryObject();
			result.Group = Core.Constants.Groups.AllPK;
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected new UPEGlbGroupsRegistryObject BizObj
		{
			get
			{
				return (UPEGlbGroupsRegistryObject)base.BizObj;
			}
		}
		#endregion
	}
}
