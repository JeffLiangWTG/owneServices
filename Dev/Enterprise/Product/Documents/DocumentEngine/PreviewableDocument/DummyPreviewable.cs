using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.PreviewableDocument
{
	public class DummyPreviewable : Disposable, IPreviewableDocument
	{
		readonly public string ErrorMessage;

		public DummyPreviewable(string errorMessage)
			=> this.ErrorMessage = errorMessage;

		public bool IsVector => true;

		public int NumberOfPages => numberOfPages;
		const int numberOfPages = 1;

		public string PreferredExtension => (NoResString)"pdf";

		public bool IsDocumentCorrupted { get; }

		public IPreviewableDocument ExtractPages(int[] pagesToExtract) => this;

		public void ExtractPages(Stream stream, int[] pagesToExtract) { }

		public Size GetPageSize(int pageNb = numberOfPages)
			=> ControlDpiScalingHelper.NewScaledSize(400, 400);

		public IPreviewableDocument Insert(int insertIndex, IPreviewableDocument file) => this;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Already scaled")]
		public void Render(Graphics g, int pageNb, Size size)
		{
			var region = new Rectangle(Point.Empty, GetPageSize());
			var format = new StringFormat();
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;
			var backBrush = SystemBrushes.ControlDark;
			g.FillRectangle(backBrush, region);
			g.DrawString(ErrorMessage, OFont.GetHeaderFont(), Brushes.DarkRed, region, format);
		}

		public void RotatePage(int pageNb, bool clockwise) { }

		public void Save(string path) { }

		protected override void Dispose(bool isDisposing) { }
	}
}
