using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class Tools : CommandProvider
	{
		public Tools(IMacroEvaluationContext macroEvalContext)
		{
			this.macroEvalContext = macroEvalContext;
		}

		readonly IMacroEvaluationContext macroEvalContext;
		readonly Command showMacroEvaluator = new Command(CommandIds.ShowMacroEvaluator, Res.GetString("359ecf44-8c5b-415d-b12d-ed155de616bd", "Macro Evaluator"));
		readonly Command showDocumentData = new Command(CommandIds.ShowDocumentData, Res.GetString("99457a9b-65bb-4c05-849d-e9ef06cbc1c5", "Document Data"));
		readonly Command showMessagingData = new Command(CommandIds.ShowMessagingData, Res.GetString("82f50e9a-bec1-4925-a74d-2d9da3772e8f", "Messaging Data"));
		readonly Command showOverriddenData = new Command(CommandIds.ShowOverriddenData, Res.GetString("32d1d1bf-ea78-44f3-97ee-674d129d406e", "Overridden Data"));

		protected override IEnumerable<ICommand> CreateCommandsCore()
		{
			yield return showMacroEvaluator;
			yield return showDocumentData;
			yield return showMessagingData;
			yield return showOverriddenData;
		}

		protected override void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo)
		{
			showMacroEvaluator.Invoker = _ => ShowMacroEvaluator(documentInfo.Services, documentInfo.Document);
			showDocumentData.Invoker   = _ => ShowDocumentData(documentInfo.Services, documentInfo.Document);
			showMessagingData.Invoker  = _ => ShowMessagingData(documentInfo.Services, documentInfo.Document, documentInfo.Descriptor);
			showOverriddenData.Invoker = _ => ShowOverridenData(documentInfo.Services, documentInfo.Document);
		}

		void ShowMacroEvaluator(IServiceContainer services, IDocument document)
		{
			if (AllowToolsAccess(services))
			{
				services.Resolve<IDocumentToolsService>().ShowMacroEvaluator(macroEvalContext, document.Scope);
			}
			else
			{
				ShowCannotAccessMenuMessage(services);
			}
		}

		void ShowDocumentData(IServiceContainer services, IDocument document)
		{
			if (AllowToolsAccess(services))
			{
				services.Resolve<IDocumentToolsService>().ShowDocumentData(document);
			}
			else
			{
				ShowCannotAccessMenuMessage(services);
			}
		}

		void ShowMessagingData(IServiceContainer services, IDocument document, IDocumentDescriptor descriptor)
		{
			if (AllowToolsAccess(services))
			{
				services.Resolve<IDocumentToolsService>().ShowMessagingData(document, descriptor.MessageInstructions.XmlNamespace, descriptor.MessageInstructions.DataContext);
			}
			else
			{
				ShowCannotAccessMenuMessage(services);
			}
		}

		void ShowOverridenData(IServiceContainer services, IDocument document)
		{
			if (AllowToolsAccess(services))
			{
				services.Resolve<IDocumentToolsService>().ShowOverridenData(document);
			}
			else
			{
				ShowCannotAccessMenuMessage(services);
			}
		}

		bool AllowToolsAccess(IServiceContainer services)
		{
			return services.Resolve<IDocumentSecurityService>().AllowToolsAccess;
		}

		void ShowCannotAccessMenuMessage(IServiceContainer services)
		{
			services.Resolve<IDocumentSecurityService>().ShowAllowToolsAccessError();
		}
	}
}
