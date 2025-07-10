using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	static class ChildCommandsLoader
	{
		public static void LoadChildCommands(DocumentCommand command, PrintTaskDocumentPackLoader loader, ZGuid sourcePivotPK = default(ZGuid), string language = "")
		{
			if (command.ChildMenus.Count > 0)
			{
				var id = new DocumentPackId(command.PK);
				if (!loader.AddedCommands.Contains(id))
				{
					loader.AddedCommands.Add(id);
					command.ChildMenus.Sort(new SortInfo(StmMenuMenuPivotSchema.Constants.SF_Index, ListSortDirection.Ascending));
				}

				var dataProviders = GetDataProviderListByDocumentCommand(command);

				foreach (StmMenuMenuPivotBase childCommandPivot in command.ChildMenus)
				{
					if (MeetsMenuFilter(childCommandPivot, dataProviders))
					{
						var childCommand = command.Factory.Load<DocumentCommand>(childCommandPivot.SF_SU_Outward);
						var mostApplicablePivotPK = sourcePivotPK == default(ZGuid) ? childCommandPivot.PK : sourcePivotPK;
						LoadChildCommandsCore(command, childCommand, loader, mostApplicablePivotPK, language);

#if DEBUG
						loader.ChildCommandMetFilterCount++;
#endif
					}
				}
			}
		}

		internal static DataProviderList GetDataProviderListByDocumentCommand(DocumentCommand command)
		{
			var topLevelBusinessObject = command.Parent;
			var topLevelDataProvider = BODocDataProvider.Get((BusinessObject)topLevelBusinessObject);

			var boDocDataProviders = GetBODocDataProvider(command);
			DataProviderList dataProviders = null;
			if (boDocDataProviders != null && boDocDataProviders.Length > 0)
			{
				dataProviders = new DataProviderList(boDocDataProviders);
				if (!boDocDataProviders.Any(provider => provider is BODocDataProvider
					&& provider.ParentBusinessObject == topLevelDataProvider.ParentBusinessObject))
				{
					dataProviders.Add(topLevelDataProvider);
				}
			}
			else
			{
				dataProviders = new DataProviderList(topLevelDataProvider);
			}

			var menupivotDataProvider = BODocDataProvider.Get(command.ChildMenus[0]);
			dataProviders.Add(menupivotDataProvider);

			return dataProviders;
		}

		public static void LoadChildCommandsFromProviderPlaceholder(DocumentCommand command, PrintTaskDocumentPackLoader loader)
		{
			var docManagerSupport = command.Parent as IDocManagerSupport;
			if (docManagerSupport != null)
			{
				var providers = docManagerSupport.DocManagerInfo.GetEDocsProviders();
				foreach (var provider in providers)
				{
					var supporter = provider.GetEDocsProviderSupporter();
					var providerPlaceholder = supporter.GetProviderPlaceholder<DocumentCommand>(command);
					if (providerPlaceholder != null)
					{
						providerPlaceholder.Parent = provider;
						LoadChildCommands(providerPlaceholder, loader);
					}
				}
			}
		}

		static void LoadChildCommandsCore(DocumentCommand parentCommand, DocumentCommand childCommand, PrintTaskDocumentPackLoader loader, ZGuid sourcePivotPK, string language)
		{
			Argument.NotNull(childCommand, "childCommand");

			childCommand.CreditControlledDocumentDeliveryGUIManager = parentCommand.CreditControlledDocumentDeliveryGUIManager;
			var overridenBusinessContext = ZString.Empty;

			if (!sourcePivotPK.IsEmpty && sourcePivotPK.IsValid)
			{
				var pivot = parentCommand.Factory.Load<StmMenuMenuPivot>(sourcePivotPK);

				if (pivot != null)
				{
					overridenBusinessContext = pivot.SF_OverriddenBusinessContext;
				}
			}

			var context = !overridenBusinessContext.IsEmpty ? overridenBusinessContext : childCommand.SU_BusinessContext;
			var documentSupportables = GetChildDocumentSupportables(parentCommand, (BusinessContext)Enum.Parse(typeof(BusinessContext), context), childCommand);

			if (documentSupportables == null || (documentSupportables.Length > 0 && documentSupportables.All(documentSupportable => documentSupportable == null)))
			{
				loader.ReasonsForEmptyPacks.Add(Res.GetString("46B989C8-5806-43DA-90EA-100C3C979D30", "Cannot produce this Document because the required data is not present"));

				return;
			}

			foreach (var documentSupportable in documentSupportables.WhereNotNull())
			{
				var id = new DocumentPackId(childCommand.PK, documentSupportable.DocumentSupporter.PK);
				if (!loader.AddedCommands.Contains(id))
				{
					loader.AddedCommands.Add(id);

					var dataState = documentSupportable.DocumentSupporter.GetDataStateBeforeRun(childCommand);
					if ((dataState == null) || dataState.IsValid)
					{
						var businessObject = documentSupportable as IStmNoteParent;
						UserControlProviderList mergedList = null;
						if (businessObject != null)
						{
							var childNote = DocumentNote.LoadNote(businessObject);
							mergedList = new UserControlProviderList(childNote.GetSystemDefinedFieldList(), childNote.UserDefinedFieldList);
						}

						childCommand.Parent = documentSupportable;
						if (childCommand.IsApplicable || IsApplicableToDocBuilderInvoice(childCommand, documentSupportable))
						{
							loader.Load(childCommand, mergedList, parentCommand, sourcePivotPK, language);
						}
					}
					else if (!string.IsNullOrEmpty(dataState.ErrorMessage) && childCommand.CreditControlledDocumentDeliveryGUIManager != null && !childCommand.CreditControlledDocumentDeliveryGUIManager.IsAuthenticationDeclined)
					{
						Globals.Message.ShowError(dataState.ErrorMessage, Res.GetString("c525503e-04e4-4024-b98a-8ff0a8e722d0", "Unable To Run This Document"));
					}
				}
			}
		}

		static IBODocDataProvider[] GetBODocDataProvider(DocumentCommand command)
		{
			if (command != null && !command.SU_MenuDataContext.IsEmpty && command.Parent != null && command.Parent.DocumentSupporter != null)
			{
				return command.Parent.DocumentSupporter.GetBODocDataProviders(new DataContextValue(command.SU_MenuDataContext), command);
			}
			return null;
		}

		internal static bool MeetsMenuFilter(StmMenuMenuPivot childCommandPivot, DataProviderList dataProviderList)
		{
			var filter = childCommandPivot.SF_Filter;
			if (dataProviderList == null || filter.IsEmpty)
			{
				return true;
			}
			else
			{
				var translator = new TranslateMacroForDocumentFilter(dataProviderList);
				var filterWithMacrosReplaced = translator.ReplaceMacros(filter);
				try
				{
					return ExpressionEvaluator.Evaluate(filterWithMacrosReplaced, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
				}
				catch (ExpressionEvaluationException)
				{
					string errorInformation =
						Res.GetString("A78C096E-6339-4993-A495-DE3E1FC28847", @"Invalid filter expression:
Document name: {0}
Document path: {1}
Child document name: {2}
Child document filter: {3}", childCommandPivot.Inward.SU_MenuName, childCommandPivot.Inward.SU_MenuPath, childCommandPivot.Outward.SU_MenuName, filter);
					throw new InvalidMenuTemplateFilterException(errorInformation);
				}
			}
		}

		static IDocumentSupportable[] GetChildDocumentSupportables(DocumentCommand parentCommand, BusinessContext context, DocumentCommand childCommand)
		{
			var result = parentCommand.ParentDocumentSupporter.GetChildCollection(parentCommand, context, childCommand);
			if ((result == null) && (parentCommand.ParentDocumentSupporter.BusinessContext == context))
			{
				result = new[] { parentCommand.Parent };
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "harded coded document title.")]
		static bool IsApplicableToDocBuilderInvoice(DocumentCommand childCommand, IDocumentSupportable documentSupportable)
		{
			return (childCommand.SU_MenuName.EqualsIgnoringCase("DocBuilder Invoice") && documentSupportable.DocumentSupporter.SupportDocBuilderInvoiceAsChildCommand);
		}
	}
}
