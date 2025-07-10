using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	public class AsycudaBillForTest : AsycudaBill
	{
		public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool CreateMessageErrorForTest { get; set; }

		public bool CreateErrorForTest { get; set; }

		public bool CreateWarningForTest { get; set; }

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public new AsycudaBillValidationForTest Validation => GetNewValidation();

		protected new AsycudaBillValidationForTest GetNewValidation() => new AsycudaBillValidationForTest(this);
	}
}
