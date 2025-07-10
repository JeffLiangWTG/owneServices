using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoiceDescription))]
	public class JobInvoiceDescriptionTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestInvoiceDescriptionClearedOnChangingJobType()
		{
			BizObj.JobType = "ALL";
			BizObj.InvoiceDescription = "For ALL";

			BizObj.JobType = "ALL";
			AssertEquals("InvoiceDescription should stay", "For ALL", BizObj.InvoiceDescription);

			BizObj.JobType = "SHP";
			Assert("InvoiceDescription must be cleared", BizObj.InvoiceDescription.IsEmpty);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			JobInvoiceDescription result = new JobInvoiceDescription();

			result.JobType = "SHP";
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Enterprise.Core.Constants.TransportModes.Air;

			return result;
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

		protected new JobInvoiceDescription BizObj
		{
			get { return (JobInvoiceDescription)base.BizObj; }
		}
	}
}
