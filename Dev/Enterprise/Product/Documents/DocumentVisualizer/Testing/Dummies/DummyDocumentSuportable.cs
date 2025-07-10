using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyDocumentSupportable : DummyBusinessObject, IDocumentSupportable
	{
		public DummyDocumentSupportable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new DummySupporter(this));
		DocumentSupporter documentSupporter;

		string IDocumentSupportable.TableName
		{
			get { return TableName; }
		}

		#region DummySupporter

		sealed class DummySupporter : DocumentSupporter
		{
			public DummySupporter(BusinessObject bizO)
				: base(bizO)
			{
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return null; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return System.Array.Empty<DocumentWrapper>();
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				return System.Array.Empty<Constants.DataContext>();
			}
		}

		#endregion
	}
}
