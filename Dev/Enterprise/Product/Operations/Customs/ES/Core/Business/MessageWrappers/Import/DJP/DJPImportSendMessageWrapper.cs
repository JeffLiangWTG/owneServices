using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DJPImportSendMessageWrapper : ImportCommonSendMessageWrapper, IDJPImportMessageDataProvider
	{
		public DJPImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		public IReadOnlyCollection<IDJPDocument> Documents
		{
			get
			{
				if (documents == null)
				{
					documents = (new List<DJPDocumentWrapper>()).AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<DJPDocumentWrapper> documents;

		public IReadOnlyCollection<IDJPDeclaration> Declarations
		{
			get
			{
				if (declarations == null)
				{
					var declarationsList = new List<DJPDeclarationWrapper>
					{
						new DJPDeclarationWrapper(entryHeader)
					};

					declarations = declarationsList.AsReadOnly();
				}
				return declarations;
			}
		}
		IReadOnlyCollection<DJPDeclarationWrapper> declarations;

		protected override ZString MRNCore => ZString.Empty;
	}
}
