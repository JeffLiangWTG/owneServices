using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks
{
	class AsycudaIncomingGMDMessageHelper
	{
		public static ZString GetBGMReferenceFromXML(string msgText)
		{
			var result = ZString.Empty;
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(msgText);
			switch (xmlDoc.DocumentElement.Name.ToUpper())
			{
				case "ASYCUDA":
					result = GetBGMReferenceFromXML(xmlDoc, "ASYCUDA/Declarant/Reference/Number");
					break;
				case "ESAD":
					result = GetBGMReferenceFromXML(xmlDoc, "ESAD/header/declarant/reference_number");
					break;
			}
			return result;
		}

		static ZString GetBGMReferenceFromXML(XmlDocument xmlDoc, string pathToReference)
		{
			var xPath = ConvertToXPath(pathToReference);
			var node = xmlDoc.SelectSingleNode(xPath);
			return node?.InnerText ?? ZString.Empty;
		}

		public static CusEntryHeader FindCusEntryHeaderByBGMReference(ZString bgmReference, GlbBranch branch, BusinessObjectFactory factory)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, branch.Company.PK);

			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			jobDeclarationQuery.AddSubQuery(branchQuery, JoinCondition.And);

			var entryQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, bgmReference);
			entryQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);

			return factory.LoadTop1<CusEntryHeader>(entryQuery);
		}

		public static IGlbStaff GetUserToReceiveAsycudaMessageViaEmail(CusEntryHeader entry)
		{
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
			var user = GetUserWithEmail(outgoingMessage?.UserWhoQueuedThisRecord);
			if (user == null && entry.Declaration is BaseJobDeclaration declaration)
			{
				user = GetUserWithEmail(declaration.CusAgent) ?? declaration.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, declaration.JE_SystemCreateUser);
			}
			return user;
		}

		static GlbStaff GetUserWithEmail(GlbStaff user)
		{
			return user != null && !user.GS_EmailAddress.IsEmpty ? user : null;
		}

		public static string ConvertToXPath(string xmlPath)
		{
			const StringSplitOptions options = new StringSplitOptions();
			var delimiters = new[] { "/" };
			var arrS = xmlPath.Split(delimiters, options);
			var sb = new StringBuilder("/");

			foreach (var str in arrS)
			{
				sb.Append($"/*[local-name()='{str}']"); // No need to call Res.GetString() for XPath expressions.
			}

			return sb.ToString();
		}

		public static ZString FormatXMLDocument(string xml)
		{
			ZString result = xml;
			using (var memStream = new MemoryStream())
			using (var writer = new XmlTextWriter(memStream, Encoding.Unicode))
			{
				var document = new XmlDocument();
				try
				{
					document.LoadXml(xml);
					writer.Formatting = Formatting.Indented;
					document.WriteContentTo(writer);
					writer.Flush();
					memStream.Flush();
					memStream.Position = 0;
					StreamReader reader = new StreamReader(memStream);
					result = reader.ReadToEnd();
				}
				catch (XmlException)
				{
				}
			}
			return result;
		}
	}
}
