using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.ExcelTemplates
{
	public abstract class ExcelTemplate
	{
		public ExcelTemplate(ZString templateName, ZString templateSourceLocation)
		{
			fTemplateName = templateName;
			fTemplateSourceLocation = templateSourceLocation;
		}
		readonly ZString fTemplateName;
		readonly ZString fTemplateSourceLocation;

		public ZString TemplateName
		{
			get { return fTemplateName; }
		}

		public ZString TemplateSourceLocation
		{
			get { return fTemplateSourceLocation; }
		}

		public bool ContainsCustomisedSections { get; set; }

		/// <summary>
		/// PLEASE DISPOSE THE STREAM WHEN YOU ARE FINISHED WITH IT. ExcelTemplate is not IDisposable and no longer keeps any reference to the stream.
		/// </summary>
		/// <returns>A stream for the contents of the template</returns>
		public Stream GetAsTemplateStream()
		{
			return GetAsTemplateStreamInternal();
		}
		protected abstract Stream GetAsTemplateStreamInternal();

		public long FileSizeInBytes => GetFileSizeInBytesCore();

		protected virtual long GetFileSizeInBytesCore()
		{
			using (var stream = GetAsTemplateStream())
			{
				return stream.Length;
			}
		}

		public byte[] GetAsByteArray()
		{
			return GetAsByteArrayInternal();
		}
		protected abstract byte[] GetAsByteArrayInternal();

		protected byte[] GetTemplateStreamAsByteArray()
		{
			using (Stream templateStream = GetAsTemplateStream())
			{
				return Enterprise.ZArchitecture.Environment.AttachmentDef.StreamToByteArray(templateStream);
			}
		}

		public ExcelInterface GetNewExcelInterface()
		{
			ExcelInterface xlInterface = null;
			try
			{
				using (var templateStream = GetAsTemplateStream())
				{
					xlInterface = ExcelInterface.GetLoadedExcelInterface(templateStream);
				}
				return xlInterface;
			}
			catch
			{
				xlInterface?.Dispose();
				throw;
			}
		}
	}
}
