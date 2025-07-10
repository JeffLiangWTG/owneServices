using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class BondActivityCodeXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		BondActivityCodeXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ActivityCodeList.Codes._1, nameof(Xsd.BondDetailActivityCode.Item1));
			yield return new Mapping(ActivityCodeList.Codes._1a, nameof(Xsd.BondDetailActivityCode.Item1a));
			yield return new Mapping(ActivityCodeList.Codes._1a1, nameof(Xsd.BondDetailActivityCode.Item1a1));
			yield return new Mapping(ActivityCodeList.Codes._2, nameof(Xsd.BondDetailActivityCode.Item2));
			yield return new Mapping(ActivityCodeList.Codes._3, nameof(Xsd.BondDetailActivityCode.Item3));
			yield return new Mapping(ActivityCodeList.Codes._3a, nameof(Xsd.BondDetailActivityCode.Item3a));
			yield return new Mapping(ActivityCodeList.Codes._3a3, nameof(Xsd.BondDetailActivityCode.Item3a3));
			yield return new Mapping(ActivityCodeList.Codes._4, nameof(Xsd.BondDetailActivityCode.Item4));
			yield return new Mapping(ActivityCodeList.Codes._5, nameof(Xsd.BondDetailActivityCode.Item5));
			yield return new Mapping(ActivityCodeList.Codes._6, nameof(Xsd.BondDetailActivityCode.Item6));
			yield return new Mapping(ActivityCodeList.Codes._7, nameof(Xsd.BondDetailActivityCode.Item7));
			yield return new Mapping(ActivityCodeList.Codes._8, nameof(Xsd.BondDetailActivityCode.Item8));
			yield return new Mapping(ActivityCodeList.Codes._9, nameof(Xsd.BondDetailActivityCode.Item9));
			yield return new Mapping(ActivityCodeList.Codes._10, nameof(Xsd.BondDetailActivityCode.Item10));
			yield return new Mapping(ActivityCodeList.Codes._11, nameof(Xsd.BondDetailActivityCode.Item11));
			yield return new Mapping(ActivityCodeList.Codes._12, nameof(Xsd.BondDetailActivityCode.Item12));
			yield return new Mapping(ActivityCodeList.Codes._13, nameof(Xsd.BondDetailActivityCode.Item13));
			yield return new Mapping(ActivityCodeList.Codes._14, nameof(Xsd.BondDetailActivityCode.Item14));
			yield return new Mapping(ActivityCodeList.Codes._15, nameof(Xsd.BondDetailActivityCode.Item15));
			yield return new Mapping(ActivityCodeList.Codes._16, nameof(Xsd.BondDetailActivityCode.Item16));
			yield return new Mapping(ActivityCodeList.Codes._17, nameof(Xsd.BondDetailActivityCode.Item17));
			yield return new Mapping(ActivityCodeList.Codes._18, nameof(Xsd.BondDetailActivityCode.Item18));
			yield return new Mapping(ActivityCodeList.Codes._19, nameof(Xsd.BondDetailActivityCode.Item19));
			yield return new Mapping(ActivityCodeList.Codes._20, nameof(Xsd.BondDetailActivityCode.Item20));
		}

		public static readonly BondActivityCodeXmlMappings Instance = new BondActivityCodeXmlMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML Mapping Name")]
		protected override string Name
		{
			get { return "Bond Activity Type"; }
		}

		public new Xsd.BondDetailActivityCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.BondDetailActivityCode.Item1, errorContext, notify);
		}
	}
}
