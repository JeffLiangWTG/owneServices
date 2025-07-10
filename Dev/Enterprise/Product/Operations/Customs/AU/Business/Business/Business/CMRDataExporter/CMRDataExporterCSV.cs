using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRDataExporterCSV : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CMRDataExporterCSV(BusinessObject bizObj)
			: base(bizObj.Factory)
		{
			this.bizObj = bizObj;
		}
		readonly BusinessObject bizObj;

		public BusinessObject BizObj
		{
			get { return bizObj; }
		}

		// Get invalid file name characters and add a space character
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Readonly, and thread-safe for read. Must not be edited/ written to after initialization.")]
		static readonly HashSet<char> InvalidFilePathCharsSet = new(Path.GetInvalidFileNameChars().Concat([' ']));

		#region Generate

		protected ZString StripLineBreakInString(ZString aString)
		{
			return Regex.Replace(aString, "\r\n|\r|\n", " ");
		}

		protected abstract StringCollectionX[] Values { get; }

		public DataExporterResult Generate()
		{
			var content = GenerateContent();
			var fileName = InitializeFileName();
			generatedData = new DataExporterResult(fileName, content);
			return generatedData;
		}

		DataExporterResult generatedData;

		public static ZString SanitizingFileName(ZString fileName)
		{
			var sanitizedFileName = new StringBuilder();

			foreach (var c in fileName)
			{
				if (!InvalidFilePathCharsSet.Contains(c))
				{
					_ = sanitizedFileName.Append(c);
				}
			}

			return sanitizedFileName.ToString();
		}

		ZString GenerateContent()
		{
			StringWriter writer = new();

			foreach (StringCollectionX strings in Values)
			{
				ArrayList fields = [];

				foreach (string value in strings)
				{
					var manipulatedValue = value == " " ? value : value.Replace(",", string.Empty).Replace("\"", "'").Trim();
					_ = fields.Add(manipulatedValue);
				}

				ArrayList includeQuotes = [];
				for (int i = 0; i < fields.Count; i++)
				{
					_ = includeQuotes.Add(false);
				}

				var line = new OCsvLine((string[])fields.ToArray(typeof(string)), (bool[])includeQuotes.ToArray(typeof(bool)));
				writer.WriteLine(line.ToString());
			}

			return writer.ToString();
		}

		ZString InitializeFileName()
		{
			return SanitizingFileName(PartFileName.Trim() + ZDateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture) + FileNameSuffix + ".csv");
		}

		public static ZString CMRDateString(ZDateTime indateTime)
		{
			return indateTime.IsValid ? indateTime.Year.ToString("0000") + indateTime.Month.ToString("00") + indateTime.Day.ToString("00") : string.Empty;
		}

		public static ZString CMRTimeString(ZDateTime indateTime)
		{
			return indateTime.IsValid ? indateTime.Hour.ToString("00") + indateTime.Minute.ToString("00") : string.Empty;
		}

		public static string AddressAsASingleLine(ZString address1, ZString address2, ZString city, ZString state, ZString postcode)
		{
			ZStringBuilder builder = new();
			_ = builder.Append(address1);
			_ = builder.Append(address2);
			_ = builder.Append(city);
			_ = builder.Append(state);
			_ = builder.Append(postcode);
			return builder.ToStringWithDelimiterBetweenAppends(" ");
		}

		#endregion

		#region Save

		public abstract string PartFileName { get; }

		public abstract string MailSubject { get; }

		protected abstract IDataExportCSVFileNameProvider FileNameProvider { get; }

		protected abstract IDocManagerSupport DocManagerSupporter { get; }

		public virtual string FileNameSuffix
		{
			get { return FileNameProvider != null ? FileNameProvider.FileNameSuffix : ZString.Empty; }
		}

		public void SaveToFile(Stream stream)
		{
			if (generatedData != null)
			{
				try
				{
					using (StreamWriter writer = new StreamWriter(stream))
					{
						writer.Write(generatedData.Content);
						writer.Flush();
						writer.Close();
					}
				}
				catch (IOException ex)
				{
					throw new ExportException(ex.Message, ex);
				}
			}
		}

		public void AttachToeDocs()
		{
			if (generatedData != null)
			{
				var docManagerInfo = DocManagerSupporter?.DocManagerInfo;
				if (docManagerInfo != null)
				{
					docManagerInfo.AddFileOrDocument(Encoding.Unicode.GetBytes(generatedData.Content), generatedData.FileName, "MCD", overwriteExistingFileIfNotImageFile: true);
					docManagerInfo.Save();
				}
			}
#if DEBUG
			numOfeDocsExposedToTest++;
#endif
		}

#if DEBUG
		public int numOfeDocsExposedToTest;
#endif

		public virtual ZString BodyText
		{
			get { return string.Empty; }
		}

		public abstract AdditionalContingencyData AdditionalData { get; }

		#endregion
	}
}
