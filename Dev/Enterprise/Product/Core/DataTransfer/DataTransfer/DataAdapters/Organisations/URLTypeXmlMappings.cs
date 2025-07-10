using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class URLTypeXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		URLTypeXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrgWebUrlList.Codes.MainWebsite, nameof(Xsd.OrgWebURLType.MAI));
			yield return new Mapping(OrgWebUrlList.Codes.CartageTracking, nameof(Xsd.OrgWebURLType.CRT));
		}

		public static readonly URLTypeXmlMappings Instance = new URLTypeXmlMappings();

		public Xsd.OrgWebURLType GetExternalCode(OrgWebURL url, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(url.PU_Type, Xsd.OrgWebURLType.MAI, errorContext, notifications);
		}

		protected override string GetExternalCodeCore(string enterpriseCode, string errorContext, INotifications notifications)
		{
			throw new NotSupportedException("Call the other overload");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Org Web URL Type"; }
		}
	}
}
