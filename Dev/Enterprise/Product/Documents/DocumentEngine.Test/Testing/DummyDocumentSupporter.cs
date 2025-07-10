using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyDocumentSupporter : DocumentSupporter
	{
		public DummyDocumentSupporter(DummyDocumentSupportable parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DummyDocumentSupportable parent;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Test; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (documentWrappers != null)
			{
				return documentWrappers;
			}

			var result = new List<DocumentWrapper>();

			switch (dataContext)
			{
				case DataContext.Dummy:
				case DataContext.UnitTest:

					result.Add(new DummyBusinessObjectWrapper(parent, Factory));
					break;

				case DataContext.DummyChildren:

					foreach (ChildDummyBusinessObject child in parent.Collection)
					{
						result.Add(new DummyChildBusinessObjectWrapper(child, Factory));
					}

					break;
			}

			return result.ToArray();
		}

		DocumentWrapper[] documentWrappers;
		internal void SetDocumentWrappers(DocumentWrapper[] documentWrappers)
		{
			this.documentWrappers = documentWrappers;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { DataContext.UnitTest, DataContext.Dummy, DataContext.DummyChildren };
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			if (ReturnInvalidDataStateForTest.Value)
			{
				return new DocumentSupporterDataState(false, "Error Test");
			}
			return base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		public static Overridable<bool> ReturnInvalidDataStateForTest { get; } = new Overridable<bool>(false);
	}
}
