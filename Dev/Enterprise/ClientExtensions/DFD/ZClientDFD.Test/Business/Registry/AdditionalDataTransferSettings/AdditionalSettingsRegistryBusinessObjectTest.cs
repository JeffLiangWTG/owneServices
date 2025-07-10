using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(AdditionalSettingsRegistryBusinessObject))]
	internal class AdditionalSettingsRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Overrides
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.ExportFileName = "ORGEXP";
			BizObj.NextRunDateTime = ZDateTime.Now.AddDays(1);
			return BizObj;
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

		protected new AdditionalSettingsRegistryBusinessObject BizObj
		{
			get
			{
				return (AdditionalSettingsRegistryBusinessObject)base.BizObj;
			}
		}
		#endregion
	}
}
