using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.MacroEvaluator
{
	public class MacroEvaluatorManager : EvaluatorManager
	{
		public MacroEvaluatorManager(BusinessObjectFactory factory, IDocumentSupportable parent, string macro = "")
			: base(factory, parent, macro)
		{ }

		public override ZString Evaluate()
		{
			try
			{
				if (SelectedDataContextEvaluator == null)
				{
					return Res.GetString("421ac9ba-a96d-43ce-bcc3-12f8d8319a01", "The {0} data context could not be created.", DataContextCode);
				}

				Output = new ZString(SelectedDataContextEvaluator(Macro)?.Replace("\n", System.Environment.NewLine)).Left(OutputInfo.MaxLength);
			}
			catch (DataContextIsInvalidException)
			{
				return Res.GetString("D0F435B6-0E3C-44EE-8550-E263E6289A84", "DataContext is invalid");
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				return ex.Message;
			}

			return ZString.Empty;
		}

		#region MenuItem

		public StmMenuItem MenuItem
		{
			get { return menuItem ?? (menuItem = Factory.New<ReadOnlyStmMenuItem>()); }
			set { menuItem = value; }
		}
		StmMenuItem menuItem;

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class ReadOnlyStmMenuItem : StmMenuItem
		{
			public ReadOnlyStmMenuItem(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool IsSavedByFactory
			{
				get { return false; }
			}
		}

		#endregion

		#region Data Context Evaluator

		Func<string, string> SelectedDataContextEvaluator
		{
			get
			{
				if (!dataContextEvaluators.ContainsKey(DataContextCode))
				{
					dataContextEvaluators.Add(DataContextCode, GetMacroEvaluator(SelectedDataContextValue));
				}

				return dataContextEvaluators[DataContextCode];
			}
		}

		readonly Dictionary<ZString, Func<string, string>> dataContextEvaluators = new Dictionary<ZString, Func<string, string>>();

		public DataContextValue SelectedDataContextValue => !DataContextCode.IsEmpty ? new DataContextValue(DataContextCode) : DataContextValue.None;

		Func<string, string> GetMacroEvaluator(DataContextValue dataContextValue)
		{
			return (input) =>
			{
				var dataProviders = Parent.DocumentSupporter.GetBODocDataProviders(dataContextValue, MenuItem) ?? throw new NotSupportedException(FormattableString.Invariant($"Document supporter did not create wrappers for '{dataContextValue}' data context"));

				var dataProviderList = new DataProviderList(dataProviders);
				var replacer = new MacroStringReplacer(dataProviderList);
				var macroResult = replacer.ReplaceMacros(input);
				return macroResult.UnEscapeAngleBrackets();
			};
		}

		#endregion

		#region Data Context

		[List("DataContext_List")]
		public ZString DataContextCode
		{
			get
			{
				if (dataContextCode.IsEmpty && DataContext_List.ContainsCode(nameof(DataContext.GenericFreightJob)))
				{
					dataContextCode = nameof(DataContext.GenericFreightJob);
				}

				return dataContextCode;
			}
			set
			{
				dataContextCode = value;
			}
		}
		ZString dataContextCode;

		public CodeDescriptionPairList DataContext_List
		{
			get
			{
				if (supportedDataContexts == null)
				{
					supportedDataContexts = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair contextPair in Parent.DocumentSupporter.ListOfSupportedDataContexts)
					{
						if (Enum.IsDefined(typeof(Core.Constants.DataContext), contextPair.Code))
						{
							supportedDataContexts.Add(contextPair);
						}
					}

					supportedDataContexts.Sort();
				}

				return supportedDataContexts;
			}
		}
		CodeDescriptionPairList supportedDataContexts;

		#endregion

		#region Property overrides

		public override bool HasDataContext
		{
			get { return true; }
		}

		public override ResourceStringData Name
		{
			get { return Res.GetData("9fe6c989-996f-44dc-b6f3-8289b5b53f44", "Macro Evaluator"); }
		}

		public override ResourceStringData MacroDescription
		{
			get { return Res.GetData("1d6987c3-ef2d-400d-bbb0-0312513eb37a", "Macro"); }
		}

		#endregion
	}
}
