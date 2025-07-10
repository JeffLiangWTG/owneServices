using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBDocumentSupporter : DocumentSupporter
	{
		public CusHAWBDocumentSupporter(CusHAWBBase cusHAWB)
			: base(cusHAWB)
		{
		}

		protected CusHAWBBase CusHAWB
		{
			get { return (CusHAWBBase)BusinessObject; }
		}

		#region Overrides

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.CusHAWB, Core.Constants.DataContext.GenericFreightJob };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusHAWB; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ACAHouseCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, CusHAWB);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper result = null;

			switch (dataContext)
			{
				case Core.Constants.DataContext.CusHAWB:
					result = DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.CusHAWB, CusHAWB, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					break;

				default:
					break; // Currently unsupported DataContext
			}

			return (result == null) ? null : new DocumentWrapper[] { result };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;
			if (contact == ContactType.Consignee)
			{
				result = new OrgHeaderContact(CusHAWB.Consignee, null);
			}
			else if (contact == ContactType.Consignor)
			{
				result = new OrgHeaderContact(CusHAWB.Consignor, null);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Core.Constants.DataContext.CusHAWB)
			{
				return Res.GetString("27054E8A-8273-4F4E-BA68-15A3F26D9A45", "This shipment is not associated with a HAWB data.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.GenericFreightJob && base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion
	}
}
