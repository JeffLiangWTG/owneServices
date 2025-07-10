using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.PreviewableDocument;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentSplitManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StorageDocsBase DocumentToSplit { get; set; }
		public BusinessObjectCollection CollectionToAddTo { get; }

		public DocumentSplitManager(StorageDocsBase sourceDocumentToSplit)
			: this(sourceDocumentToSplit, sourceDocumentToSplit.ParentMain.eDocs)
		{
		}

		public DocumentSplitManager(StorageDocsBase sourceDocumentToSplit, BusinessObjectCollection collectionToAddTo)
		{
			DocumentToSplit = Argument.NotNull(sourceDocumentToSplit, nameof(sourceDocumentToSplit));
			CollectionToAddTo = Argument.NotNull(collectionToAddTo, nameof(collectionToAddTo));
			DocumentSplitResultCollection = new DocumentSplitResultInfoCollection(this);
		}

		public void LogSplit(StorageDocsBase document)
		{
			document.ParentMain?.AddLogsForNewDocument(document.ParentMain.DocumentOwner, document);
		}

		public DocumentSplitConfigInfoCollection DocumentSplitConfigCollection
		{
			get
			{
				if (documentSplitConfigCollection == null)
				{
					using (var previewableDocument = GetPreviewableDocument())
					{
						documentSplitConfigCollection = new DocumentSplitConfigInfoCollection(this, previewableDocument.NumberOfPages);
						RegisterEditableChildObject(documentSplitConfigCollection);
					}
				}
				return documentSplitConfigCollection;
			}
		}
		DocumentSplitConfigInfoCollection documentSplitConfigCollection;

		public DocumentSplitResultInfoCollection DocumentSplitResultCollection { get; }

		#region Split Conditions

		public ZDecimal MaxSizeInKb
		{
			get { return maxSizeInKb; }
			set
			{
				SetNonPersistentPropertyValue(MaxSizeInMbInfo, ref maxSizeInMb, value / 1024.00m);
				SetNonPersistentPropertyValue(MaxSizeInKbInfo, ref maxSizeInKb, value);
				ValidateMaxSize();
				UpdateNeedSpliting();
			}
		}
		ZDecimal maxSizeInKb;

		public ZPropertyInfo MaxSizeInKbInfo
		{
			get { return GetZPropertyInfo(nameof(MaxSizeInKb)); }
		}

		public ZDecimal MaxSizeInMb
		{
			get { return maxSizeInMb; }
			set
			{
				SetNonPersistentPropertyValue(MaxSizeInMbInfo, ref maxSizeInMb, value);
				SetNonPersistentPropertyValue(MaxSizeInKbInfo, ref maxSizeInKb, value * 1024);
				ValidateMaxSize();
				UpdateNeedSpliting();
			}
		}
		ZDecimal maxSizeInMb;

		public ZPropertyInfo MaxSizeInMbInfo
		{
			get { return GetZPropertyInfo(nameof(MaxSizeInMb)); }
		}

		void UpdateNeedSpliting()
		{
			foreach (DocumentSplitConfigInfo splitInfo in DocumentSplitConfigCollection)
			{
				splitInfo.NeedsSplitting = true;
			}
		}

		void ValidateMaxSize()
		{
			MaxSizeInKbInfo.ClearAllNotifications();
			MaxSizeInMbInfo.ClearAllNotifications();

			if (!IsValidationSuspended)
			{
				if (MaxSizeInKb < 0 || MaxSizeInMb < 0)
				{
					MaxSizeInKbInfo.AddError(Res.GetString("3f389fa2-a181-4626-ac48-deb0e010bab4", "Max size should be equal or larger than zero."));
					MaxSizeInMbInfo.AddError(Res.GetString("3f389fa2-a181-4626-ac48-deb0e010bab4", "Max size should be equal or larger than zero."));
				}
				else if (MaxSizeInKb > int.MaxValue)
				{
					MaxSizeInKbInfo.AddError(Res.GetString("6BF48A19-9F6F-418B-A775-D968AFB71B7C", "Max size should be equal or less than {0}.", int.MaxValue));

					if (MaxSizeInMb > int.MaxValue)
					{
						MaxSizeInMbInfo.AddError(Res.GetString("6BF48A19-9F6F-418B-A775-D968AFB71B7C", "Max size should be equal or less than {0}.", int.MaxValue));
					}
				}
			}
		}

		#endregion

		#region Split Functions

		public IPreviewableDocument GetPreviewableDocument()
		{
			var extension = DocumentToSplit.SC_DataType == Core.Constants.DocManagerCodes.Unallocated ? DocumentToSplit.EDocFormat : DocumentToSplit.SC_DataType;
			return PreviewableDocumentHelper.GetPreviewableDocument(extension, DocumentToSplit.SC_ImageData);
		}

		public void SplitDocument()
		{
			DocumentSplitResultCollection.RemoveAll();
			var usedDocNamesInEDocs = GetUsedDocumentNamesInEDocs();
			var usedDocNamesInConfig = new Dictionary<string, int>();
			var sortedSplitConfig = DocumentSplitConfigCollection.Cast<DocumentSplitConfigInfo>().OrderBy(x => x.DocumentName).ThenBy(y => y.StartPage);

			using (var previewableDocument = GetPreviewableDocument())
			using (var extractStream = new MemoryStream())
			{
				foreach (var splitConfig in sortedSplitConfig)
				{
					var splitDocumentData = new List<SplitDocumentInfo>();
					if (MaxSizeInKb == 0)
					{
						splitDocumentData.Add(SplitDocument(splitConfig.StartPage - 1, splitConfig.EndPage - 1, extractStream, previewableDocument));
					}
					else
					{
						splitDocumentData = SplitDocument(MaxSizeInKb.ToZInt() * 1024, splitConfig.StartPage - 1, splitConfig.EndPage - 1, extractStream, previewableDocument);
					}

					for (int j = 0; j < splitDocumentData.Count; j++)
					{
						var splitedDocument = splitDocumentData[j];
						var documentSplitResult = DocumentSplitResultCollection.AddNew();

						documentSplitResult.DocumentName = GetUniqueDocumentName(splitConfig, splitedDocument, usedDocNamesInEDocs, usedDocNamesInConfig);
						documentSplitResult.DocumentType = splitConfig.DocumentType;
						documentSplitResult.DescriptionType = splitConfig.DescriptionType;
						documentSplitResult.StartPage = splitedDocument.StartPage;
						documentSplitResult.EndPage = splitedDocument.EndPage;
						documentSplitResult.DocumentData = splitedDocument.DocumentData;
						documentSplitResult.ActualSizeInKb = splitedDocument.DocumentData.Length / 1024;
						documentSplitResult.IsPublished = splitConfig.IsPublished;
					}
					splitConfig.NeedsSplitting = false;
				}
			}
		}

		SplitDocumentInfo SplitDocument(int startPage, int endPage, MemoryStream extractStream, IPreviewableDocument previewableDocument)
		{
			CheckPageNumbers(startPage, endPage, previewableDocument.NumberOfPages);

			var res = new SplitDocumentInfo() { StartPage = startPage + 1, EndPage = endPage + 1 };
			ResetStreamAndExtract(extractStream, previewableDocument, startPage, endPage);
			res.DocumentData = extractStream.ToArray();
			return res;
		}

		List<SplitDocumentInfo> SplitDocument(int maxSize, int startPage, int endPage, MemoryStream extractStream, IPreviewableDocument previewableDocument)
		{
			CheckPageNumbers(startPage, endPage, previewableDocument.NumberOfPages);

			var res = new List<SplitDocumentInfo>();
			var nextStart = startPage;
			while (nextStart <= endPage)
			{
				var splitData = ExtractPagesWithSizeCloseToMaxSize(previewableDocument, extractStream, maxSize, nextStart, endPage);
				res.Add(splitData);
				nextStart = splitData.EndPage;
			}

			return res;
		}

		static void CheckPageNumbers(int startPage, int endPage, int numberOfPages)
		{
			if (endPage < startPage || startPage < 0 || endPage >= numberOfPages)
			{
				throw new ArgumentException("All page numbers must be valid");
			}
		}

		SplitDocumentInfo ExtractPagesWithSizeCloseToMaxSize(IPreviewableDocument previewableDocument, MemoryStream extractStream, int maxSize, int startPage, int maxEndPage)
		{
			int min = startPage, max = maxEndPage;
			var splitEndPage = -1;
			while (min <= max)
			{
				splitEndPage = (min + max) / 2;
				ResetStreamAndExtract(extractStream, previewableDocument, startPage, splitEndPage);

				if (maxSize < extractStream.Length)
				{
					max = splitEndPage - 1;
				}
				else if (maxSize > extractStream.Length)
				{
					min = splitEndPage + 1;
				}
				else // Rediculously unlikely
				{
					break;
				}
			}

			if (min > startPage && extractStream.Length > maxSize)
			{
				splitEndPage = Math.Min(min, max);
				ResetStreamAndExtract(extractStream, previewableDocument, startPage, splitEndPage);
			}

			return new SplitDocumentInfo
			{
				StartPage = startPage + 1,
				EndPage = splitEndPage + 1,
				DocumentData = extractStream.ToArray()
			};
		}

		void ResetStreamAndExtract(MemoryStream stream, IPreviewableDocument document, int startPage, int endPage)
		{
			stream.SetLength(0);
			document.ExtractPages(stream, Enumerable.Range(startPage, endPage - startPage + 1).ToArray());
		}
#if DEBUG
		public
#else
		internal
#endif
		HashSet<string> GetUsedDocumentNamesInEDocs()
		{
			var docNames = DocumentToSplit.ParentMain?.eDocs
				.Cast<StorageDocsBase>()
				.Where(doc => !doc.IsDeleted)
				.Select(doc => doc.SC_FileName.ToString());

			return new HashSet<string>(docNames ?? Enumerable.Empty<string>());
		}

		string GetUniqueDocumentName(DocumentSplitConfigInfo splitConfig, SplitDocumentInfo splitResult, HashSet<string> usedDocNamesInEDocs, Dictionary<string, int> usedDocNamesInConfig)
		{
			var originalDocumentName = splitConfig.DocumentName;
			var result = string.Empty;

			if (!usedDocNamesInConfig.ContainsKey(splitConfig.DocumentName))
			{
				usedDocNamesInConfig.Add(splitConfig.DocumentName, 0);
			}

			var endPageNumberOfPreviousConfig = usedDocNamesInConfig[originalDocumentName];
			var startPage = endPageNumberOfPreviousConfig + 1;
			var endPage = splitResult.EndPage - splitResult.StartPage + endPageNumberOfPreviousConfig + 1;

			if (splitConfig.AppendPageNumber)
			{
				if (splitResult.StartPage == splitResult.EndPage)
				{
					result = Res.GetString("00AC7C85-1C14-42B0-BFBB-AB05324B0BBD", "{0} (Page {1})", originalDocumentName, startPage);
				}
				else
				{
					result = Res.GetString("35494869-AF7A-4E55-AD08-28B2AA22F4CF", "{0} (Page {1} - {2})", originalDocumentName, startPage, endPage);
				}
			}
			else
			{
				result = splitConfig.DocumentName;
			}

			var tempName = result;
			var suffixNumber = 1;
			while (usedDocNamesInEDocs.Contains(tempName))
			{
				tempName = string.Concat(result, '[', suffixNumber, ']');
				suffixNumber++;
			}

			result = tempName;
			usedDocNamesInEDocs.Add(result);
			usedDocNamesInConfig[originalDocumentName] = endPage;

			return result;
		}

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidateMaxSize();
			DocumentSplitConfigCollection.ValidateAll();
		}

		#endregion
	}

	public class SplitDocumentInfo
	{
		public int StartPage
		{
			get;
			set;
		}

		public int EndPage
		{
			get;
			set;
		}

		public ZBlob DocumentData
		{
			get;
			set;
		}
	}
}
