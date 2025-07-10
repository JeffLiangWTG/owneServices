using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override CodeDescriptionPairList CustomsUQList =>
			Parent.IsDataLoadingModule
				? Factory.GetCachedValue<UnitOfMeasureListForDLM>()
				: Factory.GetCachedValue<CustomsUnitOfMeasureList>();

		public BusinessObjectCollection Tariffs => TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem, Parent.EffectiveDateForDutyRate);

		public CACusRulingFindBoxCollection AuthorityNumberList
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				var org = declaration != null ? declaration.ImporterOfRecord ?? declaration.Importer : InvoiceLine.InvoiceHeader?.Buyer;

				return Factory.GetCachedValue(ZString.Format("CusClassPartPivotLookups_{0}_{1}_{2}", org?.PK, Parent.CA_AuthorityNumber, Parent.CA_CalculationMethod),
					() => new CACusRulingFindBoxCollection(Factory, InvoiceLine.CA_CalculationMethod, InvoiceLine.CA_AuthorityNumber, org, null));
			}
		}

		public override IBaseClassificationCollection<BaseCusClassification> ClassificationList =>
			Parent.IsDataLoadingModule
				? new ExportClassificationCollection(Factory)
				: new HTSClassificationCollection(Factory);

		public CalculationMethods CalculationMethods
		{
			get
			{
				var calculationMethods = InvoiceLine.Declaration != null && InvoiceLine.Declaration.IsLVS
					? CACalculationMethodsWithDDPAndRRRemoved
					: CACalculationMethodsWithDDPRemoved;
				return calculationMethods;
			}
		}

		CalculationMethods CACalculationMethodsWithDDPAndRRRemoved
		{
			get
			{
				return Factory.GetCachedValue("CACalculationMethodsWithDDPAndRRRemoved", () =>
				{
					var result = new CalculationMethods();
					result.RemoveCode(CalculationMethods.Codes.DeliveredDutyPaid);
					result.RemoveCode(CalculationMethods.Codes.RepairsRemission);
					result.RemoveCode(CalculationMethods.Codes.DutyDeferral);
					result.RemoveCode(CalculationMethods.Codes.WarrantyRepairsRemission);
					return result;
				});
			}
		}

		CalculationMethods CACalculationMethodsWithDDPRemoved
		{
			get
			{
				return Factory.GetCachedValue("CACalculationMethodsWithDDPRemoved", () =>
				{
					var result = new CalculationMethods();
					result.RemoveCode(CalculationMethods.Codes.DeliveredDutyPaid);
					return result;
				});
			}
		}

		public AmountTypes AmountTypes => Factory.GetCachedValue<AmountTypes>();

		public override CodeDescriptionPairList InvoiceUQList
		{
			get
			{
				var result = base.InvoiceUQList;
				var declaration = Parent.Declaration;
				if (declaration != null && declaration.IsIID)
				{
					result = Factory.GetCachedValue<IIDUnitOfCountCodeList>();
				}
				return result;
			}
		}

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			var declaration = InvoiceLine.Declaration;
			if (declaration != null)
			{
				if (declaration.IsLVS)
				{
					var invoice = InvoiceLine.InvoiceHeader;
					return invoice == null ? new OrgSupplierPartCollection(Factory)
									: new OrgSupplierPartCollection(Factory, InvoiceLine, invoice.Supplier_Effective, invoice.Importer_Effective, invoice?.IsExport ?? ZBool.False);
				}
				else
				{
					return new OrgSupplierPartCollection(Factory, InvoiceLine, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
				}
			}
			return new OrgSupplierPartCollection(Factory);
		}

		public OrgHeaderCollection DeliveredParties => new ConsigneeCollection(Factory);

		public UNDGSubstanceCollection UNDGs => PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);

		#region Country and States

		public CodeDescriptionPairList StateCodesList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Parent.IsExport)
				{
					result = Factory.GetCachedValue<CanadianProvinceList>();
				}
				else
				{
					if (Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.UnitedStates)
					{
						result = Factory.GetCachedValue<USStatesList>();
					}
					else if (Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.Canada)
					{
						result = Factory.GetCachedValue<CanadianProvinceList>();
					}
				}
				return result;
			}
		}

		public OrganisationsFindBoxCollection Consignee => new OrganisationsFindBoxCollection(Factory);

		#endregion

		protected override ZString GetClassificationType()
		{
			var result = ZString.Empty;
			if (Parent.IsExport)
			{
				if (Parent.IsDataLoadingModule)
				{
					result = ClassificationTypeList.Codes.SHB;
				}
				else
				{
					result = ClassificationTypeList.Codes.HTE;
				}
			}
			else
			{
				result = ClassificationTypeList.Codes.HTI;
			}
			return result;
		}

		public override CodeDescriptionPairList VolumeUQList
		{
			get
			{
				var isIID = InvoiceLine.Declaration?.IsIID ?? false;
				return Factory.GetCachedValue("Enterprise.Customs.CA.Business.JobComInvoiceLineLookups.VolumeUQList,IsIID=" + isIID, () =>
				{
					var result = new CodeDescriptionPairList(base.VolumeUQList);
					if (isIID)
					{
						result.RemoveCode(Core.Constants.Volume.TeaChest);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList ParentIDList
		{
			get
			{
				var parentIdList = new CodeDescriptionPairList();
				var asAccountedHeader = Parent.InvoiceHeader?.CorrespondingAsAccountedForInvoice;
				if (asAccountedHeader != null)
				{
					var parentList = asAccountedHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>();
					parentList.ForEach(line => parentIdList.AddPair(line.PK, asAccountedHeader.JZ_InvoiceNumber + " - " + line.JI_LineNo, line.JI_Description));
				}
				return parentIdList;
			}
		}
	}
}
