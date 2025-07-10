using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	[CodeProperty(DummyBizoSchema.Constants.Z0_Description)]
	public class DummyBusinessObjectWithDocumentSupport : DummyEnterpriseBusinessObject, IDocumentSupportable
	{
		public const BusinessContext BusinessContext = (BusinessContext)(-1);
		public DummyBusinessObjectWithDocumentSupport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public virtual DummyBusinessObjectWithDocumentSupport Other
		{
			get
			{
				return null;
			}
		}

		[ActionFieldFollow(true)]
		public new virtual DummyChildBusinessObjectCollection Collection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.Collection;
			}
		}

		[ActionFieldFollow(true)]
		public new virtual DummyChildBusinessObjectCollection FilteredCollection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.FilteredCollection;
			}
		}

		[ActionFieldFollow(true)]
		public virtual DummyBusinessObjectWithDocumentSupportCollection AnotherFilteredCollection
		{
			get
			{
				if (anotherFilteredCollection == null)
				{
					anotherFilteredCollection = new DummyBusinessObjectWithDocumentSupportCollection(Factory);
					anotherFilteredCollection.Load();
				}

				return anotherFilteredCollection;
			}
		}

		DummyBusinessObjectWithDocumentSupportCollection anotherFilteredCollection;
		#region IDocumentSupportable Members
		public virtual DocumentSupporter DocumentSupporter
		{
			get
			{
				return new Supporter(this);
			}
		}

		public class Supporter : DocumentSupporter
		{
			public Supporter(DummyBusinessObjectWithDocumentSupport parent) : base(parent)
			{
			}

			public override BusinessContext BusinessContext
			{
				get
				{
					return DummyBusinessObjectWithDocumentSupport.BusinessContext;
				}
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get
				{
					return Env.Security.None;
				}
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
