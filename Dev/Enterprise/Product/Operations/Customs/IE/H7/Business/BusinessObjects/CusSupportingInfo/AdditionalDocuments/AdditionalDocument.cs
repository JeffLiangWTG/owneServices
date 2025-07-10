using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalDocument : EU.H7.Business.AdditionalDocument
	{
		public AdditionalDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AdditionalDocumentValidation Validation => (AdditionalDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalDocumentValidation(this);

		public new ValidationConfiguration ValidationConfiguration => (ValidationConfiguration)base.ValidationConfiguration;

		protected override EU.H7.Business.ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public ValidationMessage ValidationMessage => ValidationConfiguration.ValidationMessage;

		public new AdditionalDocumentLookups Lookups => (AdditionalDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalDocumentLookups(this);
	}
}
