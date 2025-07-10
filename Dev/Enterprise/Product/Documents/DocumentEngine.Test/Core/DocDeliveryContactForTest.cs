using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocDeliveryContactForTest : DocDeliveryContact
	{
		public DocDeliveryContactForTest(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override DocDeliveryContactValidation GetNewValidation()
		{
			if (validationTest == null)
			{
				validationTest = new DocDeliveryContactValidationForTest(this);
			}
			return validationTest;
		}

		DocDeliveryContactValidationForTest validationTest;
	}
}
