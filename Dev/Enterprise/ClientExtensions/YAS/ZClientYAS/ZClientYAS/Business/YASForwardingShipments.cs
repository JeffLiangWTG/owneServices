using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.YAS.Business
{
	public class YASForwardingShipment : ForwardingShipment
	{
		public YASForwardingShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new YASForwardingShipment New(BusinessObjectFactory factory)
		{
			return factory.New<YASForwardingShipment>();
		}

		public bool IsExportingYASBillOfLading
		{
			get;
			set;
		}

		protected override JobDocAddress AddEventHandlersToConsigneeDocumentaryAddressEvents()
		{
			var address = base.AddEventHandlersToConsigneeDocumentaryAddressEvents();

			address.OrgAddressBeforeChange -= ConsigneeDocumentaryAddress_OrgAddressBeforeChange;
			address.OrgAddressBeforeChange += ConsigneeDocumentaryAddress_OrgAddressBeforeChange;

			return address;
		}

		protected override void ConsigneeDocumentaryAddressChanged()
		{
			base.ConsigneeDocumentaryAddressChanged();
			if (consigneeDocAddressBeforeChange.IsUnmatched() && ConsigneeDocumentaryAddress.E2_CompanyName != UnmatchedOrganisation &&
				Globals.CanShowDialogs && Consignee != null && !Consignee.OH_Code.IsEmpty)
			{
				YASExtentions.CheckIfAutoCodeMappingShouldBeDone(Consignee, Factory,
					Notes, new UnmatchOrgRecords(this), OrganisationTypes.Consignee,
					ZString.Empty, false, "Consignee:-", "Consignee", 20);
			}
		}

		internal virtual void ConsigneeDocumentaryAddress_OrgAddressBeforeChange(object sender, EventArgs e)
		{
			JobDocAddress address = sender as JobDocAddress;
			if (address != null && !address.IsRowDeletedOrDetachedOrNull)
			{
				consigneeDocAddressBeforeChange = (JobDocAddress)address.Clone();
			}
		}

		JobDocAddress consigneeDocAddressBeforeChange;

		public static readonly string UnmatchedOrganisation = "UNMATCHED ORGANISATION";

		public override ZString JS_BookingReference
		{
			get { return base.JS_BookingReference; }
			set
			{
				base.JS_BookingReference = value;

				if (!value.IsEmpty)
				{
					var factoryCopyService = Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>();
					if (factoryCopyService != null)
					{
						factoryCopyService.AddOnCopyFinishedAction(AddShippersReferenceNotIfNeeded);
					}
					else
					{
						AddShippersReferenceNotIfNeeded();
					}
				}
			}
		}

		void AddShippersReferenceNotIfNeeded()
		{
			if (!JS_BookingReference.IsEmpty)
			{
				StmNote[] notes = Notes.FindByDescription("Shipper's Reference");
				if (notes.Length == 0)
				{
					Notes.AddNew(false, "Shipper's Reference", JS_BookingReference);
				}
			}
		}

		protected override NumberGeneratorContext NewBillOfLadingGeneratorContextCore()
		{
			if (!Env.CurrentUser.IsBatchProcessor || (Env.CurrentUser.IsBatchProcessor && Job == null))
			{
				return new NumberGeneratorContext();
			}
			else
			{
				return new NumberGeneratorContext(GlbCompany.CurrentCompany.PK, this.Job.JH_GB, GlbDepartment.CurrentDepartment.PK);
			}
		}
	}
}
