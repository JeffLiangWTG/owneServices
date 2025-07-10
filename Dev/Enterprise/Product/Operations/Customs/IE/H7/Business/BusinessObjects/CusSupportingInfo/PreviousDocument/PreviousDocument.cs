using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class PreviousDocument : EU.H7.Business.PreviousDocument
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

		public new ValidationConfiguration ValidationConfiguration => (ValidationConfiguration)base.ValidationConfiguration;

		protected override EU.H7.Business.ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public ValidationMessage ValidationMessage => ValidationConfiguration.ValidationMessage;

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);
	}
}
