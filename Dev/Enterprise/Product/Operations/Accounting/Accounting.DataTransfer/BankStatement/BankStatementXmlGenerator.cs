using System;
using System.IO;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;

namespace Enterprise.Accounting.DataTransfer.BankStatement
{
	public static class BankStatementXmlGenerator
	{
		#region GeneratXmlFromNab

		public static string GeneratXmlFromNab(string nabFileName)
		{
			string xmlFileName = Env.GetTempFileName(Env.TempPath, "xml"); // Hard-coded file extension
			var fieldTags = GetFieldTags();

			using (StreamReader reader = new StreamReader(nabFileName))
			{
				using (StreamWriter writer = new StreamWriter(xmlFileName))
				{
					writer.WriteLine("<BankStatement>");
					writer.WriteLine("  <BankStatementLines>");

					CsvFlatFileFormat format = new CsvFlatFileFormat(false);
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						NabRecord record = new NabRecord(line, format);
						if (record.Type != NabRecord.NabTypes.ClosingBalance)
						{
							writer.WriteLine("    <BankStatementLine>");
							writer.WriteLine("      <" + fieldTags[0] + ">" + record.Type + "</" + fieldTags[0] + ">");
							writer.WriteLine("      <" + fieldTags[1] + ">" + record.Reference + "</" + fieldTags[1] + ">");
							writer.WriteLine("      <" + fieldTags[2] + ">" + record.Description + "</" + fieldTags[2] + ">");
							writer.WriteLine("      <" + fieldTags[3] + ">" + "" + "</" + fieldTags[3] + ">");
							writer.WriteLine("      <" + fieldTags[4] + ">" + record.Date.ToString("yyyy-MM-dd") + "</" + fieldTags[4] + ">");
							writer.WriteLine("      <" + fieldTags[5] + ">" + record.Date.ToString("yyyy-MM-dd") + "</" + fieldTags[5] + ">");
							writer.WriteLine("      <" + fieldTags[6] + ">" + record.Amount.ToString(".00") + "</" + fieldTags[6] + ">");
							writer.WriteLine("      <" + fieldTags[7] + ">" + record.Currency + "</" + fieldTags[7] + ">");
							writer.WriteLine("    </BankStatementLine>"); // Hard-coded constant
						}
					}

					writer.WriteLine("  </BankStatementLines>"); // Hard-coded constant
					writer.WriteLine("</BankStatement>"); // Hard-coded constant
				}
			}

			return xmlFileName;
		}

		#endregion

		#region GeneratXmlFromANZ

		public static string GeneratXmlFromANZ(string aNZFileName)
		{
			string xmlFileName = Env.GetTempFileName(Env.TempPath, "xml"); // Hard-coded constant
			var fieldTags = GetFieldTags();

			using (StreamReader reader = new StreamReader(aNZFileName))
			{
				using (StreamWriter writer = new StreamWriter(xmlFileName))
				{
					writer.WriteLine("<BankStatement>");
					writer.WriteLine("  <BankStatementLines>");

					CsvFlatFileFormat format = new CsvFlatFileFormat(true);
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						ANZRecord record = new ANZRecord(line, format);
						if (!record.IsNotSupported)
						{
							writer.WriteLine("    <BankStatementLine>");
							writer.WriteLine("      <" + fieldTags[0] + ">" + record.Type + "</" + fieldTags[0] + ">");
							writer.WriteLine("      <" + fieldTags[1] + ">" + record.Reference + "</" + fieldTags[1] + ">");
							writer.WriteLine("      <" + fieldTags[6] + ">" + record.Amount.ToString(".00") + "</" + fieldTags[6] + ">");
							writer.WriteLine("    </BankStatementLine>"); // Hard-coded constant
						}
					}

					writer.WriteLine("  </BankStatementLines>"); // Hard-coded constant
					writer.WriteLine("</BankStatement>"); // Hard-coded constant
				}
			}

			return xmlFileName;
		}

		#endregion

		#region GeneratXmlFromWestpac

		public static string GeneratXmlFromWestpac(string westpacFileName)
		{
			string xmlFileName = Env.GetTempFileName(Env.TempPath, "xml"); // Hard-coded constant
			var fieldTags = GetFieldTags();
			try
			{
				using (StreamReader reader = new StreamReader(westpacFileName))
				{
					using (StreamWriter writer = new StreamWriter(xmlFileName))
					{
						writer.WriteLine("<BankStatement>");
						writer.WriteLine("  <BankStatementLines>");

						CsvFlatFileFormat format = new CsvFlatFileFormat(true);
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							WestpacRecord record = new WestpacRecord(line, format);
							if (!record.IsNotSupported)
							{
								writer.WriteLine("    <BankStatementLine>");
								writer.WriteLine("      <" + fieldTags[0] + ">" + record.Type + "</" + fieldTags[0] + ">");
								writer.WriteLine("      <" + fieldTags[1] + ">" + record.Reference + "</" + fieldTags[1] + ">");
								writer.WriteLine("      <" + fieldTags[6] + ">" + record.Amount.ToString(".00") + "</" + fieldTags[6] + ">");
								writer.WriteLine("    </BankStatementLine>"); // Hard-coded constant
							}
						}

						writer.WriteLine("  </BankStatementLines>"); // Hard-coded constant
						writer.WriteLine("</BankStatement>"); // Hard-coded constant
					}
				}
			}
			catch (FormatException)
			{
				if (File.Exists(xmlFileName))
				{
					try
					{
						File.Delete(xmlFileName);
					}
					// swallow exceptions on cleanup
					catch (IOException) { }
					catch (UnauthorizedAccessException) { }
				}

				throw;
			}

			return xmlFileName;
		}

		#endregion

		#region Implementation

		static string[] GetFieldTags()
		{
			return new string[] {
				"TransactionType",
				"ChequeOrReferenceNumber",
				"TransactionDescription",
				"PayeeName",
				"TransactionDate",
				"StatementDate",
				"TransactionAmount",
				"CurrencyCode" };
		}

		#endregion
	}
}
