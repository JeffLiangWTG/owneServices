using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class UPEJobDeclarationDocumentSupporter : JobDeclarationDocumentSupporter
	{
		public UPEJobDeclarationDocumentSupporter(UPEJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			if (CalloutDocumentSupporter != null)
			{
				CalloutDocumentSupporter.Initialise(documentEventSource);
			}
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(OnDocumentEventSource_DocumentPrinted);
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			ArrayList result = new ArrayList(base.GetSupportedDataContexts());
			result.Add(Core.Constants.DataContext.CusHAWB);
			return (Core.Constants.DataContext[])result.ToArray(typeof(Core.Constants.DataContext));
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			string result;
			if (filterType == MenuTemplateFilterType.PrintClientSpecificInvoice)
			{
				ZBool deliverTaxInvoice = (JobDeclaration.Importer.IsITFChargableForThisImporter || CalloutDocumentSupporter.Callout.FinanceFreightChargeIncludingGST != 0m);
				result = deliverTaxInvoice.ToString();
			}
			else if (filterType == MenuTemplateFilterType.PrintStandardInvoice)
			{
				result = JobDeclaration.HasCommercialInvoice.ToString();
			}
			else
			{
				result = base.GetMenuTemplateFilterValue(filterType, docWrapperForCurrentPivot);
			}
			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;
			if (menuName == UPEDocumentMenuItemLoader.AlternateBrokerDocumentPackMenuName)
			{
				OrgHeader contactOrganisation = JobDeclaration.AlternateBroker;
				result = new OrgHeaderContact(contactOrganisation, null);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contactType, direction);
			}
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			if (CalloutDocumentSupporter != null && dataContext == Core.Constants.DataContext.CusHAWB)
			{
				result = CalloutDocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
			return result;
		}

		#region AddCommercialInvoiceNotAvailableNoteIfRequired

		public void AddCommercialInvoiceNotAvailableNoteIfRequired()
		{
			if (!JobDeclaration.HasCommercialInvoice)
			{
				AddCommercialInvoiceNotAvailableNote();
			}
		}

		void AddCommercialInvoiceNotAvailableNote()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declarationInOtherFactory = (JobDeclaration)newFactory.Load(typeof(JobDeclaration), JobDeclaration.PK);
			if (declarationInOtherFactory != null)
			{
				declarationInOtherFactory.Notes.AddNew(true, CommercialInvoiceNotAvailableNoteDescription, "Commercial Invoice Image from eDocs is not available for delivery of documents to the alternate broker");
			}
			newFactory.Save();
		}

		#endregion

		#region Implementation

		protected new UPEJobDeclaration JobDeclaration
		{
			get { return (UPEJobDeclaration)base.JobDeclaration; }
		}

		protected CalloutDocumentSupporter CalloutDocumentSupporter
		{
			get
			{
				if (!fCalloutDocumentSupporterSet)
				{
					UPECusHAWB firstCusHAWB = JobDeclaration.FirstCusHAWB;
					if (firstCusHAWB != null)
					{
						fCalloutDocumentSupporter = GetDocumentSupporterFromCusHAWB(firstCusHAWB);
					}
					fCalloutDocumentSupporterSet = true;
				}
				return fCalloutDocumentSupporter;
			}
		}
		CalloutDocumentSupporter fCalloutDocumentSupporter;
		bool fCalloutDocumentSupporterSet;

		protected virtual CalloutDocumentSupporter GetDocumentSupporterFromCusHAWB(UPECusHAWB cusHAWB)
		{
			return (cusHAWB == null) ? null : new CalloutDocumentSupporter(cusHAWB);
		}

		void OnDocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (e.MenuItem.SU_MenuName == UPEDocumentMenuItemLoader.AlternateBrokerDocumentPackMenuName &&
				e.DeliveryInstructionDestinationType != DeliveryInstructionDestination.None &&
				e.DeliveryInstructionDestinationType != DeliveryInstructionDestination.UserCancelled &&
				e.DeliveryInstructionDestinationType != DeliveryInstructionDestination.Preview)
			{
				AddCommercialInvoiceNotAvailableNoteIfRequired();
			}
		}

		protected const string CommercialInvoiceNotAvailableNoteDescription = "Commercial Invoice Not Available";

		#endregion
	}
}
