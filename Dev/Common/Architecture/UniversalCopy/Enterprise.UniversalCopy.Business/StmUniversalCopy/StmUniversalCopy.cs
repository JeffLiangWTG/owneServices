using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business
{
	public class StmUniversalCopy : AutoStmUniversalCopy
	{
		public StmUniversalCopy(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public sealed override StmModuleFilter CopyTemplate
		{
			get { return Template; }
		}

		public UniversalCopyTemplate Template
		{
			get
			{
				if (template?.PK != SUC_S9_CopyTemplate)
				{
					template = Factory.Load<UniversalCopyTemplate>(SUC_S9_CopyTemplate);
					if (template != null)
					{
						RemoveIgnoreElement?.Invoke(template);
					}
				}
				return template;
			}
		}

		UniversalCopyTemplate template;
		public Action<UniversalCopyTemplate> RemoveIgnoreElement;

		public StmUniversalCopyScheduleTask ScheduleTask
		{
			get
			{
				var query = new ZQuery(StmScheduleTaskSchema.S5_ParentID, PK);
				query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, TablePrefix);
				return Factory.LoadTop1<StmUniversalCopyScheduleTask>(query);
			}
		}

		public ZString ModuleDescription
		{
			get
			{
				if (ModuleIdentifier != null)
				{
					return ModuleIdentifier.Description;
				}
				else
				{
					return DataBoundResourceStrings.GetStringForTable(CopyObject.GetType());
				}
			}
		}

		public ZString GridContext
		{
			get
			{
				if (gridContext.IsEmpty && CopyTemplate != null)
				{
					gridContext = this.CopyTemplate.S9_ModuleID;
				}
				return gridContext;
			}
			set
			{
				if (!IsInDatabase)
				{
					gridContext = value;
				}
				else
				{
					throw new InvalidOperationException("Cannot set GridContext for a saved copy job");
				}
			}
		}
		ZString gridContext;

		public ModuleIdentifier ModuleIdentifier
		{
			get
			{
				if (moduleIdentifier == null)
				{
					moduleIdentifier = new ModuleList().GetRegisteredIdentifierByName(GridContext.Left(GridContext.Length - 3));
				}
				return moduleIdentifier;
			}
		}
		ModuleIdentifier moduleIdentifier;

		public ZString CopyObjectDescription
		{
			get
			{
				return (CopyObject?.HumanReadableName) ?? ZString.Empty;
			}
		}

		public string CopyObjectLink
		{
			get
			{
				string result = null;
				var currentModuleIdentifier = ModuleIdentifier;
				var currentCopyObject = CopyObject;
				if (currentModuleIdentifier != null && currentCopyObject != null)
				{
					var controllerID = ObjectFactory.Get<IControllerFactory>().GetRegisteredIdentifierByName(currentModuleIdentifier.Name);
					if (controllerID != null)
					{
						result = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, currentCopyObject.PK.ToGuid());
					}
				}
				return result;
			}
		}

		public BusinessObject CopyObject
		{
			get
			{
				if (copyObject == null)
				{
					copyObject = Factory.Load(SUC_CopyObjectTableCode, SUC_CopyObjectId);
				}

				return copyObject;
			}
			set
			{
				if (!IsInDatabase)
				{
					copyObject = value;
					SUC_CopyObjectId = copyObject.PK;
					SUC_CopyObjectTableCode = copyObject.TablePrefix;
				}
				else
				{
					throw new InvalidOperationException("Cannot set CopyObject for a saved copy job");
				}
			}
		}
		BusinessObject copyObject;

		protected override ZString HumanReadableNameCore => Res.GetString("3f7a66ae-d65a-4c8a-8586-3867f7dd1437", "Universal Copy: {0}", CopyObject?.HumanReadableName ?? SUC_CopyObjectTableCode);

		public override void Delete()
		{
			if (!inDelete)
			{
				inDelete = true;
				try
				{
					var scheduleTask = ScheduleTask;
					if (scheduleTask != null && !scheduleTask.IsDeleted && !scheduleTask.IsDeleting)
					{
						scheduleTask.Delete();
					}

					base.Delete();
				}
				finally
				{
					inDelete = false;
				}
			}
		}
		bool inDelete;

		internal (BusinessObject Copy, string Error) RunCopyAndGetCopyObjectInNewFactory()
		{
			var localFactory = new BusinessObjectFactory();
			try
			{
				var localSource = localFactory.ImportFromAnotherFactory(CopyObject);
				var stmUniversalCopyModuleService = new StmUniversalCopyModuleService { ScheduleTaskModuleName = ModuleDescription };
				localFactory.ServiceContainer.AddService(stmUniversalCopyModuleService, typeof(IScheduleTaskModuleService));
				var result = new BusinessObjectCopyManager().Copy(localSource, Template.CopyTemplateTree.CopyTemplateNode);
				return (Copy: result.Object as BusinessObject, Error: result.ErrorMessage);
			}
			finally
			{
				localFactory.ServiceContainer.RemoveService<IScheduleTaskModuleService>();
			}
		}
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			SUC_CopyObjectTableCode = TablePrefix;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
