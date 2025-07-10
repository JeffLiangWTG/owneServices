using System;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("{ModuleName}")]
	public class OperationalActionContext
	{
		public OperationalActionContext(OperationalActionSupporter supporter, string moduleName, string workflowType = null)
		{
			if (supporter == null)
			{
				throw new ArgumentNullException(nameof(supporter));
			}

			if (moduleName == null)
			{
				throw new ArgumentNullException(nameof(moduleName));
			}

			this.supporter = supporter;
			this.moduleName = moduleName;
			this.workflowType = workflowType;
		}

		public string ModuleName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return moduleName; }
		}

		public string WorkflowType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return workflowType; }
		}

		public OperationalActionSupporter Supporter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return supporter; }
		}

		public OperationalActionFieldSupporterList FieldSupporters
		{
			get { return fieldSupporters ?? (fieldSupporters = new OperationalActionFieldSupporterList(Supporter.RootType, workflowType)); }
		}

		public bool SupportsDocuments
		{
			get { return typeof(IDocumentSupportable).IsAssignableFrom(Supporter.RootType); }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		OperationalActionFieldSupporterList fieldSupporters;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string moduleName;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string workflowType;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionSupporter supporter;
	}
}
