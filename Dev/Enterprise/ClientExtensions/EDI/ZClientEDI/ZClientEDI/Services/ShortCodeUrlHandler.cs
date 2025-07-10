using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Core;
using Enterprise.ProcessManagement.Business;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.Services
{
	public struct TypeAndCol
	{
		public Type Type { get; set; }
		public CargoWise.Schema.SchemaStringColumn ColumnName { get; set; }
		public string ControllerName { get; set; }
	}

	[ImmutableObject(true)]
	public sealed class ShortCodeUrlHandler : ShowFormUrlHandler
	{
		[ThreadSafe]
		public static ImmutableDictionary<string, TypeAndCol> SupportedCodes =
			new Dictionary<string, TypeAndCol>
			{
				{ "WI", new TypeAndCol { Type = typeof(WorkItem), ColumnName = WorkItemSchema.WKI_WorkItemNumber, ControllerName = ControllerIDs.WorkItem.Name } },
				{ "WKI", new TypeAndCol { Type = typeof(WorkItem), ColumnName = WorkItemSchema.WKI_WorkItemNumber, ControllerName = ControllerIDs.WorkItem.Name } },
				{ "WORKITEM", new TypeAndCol { Type = typeof(WorkItem), ColumnName = WorkItemSchema.WKI_WorkItemNumber, ControllerName = ControllerIDs.WorkItem.Name } },
				{ "CS", new TypeAndCol { Type = typeof(SupportIncident), ColumnName = IncidentMainSchema.IM_IncidentNumber, ControllerName = Modules.ClientControllerRegistration.SupportIncident.Name } },
				{ "INC", new TypeAndCol { Type = typeof(SupportIncident), ColumnName = IncidentMainSchema.IM_IncidentNumber, ControllerName = Modules.ClientControllerRegistration.SupportIncident.Name } },
				{ "SUPPORTINCIDENT", new TypeAndCol { Type = typeof(SupportIncident), ColumnName = IncidentMainSchema.IM_IncidentNumber, ControllerName = Modules.ClientControllerRegistration.SupportIncident.Name } },
				{ "PRJ", new TypeAndCol { Type = typeof(Project), ColumnName = WorkProjectSchema.WKP_ProjectNumber, ControllerName = ControllerIDs.Project.Name } },
				{ "PROJECT", new TypeAndCol { Type = typeof(Project), ColumnName = WorkProjectSchema.WKP_ProjectNumber, ControllerName = ControllerIDs.Project.Name } }
			}
		.ToImmutableDictionary();

		public const string Command = "ShortCode";

		public static ShortCodeUrlHandler Instance
		{
			get { return instance; }
		}

		[ThreadSafe]
		static readonly ShortCodeUrlHandler instance = new ShortCodeUrlHandler();

		protected override string ExpectedCommandText
		{
			get
			{
				return Command;
			}
		}

		ShortCodeUrlHandler()
		{
		}

		protected override bool HandleCore(QueryString queryString)
		{
			var type = queryString.Get("Type")?.ToUpper();
			var id = queryString.Get("Id")?.ToUpper();

			string controller = null;
			Guid pk = Guid.Empty;
			if (type != null && id != null)
			{
				var factory = new BusinessObjectFactory();
				if (SupportedCodes.TryGetValue(type, out TypeAndCol dictValue))
				{
					controller = dictValue.ControllerName;
					var result = factory.LoadFromNaturalKey(dictValue.Type, dictValue.ColumnName, id);
					if (result != null)
					{
						pk = result.PK.ToGuid();
					}
				}
			}

			if (pk == Guid.Empty)
			{
				// We matched WI/CS but couldn't find a matching record.
				if (controller != null)
				{
					throw new EnterpriseUrlHandlerException($"{Constants.ProductName} was unable to find an Entity with the Id {id} for type {type}");
				}

				// we couldn't match anything
				throw new EnterpriseUrlHandlerException($"{Constants.ProductName} doesn't yet support Entities of type {type}");
			}

			var args = GetArgs(queryString);
			var isOpenFromWindowPersister = GetWindowPersisterStatus(queryString);
			var controllerID = ZControllerFactory.Instance.GetRegisteredIdentifierByName(controller);

			if (controllerID == null)
			{
				return false;
			}

			ShowForm(controllerID, pk, args, isOpenFromWindowPersister);
			return true;
		}

		protected override Form PerformFormAction(ControllerID controllerID, ZGuid pk, IEnumerable<string> args)
		{
			var result = ZControllerFactory.GetCorrectControllerAndBusinessObject(controllerID, pk, IsReportError);
			var controller = result.Controller;
			var bizO = result.BusinessObject;

			using (controller.SetArgsForNewForm(args))
			{
				return (Form)controller.ShowEditForm(bizO);
			}
		}
	}
}
