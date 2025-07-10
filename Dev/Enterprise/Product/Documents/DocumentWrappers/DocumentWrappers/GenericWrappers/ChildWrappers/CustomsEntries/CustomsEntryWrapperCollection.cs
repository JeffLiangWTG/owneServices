using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CustomsEntryWrapperCollection : GenericWrapperCollection<CustomsEntryWrapper>
	{
		public CustomsEntryWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CustomsEntryWrapperCollection(CommonShipment shipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentBO != null)
			{
				AddFromShipment(shipmentBO);
				AddInbondTransitUS(shipmentBO);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CustomsEntryWrapperCollection(CommonConsol consolBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consolBO != null)
			{
				AddFromCusEntryNums(consolBO.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false);
				foreach (CommonShipment shipmentBO in consolBO.Shipments)
				{
					AddFromShipment(shipmentBO);
				}
			}
		}

		public CustomsEntryWrapperCollection(DtbBookingConsignment consignmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consignmentBO != null)
			{
				foreach (CusEntryNumber referenceType in consignmentBO.AdditionalReferenceNumbers)
				{
					Add(new CustomsEntryWrapperForConsignmentAdditionalReference(referenceType, Factory));
				}
			}
		}

		public CustomsEntryWrapperCollection(BaseJobDeclaration declarationBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (declarationBO != null)
			{
				if (declarationBO.IsExport && declarationBO is IJobDeclaration)
				{
					AddExportCA(declarationBO);
				}
				if (this.Count == 0)
				{
					AddFromDeclaration(declarationBO);

					if (declarationBO.Shipment != null)
					{
						AddInbondTransitUS(declarationBO.Shipment);
					}
				}
			}
		}

		public CustomsEntryWrapperCollection(WhsDocket docketBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (docketBO != null)
			{
				var wrappers = docketBO.References.Cast<WhsDocketReference>().Select(r => new CustomsEntryWrapperFromWarehouseReference(r, Factory));
				AddRange(wrappers);
			}
		}

		public CustomsEntryWrapperCollection(PkgPackage packageBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (packageBO != null)
			{
				var wrappers = packageBO.CusEntryNumReferences.Cast<CusEntryNumber>().Select(p => new CustomsEntryWrapperFromPkgPackage(p, Factory));
				AddRange(wrappers);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CustomsEntryWrapperCollection(BusinessObject parentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (parentBO != null)
			{
				AddFromCusEntryNums(parentBO.PK, ZString.Empty, false);
			}
		}

		void AddFromShipment(CommonShipment shipmentBO)
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)shipmentBO.DeclarationForDocuments;
			if (declaration != null)
			{
				AddFromDeclaration(declaration);
			}
			else
			{
				AddFromCusEntryNums(shipmentBO.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false);
			}
		}

		void AddFromDeclaration(BaseJobDeclaration declarationBO)
		{
			if (declarationBO.CustomsEntryHeaders.Count > 0)
			{
				foreach (CusEntryHeader entryHeader in declarationBO.CustomsEntryHeaders)
				{
					if (!entryHeader.EntryNumber.IsEmpty)
					{
						Add(new CustomsEntryWrapperFromCusEntryHeader(entryHeader, Factory));
					}
				}
			}
			else
			{
				AddFromCusEntryNums(declarationBO.PK, ZString.Empty, false);
			}

			if (declarationBO.Shipment != null)
			{
				var excludableNumberTypes = new List<ZString>();
				foreach (CustomsEntryWrapper exisitingNumber in this)
				{
					if (!exisitingNumber.EntryType.Code.IsEmpty)
					{
						excludableNumberTypes.Add(exisitingNumber.EntryType.Code);
					}
				}

				AddFromCusEntryNums(declarationBO.Shipment.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, false, excludableNumberTypes);
			}
		}

		void AddFromCusEntryNums(ZGuid parentPK, ZString countryCode, bool uniqueOnly)
		{
			AddFromCusEntryNums(parentPK, countryCode, ZString.Empty, uniqueOnly);
		}

		void AddFromCusEntryNums(ZGuid parentPK, ZString countryCode, ZString entryType, bool uniqueOnly, List<ZString> excludableNumberTypes = null)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parentPK);
			if (!countryCode.IsEmpty)
			{
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			}
			if (!entryType.IsEmpty)
			{
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			}

			var entryNumbers = Factory.Load<CusEntryNumber>(query);

			foreach (CusEntryNumber entryNumber in entryNumbers)
			{
				if (excludableNumberTypes != null && excludableNumberTypes.Contains(entryNumber.CE_EntryType))
				{
					continue;
				}

				if (!uniqueOnly || !ContainsType(entryNumber.CE_EntryType))
				{
					Add(new CustomsEntryWrapperFromCusEntryNumber(entryNumber, Factory));
				}
			}
		}

		void AddInbondTransitUS(CommonShipment shipmentBO)
		{
			var number = this[UnitedStatesAdditionalReferenceNumberTypes.Codes.IT];
			if (number == null && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				var consol = GetLastImportConsol(shipmentBO);
				if (consol != null)
				{
					AddFromCusEntryNums(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, false);
				}
			}
		}

		void AddExportCA(BaseJobDeclaration declaration)
		{
			var por = (CusEntryNumber)((IJobDeclaration)declaration).JE_CAEDProofOfReportCusEntryNumber;
			if (declaration.Shipment != null && (por == null || por.CE_EntryNum.IsEmpty))
			{
				por = CusEntryNumber.Load(declaration.Shipment, CanadaAdditionalReferenceNumberTypes.Codes.CTN, Core.Constants.CountryCodes.Canada);
			}
			if (por != null)
			{
				Add(new CustomsEntryWrapperFromCusEntryNumber(por, Factory));
			}
		}

		ForwardingConsol GetLastImportConsol(CommonShipment shipmentBO)
		{
			var shipment = shipmentBO as ForwardingShipment;
			if (shipment != null)
			{
				var consols = shipment.Consols.ToArray<ForwardingConsol>();
				MovementLegComparer.SortMovementLegsByPorts(consols);
				for (int index = consols.Length - 1; index >= 0; index--)
				{
					if (consols[index].IsImport())
					{
						return consols[index];
					}
				}
			}

			return null;
		}

		internal bool ContainsType(ZString type)
		{
			foreach (CustomsEntryWrapper numberWrapper in this)
			{
				if (numberWrapper.EntryType.Code == type)
				{
					return true;
				}
			}
			return false;
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			index = index.ToUpper();
			if (index == "CLEARANCE")
			{
				foreach (CustomsEntryWrapper wrapper in this)
				{
					if (wrapper.EntryCategory == CusEntryNumber.Categories.CustomsPermitClearanceNumber)
					{
						return wrapper;
					}
				}
			}

			CusEntryNumSearchCriteria criteria = new CusEntryNumSearchCriteria(index);

			CustomsEntryWrapper customsEntryWrapper = (CustomsEntryWrapper)criteria.Find(this.Cast<ISearcheableCusEntryNumber>());

			if (customsEntryWrapper != null)
			{
				return customsEntryWrapper;
			}

			return base.GetRow(index);
		}

		public ZString CustomsEntryNumberSummary
		{
			get { return GetCustomsEntryNumberSummary(this.Cast<CustomsEntryWrapper>()); }
		}

		public ZString GetCustomsEntryNumberSummaryForType(ZString type)
		{
			return GetCustomsEntryNumberSummary(this.Cast<CustomsEntryWrapper>().Where(ce => ce.EntryType.Code == type));
		}

		ZString GetCustomsEntryNumberSummary(IEnumerable<CustomsEntryWrapper> customsEntries)
		{
			var result = new List<string>();
			foreach (var wrapper in customsEntries)
			{
				if (!wrapper.EntryNumber.IsEmpty)
				{
					result.Add(wrapper.EntryNumber);
				}
			}

			return string.Join(", ", result.ToArray());
		}
	}
}
