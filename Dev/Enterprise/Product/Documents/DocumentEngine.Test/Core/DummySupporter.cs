using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummySupporter : DocumentSupporter
	{
		public DummySupporter(DummySupportable parent)
			: base(parent)
		{
			this.client = parent.Client;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			if (contactType == ContactType.LocalClient)
			{
				return new OrgHeaderContact(client, null);
			}
			else
			{
				return base.GetContactOrganisation(menuName, contactType, direction);
			}
		}

		#region Not Implemented

		public override BusinessContext BusinessContext
		{
			get { throw new NotImplementedException(); }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { throw new NotImplementedException(); }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			throw new NotImplementedException();
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			throw new NotImplementedException();
		}

		#endregion

		readonly OrgHeader client;
	}
}
