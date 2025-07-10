using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ZeroAmountTaxTypesDescriptions))]
	public class ZeroAmountTaxTypesDescriptionsTest : RegistryBusinessObjectTemplateTestCase<ZeroAmountTaxTypesDescriptions>
	{
		#region Properties

		public void TestTaxType()
		{
			BizObj.TaxType = Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt;
			AssertEquals(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt, BizObj.TaxType);
		}

		public void TestDescription()
		{
			BizObj.Description = "Description Test";
			AssertEquals("Description Test", BizObj.Description);
		}

		public void TestDefaultValue()
		{
			BizObj.DefaultValue = (NoResString)"Default Value";
			AssertEquals("Default Value", BizObj.DefaultValue);
		}

		public void TestOverrideValue()
		{
			BizObj.OverrideValue = (NoResString)"Override Value";
			AssertEquals("Override Value", BizObj.OverrideValue);
		}

		#endregion

		#region Implementation

		protected override ZeroAmountTaxTypesDescriptions GetBusinessObjectToClone()
		{
			return new ZeroAmountTaxTypesDescriptions();
		}

		protected override ZeroAmountTaxTypesDescriptions GetBusinessObjectToSerialise()
		{
			return new ZeroAmountTaxTypesDescriptions();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ZeroAmountTaxTypesDescriptions BizObj
		{
			get { return base.BizObj; }
		}

		#endregion
	}
}
