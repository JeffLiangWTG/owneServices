using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.DocumentScanning.Web
{
	/// <summary>
	/// HTTP handler responsible for serving files and images from eDocs
	/// </summary>
	public class eDocsRequestHandler : DataRequestHandler<eDocsRequestHelper>
	{
		public override string FileName
		{
			get
			{
				string fileName = (NoResString)"*INVALID FILENAME*";
				if (Document != null)
				{
					if (Document.SC_FileName.Trim().IsEmpty)
					{
						if (Document.SC_DescMultilingual.Trim().IsEmpty)
						{
							fileName = Document.PK.ToString() + Document.SC_FileNameWithExtension;
						}
						else
						{
							fileName = Document.SC_DescMultilingual.Trim() + Document.SC_FileNameWithExtension;
						}
					}
					else
					{
						fileName = Document.SC_FileNameWithExtension;
					}

					var invalidChars = Path.GetInvalidFileNameChars();
					if (fileName.IndexOfAny(invalidChars) != -1)
					{
						fileName = PathValidation.GetSafeFilename(fileName, ' ');
					}
				}

				return fileName;
			}
		}

		public override ZBlob GetBinaryData()
		{
			return (Document != null) ? Document.SC_ImageData : ZBlob.Empty;
		}

		public StorageDocsBase Document
		{
			get { return BusinessObjects[0] as StorageDocsBase; }
		}

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			ZGuid parentPK = GetZGuidFromStringSafely(QueryString[eDocsRequestHelper.ParentKey]);

			return new BusinessObject[] { GetSelectedDocument(parentPK, PKs[0]) };
		}

		protected override ZGuid[] GetNewPKs()
		{
			return new ZGuid[] { GetZGuidFromStringSafely(QueryString[eDocsRequestHelper.DocumentKey]) };
		}

		StorageDocsBase GetSelectedDocument(ZGuid parentPK, ZGuid documentPK)
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain parent = masterFactory.GetStorageMainForPK(parentPK);

			if (parent != null)
			{
				StorageDocsCollectionViewBase parentDocuments = parent.PublishedEDocsAndFiles;
				return parentDocuments.FindByPK(documentPK) as StorageDocsBase;
			}
			else
			{
				return null;
			}
		}
	}
}
