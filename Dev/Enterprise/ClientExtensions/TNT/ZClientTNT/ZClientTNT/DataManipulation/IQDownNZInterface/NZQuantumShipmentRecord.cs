
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.NZ
{
	internal class NZQuantumShipmentRecord : QuantumShipmentRecord
	{
		public NZQuantumShipmentRecord(ZString branchCode, ZString mBagNo, ZString line)
			: base(branchCode, mBagNo, line)
		{
		}

		protected override ForwardingShipment CreateShipmentCore(BusinessObjectFactory factory, bool createDeclaration, INotifications notify)
		{
			ForwardingShipment shipment = base.CreateShipmentCore(factory, createDeclaration, notify);
			TNTStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(shipment.JS_F3_NKPackTypeInfo, Constants.PkgUnit.Package, ForeignKeyType.None, notify);
			return shipment;
		}

		protected override void CreateNewDeclarationForShipment(ForwardingShipment shipment)
		{
			// Do Nothing
		}

		internal override void SetShipmentCustomsEntryNumber(ForwardingShipment shipment)
		{
			// Do Nothing
		}

		protected override ForwardingShipment FindFirstMatchingShipmentCore(ForwardingConsol consol)
		{
			ForwardingShipment result = null;
			ZQuery query = new ZQuery(JobShipmentSchema.JS_HouseBill, HouseBill);
			ForwardingShipment[] shipments = (ForwardingShipment[])consol.Shipments.Find(query);
			if (shipments.Length > 0)
			{
				result = shipments[0];
			}
			return result;
		}

		protected override OrgHeader GetConsignorByLegacyCode(BusinessObjectFactory factory, INotifications notify, TemporaryOrganisationCreator temporaryCreator)
		{
			ZString legacyCode = new ZString(ConsignorCountry.Left(2) + base.ConsignorLegacyCode).Left(OrgCusCode.Schema.OK_CustomsRegNoMaxLength);
			return OrgHeader.FindByOrgCusCode(factory, OrgCusCode.CodeTypes.LegacySystemCode, legacyCode);
		}

		protected override OrgMatcher GetOrgMatcher(BusinessObjectFactory factory)
		{
			return new NZOrgMatcher(factory);
		}
	}
}
