using System;
using System.IO;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.Testing
{
	public static class AttachedDocumentCreator
	{
		public static AttachedDocument Create(string fileName, string base64ImageData, string documentType, string documentDescription = "BLAH", bool isPublished = true)
		{
			var imageStream = (SubStreamableStream)new MemoryStream(Convert.FromBase64String(base64ImageData));

			var attachedDocument = new AttachedDocument
			{
				FileName = fileName,
				ImageData = imageStream,
				Type = new DocumentType { Code = documentType, Description = documentDescription },
				IsPublished = isPublished
			};
			return attachedDocument;
		}
	}
}
