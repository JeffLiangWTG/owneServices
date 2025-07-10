using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCasesAttribute]
	public class DummyCustomsBusinessObject : DummyEnterpriseBusinessObject, IDocumentSupportable, ICreditControlledDocumentDelivery
	{
		DocumentCommandCollection documentCommands;
		DummyCustomsBusinessObjectDocumentSupporter documentSupporter;
		DummyBusinessObjectCollection shipments;

		public DummyCustomsBusinessObject(BusinessObjectFactory factory, DataRow row)
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

		public DummyCustomsBusinessObjectDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DummyCustomsBusinessObjectDocumentSupporter(this)); }
			set { documentSupporter = value; }
		}

		public DummyBusinessObjectCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new DummyBusinessObjectCollection(Factory);
					ZQuery filter = new ZQuery(DummyBizoSchema.Z0_FK_Code, Z0_Code);
					filter.OrderBy = DummyBizoSchema.Constants.Z0_Code;
					shipments.AddRange(Factory.Load(typeof(DummyShipmentBusinessObject), filter));
				}
				return shipments;
			}
		}

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return DocumentSupporter; }
		}

		#endregion

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

		public ScreeningParty[] DPSPartiesForTest = Array.Empty<ScreeningParty>();
		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return DPSPartiesForTest;
		}

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

	#region DummyCustomsBusinessObjectDocumentSupporter

	[ExcludeDocumentSupporterTest]
	public class DummyCustomsBusinessObjectDocumentSupporter : DocumentSupporter
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool returnNullWrappers;

		public DummyCustomsBusinessObjectDocumentSupporter(DummyCustomsBusinessObject parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Customs; }
		}

		protected new DummyCustomsBusinessObject BusinessObject
		{
			get { return (DummyCustomsBusinessObject)base.BusinessObject; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public bool ReturnNullWrappers
		{
			get { return returnNullWrappers; }
			set { returnNullWrappers = value; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.AUCustoms };
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext childBusinessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;
			if (childBusinessContext == BusinessContext.Shipment)
			{
				result = (IDocumentSupportable[])BusinessObject.Shipments.ToArray(typeof(IDocumentSupportable));
			}
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (!ReturnNullWrappers && (dataContext == Core.Constants.DataContext.AUCustoms))
			{
				result = new DocumentWrapper[] { new DummyCustomsDocumentWrapper(BusinessObject, Factory) };
			}
			return result;
		}
	}

	#endregion

	#region DummyCustomsDocumentWrapper

	class DummyCustomsDocumentWrapper : DocumentWrapper
	{
		public DummyCustomsDocumentWrapper(DummyCustomsBusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public override string ToString()
		{
			return null;
		}
	}

	#endregion
}
