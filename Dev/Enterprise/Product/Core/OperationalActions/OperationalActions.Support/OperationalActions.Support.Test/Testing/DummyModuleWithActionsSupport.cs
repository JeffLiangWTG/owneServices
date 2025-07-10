using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	public sealed class DummyModuleWithActionsSupport<DummyT> : DummyFilterGridModule, IOperationalActionSupportable where DummyT : DummyBusinessObject
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new Collection(Factory);
		}

		sealed class Collection : BusinessObjectCollection<DummyT>
		{
			public Collection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return fControllerID != null ? ZControllerFactory.Create(fControllerID) : base.GetNewController(selectedBusinessObject);
		}

		public ControllerID fControllerID { get; set; }

		#region IOperationalActionSupportable Members
		public OperationalActionSupporter OperationalActionSupporter
		{
			get
			{
				return new Supporter();
			}
		}

		public sealed class Supporter : OperationalActionSupporter
		{
			public override BusinessContext BusinessContext
			{
				get
				{
					return DummyBusinessObjectWithDocumentSupport.BusinessContext;
				}
			}

			public override Type RootType
			{
				get
				{
					return typeof(DummyT);
				}
			}

			public override SecurityCheckpoint BaseCheckpoint => Env.Security.Organisation;
		}
		#endregion
	}
}
