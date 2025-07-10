using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Data;
using CargoWise.FeatureControl;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public static class FeatureControlExtensions
	{
		public static string ToXmlString(this CargoWise.FeatureControl.FeatureControl featureControl)
		{
			using var stream = new MemoryStream();
			var xmlSerializer = new XmlSerializer(typeof(CargoWise.FeatureControl.FeatureControl));
			xmlSerializer.Serialize(stream, featureControl);
			return StreamConverter.StreamToString(stream);
		}

		public static byte[] Compress(this CargoWise.FeatureControl.FeatureControl featureControl)
		{
			using var ms = new MemoryStream();
			using (var zipStream = new GZipStream(ms, CompressionMode.Compress))
			{
				var xmlBytes = Encoding.UTF8.GetBytes(featureControl.ToXmlString());
				zipStream.Write(xmlBytes, 0, xmlBytes.Length);
			}
			return ms.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "uses complex SQL scripts that can't be accomplished by using Business Objects")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "uses complex SQL scripts that can't be accomplished by using Business Objects")]
		public static CargoWise.FeatureControl.FeatureControl LoadFromDatabase(ZDateTime clientRuleTimestamp, ZGuid clientDatabasePK)
		{
			var result = new CargoWise.FeatureControl.FeatureControl();

			using (var cmd = Db.Connection.Command("EXEC dbo.EdiLoadFeatureControlRule @ClientRuleTimestamp, @ClientDatabasePK;"))
			{
				cmd.AddParameter("@ClientRuleTimestamp", SqlDbType.DateTime, clientRuleTimestamp.IsValid && clientRuleTimestamp.IsValidSqlDateTime
																					? clientRuleTimestamp.ToDateTime() : SqlDateTime.MinValue.Value);
				cmd.AddParameter("@ClientDatabasePK", SqlDbType.UniqueIdentifier, clientDatabasePK.IsEmpty ? Guid.Empty : clientDatabasePK.ToGuid());

				using var adapter = cmd.NewDataAdapter();
				using var dataSet = new DataSet();
				adapter.Fill(dataSet);
				if (dataSet.Tables[0].Rows[0][0] is DateTime systemRuleTimestampUtc)
				{
					result.TimestampUtc = EnsureUtcKind(systemRuleTimestampUtc);
					if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
					{
						var ruleList = new List<CargoWise.FeatureControl.FeatureControlRule>();
						foreach (var row in dataSet.Tables[1].Rows.OfType<DataRow>())
						{
							var rule = new CargoWise.FeatureControl.FeatureControlRule();
							rule.FCM_FeatureControlCode = (string)row[FeatureControlHeaderSchema.Constants.FCM_FeatureControlCode];
							rule.FCR_RuleType = (FeatureControlRuleFCR_RuleType)Enum.Parse(typeof(FeatureControlRuleFCR_RuleType), (string)row[FeatureControlRuleSchema.Constants.FCR_RuleType]);
							rule.FCR_StartDateUtc = EnsureUtcKind((DateTime)row[FeatureControlRuleSchema.Constants.FCR_StartDateUtc]);
							if (row[FeatureControlRuleSchema.Constants.FCR_EndDateUtc] is DateTime endDate)
							{
								rule.FCR_EndDateUtc = EnsureUtcKind(endDate);
								rule.FCR_EndDateUtcSpecified = true;
							}
							else
							{
								rule.FCR_EndDateUtc = EnsureUtcKind(DateTime.MinValue);
							}
							rule.FCR_Parameters = (string)row[FeatureControlRuleSchema.Constants.FCR_Parameters];

							ruleList.Add(rule);
						}

						result.Rules = ruleList.ToArray();
					}
				}
			}

			return result;
		}

		static DateTime EnsureUtcKind(DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
	}
}
