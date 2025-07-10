using System;
using System.Drawing;
using System.IO;

namespace Enterprise.DocumentEngine.PreviewableDocument
{
	public interface IPreviewableDocument : IDisposable
	{
		bool IsVector { get; }
		int NumberOfPages { get; }
		string PreferredExtension { get; }

		bool IsDocumentCorrupted { get; }

		Size GetPageSize(int pageNb);
		void Render(Graphics g, int pageNb, Size size);

		void RotatePage(int pageNb, bool clockwise);

		IPreviewableDocument Insert(int insertIndex, IPreviewableDocument file);
		IPreviewableDocument ExtractPages(int[] pagesToExtract);

		void ExtractPages(Stream stream, int[] pagesToExtract);

		void Save(string path);
	}
}
