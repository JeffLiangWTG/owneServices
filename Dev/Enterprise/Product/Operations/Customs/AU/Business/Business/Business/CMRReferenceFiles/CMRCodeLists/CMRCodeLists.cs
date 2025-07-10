using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CMRCodeLists.Schema.CI_Code), DescriptionProperty(CMRCodeLists.Schema.CI_Description)]
	public class CMRCodeLists : AutoCMRCodeLists
	{
		public static class CodeTypes
		{
			public const string ACSREGCD = "ACSREGCD";
			public const string ADVCETYP = "ADVCETYP";
			public const string ADVICESTAT = "ADVICESTAT";
			public const string BAOWNERTYP = "BAOWNERTYP";
			public const string CARGOCODE = "CARGOCODE";
			public const string CARGTYP = "CARGTYP";
			public const string CHNLOTRADE = "CHNLOTRADE";
			public const string CONFEXTP = "CONFEXTP";
			public const string COUNTRY = "COUNTRY";
			public const string CPRISKPTYP = "CPRISKPTYP";
			public const string CPRISKTYP = "CPRISKTYP";
			public const string CRGOIDTYPE = "CRGOIDTYPE";
			public const string CURCOUNTRY = "CURCOUNTRY";
			public const string CURRENCY = "CURRENCY";
			public const string DUMPEXETYP = "DUMPEXETYP";
			public const string ELMNTSYMB = "ELMNTSYMB";
			public const string EXDECCRGO = "EXDECCRGO";
			public const string EXDECEXCOD = "EXDECEXCOD";
			public const string EXGOODSTYP = "EXGOODSTYP";
			public const string GSTX = "GSTX";
			public const string HEADARSTYP = "HEADARSTYP";
			public const string HEADNATTYP = "HEADNATTYP";
			public const string IDLNECPTYP = "IDLNECPTYP";
			public const string INSTRMTTYP = "INSTRMTTYP";
			public const string INVTERMTYP = "INVTERMTYP";
			public const string LCTX = "LCTX";
			public const string LINEARSTYP = "LINEARSTYP";
			public const string LINENATTYP = "LINENATTYP";
			public const string LNACTIONCD = "LNACTIONCD";
			public const string LODQSTYP = "LODQSTYP";
			public const string ORIGINCDE = "ORIGINCDE";
			public const string PRFLINITAG = "PRFLINITAG";
			public const string QUANUNIT = "QUANUNIT";
			public const string RPTYTYPE = "RPTYTYPE";
			public const string TRANTYPE = "TRANTYPE";
			public const string TRNSPRTMDE = "TRNSPRTMDE";
			public const string WETX = "WETX";
		}

		public CMRCodeLists(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Loader : AutoCMRCodeLists.Loader
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CMRCodeLists);
			}

			public CMRCodeLists Load(string code, string type, ZDateTime effectiveDate)
			{
				ZQuery query = new ZQuery(CMRCodeListsSchema.CI_Code, code);
				query.AddToFilter(CMRCodeListsSchema.CI_CodeType, type);
				query.AddToFilter(CMRCodeListsSchema.CI_Startdate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);

				var endDateQuery = new ZQuery(CMRCodeListsSchema.CI_EndDate, DBNull.Value);
				endDateQuery.AddToFilter(JoinCondition.Or, CMRCodeListsSchema.CI_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);

				query.AddToFilter(endDateQuery);

				return Factory.LoadTop1<CMRCodeLists>(query);
			}
		}

		public static CMRCodeLists New(BusinessObjectFactory factory)
		{
			return factory.New<CMRCodeLists>();
		}

		public static CMRCodeLists Load(BusinessObjectFactory factory, ZQuery filter)
		{
			return factory.LoadTop1<CMRCodeLists>(filter);
		}
	}
}
