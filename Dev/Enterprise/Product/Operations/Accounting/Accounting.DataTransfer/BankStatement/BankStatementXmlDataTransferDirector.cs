using System;
using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.BankStatement
{
	public class BankStatementXmlDataTransferDirector : XmlDataTransferDirector
	{
		public BankStatementXmlDataTransferDirector(Business.Base.AccStatement.BankStatement bankStatement, BankStatementFormat.StatementFileFormats fileFormat, BankStatementDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
			this.BankStatement = bankStatement;
			this.FileFormat = fileFormat;
		}

		#region ImportCore

		protected override void ImportCore(string fileName, INotifications notify, ISourceInfo info)
		{
			try
			{
				string xmlFileName = "";
				try
				{
					xmlFileName = GetXmlFileName(fileName);
					if (File.Exists(xmlFileName))
					{
						using (FileStream xmlStream = File.OpenRead(xmlFileName))
						{
							var context = new ValueObjectImportContext(BankStatement.Factory, notify);
							ImportFromXml(xmlStream, context);
						}
					}
				}
				finally
				{
					if (xmlFileName != fileName && File.Exists(xmlFileName))
					{
						File.Delete(xmlFileName);
					}
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (IOException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			finally
			{
				int noOfStatements = ((BankStatementDataAdapter)Adapter).NoOfStatementsImportedSuccessfully;
				if (noOfStatements > 0)
				{
					int noOfTransactions = ((BankStatementDataAdapter)Adapter).NoOfDirectTransactionsCreated;
					Globals.Message.ShowInformation(Res.GetString("c0a60b27-44a6-4900-a42a-4d150829c44c", "{0} statement(s) imported successfully.\r\n{1} direct transaction(s) created.", noOfStatements.ToString(), noOfTransactions.ToString()));
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("12682939-c0be-408f-9ed6-8f6ef3f4eee9", "No statement imported."));
				}
			}
		}

		#endregion

		#region Implementations

		readonly Business.Base.AccStatement.BankStatement BankStatement;
		readonly BankStatementFormat.StatementFileFormats FileFormat;

		#region GetXmlFileName

		string GetXmlFileName(string fileName)
		{
			string result = "";

			try
			{
				switch (FileFormat)
				{
					case BankStatementFormat.StatementFileFormats.NativeXML:
						result = fileName;
						break;
					case BankStatementFormat.StatementFileFormats.NABAustralia:
						result = BankStatementXmlGenerator.GeneratXmlFromNab(fileName);
						break;
					case BankStatementFormat.StatementFileFormats.ANZNewZealand:
						result = BankStatementXmlGenerator.GeneratXmlFromANZ(fileName);
						break;
					case BankStatementFormat.StatementFileFormats.WestpacNewZealand:
						result = BankStatementXmlGenerator.GeneratXmlFromWestpac(fileName);
						break;
				}
			}
			catch (FormatException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}

			return result;
		}

		#endregion

		#region ImportFromXml

		void ImportFromXml(FileStream xmlStream, ValueObjectImportContext context)
		{
			XmlDocument xmlDoc = LoadXmlFileIntoXmlDocument(xmlStream);
			Xsd.BankStatement value = ExtractFromXmlDocument(xmlDoc);
			ImportFromValueObject(value, context);
		}

		#endregion

		#region LoadXmlFileIntoXmlDocument

		XmlDocument LoadXmlFileIntoXmlDocument(FileStream xmlStream)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(xmlStream);
			bool exceptionOccuredViaXmlManipulation = false;
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlTextReader);
			}
			catch (XmlException)
			{
				exceptionOccuredViaXmlManipulation = true;
				Globals.Message.ShowError(Res.GetString("ad2e79cc-fd95-48ba-9cf0-553ae7a0a2b6", "The XML format of the file you tried to import was invalid."), Res.GetString("4e9bb3e6-977e-43e0-940d-6a246842740f", "Invalid XML Document"));
			}

			return exceptionOccuredViaXmlManipulation ? null : xmlDoc;
		}

		#endregion

		#region ExtractFromXmlDocument

		Xsd.BankStatement ExtractFromXmlDocument(XmlDocument xmlDoc)
		{
			Xsd.BankStatement result = null;

			if (xmlDoc != null)
			{
				XmlNodeList nodeList = xmlDoc.GetElementsByTagName("BankStatements");

				if (nodeList.Count == 1)
				{
					XmlNode node = nodeList[0];
					if (node.ChildNodes.Count > 1)
					{
						Globals.Message.ShowError(BankStatementDataAdapter.MoreThanOneBankStatementErrorMsg);
					}
					else
					{
						XmlNode bankStatementNode = node.ChildNodes[0];
						XmlValueObjectSerializer xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.BankStatement));
						result = (Xsd.BankStatement)xmlValueObjectSerializer.Deserialize(new XmlNodeReader(bankStatementNode));
					}
				}
				else if (nodeList.Count == 0)
				{
					nodeList = xmlDoc.GetElementsByTagName("BankStatement");
					if (nodeList.Count == 1)
					{
						XmlValueObjectSerializer xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.BankStatement));
						result = (Xsd.BankStatement)xmlValueObjectSerializer.Deserialize(new XmlNodeReader(nodeList[0]));
					}
				}
			}
			return result;
		}

		#endregion

		#region ImportFromValueObject

		void ImportFromValueObject(Xsd.BankStatement value, ValueObjectImportContext context)
		{
			if (IsValidValueObject(value))
			{
				Adapter.ImportFromValueObject(BankStatement, value, context);
			}
		}

		#endregion

		#region IsValidValueObject

		bool IsValidValueObject(Xsd.BankStatement value)
		{
			bool result = true;

			if (value != null && FileFormat != BankStatementFormat.StatementFileFormats.ANZNewZealand &&
				FileFormat != BankStatementFormat.StatementFileFormats.WestpacNewZealand)
			{
				foreach (Xsd.BankStatementBankStatementLine line in value.BankStatementLines)
				{
					if (line.CurrencyCode != BankStatement.AB_RX_NKAccountCurrency)
					{
						Globals.Message.ShowError(Res.GetString("3cc0946b-f580-49d5-a3cf-a634df85f07b", "Statement currency cannot be different from the bank currency."));
						result = false;
						break;
					}
					else if (line.StatementDate > BankStatement.AB_LastStatementDate)
					{
						Globals.Message.ShowError(Res.GetString("74fdbd0b-fe76-42bb-8309-9f64a65718ea", "Statement date cannot be after the bank's statement date."));
						result = false;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
