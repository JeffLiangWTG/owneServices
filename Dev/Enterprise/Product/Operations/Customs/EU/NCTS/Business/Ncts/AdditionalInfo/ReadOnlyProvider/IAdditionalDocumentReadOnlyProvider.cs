namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IAdditionalDocumentReadOnlyProvider
	{
		bool ReferenceNumberReadOnly { get; }

		bool ReferenceNumber2ReadOnly { get; }

		bool DescriptionReadOnly { get; }

		bool AdditionalInfoReadOnly { get; }

		bool LineNoReadOnly { get; }

		bool StatusReadOnly { get; }
	}
}
