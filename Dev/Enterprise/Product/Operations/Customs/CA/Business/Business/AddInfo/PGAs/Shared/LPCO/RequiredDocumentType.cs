using System;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface IRequiredDocumentType
	{
		ZString Code { get; }
		DocumentTypeRequieredType RequiredType { get; }
		RequiredDocumentTypeCategory Category { get; }
	}

	public class RequiredDocumentType : IRequiredDocumentType
	{
		public RequiredDocumentType(ZString code, DocumentTypeRequieredType requiredType = DocumentTypeRequieredType.Mandatory, RequiredDocumentTypeCategory category = null)
		{
			Code = code;
			RequiredType = requiredType;
			Category = category ?? DefaultRequiredDocumentTypeCategory;
		}

		public ZString Code { get; }
		public DocumentTypeRequieredType RequiredType { get; }
		public RequiredDocumentTypeCategory Category { get; }

		public static RequiredDocumentTypeCategory DefaultRequiredDocumentTypeCategory => defaultRequiredDocumentTypeCategory ?? (defaultRequiredDocumentTypeCategory = new RequiredDocumentTypeCategory());
		[ThreadStatic]
		static RequiredDocumentTypeCategory defaultRequiredDocumentTypeCategory;
	}

	public enum DocumentTypeRequieredType
	{
		Mandatory,
		Optional,
		Alternative,
		AtLeastOne
	}
}
