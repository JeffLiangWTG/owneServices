using System;
using System.IO;

namespace CargoWise.EntityFramework
{
	public class FileTypeValidation
	{
		public bool IsValidFileExtensionForEmail(string fileName)
		{
			return !FileMatchesDangerousExtension(new FileInfo(fileName));
		}

		public bool IsValidFileExtensionForFax(string fileName)
		{
			return IsFileType(fileName, ".tif") || IsFileType(fileName, ".tiff");
		}

		/// <summary>
		/// Returns true for any file with a dangerous file extension.
		/// </summary>
		public bool IsDangerousFile(string fileName)
		{
			try
			{
				return FileMatchesDangerousExtension(new FileInfo(fileName));
			}
			catch (ArgumentException)
			{
				return true;
			}
		}

		/// <summary>
		/// Returns true for any file with a dangerous file extension.
		/// Uses the file's data to report what the file extension appears to be.
		/// </summary>
		public bool IsDangerousFile(Stream fileData, out string apparentFileName)
		{
			apparentFileName = FileTypeChecker.GetFileType(fileData);
			if (!string.IsNullOrEmpty(apparentFileName))
			{
				return ExtensionMatchesDangerousExtension(apparentFileName);
			}
			return false;
		}

		/// <summary>
		/// Checks if the specified filename is of the specified file type.
		/// </summary>
		/// <param name="fileName">The filename to check</param>
		/// <param name="fileExtension">The extension to check for, with the leading dot (.)</param>
		/// <returns>True if the file types match</returns>
		public bool IsFileType(string fileName, string fileExtension)
		{
			return fileExtension.Equals(new FileInfo(fileName).Extension, StringComparison.InvariantCultureIgnoreCase);
		}

		#region Dangerous File Extensions

		bool FileMatchesDangerousExtension(FileInfo file)
		{
			if (ExtensionMatchesDangerousExtension(file.Extension))
			{
				return true;
			}

			// Check for invalid file name characters
			if (!IsValidFileName(file.Name))
			{
				return true;
			}

			return false;
		}

		bool ExtensionMatchesDangerousExtension(string ext)
		{
			ext = ext.ToUpperInvariant();
			if (!ext.StartsWith(".", StringComparison.Ordinal))
			{
				ext = "." + ext;
			}

			bool result = false;

			foreach (string nextExt in DangerousFileExtensions)
			{
				bool match = nextExt.Length == ext.Length;
				if (match)
				{
					for (int i = 0; i < nextExt.Length; i++)
					{
						if (nextExt[i] != '?' && nextExt[i] != ext[i])
						{
							match = false;
						}
					}
				}
				if (match)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		bool IsValidFileName(string fileName)
		{
			var invalidChars = Path.GetInvalidFileNameChars();
			return fileName.IndexOfAny(invalidChars) == -1;
		}

		protected internal static readonly string[] DangerousFileExtensions = new string[]
		{
			".ADE",
			".ADP",
			".APP",
			".APPLICATION",
			".APPREF-MS",
			".ASP",
			".ASPX",
			".ASX",
			".BAS",
			".BAT",
			".BGI",
			".CAB",
			".CER",
			".CHM",
			".CMD",
			".CNT",
			".COM",
			".CPL",
			".CRT",
			".CSH",
			".DER",
			".DIAGCAB",
			".EXE",
			".FXP",
			".GADGET",
			".GRP",
			".HLP",
			".HPJ",
			".HTA",
			".HTC",
			".INF",
			".INS",
			".ISO",
			".ISP",
			".ITS",
			".JAR",
			".JNLP",
			".JS",
			".JSE",
			".KSH",
			".LNK",
			".MAD",
			".MAF",
			".MAG",
			".MAM",
			".MAQ",
			".MAR",
			".MAS",
			".MAT",
			".MAU",
			".MAV",
			".MAW",
			".MCF",
			".MDA",
			".MDB",
			".MDE",
			".MDT",
			".MDW",
			".MDZ",
			".MSC",
			".MSH",
			".MSH1",
			".MSH2",
			".MSHXML",
			".MSH1XML",
			".MSH2XML",
			".MSI",
			".MSP",
			".MST",
			".MSU",
			".OPS",
			".OSD",
			".PCD",
			".PIF",
			".PL",
			".PLG",
			".PRF",
			".PRG",
			".PRINTEREXPORT",
			".PS1",
			".PS1XML",
			".PS2",
			".PS2XML",
			".PSC1",
			".PSC2",
			".PSD1",
			".PSDM1",
			".PST",
			".PY",
			".PYC",
			".PYO",
			".PYW",
			".PYZ",
			".PYZW",
			".REG",
			".SCF",
			".SCR",
			".SCT",
			".SHB",
			".SHS",
			".THEME",
			".TMP",
			".URL",
			".VB",
			".VBE",
			".VBP",
			".VBS",
			".VHD",
			".VHDX",
			".VSMACROS",
			".VSW",
			".WEBPNP",
			".WEBSITE",
			".WS",
			".WSC",
			".WSF",
			".WSH",
			".XBAP",
			".XLL",
			".XNK"
		};

		#endregion
	}
}
