using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Data;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eHub;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestSerializer
	{
		readonly XNamespace ns = ReferenceDataXMLForSerialize.Instance.NameSpace;

		/// <summary>
		/// Note:
		/// RequestSerializer does not close outputStream
		/// Caller for this method have the responsibility to close it
		/// </summary>
		/// <param name="outputStream"></param>
		/// <param name="request"></param>
		/// <param name="orgHeaderPK"></param>
		public void Serialize(Stream outputStream, Request request, Guid orgHeaderPK = new Guid())
		{
			var requestXml = Serialize(request, orgHeaderPK);
			using (var xmlWriter = NativeXmlWriter.Create(outputStream))
			using (var cleanXmlWriter = new CleanXmlWriter(xmlWriter))
			{
				requestXml.WriteTo(cleanXmlWriter);
			}
		}

		public XElement Serialize(Request request, Guid orgHeaderPK = new Guid())
		{
			var headerXml = SerializeHeader(request.Settings);
			var bodyXml = new XElement(ns + ReferenceDataXMLForSerialize.Instance.BodyElementName, request.EntitySets);

			if (orgHeaderPK != Guid.Empty)
			{
				AddConsolsInfo(bodyXml, orgHeaderPK);
			}

			var requestXml = new XElement(ns + ReferenceDataXMLForSerialize.Instance.RootElementName, headerXml, bodyXml);
			requestXml.Add(new XAttribute(NativeXsd.Tag.Xmlns, ns.NamespaceName));
			requestXml.Add(new XAttribute(NativeXsd.Tag.Version, NativeXmlInfo.Version_2011_11));
			return requestXml;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "xml special string")]
		void AddConsolsInfo(XElement bodyXml, Guid orgHeaderPK)
		{
			string orgConsolXmlAsString = RetrieveConsolXmlAsString(orgHeaderPK);

			if (!string.IsNullOrEmpty(orgConsolXmlAsString))
			{
				var orgConsolXml = XElement.Parse(orgConsolXmlAsString);
				SetupConsolXml(orgConsolXml);

				bodyXml.Element(ns + "Organization").Element(ns + "OrgHeader").Add(orgConsolXml);
			}
		}

		XElement SerializeHeader(HeaderData headerSettings)
		{
			var typeToSerialize = headerSettings != null
				? headerSettings.GetType()
				: GetDefaultUpdateSettingsType(ReferenceDataXMLForSerialize.Instance.NameSpace);
			var serializer = new ObjectXmlSerializer(typeToSerialize);
			var headerElement = serializer.Serialize(headerSettings);
			headerElement.RemoveAttributes(); // Clears out unnecessary namespace redefinitions added by ObjectXmlSerializer.
			headerElement.Elements(ns + "DataContext").ToList().ForEach(AddUniversalNamespaceAndVersion);
			headerElement.Elements(ns + "MessageNumber").ToList().ForEach(AddUniversalNamespaceAndVersion);
			return headerElement;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "special element string")]
		void AddUniversalNamespaceAndVersion(XElement dataContextElement)
		{
			XNamespace universalNamespace = UniversalXmlInfo.Namespace_2011_11;
			XNamespace nativeNamespace = ns;

			foreach (var childElement in dataContextElement.DescendantsAndSelf())
			{
				childElement.Name = universalNamespace + childElement.Name.LocalName;
			}

			dataContextElement.Name = nativeNamespace + dataContextElement.Name.LocalName;

			dataContextElement.Add(new XAttribute(NativeXsd.Tag.Xmlns, universalNamespace), new XAttribute(XNamespace.Xmlns + "nv", nativeNamespace));
		}

		Type GetDefaultUpdateSettingsType(string nameSpace)
		{
			switch (nameSpace)
			{
				case NativeXmlInfo.Namespace_2011_11:
					return typeof(HeaderData_Versioned_Native);
				case ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native:
					return typeof(HeaderData_Unversioned_Native);
				default:
					return typeof(HeaderData_Universal);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		internal
#endif
 string RetrieveConsolXmlAsString(Guid orgHeaderPK)
		{
			ScavengingSetting setting = GetOrgScavengingTaskSettings();
			if (setting == null || !setting.PeriodStart.IsValidSmallDateTime || !setting.PeriodEnd.IsValidSmallDateTime)
			{
				throw new Exception("OrgCollection in scavenging setting is invalid.");
			}

			string sqlString = @"
IF NOT EXISTS (SELECT 1 FROM dbo.GlbBranch JOIN dbo.GlbCompany ON GB_OH_OrgProxy = @orgHeaderPK OR GC_OH_OrgProxy = @orgHeaderPK)
SELECT 'Sending' AS AgentType,
		JK_TransportMode AS TransMode, JK_RL_NKLoadPort AS LoadPort, JK_RL_NKDischargePort AS DischargePort,
		CONVERT(DECIMAL(38, 3),(SUM(w.Value))) AS TotalWeight,
		CONVERT(DECIMAL(38, 3),(SUM(v.Value))) AS TotalVolume,
		COUNT(JS_PK) AS ShipCount,
		CONVERT(varchar(24), JK_SystemCreateTimeUtc, 121) AS CreateUTC,
		CONVERT(varchar(24), GETUTCDATE(), 121) AS ExportUTC
FROM dbo.JobConsol
		LEFT JOIN dbo.OrgAddress s ON JK_OA_SendingForwarderAddress = s.OA_PK
		LEFT JOIN dbo.JobConShipLink ON JK_PK = JN_JK
		LEFT JOIN dbo.JobShipment sh
			CROSS APPLY dbo.ConvertWeight(sh.JS_ActualWeight, sh.JS_UnitOfWeight, 'KG') w
			CROSS APPLY dbo.ConvertVolume(sh.JS_ActualVolume, sh.JS_UnitOfVolume, 'M3') v
		ON JN_JS = JS_PK
WHERE (s.OA_OH = @orgHeaderPK)
		AND JK_SystemCreateTimeUtc >= @periodStart
		AND JK_SystemCreateTimeUtc < @periodEnd
GROUP BY JK_PK, JK_TransportMode, JK_RL_NKLoadPort, JK_RL_NKDischargePort, JK_SystemCreateTimeUtc
union all
SELECT 'Receiving' AS AgentType,
		JK_TransportMode AS TransMode, JK_RL_NKLoadPort AS LoadPort, JK_RL_NKDischargePort AS DischargePort,
		CONVERT(DECIMAL(38, 3),(SUM(w.Value))) AS TotalWeight,
		CONVERT(DECIMAL(38, 3),(SUM(v.Value))) AS TotalVolume,
		COUNT(JS_PK) AS ShipCount,
		CONVERT(varchar(24), JK_SystemCreateTimeUtc, 121) AS CreateUTC,
		CONVERT(varchar(24), GETUTCDATE(), 121) AS ExportUTC
FROM dbo.JobConsol
		LEFT JOIN dbo.OrgAddress r ON JK_OA_ReceivingForwarderAddress = r.OA_PK
		LEFT JOIN dbo.JobConShipLink ON JK_PK = JN_JK
		LEFT JOIN dbo.JobShipment sh
			CROSS APPLY dbo.ConvertWeight(sh.JS_ActualWeight, sh.JS_UnitOfWeight, 'KG') w
			CROSS APPLY dbo.ConvertVolume(sh.JS_ActualVolume, sh.JS_UnitOfVolume, 'M3') v
		ON JN_JS = JS_PK
WHERE (r.OA_OH = @orgHeaderPK)
		AND JK_SystemCreateTimeUtc >= @periodStart
		AND JK_SystemCreateTimeUtc < @periodEnd
GROUP BY JK_PK, JK_TransportMode, JK_RL_NKLoadPort, JK_RL_NKDischargePort, JK_SystemCreateTimeUtc
FOR XML PATH('ClientOrgConsol'), ROOT ('ClientOrgConsolCollection')";

			var result = new StringBuilder();
			using (var command = Db.Connection.Command(sqlString))
			{
				command.AddParameter("@orgHeaderPK", System.Data.SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.AddParameter("@periodStart", System.Data.SqlDbType.SmallDateTime, setting.PeriodStart.ToDateTime());
				command.AddParameter("@periodEnd", System.Data.SqlDbType.SmallDateTime, setting.PeriodEnd.ToDateTime());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Append(reader[0]);
					}
				}
			}
			return result.ToString();
		}

		ScavengingSetting GetOrgScavengingTaskSettings()
		{
			return eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Get("OrgCollection");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "special element string")]
#if DEBUG
		internal
#endif
 void SetupConsolXml(XElement orgConsolXml)
		{
			orgConsolXml.SetDefaultNameSpace(ns);
			foreach (var consolElement in orgConsolXml.Elements(ns + "ClientOrgConsol"))
			{
				consolElement.SetAttributeValue("Action", "INSERT");
			}
		}
	}
}
