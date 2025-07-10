using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine
{
	public class DocumentDeliveryManager : StmDocumentDeliveryManager
	{
		#region Process

		public override void ProcessDocumentDelivery(StmDocumentDelivery stmDocumentDelivery)
		{
			using (Env.SetTemporaryUserContext(stmDocumentDelivery.SDL_GS.ToGuid(), stmDocumentDelivery.SDL_GB.ToGuid(), stmDocumentDelivery.SDL_GE.ToGuid()))
			{
				var documentSupportable = GetDocumentSupportable(stmDocumentDelivery) ?? throw new DocumentEngineException($"Failed to load DocumentSupportable by using ControllerId or TableCode '{stmDocumentDelivery.SDL_ParentControllerIdOrTableCode}', and ParentID is '{stmDocumentDelivery.SDL_ParentId}'.");

				var documentEventsForMenu = new DocumentEventsForMenu(documentSupportable as BusinessObject);
				var documentNote = DocumentNote.LoadNote((IStmNoteParent)documentSupportable);
				var userDefinedFieldList = documentNote.UserDefinedFieldList;
				var systemDefinedFieldList = documentNote.GetSystemDefinedFieldList();

				var documentCommand = Factory.Load<DocumentCommand>(stmDocumentDelivery.SDL_SU);
				documentCommand.Parent = documentSupportable;
				using (DocumentRunner.BackgroundDelivery(stmDocumentDelivery))
				{
					var runner = new DocumentRunner(documentEventsForMenu: documentEventsForMenu, parentUserFieldList: userDefinedFieldList, parentSystemDefinedFieldList: systemDefinedFieldList);
					runner.Run(documentCommand);
				}
			}
		}

		protected IDocumentSupportable GetDocumentSupportable(StmDocumentDelivery stmDocumentDelivery)
		{
			BusinessObject businessObject;
			try
			{
				var controllerFactory = ObjectFactory.Get<IControllerFactory>();
				var controllerId = controllerFactory.GetRegisteredIdentifierByName(stmDocumentDelivery.SDL_ParentControllerIdOrTableCode);
				var controllerAndBiz = controllerFactory.GetCorrectControllerAndBusinessObject(controllerId, stmDocumentDelivery.SDL_ParentId, false);
				var controller = controllerAndBiz.Controller;
				businessObject = controller.GetFormBusinessObject(controllerAndBiz.BusinessObject);
			}
			catch
			{
				businessObject = Factory.Load(stmDocumentDelivery.SDL_ParentControllerIdOrTableCode, stmDocumentDelivery.SDL_ParentId);
			}

			return businessObject as IDocumentSupportable;
		}

		BusinessObjectFactory Factory
		{
			get
			{
				return factory ??= new BusinessObjectFactory
				{
					NameForDebugging = "Background Document Delivery Task Service (BDD)"
				};
			}
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
