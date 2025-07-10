using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.IDEA
{
	[System.Runtime.Serialization.DataContract]
	public class IDEAPersistentData
	{
		IDEAPersistentData(AccComplianceReport report)
		{
			ComplianceReport = report;
		}

		[System.Runtime.Serialization.DataMember(Name = nameof(AccountBalancePerPeriod))]
		public Dictionary<(string accountNumber, int period), (decimal debit, decimal credit)> AccountBalancePerPeriod = new Dictionary<(string accountNumber, int period), (decimal debit, decimal credit)>();

		[System.Runtime.Serialization.DataMember(Name = nameof(AccountLastPostDate))]
		public Dictionary<string, ZDate> AccountLastPostDate = new Dictionary<string, ZDate>();

		[System.Runtime.Serialization.DataMember(Name = nameof(AllOrganisations))]
		public HashSet<string> AllOrganisations = new HashSet<string>();

		[System.Runtime.Serialization.DataMember(Name = nameof(AllCSVFiles))]
		public HashSet<string> AllCSVFiles = new HashSet<string>();

		[System.Runtime.Serialization.DataMember(Name = nameof(AllUnmappedAccountPKs))]
		public HashSet<ZGuid> AllUnmappedAccountPKs = new HashSet<ZGuid>();

		public HashSet<string> AllAccounts = new HashSet<string>();

		AccComplianceReport ComplianceReport;

		internal const string IncompleteIDEADataDescription = "IDEAIncompleteData";

		public static IDEAPersistentData Load(AccComplianceReport report)
		{
			IDEAPersistentData newInstance;

			var xmlString = LoadOrCreateIncompleteIDEADataNote(report).ST_NoteDataAsText;
			if (xmlString.IsEmpty)
			{
				newInstance = new IDEAPersistentData(report);
			}
			else
			{
				newInstance = Deserialize(xmlString);
				newInstance.ComplianceReport = report;
				newInstance.CreateAllAccountsFromDictionary();
			}

			return newInstance;
		}

		public void Store()
		{
			LoadOrCreateIncompleteIDEADataNote(ComplianceReport).ST_NoteDataAsText = Serialize();
		}

		public void Delete()
		{
			var note = LoadOrCreateIncompleteIDEADataNote(ComplianceReport);
			note.Delete();
		}

		public (int periodMinimum, int periodMaximum) CalculateMinMaxPeriod()
		{
			var minimum = 999999;
			var maximum = 0;
			foreach (var accountAndPeriod in AccountBalancePerPeriod.Keys)
			{
				minimum = Math.Min(minimum, accountAndPeriod.period);
				maximum = Math.Max(maximum, accountAndPeriod.period);
			}

			return (minimum, maximum);
		}

		void CreateAllAccountsFromDictionary()
		{
			AllAccounts = AccountLastPostDate.Keys.ToHashSet();
		}

		string Serialize()
		{
			var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(IDEAPersistentData));
			string xmlString;
			using (var sw = new StringWriter())
			{
				using (var writer = new XmlTextWriter(sw))
				{
					writer.Formatting = Formatting.Indented;
					serializer.WriteObject(writer, this);
					writer.Flush();
					xmlString = sw.ToString();
				}
			}

			return xmlString;
		}

		static IDEAPersistentData Deserialize(string xmlString)
		{
			IDEAPersistentData data;
			var deserializer = new System.Runtime.Serialization.DataContractSerializer(typeof(IDEAPersistentData));
			using (var sr = new StringReader(xmlString))
			{
				using (var xmlReader = new XmlTextReader(sr))
				{
					data = (IDEAPersistentData)deserializer.ReadObject(xmlReader, true);
				}
			}

			return data;
		}

		static HiddenStmNote LoadOrCreateIncompleteIDEADataNote(AccComplianceReport report)
		{
			var note = GetIDEANoteIfItExists(report);
			if (note == null)
			{
				note = report.Factory.New<HiddenStmNoteNotAutoLogged>();
				note.ST_Description = IncompleteIDEADataDescription;
				note.ST_Table = AccComplianceReportSchema.Constants.TableName;
				note.ST_ParentID = report.PK;
			}
			return note;
		}

		internal static HiddenStmNote GetIDEANoteIfItExists(AccComplianceReport report)
		{
			var filter = new ZQuery(StmNoteSchema.ST_Description, IncompleteIDEADataDescription);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, report.PK);
			return report.Factory.LoadTop1<HiddenStmNote>(filter);
		}
	}
}
