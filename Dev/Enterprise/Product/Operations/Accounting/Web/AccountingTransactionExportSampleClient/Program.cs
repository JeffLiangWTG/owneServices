using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using AccountingTransactionExportSampleClient.AccountingTransactionExportServiceReference;
using CargoWise.Common;

namespace AccountingTransactionExportSampleClient
{
	[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Sample console application")]
	class Program
	{
		static void Main(string[] args)
		{
			bool showHelp = false;

			try
			{
				if (args.Length > 3)
				{
					string filePath = string.Empty;
					if (args.Length > 4)
					{
						filePath = args[4];
					}

					if (args[0] == "both")
					{
						DateTime startTime = DateTime.Now;
						Console.Out.WriteLine(CreateBatchAndExportAccountingTransactions(args[1], args[2], args[3], filePath));
						DateTime endTime = DateTime.Now;
						Console.Out.WriteLine(string.Format("Duration={0}", endTime.Subtract(startTime).ToString()));
						Console.In.ReadLine();
					}
					else if (args[0] == "create")
					{
						DateTime startTime = DateTime.Now;
						Console.Out.WriteLine(CreateBatch(args[1], args[2], args[3], filePath));
						DateTime endTime = DateTime.Now;
						Console.Out.WriteLine(string.Format("Duration={0}", endTime.Subtract(startTime).ToString()));
						Console.In.ReadLine();
					}
					else if (args[0] == "export")
					{
						if (args.Length > 4)
						{
							if (args.Length > 5)
							{
								filePath = args[5];
							}

							int batchNumber = Convert.ToInt32(args[1]);

							DateTime startTime = DateTime.Now;
							Console.Out.WriteLine(ExportAccountingTransactions(args[2], args[3], args[4], batchNumber, filePath));
							DateTime endTime = DateTime.Now;
							Console.Out.WriteLine(string.Format("Duration={0}", endTime.Subtract(startTime).ToString()));
							Console.In.ReadLine();
						}
						else
						{
							showHelp = true;
						}
					}
					else
					{
						showHelp = true;
					}
				}
				else if (args.Length > 2)
				{
					if (args[0] == "load")
					{
						string xsdFile = args[1];
						string xmlFile = args[2];

						LoadXmlUsingXsd(xsdFile, xmlFile);
						Console.In.ReadLine();
					}
					else
					{
						showHelp = true;
					}
				}
				else
				{
					showHelp = true;
				}
			}
			catch (Exception ex) // CriticalExceptionIsHandled Reason = Top level handler with command line out
			{
				Console.Out.WriteLine(ex.Message);
				showHelp = true;
			}

			if (showHelp)
			{
				Console.Out.WriteLine("AccountingTransactionExportSampleClient.exe create|[export batchNumber]|both username password companyCode [filePath]");
				Console.Out.WriteLine("AccountingTransactionExportSampleClient.exe load xsdFile xmlFile");
			}
		}

		static string CreateBatch(string username, string password, string companyCode, string filePath)
		{
			Argument.NotNull(username, nameof(username));
			Argument.NotNull(password, nameof(password));

			string result = string.Empty;
			try
			{
				AccountingTransactionExportServiceSoapClient serviceClient = new AccountingTransactionExportServiceSoapClient("AccountingTransactionExportServiceSoap");
				SecuritySOAPHeader securityHeader = new SecuritySOAPHeader() { UserName = username, Password = password };
				AccountingTransactionCreateBatchRequest request = new AccountingTransactionCreateBatchRequest()
				{
					CompanyCode = companyCode
				};

				Console.Out.WriteLine("Calling CreateBatch...");
				AccountingTransactionExportResponse response = serviceClient.CreateBatch(securityHeader, request);

				if (response != null)
				{
					Console.Out.WriteLine("BatchNumber={0}, Succeeded={1}, ErrorMessage={2}", response.BatchNumber, response.Succeeded, response.ErrorMessage);
					Console.Out.WriteLine("PayLoad={0}", response.PayLoad);
				}
				else
				{
					Console.Out.WriteLine("response was null");
				}
			}
			catch (Exception ex) // CriticalExceptionIsHandled Reason = Sample for customer use.
			{
				result = ex.Message;
				Console.Out.WriteLine(ex.Message);
				Console.Out.WriteLine(ex.StackTrace);
				if (ex.InnerException != null)
				{
					Console.Out.WriteLine(ex.InnerException.Message);
					Console.Out.WriteLine(ex.InnerException.StackTrace);
				}
			}
			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Sample Client requires hard coded path")]
		static string ExportAccountingTransactions(string username, string password, string companyCode, int batchNumber, string filePath)
		{
			Argument.NotNull(username, nameof(username));
			Argument.NotNull(password, nameof(password));

			string result = string.Empty;
			try
			{
				AccountingTransactionExportServiceSoapClient serviceClient = new AccountingTransactionExportServiceSoapClient("AccountingTransactionExportServiceSoap");
				SecuritySOAPHeader securityHeader = new SecuritySOAPHeader() { UserName = username, Password = password };

				AccountingTransactionExportRequest request = new AccountingTransactionExportRequest()
				{
					CompanyCode = companyCode,
					BatchNumber = batchNumber
					//,Namespace = "http://www.cargowise.com/Schemas/Universal/2012/11"
				};

				Console.Out.WriteLine("Calling ExportBatch...");
				AccountingTransactionExportResponse response = serviceClient.ExportBatch(securityHeader, request);

				if (response != null)
				{
					Console.Out.WriteLine("BatchNumber={0}, Succeeded={1}, ErrorMessage={2}", response.BatchNumber, response.Succeeded, response.ErrorMessage);
					Console.Out.WriteLine(response.PayLoad);

					if (response.Succeeded)
					{
						if (String.IsNullOrEmpty(filePath))
						{
							filePath = @"c:\exportPayload.xml";
						}
						using (StreamWriter sw = File.CreateText(filePath))
						{
							sw.Write(response.PayLoad);
							sw.Flush();
							sw.Close();
						}
					}
				}
				else
				{
					Console.Out.WriteLine("response was null");
				}
			}
			catch (Exception ex)  // CriticalExceptionIsHandled Reason = Sample for customer use.
			{
				result = ex.Message;
				Console.Out.WriteLine(ex.Message);
				Console.Out.WriteLine(ex.StackTrace);
				if (ex.InnerException != null)
				{
					Console.Out.WriteLine(ex.InnerException.Message);
					Console.Out.WriteLine(ex.InnerException.StackTrace);
				}
			}
			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Sample Client requires hard coded path")]
		static string CreateBatchAndExportAccountingTransactions(string username, string password, string companyCode, string filePath)
		{
			Argument.NotNull(username, nameof(username));
			Argument.NotNull(password, nameof(password));

			string result = string.Empty;
			try
			{
				AccountingTransactionExportServiceSoapClient serviceClient = new AccountingTransactionExportServiceSoapClient("AccountingTransactionExportServiceSoap");
				SecuritySOAPHeader securityHeader = new SecuritySOAPHeader() { UserName = username, Password = password };
				AccountingTransactionCreateBatchRequest request = new AccountingTransactionCreateBatchRequest()
				{
					CompanyCode = companyCode
				};

				Console.Out.WriteLine("Calling CreateBatch...");
				AccountingTransactionExportResponse response = serviceClient.CreateBatch(securityHeader, request);

				if (response != null)
				{
					Console.Out.WriteLine("BatchNumber={0}, Succeeded={1}, ErrorMessage={2}", response.BatchNumber, response.Succeeded, response.ErrorMessage);
					Console.Out.WriteLine(response.PayLoad);

					AccountingTransactionExportRequest request2 = new AccountingTransactionExportRequest()
					{
						CompanyCode = companyCode,
						BatchNumber = response.BatchNumber
						//,Namespace = "http://www.cargowise.com/Schemas/Universal/2012/11"
					};

					Console.Out.WriteLine("Calling ExportBatch...");
					response = serviceClient.ExportBatch(securityHeader, request2);

					if (response != null)
					{
						Console.Out.WriteLine("BatchNumber={0}, Succeeded={1}, ErrorMessage={2}", response.BatchNumber, response.Succeeded, response.ErrorMessage);
						Console.Out.WriteLine(response.PayLoad);

						if (response.Succeeded)
						{
							if (String.IsNullOrEmpty(filePath))
							{
								filePath = @"c:\exportPayload.xml";
							}
							using (StreamWriter sw = File.CreateText(filePath))
							{
								sw.Write(response.PayLoad);
								sw.Flush();
								sw.Close();
							}
						}
					}
					else
					{
						Console.Out.WriteLine("response was null");
					}
				}
				else
				{
					Console.Out.WriteLine("response was null");
				}
			}
			catch (Exception ex) // CriticalExceptionIsHandled Reason = Sample for customer use.
			{
				result = ex.Message;
				Console.Out.WriteLine(ex.Message);
				Console.Out.WriteLine(ex.StackTrace);
				if (ex.InnerException != null)
				{
					Console.Out.WriteLine(ex.InnerException.Message);
					Console.Out.WriteLine(ex.InnerException.StackTrace);
				}
			}
			return result;
		}

		static void LoadXmlUsingXsd(string xsdFile, string xmlFile)
		{
			Argument.NotNull(xsdFile, nameof(xsdFile));
			Argument.NotNull(xmlFile, nameof(xmlFile));

			try
			{
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.CloseInput = true;
				settings.IgnoreWhitespace = false;
				settings.ValidationType = ValidationType.Schema;
				settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
				settings = LoadSettingsUsingXsd(settings, xsdFile);
				using (XmlReader reader = XmlReader.Create(xmlFile, settings))
				{
					Console.Out.WriteLine("Processing XML");
					while (reader.Read())
					{
					}
					reader.Close();
					Console.Out.WriteLine("XML is valid");
				}
			}
			catch (Exception ex) // CriticalExceptionIsHandled Reason = Sample for customer use.
			{
				Console.Out.WriteLine("Error: " + ex.Message);
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Requires-15-26", Justification = "Add() does accept null values for parameter targetNamespace")]
		static XmlReaderSettings LoadSettingsUsingXsd(XmlReaderSettings settings, string xsdFile)
		{
			Argument.NotNull(settings, nameof(settings));
			Argument.NotNull(xsdFile, nameof(xsdFile));

			settings.Schemas.Add(null, xsdFile);
			return settings;
		}
	}
}
