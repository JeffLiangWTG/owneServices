using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.XPath;
using CargoWise.Schema;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Schema.Test
{
	internal class ObsoleteColumnsTest : TestCase
	{
		public void TestObsoleteFieldsInDatabaseTablesAreRemovedByDueDate()
		{
			var errorMessage = new StringBuilder();

			if (IsAlphaBuild)
			{
				foreach (Type schemaType in Assembly.GetExecutingAssembly().GetTypes())
				{
					if (schemaType.IsSubclassOf(typeof(CargoWise.Schema.Schema)))
					{
						PropertyInfo allPropertyInfo = schemaType.GetProperty("All", BindingFlags.Static | BindingFlags.Public);
						SchemaColumnCollection columns = (SchemaColumnCollection)allPropertyInfo.GetValue(null, null);

						foreach (SchemaColumn column in columns)
						{
							if (column.Name.Contains("REMOVE_BEFORE"))
							{
								var expiryDateText = column.Name.Substring(column.Name.Length - 9, 9);

								if (!DateTime.TryParseExact(expiryDateText, "ddMMMyyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var expiryDate))
								{
									errorMessage
										.Append("Error in parsing column: ")
										.Append(column.Name)
										.Append(" in ")
										.Append(column.TableName);
								}
								else if (DateTime.Today > expiryDate)
								{
									errorMessage
										.Append("Table ")
										.Append(column.TableName)
										.Append(" has obsolete columns that need to be removed, as the expiration date (")
										.Append(expiryDateText)
										.Append(") for the task has now passed.");
								}

								break;
							}
						}
					}
				}
			}

			if (errorMessage.Length > 0)
			{
				Fail(errorMessage.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		bool IsAlphaBuild
		{
			get
			{
				XPathDocument document;
				Assembly assembly = Assembly.Load(Path.GetFileNameWithoutExtension(ExeFileNames.CargoWiseWindowsDesktopExe));
				using (Stream stream = assembly.GetManifestResourceStream("Enterprise.ReleaseInfo.xml"))
				{
					document = new XPathDocument(stream);
				}
				return string.Equals(document.CreateNavigator().SelectSingleNode("//ReleaseInfo/ReleaseRing").Value, "ALP", StringComparison.OrdinalIgnoreCase);
			}
		}
	}
}
