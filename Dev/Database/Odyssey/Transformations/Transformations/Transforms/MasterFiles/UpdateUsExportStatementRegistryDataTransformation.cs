using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	abstract class UpdateUsExportStatementRegistryDataTransformation : RegistryDataTransformation
	{
		public override string UserDescription => @"Updating the list of export statements for the United States according to the ACE Export Manifest Appendix 1: Reference & Exemption Codes (https://www.cbp.gov/sites/default/files/assets/documents/2021-Oct/Export%20Manifest%20Appendix%20I-Reference%20and%20exemption%20Codes_10122021_508c_0.pdf)";

		protected override void OfflinePostUpgradeTransform()
		{
			using (var dataTable = GetDataTable(ExportStatementRegistryItemName))
			{
				foreach (DataRow dataRow in dataTable.Rows)
				{
					var pk = dataRow.Field<Guid>(StmDataSchema.Constants.PK);
					var originalBinaryValue = dataRow.Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);

					if (originalBinaryValue == null)
					{
						continue;
					}

					var newBinaryValue = TransformBinaryValue(originalBinaryValue);
					var updateSqlQuery = $@"UPDATE {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}
SET {StmDataSchema.Constants.SD_BinaryValue} = @SD_BinaryValue
WHERE {StmDataSchema.Constants.PK} = @SD_PK;";

					using (var cmd = Db.Connection.Command(updateSqlQuery))
					{
						cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
						cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", newBinaryValue, StmDataSchema.SD_BinaryValue);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		byte[] TransformBinaryValue(byte[] originalValue)
		{
			var document = GetDocument(originalValue);
			TransformDocument(document);
			return GetBytes(document);
		}

		void TransformDocument(XNode rootNode)
		{
			RenameExportStatementSettingElements(rootNode);
			AddMissingExportStatementSettingElements(rootNode);
		}

		static void RenameExportStatementSettingElements(XNode rootNode)
		{
			var countryAgnosticExportStatementSettingElements = rootNode.XPathSelectElements(CountryAgnosticExportStatementSettingsExpression);
			foreach (var countryAgnosticExportStatementSettingElement in countryAgnosticExportStatementSettingElements)
			{
				var nameElement = countryAgnosticExportStatementSettingElement.Element("Name");
				if (nameElement != null)
				{
					nameElement.Name = XName.Get("Code");
				}
			}
		}

		void AddMissingExportStatementSettingElements(XNode rootNode)
		{
			var statements = rootNode.XPathSelectElement(UsStatementsExpression);

			if (statements == null)
			{
				return;
			}

			var exportStatementSettingsDictionary = statements.Elements("ExportStatementSetting")
				.ToDictionary(setting => ((string)setting.Element("Code"),
					(string)setting.Element("Statement"), (string)setting.Element("StatementDescription")));
			foreach (var usReferenceStatement in GetUsReferenceStatements())
			{
				if (exportStatementSettingsDictionary.TryGetValue(usReferenceStatement, out var settingElement))
				{
					settingElement.Remove();
				}
				else
				{
					settingElement = ConstructUsDefaultExportStatementSetting(usReferenceStatement.Code,
						usReferenceStatement.Statement, usReferenceStatement.Description);
				}

				statements.AddFirst(settingElement);
			}
		}

		static XElement ConstructUsDefaultExportStatementSetting(string code, string statement, string statementDescription)
		{
			return new XElement("ExportStatementSetting",
				new XElement("Code", code),
				new XElement("Statement", statement),
				new XElement("Field1"),
				new XElement("Field2"),
				new XElement("Visibility", "UDF"),
				new XElement("UseOnHawb", "Y"),
				new XElement("UseOnDirectIATAMawb", "Y"),
				new XElement("UseOnConsolidationMawb", "Y"),
				new XElement("UseOnHouseBillOfLading", "Y"),
				new XElement("UseOnDirectMasterBillOfLading", "Y"),
				new XElement("UseOnConsolidationMasterBillOfLading", "Y"),
				new XElement("StatementDescription", statementDescription));
		}

		protected virtual IEnumerable<(string Code, string Statement, string Description)> GetUsReferenceStatements()
		{
			yield return ("FME", "NOEEI §30.40(c)", "NOEEI §30.40(c) - Food, medicines, supplies for US gov");
			yield return ("HHG", "NOEEI §30.40(b)", "NOEEI §30.40(b) - Household goods for US gov employees");
			yield return ("OFE", "NOEEI §30.40(a)", "NOEEI §30.40(a) - Office equipment for US gov offices");
			yield return ("E12", "NOEEI §30.37(y)", "NOEEI §30.37(y) - 15 CFR 740 Groups E:1 and E:2 exceptions");
			yield return ("BAG", "NOEEI §30.37(x)", "NOEEI §30.37(x) - 15 CFR 740.14 Baggage");
			yield return ("APO", "NOEEI §30.37(w)", "NOEEI §30.37(w) - Shipments to Army Post Office, Diplomatic Post Office, Fleet Post Office");
			yield return ("VSL", "NOEEI §30.37(v)", "NOEEI §30.37(v) - Shipping containers");
			yield return ("DAT", "NOEEI §30.37(u)", "NOEEI §30.37(u) - 22 CFR 123.22(b)(3)(iii) Technical data, Defense services");
			yield return ("TDC", "NOEEI §30.37(t)", "NOEEI §30.37(t) - International transaction documents");
			yield return ("BNK", "NOEEI §30.37(s)", "NOEEI §30.37(s) - Issued bank notes, securities, coins");
			yield return ("BGG", "NOEEI §30.37(p)", "NOEEI §30.37(p) - Passenger, Crew Baggage");
			yield return ("DUN", "NOEEI §30.37(n)", "NOEEI §30.37(n) - Dunnage");
			yield return ("CAR", "NOEEI §30.37(m)", "NOEEI §30.37(m) - Carriers' stores");
			yield return ("PET", "NOEEI §30.37(l)", "NOEEI §30.37(l) - Pets as baggage");
			yield return ("BUS", "NOEEI §30.37(k)", "NOEEI §30.37(k) - Company records");
			yield return ("BKS", "NOEEI §30.37(g)", "NOEEI §30.37(g) - Literature to libraries, governments");
			yield return ("TSW", "NOEEI §30.37(f)", "NOEEI §30.37(f) - 15 CFR 772 Technology and software");
			yield return ("MCU", "NOEEI §30.37(d)", "NOEEI §30.37(d) - Shipments from MX to CA or CA to MX transiting through US");
			yield return ("UMC", "NOEEI §30.37(c)", "NOEEI §30.37(c) - Shipments from US to US transiting through MX or CA");
			yield return ("IWA", "NOEEI §30.2(d)(5)", "NOEEI §30.2(d)(5) - Ultimate Dest. US or Int'l Waters for US person");
			yield return ("GBN", "NOEEI §30.2(d)(4)", "NOEEI §30.2(d)(4) - Goods to Guantanamo Bay Naval Base");
			yield return ("TME", "NOEEI §30.37(q)", "NOEEI §30.37(q) - Temporary Exports (1 YR)");
			yield return ("AVS", "NOEEI §30.37(o)", "NOEEI §30.37(o) - 15 CFR 740.15(c) Parts for US airlines");
			yield return ("REM", "NOEEI §30.37(j)", "NOEEI §30.37(j) - Human remains");
			yield return ("DIP", "NOEEI §30.37(i)", "NOEEI §30.37(i) - Diplomatic pouches");
			yield return ("GFT", "NOEEI §30.37(h)", "NOEEI §30.37(h) - 15 CFR 740.12(a)&(b) Gifts and donations");
			yield return ("TER", "NOEEI §30.2(d)(2)", "NOEEI §30.2(d)(2) - Goods between US and US territories (not PR, VI)");
			yield return ("BND", "NOEEI §30.2(d)(1)", "NOEEI §30.2(d)(1) - Goods under CBP bond, not in consumption");
			yield return ("ARM", "NOEEI §30.39", "NOEEI §30.39 - Shipments to US armed services");
			yield return ("CAS", "NOEEI §30.36", "NOEEI §30.36");
			yield return ("TMP", "NOEEI §30.37(r)", "NOEEI §30.37(r) - Return of Temporary Import Bond");
			yield return ("TOT", "NOEEI §30.37(b)", "NOEEI §30.37(b) - Tools of trade");
			yield return ("LOW", "NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)");
			yield return ("DWN", "AESDOWN", "AES Downtime Citation");
			yield return ("PDA", "AESPOST", "Postdeparture Citation-Agent");
			yield return ("PDU", "AESPOST", "Postdeparture Citation-USPPI");
			yield return ("ASH", "AES", "AES Split Shipments");
			yield return ("PRF", "AES", "AES Proof of Filing Citation");
		}

		XDocument GetDocument(byte[] bytes)
		{
			using (var memoryStream = new MemoryStream(bytes))
			using (var streamReader = new StreamReader(memoryStream, Utf16Encoding))
			using (var xmlReader = XmlReader.Create(streamReader))
			{
				return XDocument.Load(xmlReader);
			}
		}

		byte[] GetBytes(XDocument document)
		{
			var xmlWriterSettings = new XmlWriterSettings
			{
				Encoding = Utf16Encoding,
				OmitXmlDeclaration = false,
				Indent = false
			};

			using (var memoryStream = new MemoryStream())
			using (var xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
			{
				document.Save(xmlWriter);
				xmlWriter.Flush();
				memoryStream.Seek(0, SeekOrigin.Begin);
				return memoryStream.ToArray();
			}
		}

		const string CountryAgnosticExportStatementSettingsExpression =
			"/ArrayOfCountryExportStatementSetting/CountryExportStatementSetting/Statements/ExportStatementSetting";

		const string UsStatementsExpression =
			"/ArrayOfCountryExportStatementSetting/CountryExportStatementSetting[CountryCode='US']/Statements";

		const string ExportStatementRegistryItemName = "ExportStatementSetting";

		readonly Encoding Utf16Encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
	}
}
