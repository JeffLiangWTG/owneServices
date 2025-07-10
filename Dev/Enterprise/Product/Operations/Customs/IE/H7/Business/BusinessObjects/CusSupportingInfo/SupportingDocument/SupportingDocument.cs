using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class SupportingDocument : EU.H7.Business.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

		public new ValidationConfiguration ValidationConfiguration => (ValidationConfiguration)base.ValidationConfiguration;

		protected override EU.H7.Business.ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public ValidationMessage ValidationMessage => ValidationConfiguration.ValidationMessage;

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);
	}
}
