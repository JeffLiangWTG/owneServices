using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Testing
{
	public class DummyBODocSupportableDocumentSupporter : DocumentSupporter
	{
		public DummyBODocSupportableDocumentSupporter(DummyBODocSupportable docDummyBusinessObject)
			: base(docDummyBusinessObject)
		{
			Parent = docDummyBusinessObject;
		}
		protected readonly DummyBODocSupportable Parent;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Test; }
		}

		protected override Enterprise.Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Enterprise.Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.UnitTest };
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (commandBeingRun.SU_MenuName == "NULL")
			{
				return null;
			}
			else
			{
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return "NOT FOUND";
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint { get { return Env.Security.None; } }

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = new List<DocumentWrapper>();

			var dummyDocumentWrapper = new DummyDocumentWrapper(this.Parent, this.Parent.Factory);
			dummyDocumentWrapper.JobNumber = Parent.JobNumber;
			dummyDocumentWrapper.Int32Number = Parent.Z0_Number;
			result.Add(dummyDocumentWrapper);

			if (Parent.Collection != null)
			{
				dummyDocumentWrapper.CollectionWrapper = new DummyCollectionDocumentWrapper(Parent.Factory);
				foreach (DummyChildBusinessObject dummy in Parent.Collection)
				{
					var childWrapper = new DummyDocumentWrapper(dummy, dummy.Factory);
					childWrapper.Int32Number = dummy.Z0_Number;
					dummyDocumentWrapper.CollectionWrapper.Add(childWrapper);

					if (dummy.RelatedDummy != null)
					{
						var relatedDummyWrapper = new DummyDocumentWrapper(dummy.RelatedDummy, dummy.RelatedDummy.Factory);
						relatedDummyWrapper.Int32Number = dummy.RelatedDummy.Z0_Number;
						childWrapper.RelatedDummyWrapper = relatedDummyWrapper;
					}
				}
			}

			if (commandBeingRun != null && commandBeingRun.SU_MenuName.ToUpper().Contains("PUT ITEMS OF COLLECTION INTO THEIR OWN WRAPPER."))
			{
				foreach (var dummy in Parent.Collection)
				{
					result.Add(new DummyDocumentWrapper(dummy, dummy.Factory));
				}
			}

			return result.ToArray();
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (!Parent.Z0_Guid.IsEmpty)
			{
				var organisation = Factory.Load<OrgHeader>(Parent.Z0_Guid);
				if (organisation != null)
				{
					return new OrgHeaderContact(organisation, null);
				}
			}

			return null;
		}
	}
}
