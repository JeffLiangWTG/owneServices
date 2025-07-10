using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetBusinessObjectValue : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetBusinessObjectValue({businessobjecttype},{pk},{property})>",
				ResString.GetMultilingualString("6696270f-8ba8-4ef7-9444-800b82c7adf6",
				@"If you know the type of business object you want data from and the PK required to load it, you can access any property you like from the associated Document Wrapper even if you're writing a report. 
Available types: {0}, {1}, {2}, {3}. Generally this functionality is only used in Reports (not Documents) as Document Wrappers already give direct access to most related and child business objects.",
				"CusEntryHeader (DocBaseCusEntryHeader)", "JobComInvoiceLine (DocBaseJobComInvoiceLine)", "JobDeclaration (DocBaseJobDeclaration)", "ARInvoice (DocARInvoice)"),
				new List<(string example, object expectedResult)> { ("<GetBusinessObjectValue(JobComInvoiceLine, <EntryHeader.CH_PK>, LineNo)>", new ZShort(1)) });
		}

		protected internal override void PreSetupForGettingValue(string macro, Passes pass, Report report)
		{
			base.PreSetupForGettingValue(macro, pass, report);

			var matchedResult = GetMatchedResult(macro);
			var boType = matchedResult.BusinessObjectType;
			if (pass == Passes.FirstPass && report?.Style == Report.Styles.Report && report.Renderer.CurrentAreaToProcess is SectionBodyArea bodyArea && bodyArea.DBRowCount > 1 && bodyArea.DataRowSource is IDataSourceTable dataSource)
			{
				var table = dataSource.Table;
				var pkMacro = matchedResult.PK.Trim();

				if (RegexProvider.InnermostMacrosRegex.IsMatch(pkMacro))
				{
					var macroWithAngleBrackets = pkMacro.Substring(1, pkMacro.Length - 2);
					var split = macroWithAngleBrackets.Split(".".ToCharArray(), 2);
					var tableName = split[0];
					var columnName = split[1];

					if (table != null && !BOAndPKDic.ContainsKey((boType, table.TableName, columnName))
						&& tableName == table.TableName && table.Columns.Contains(columnName))
					{
						var pkList = table.AsEnumerable().Select(r => r[columnName]).OfType<Guid>().ToList();
						BOAndPKDic[(boType, tableName, columnName)] = new PKListWrapperForFetchHint { PKList = pkList };
					}
				}
			}
		}

		internal Dictionary<(string BusinessObjectType, string TableName, string ColumnName), PKListWrapperForFetchHint> BOAndPKDic { get; } = new Dictionary<(string, string, string), PKListWrapperForFetchHint>();

		(string BusinessObjectType, string PK, string Property) GetMatchedResult(string macro)
		{
			var groups = Regex.Match(macro).Groups;
			return (groups[1].Value, groups[2].Value, groups[3].Value);
		}

		protected internal override bool ShouldResetFactory => false;
		protected override void ResetCore()
		{
			base.ResetCore();
			BOAndPKDic.Clear();
			ForceReplaceFactory();
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var matchedResult = GetMatchedResult(macro);
			string businessObjectWrapperType = matchedResult.BusinessObjectType;
			string pkAsString = matchedResult.PK;
			string boPropertyName = matchedResult.Property;

			IBizAndWrapperType typeFactory = GetTypeFactory(businessObjectWrapperType);
			if (typeFactory == null)
			{
				ReportMacroError(report, Res.GetString("b9f04dca-8b6c-455b-9273-6c61ba0615f5", "{0} [{1}] not supported by this macro.", "BusinessObjectType", businessObjectWrapperType));
				return null;
			}

			ZGuid boPK;
			if (!ZGuid.TryParse(pkAsString, out boPK))
			{
				ReportMacroError(report, Res.GetString("91a6a2ae-b071-4ddc-ae66-d8cb50f11d81", "Second parameter [{0}] is not a valid PK.", pkAsString));
				return null;
			}

			if (report?.Style == Report.Styles.Report && report.Renderer.CurrentAreaToProcess is SectionBodyArea bodyArea && bodyArea.DataRowSource is IDataSourceTable dataSource)
			{
				AddFetchHint(dataSource, businessObjectWrapperType, boPK);
			}

			BusinessObject loadedBO = Factory.Load(typeFactory.BizObjType, boPK);
			if (loadedBO == null)
			{
				ReportMacroError(report, Res.GetString("56d6c8ee-0555-4193-878d-f4b26ea0a58c", "Could not load a [{0}] using PK [{1}].", businessObjectWrapperType, pkAsString));
				return null;
			}

			MethodInfo staticNewConstructorMethod = typeFactory.WrapperType.GetMethod("New",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new Type[] { loadedBO.GetType(), typeof(BusinessObjectFactory) },
				null);
			DocumentWrapper documentWrapper = (DocumentWrapper)staticNewConstructorMethod.Invoke(this, new Object[] { loadedBO, loadedBO.Factory });

			return TryParseWithReportOnFail(
				report: report,
				defaultValue: null,
				errorMessage: Res.GetString("23b9b267-67f8-41cf-8357-a00b9e369740", "Property {0} is not accessible or not defined in {1}", boPropertyName, businessObjectWrapperType),
				useExceptionMessage: false,
				parseFunc: () => documentWrapper[boPropertyName]);
		}

		void AddFetchHint(IDataSourceTable dataSource, string businessObjectWrapperType, ZGuid pk)
		{
			if (dataSource?.Table == null)
			{
				return;
			}

			var table = dataSource.Table;
			foreach (var item in BOAndPKDic)
			{
				var (businessObjectType, tableName, columnName) = item.Key;
				if (tableName == table.TableName && businessObjectType == businessObjectWrapperType && table.Columns.Contains(columnName))
				{
					var wrapperForFetchHint = item.Value;
					if (wrapperForFetchHint.FetchHintAdded)
					{
						continue;
					}
					if (wrapperForFetchHint.PKList.Any(g => g == pk))
					{
						var typeFactory = GetTypeFactory(businessObjectWrapperType);
						var pkSchema = BusinessObjectFactory.GetTableSchemaFromType(typeFactory.BizObjType)?.PK;
						var query = new ZQuery { AllowTableValuedParameters = true };
						query.AddToFilter(pkSchema, wrapperForFetchHint.PKList);
						Factory.AddFetchHint(typeFactory.BizObjType, query);
						wrapperForFetchHint.FetchHintAdded = true;
					}
				}
			}
		}

		IBizAndWrapperType GetTypeFactory(string macroTypeName)
		{
			switch (macroTypeName.ToUpperInvariant())
			{
				case "CUSENTRYHEADER":
					return new BizAndWrapperType<Enterprise.Integration.Customs.ICusEntryHeader, Enterprise.Integration.DocumentWrappers.IDocBaseCusEntryHeader>();

				case "JOBCOMINVOICELINE":
					return new BizAndWrapperType<Enterprise.Integration.Customs.IBaseJobComInvoiceLine, Enterprise.Integration.DocumentWrappers.IDocBaseJobComInvoiceLine>();

				case "JOBDECLARATION":
					return new BizAndWrapperType<Enterprise.Integration.Customs.IBaseJobDeclaration, Enterprise.Integration.DocumentWrappers.IDocBaseJobDeclaration>();

				case "ARINVOICE":
					return new BizAndWrapperType<Enterprise.Accounting.Integration.IARInvoice, Enterprise.Integration.DocumentWrappers.IDocARInvoice>();

				case "STMENTITYSCREENINGLOG":
					return new BizAndWrapperType<Enterprise.DeniedPartyScreening.Integration.IStmEntityScreeningLog, Enterprise.Integration.DocumentWrappers.IDocOrgPartyScreeningStatus>();
			}
			return null;
		}

		interface IBizAndWrapperType
		{
			Type BizObjType { get; }
			Type WrapperType { get; }
		}

		class BizAndWrapperType<TBusinessObject, TDocumentWrapper> : IBizAndWrapperType
		{
			public Type BizObjType
			{
				get { return ObjectFactory.GetType<TBusinessObject>(); }
			}

			public Type WrapperType
			{
				get { return ObjectFactory.GetType<TDocumentWrapper>(); }
			}
		}

		internal class PKListWrapperForFetchHint
		{
			internal List<Guid> PKList { get; set; }
			internal bool FetchHintAdded { get; set; }
		}

		#region Regex

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"
						^ # start of string
						< # left angle bracket
						\s*GetBusinessObjectValue\s* # duty (optional whitespace either side)
						\( # left parenthesis
						\s*(\S+)\s*, # optional whitespace, some non-whitespace characters, optional whitespace, and a comma
						\s*(\S+)\s*, # optional whitespace, some non-whitespace characters, optional whitespace, and a comma
						\s*(\S+)\s*  # optional whitespace, some non-whitespace characters, optional whitespace
						\) # right parenthesis
						\s* # optional whitespace
						> # right angle bracket
						$ # end of string
						",
						RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

		#endregion
	}
}
