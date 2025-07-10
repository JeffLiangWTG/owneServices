using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class LPCODeclarationFilterStripBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		public override ZQuery Filter => base.Filter.AddToFilter(JobDeclarationSchema.JE_MessageType, BRJobMessageTypeList.Codes.LPCO);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = base.GetModuleFiltersCore();

			foreach (var filter in GetFilterToRemove())
			{
				collection.RemoveFilter(collection[filter]);
			}

			collection[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.EntryNumber].MultilingualDescription = LPCONumberText;
			collection[Customs.Module.DeclarationFilterConstants.EntryStatusText].MultilingualDescription = LPCOStatusText;

			return collection;
		}

		internal static MultilingualString LPCONumberText = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|EntryNumber", "License #");
		internal static MultilingualString LPCOStatusText = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|EntryStatusText", "LPCO Status");

		IEnumerable<ZString> GetFilterToRemove()
		{
			yield return Customs.GUI.InvoiceLineFilterConstants.ContainerNumber;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentNumber;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentAmount;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.DateOfExport;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETDOfLoading;
			yield return Customs.Module.DeclarationFilterConstants.PortFilterTypes.LoadDischarge;
			yield return Customs.Module.DeclarationFilterConstants.PortFilterTypes.PortOfFirstArrival;
			yield return Customs.Module.DeclarationFilterConstants.PortFilterTypes.OriginDestination;
			yield return Customs.Module.DeclarationFilterConstants.FlightVoyageVessel;
			yield return Customs.Module.DeclarationFilterConstants.ServiceLevel;
			yield return Customs.Module.DeclarationFilterConstants.ServiceType;
			yield return Customs.Module.DeclarationFilterConstants.ShipmentSubType;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.PickupTransportCompany;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.DeliveryTransportCompany;
			yield return Customs.Module.DeclarationFilterConstants.RelatedTransportBookings;
			yield return Customs.Module.DeclarationFilterConstants.RelatedContainers;
		}

		public new LPCODeclarationFilterLookups Lookups
		{
			get { return (LPCODeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new LPCODeclarationFilterLookups(this);
		}

		protected override void AddAdditionalDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.ShipmentExpiryDate, GetValidityILShipmentDateQuery).MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|ShipmentExpiryDate", DeclarationFilterConstants.ShipmentExpiryDate);
		}

		ZQuery GetValidityILShipmentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
			=> EntryHeaderModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewConstants.BRCusEntryHeader.ClusterKey, ModelViewConstants.BRCusEntryHeader.Name, ModelViewConstants.BRCusEntryHeader.ValidityILShipmentDate, comparisonOperator, startDate, endDate);

		ModelViewColumnQueryHelper<Business.CusEntryHeader> EntryHeaderModelViewColumnHelper => entryHeadermodelViewColumnHelper ?? (entryHeadermodelViewColumnHelper = new ModelViewColumnQueryHelper<Business.CusEntryHeader>());
		ModelViewColumnQueryHelper<Business.CusEntryHeader> entryHeadermodelViewColumnHelper;
	}
}
