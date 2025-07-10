using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers
{
	public sealed class DocActionManagerWrapper : DocumentWrapper
	{
		public static DocActionManagerWrapper New(OperationalActionManager manager)
		{
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}

			return new DocActionManagerWrapper(manager);
		}

		DocActionManagerWrapper(OperationalActionManager manager)
			: base(manager, null) { }

		public ZString ModuleName
		{
			get { return Manager.ModuleName; }
		}

		public DocActionMethodWrapperCollection Methods
		{
			get { return methods ?? (methods = DocActionMethodWrapperCollection.New(Manager.ActionSupporter)); }
		}

		public DocConstraintWrapperCollection Constraints
		{
			get { return constraints ?? (constraints = DocConstraintWrapperCollection.New()); }
		}

		DocActionMethodWrapperCollection methods;
		DocConstraintWrapperCollection constraints;

		#region Implementation

		OperationalActionManager Manager
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalActionManager)WrappedObject; }
		}

		#endregion
	}
}
