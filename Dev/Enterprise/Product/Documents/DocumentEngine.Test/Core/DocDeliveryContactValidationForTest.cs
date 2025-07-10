using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocDeliveryContactValidationForTest : DocDeliveryContactValidation
	{
		public DocDeliveryContactValidationForTest(DocDeliveryContact parent)
			: base(parent)
		{
		}

		internal new void CheckDeliveryMethod()
		{
			CallCountForValidateDeliveryMethod++;
		}

		internal int CallCountForValidateDeliveryMethod { get; set; }
	}
}
