using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCasesAttribute]
	public class DummyShipmentBusinessObject : DummyEnterpriseBusinessObject, IDocumentSupportable, ICreditControlledDocumentDelivery
	{
		DocumentCommandCollection documentCommands;
		DocumentSupporter documentSupporter;
		IDocumentSupportable parentBusinessObjectCalledByGetDocumentTitlesForPivot;
		public static readonly ZGuid TestOrganisationPK = new ZGuid("47F51331-B556-4BD2-977C-F99132751BA9");

		public DummyShipmentBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DocumentCommandCollection DocumentCommands
		{
			get
			{
				if (documentCommands == null)
				{
					documentCommands = new DocumentCommandCollection(this);
					documentCommands.Load();
				}
				return documentCommands;
			}
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DummyShipmentBusinessObjectDocumentSupporter(this)); }
			set { documentSupporter = value; }
		}

		public IDocumentSupportable ParentBusinessObjectCalledByGetDocumentTitlesForPivot
		{
			get { return parentBusinessObjectCalledByGetDocumentTitlesForPivot; }
			set { parentBusinessObjectCalledByGetDocumentTitlesForPivot = value; }
		}

		public IList<DummyShipmentBusinessObject> CoLoadShipments
		{
			get
			{
				EnsureCoLoadShipmentCollectionIsInitialised();
				return fCoLoadShipments;
			}
		}
		IList<DummyShipmentBusinessObject> fCoLoadShipments;

		void EnsureCoLoadShipmentCollectionIsInitialised()
		{
			if (fCoLoadShipments == null)
			{
				fCoLoadShipments = new List<DummyShipmentBusinessObject>();
			}
		}

		#region ICreditControlledDocumentDelivery Member
		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return "Consignee, Consignor or Local Client for Billing"; }
		}

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { fGetDocumentLogin += value; }
			remove { fGetDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		public OrgHeader[] OrganisationsForCreditChecksForTest;
		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get { return OrganisationsForCreditChecksForTest; }
		}

		public bool IsDPSFreightMovementRestrictedForTest;
		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get { return IsDPSFreightMovementRestrictedForTest; }
		}

		public ScreeningParty[] GetScreeningParties()
		{
			return DPSPartiesForTest;
		}

		public ScreeningParty[] DPSPartiesForTest = Array.Empty<ScreeningParty>();

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (fGetDocumentLogin != null)
			{
				fGetDocumentLogin(this, e);
			}
		}

		#endregion

		string[] IRelatedJobNumber.JobNumber
		{
			get { return new[] { "123" }; }
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;
	}

	#region DummyShipmentBusinessObjectDocumentSupporter

	class DummyShipmentBusinessObjectDocumentSupporter : DocumentSupporter
	{
		public DummyShipmentBusinessObjectDocumentSupporter(DummyShipmentBusinessObject parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected new DummyShipmentBusinessObject BusinessObject
		{
			get { return (DummyShipmentBusinessObject)base.BusinessObject; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] supportables;
			switch (businessContext)
			{
				case BusinessContext.Shipment:
					supportables = new IDocumentSupportable[] { BusinessObject };
					break;
				case BusinessContext.SubShipment:
					supportables = BusinessObject.CoLoadShipments.Cast<IDocumentSupportable>().ToArray();
					break;
				default:
					supportables = base.GetChildCollection(menuToBeRun, businessContext, childCommand);
					break;
			}
			return supportables;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			OrgHeaderContact result = null;
			if (contact == ContactType.Consignee)
			{
				OrgHeader org = Factory.Load<OrgHeader>(DummyShipmentBusinessObject.TestOrganisationPK);
				result = new OrgHeaderContact(org, org, null);
			}
			return result;
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString documentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			BusinessObject.ParentBusinessObjectCalledByGetDocumentTitlesForPivot = parentBusinessObject;
			return null;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (dataContext == Core.Constants.DataContext.Shipment)
			{
				result = new DocumentWrapper[] { new DummyShipmentDocumentWrapper(BusinessObject, Factory) };
			}
			return result;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.Shipment };
		}
	}

	#endregion

	#region DummyShipmentDocumentWrapper

	class DummyShipmentDocumentWrapper : DocumentWrapper
	{
		public DummyShipmentDocumentWrapper(DummyShipmentBusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public string Code
		{
			get { return WrappedObject.Z0_Code; }
		}

		public new DummyShipmentBusinessObject WrappedObject
		{
			get { return (DummyShipmentBusinessObject)base.WrappedObject; }
		}

		public override string ToString()
		{
			return null;
		}

		public OrgHeader Consignee { get; set; }

		public OrgHeader Consignor { get; set; }
	}

	#endregion
}
