using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public sealed class MockOperationalActionSupportable : IOperationalActionSupportable
	{
		public MockOperationalActionSupportable() : this(typeof(DummyBusinessObjectWithDocumentSupport))
		{
		}

		public MockOperationalActionSupportable(Type rootType)
		{
			this.rootType = rootType;
			this.supportsBulkUpdate = true;
		}

		public static readonly BusinessContext BusinessContext = BusinessContext.Shipment;
		public bool supportsBulkUpdate;
		public SecurityCheckpoint customizationCheckpoint;
		public SecurityCheckpoint runCheckpoint;
		public BusinessContext businessContext = BusinessContext;
		public readonly Type rootType;
		#region IOperationalActionSupportable Members
		public OperationalActionSupporter OperationalActionSupporter
		{
			get
			{
				return new Supporter(this);
			}
		}

		sealed class Supporter : OperationalActionSupporter
		{
			public Supporter(MockOperationalActionSupportable parent)
			{
				this.parent = parent;
			}

			public override BusinessContext BusinessContext
			{
				get
				{
					return parent.businessContext;
				}
			}

			protected override bool SupportsBulkUpdatesCore
			{
				get
				{
					return parent.supportsBulkUpdate;
				}
			}

			public override SecurityCheckpoint BaseCheckpoint => Env.Security.Organisation;
			public override SecurityCheckpoint CustomizationSecurityCheckpoint
			{
				get
				{
					return parent.customizationCheckpoint ?? base.CustomizationSecurityCheckpoint;
				}
			}

			public override SecurityCheckpoint RunSecurityCheckpoint
			{
				get
				{
					return parent.runCheckpoint ?? base.RunSecurityCheckpoint;
				}
			}

			public override Type RootType
			{
				get
				{
					return parent.rootType;
				}
			}

			readonly MockOperationalActionSupportable parent;
		}
		#endregion
	}
}
